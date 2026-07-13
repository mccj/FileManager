using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Composite
{
    public sealed class CompositeFileStore : IMultiDriveFileStore, IFileStoreCapabilities
    {
        private readonly IReadOnlyDictionary<string, IFileStore> _drives;

        public CompositeFileStore(IReadOnlyDictionary<string, IFileStore> drives)
        {
            if (drives == null || drives.Count == 0)
                throw new ArgumentException("At least one drive is required.", nameof(drives));

            var map = new Dictionary<string, IFileStore>(StringComparer.OrdinalIgnoreCase);
            foreach (var pair in drives)
                map[pair.Key] = pair.Value;
            _drives = map;
        }

        public IReadOnlyCollection<string> DriveNames => _drives.Keys.ToArray();

        public FileStoreCapabilityFlags Capabilities
        {
            get
            {
                var flags = FileStoreCapabilityFlags.None;
                foreach (var store in _drives.Values)
                    flags |= store.GetCapabilities();
                return flags;
            }
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            Resolve(path, out var store, out var storePath, out var drive, out var isDriveRoot);
            if (isDriveRoot)
                return null;

            var entry = await store.GetFileInfoAsync(storePath, cancellationToken);
            return entry == null ? null : new CompositeStoreEntry(CombineCompositePath(drive, entry.Path), entry);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = VirtualPath.Normalize(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                return CompositeStoreEntry.CreateRoot();

            Resolve(path, out var store, out var storePath, out var drive, out var isDriveRoot);
            if (isDriveRoot)
                return new CompositeStoreEntry(drive, isDriveRoot: true);

            var entry = await store.GetDirectoryInfoAsync(storePath, cancellationToken);
            return entry == null ? null : new CompositeStoreEntry(CombineCompositePath(drive, entry.Path), entry);
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = VirtualPath.Normalize(path) ?? string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                return _drives.Keys
                    .Select(driveName => (IFileStoreEntry)new CompositeStoreEntry(driveName, isDriveRoot: true))
                    .ToArray();
            }

            Resolve(path, out var store, out var storePath, out var drive, out var isDriveRoot);
            var entries = await store.GetDirectoryContentAsync(storePath, includeSubDirectories, cancellationToken);
            return entries.Select(entry => new CompositeStoreEntry(CombineCompositePath(drive, entry.Path), entry)).ToArray();
        }

        public async Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            Resolve(path, out var store, out var storePath, out _, out var isDriveRoot);
            if (isDriveRoot)
                throw new FileStoreException("Cannot create a drive at the composite root.");
            return await store.TryCreateDirectoryAsync(storePath, cancellationToken);
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            Resolve(path, out var store, out var storePath, out _, out var isDriveRoot);
            if (isDriveRoot)
                return false;
            return await store.TryDeleteFileAsync(storePath, cancellationToken);
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = VirtualPath.Normalize(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the composite root.");

            Resolve(path, out var store, out var storePath, out var drive, out var isDriveRoot);
            if (isDriveRoot)
                throw new FileStoreException($"Cannot delete drive '{drive}'.");

            return await store.TryDeleteDirectoryAsync(storePath, cancellationToken);
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            Resolve(oldPath, out var oldStore, out var oldStorePath, out var oldDrive, out var oldDriveRoot);
            Resolve(newPath, out var newStore, out var newStorePath, out var newDrive, out var newDriveRoot);
            if (oldDriveRoot || newDriveRoot)
                throw new FileStoreException("Cannot move a drive root.");
            if (!string.Equals(oldDrive, newDrive, StringComparison.OrdinalIgnoreCase))
                throw new FileStoreException("Cross-drive move is not supported. Use copy and delete instead.");

            await oldStore.MoveFileAsync(oldStorePath, newStorePath, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            Resolve(srcPath, out var srcStore, out var srcStorePath, out var srcDrive, out var srcDriveRoot);
            Resolve(dstPath, out var dstStore, out var dstStorePath, out var dstDrive, out var dstDriveRoot);
            if (srcDriveRoot || dstDriveRoot)
                throw new FileStoreException("Cannot copy a drive root.");

            if (string.Equals(srcDrive, dstDrive, StringComparison.OrdinalIgnoreCase))
            {
                await srcStore.CopyFileAsync(srcStorePath, dstStorePath, cancellationToken);
                return;
            }

            using (var stream = await srcStore.GetFileStreamAsync(srcStorePath, cancellationToken))
            {
                await dstStore.CreateFileFromStreamAsync(dstStorePath, stream, overwrite: false, cancellationToken);
            }
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            Resolve(path, out var store, out var storePath, out _, out var isDriveRoot);
            if (isDriveRoot)
                throw new FileStoreException("Cannot open a drive root as a file.");
            return await store.GetFileStreamAsync(storePath, cancellationToken);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            Resolve(path, out var store, out var storePath, out _, out var isDriveRoot);
            if (isDriveRoot)
                throw new FileStoreException("Cannot create a file at a drive root.");
            await store.CreateFileFromStreamAsync(storePath, inputStream, overwrite, cancellationToken);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            Resolve(oldPath, out var oldStore, out var oldStorePath, out var oldDrive, out var oldDriveRoot);
            Resolve(newPath, out var newStore, out var newStorePath, out var newDrive, out var newDriveRoot);
            if (oldDriveRoot || newDriveRoot)
                throw new FileStoreException("Cannot move a drive root.");
            if (!string.Equals(oldDrive, newDrive, StringComparison.OrdinalIgnoreCase))
                throw new FileStoreException("Cross-drive directory move is not supported.");

            await oldStore.MoveDirectoryAsync(oldStorePath, newStorePath, cancellationToken);
        }

        private void Resolve(string path, out IFileStore store, out string storePath, out string drive, out bool isDriveRoot)
        {
            path = VirtualPath.Normalize(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("A drive name is required. Example: local/documents/file.txt");

            if (!DrivePath.TrySplit(path, out drive, out storePath))
            {
                if (!_drives.TryGetValue(path, out store))
                    throw new FileStoreException($"Unknown drive '{path}'.");
                isDriveRoot = true;
                storePath = string.Empty;
                return;
            }

            if (!_drives.TryGetValue(drive, out store))
                throw new FileStoreException($"Unknown drive '{drive}'.");

            isDriveRoot = string.IsNullOrEmpty(storePath);
        }

        private static string CombineCompositePath(string drive, string innerPath)
        {
            if (string.IsNullOrEmpty(innerPath))
                return drive;
            return drive.TrimEnd('/') + "/" + innerPath.TrimStart('/');
        }

    }
}
