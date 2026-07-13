using FileManager.FileStorage;
using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Compress
{
    public class CompressStore : IFileStore, IFileStoreCapabilities
    {
        private readonly Stream _zipFileStream;
        private readonly string _password;
        private readonly object _indexLock = new object();
        private List<IndexedEntry> _entries;

        private static readonly FileStoreCapabilityFlags ReadOnlyCapabilities =
            FileStoreCapabilityFlags.Read | FileStoreCapabilityFlags.List | FileStoreCapabilityFlags.Download | FileStoreCapabilityFlags.Preview;

        public FileStoreCapabilityFlags Capabilities => ReadOnlyCapabilities;

        public CompressStore(Stream zipFileStream) : this(zipFileStream, null)
        {
        }

        public CompressStore(Stream zipFileStream, string password)
        {
            _zipFileStream = zipFileStream ?? throw new ArgumentNullException(nameof(zipFileStream));
            _password = password;
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var entry = EnsureIndex().FirstOrDefault(item => !item.IsDirectory && item.VirtualPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult<IFileStoreEntry>(entry == null ? null : new CompressStoreEntry(path, entry.ArchiveEntry));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var directoryEntry = EnsureIndex().FirstOrDefault(item => item.IsDirectory && item.VirtualPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (directoryEntry != null)
                return Task.FromResult<IFileStoreEntry>(new CompressStoreEntry(path, directoryEntry.ArchiveEntry));

            var hasChildren = EnsureIndex().Any(item => IsDirectChild(path, item.VirtualPath));
            return Task.FromResult<IFileStoreEntry>(hasChildren ? new CompressStoreEntry(path, isDirectory: true) : null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            return Task.FromResult(MapDirectoryEntries(path, EnsureIndex()));
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            _zipFileStream.Position = 0;
            using (var reader = ReaderFactory.OpenReader(_zipFileStream, new ReaderOptions { Password = _password, LeaveStreamOpen = true }))
            {
                while (reader.MoveToNextEntry())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (reader.Entry.IsDirectory)
                        continue;

                    if (!ConvertArchivePath(reader.Entry.Key, isZipPath: true).Equals(path, StringComparison.OrdinalIgnoreCase))
                        continue;

                    using (var entryStream = reader.OpenEntryStream())
                    {
                        var buffer = new MemoryStream();
                        await entryStream.CopyToAsync(buffer, 81920, cancellationToken);
                        buffer.Position = 0;
                        return buffer;
                    }
                }
            }

            throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        private List<IndexedEntry> EnsureIndex()
        {
            if (_entries != null)
                return _entries;

            lock (_indexLock)
            {
                if (_entries != null)
                    return _entries;

                _zipFileStream.Position = 0;
                var list = new List<IndexedEntry>();
                using (var reader = ReaderFactory.OpenReader(_zipFileStream, new ReaderOptions { Password = _password, LeaveStreamOpen = true }))
                {
                    while (reader.MoveToNextEntry())
                    {
                        list.Add(new IndexedEntry
                        {
                            VirtualPath = ConvertArchivePath(reader.Entry.Key, isZipPath: true),
                            IsDirectory = reader.Entry.IsDirectory,
                            ArchiveEntry = reader.Entry
                        });
                    }
                }

                _entries = list;
                return _entries;
            }
        }

        private IEnumerable<IFileStoreEntry> MapDirectoryEntries(string path, IEnumerable<IndexedEntry> fileInfos)
        {
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var depth = (path ?? string.Empty).Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Length;

            foreach (var item in fileInfos.Where(f => IsDirectChild(path, f.VirtualPath)))
            {
                var relativeSegments = item.VirtualPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Skip(depth).ToArray();
                var fileName = relativeSegments.FirstOrDefault();
                if (string.IsNullOrEmpty(fileName) || entries.ContainsKey(fileName))
                    continue;

                var childPath = string.IsNullOrEmpty(path) ? fileName : path + "/" + fileName;
                if (relativeSegments.Length > 1 || item.IsDirectory)
                    entries[fileName] = new CompressStoreEntry(childPath, isDirectory: true);
                else
                    entries[fileName] = new CompressStoreEntry(item.VirtualPath, item.ArchiveEntry);
            }

            return entries.Values.ToArray();
        }

        private static string ConvertArchivePath(string path, bool isZipPath)
        {
            var up = path.Replace("/", "\\");
            if (isZipPath)
                up = "\\" + string.Join("\\", up.Split(new[] { "\\" }, StringSplitOptions.RemoveEmptyEntries).Skip(1));

            return up.Trim('\\').Replace("\\", "/");
        }

        private static bool IsDirectChild(string path, string entryPath)
        {
            path = path ?? string.Empty;
            var expectedDepth = path.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Length + 1;
            return entryPath.StartsWith(path, StringComparison.OrdinalIgnoreCase)
                && entryPath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries).Length == expectedDepth;
        }

        private static Task ThrowReadOnlyAsync()
        {
            throw new FileStoreException("CompressStore is read-only.");
        }

        private static Task<T> ThrowReadOnlyAsync<T>()
        {
            throw new FileStoreException("CompressStore is read-only.");
        }

        private sealed class IndexedEntry
        {
            public string VirtualPath { get; set; }
            public bool IsDirectory { get; set; }
            public SharpCompress.Common.IEntry ArchiveEntry { get; set; }
        }
    }
}
