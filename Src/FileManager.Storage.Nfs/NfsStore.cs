using FileManager.FileStorage;
using FileManager.FileStorage.Standard;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Nfs
{
    public sealed class NfsStore : IFileStore
    {
        private readonly FileSystemStore _inner;

        public NfsStore(string mountPath)
        {
            if (string.IsNullOrWhiteSpace(mountPath))
                throw new ArgumentException("NFS mount path is required.", nameof(mountPath));
            _inner = new FileSystemStore(mountPath);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
            => WrapEntry(_inner.GetFileInfoAsync(path, cancellationToken));

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
            => WrapEntry(_inner.GetDirectoryInfoAsync(path, cancellationToken));

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            var entries = await _inner.GetDirectoryContentAsync(path, includeSubDirectories, cancellationToken);
            return entries.Select(e => (IFileStoreEntry)new NfsStoreEntry(e));
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
            => _inner.TryCreateDirectoryAsync(path, cancellationToken);

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
            => _inner.TryDeleteFileAsync(path, cancellationToken);

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
            => _inner.TryDeleteDirectoryAsync(path, cancellationToken);

        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
            => _inner.MoveFileAsync(oldPath, newPath, cancellationToken);

        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
            => _inner.CopyFileAsync(srcPath, dstPath, cancellationToken);

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
            => _inner.GetFileStreamAsync(path, cancellationToken);

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
            => _inner.GetFileStreamAsync(fileStoreEntry, cancellationToken);

        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
            => _inner.CreateFileFromStreamAsync(path, inputStream, overwrite, cancellationToken);

        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
            => _inner.MoveDirectoryAsync(oldPath, newPath, cancellationToken);

        private static async Task<IFileStoreEntry> WrapEntry(Task<IFileStoreEntry> task)
        {
            var entry = await task;
            return entry == null ? null : new NfsStoreEntry(entry);
        }
    }
}
