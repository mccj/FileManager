using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage.Standard
{
    public class FileProviderStore : IFileStore, IFileStoreCapabilities
    {
        private readonly IFileProvider _fileProvider;
        private static readonly FileStoreCapabilityFlags ReadOnlyCapabilities =
            FileStoreCapabilityFlags.Read | FileStoreCapabilityFlags.List | FileStoreCapabilityFlags.Download | FileStoreCapabilityFlags.Preview;

        public FileProviderStore(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        public FileStoreCapabilityFlags Capabilities => ReadOnlyCapabilities;

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPath = this.NormalizePath(path);
            var fileInfo = _fileProvider.GetFileInfo(normalizedPath ?? string.Empty);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
                return Task.FromResult<IFileStoreEntry>(null);

            return Task.FromResult<IFileStoreEntry>(new FileProviderStoreEntry(normalizedPath, fileInfo));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPath = this.NormalizePath(path);
            var contents = _fileProvider.GetDirectoryContents(normalizedPath ?? string.Empty);
            if (!contents.Exists)
                return Task.FromResult<IFileStoreEntry>(null);

            var directoryInfo = _fileProvider.GetFileInfo(normalizedPath ?? string.Empty);
            if (directoryInfo.Exists && directoryInfo.IsDirectory)
                return Task.FromResult<IFileStoreEntry>(new FileProviderStoreEntry(normalizedPath ?? string.Empty, directoryInfo));

            return Task.FromResult<IFileStoreEntry>(new FileProviderStoreEntry(
                normalizedPath ?? string.Empty,
                new VirtualDirectoryFileInfo(normalizedPath ?? string.Empty)));
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPath = this.NormalizePath(path);
            var contents = _fileProvider.GetDirectoryContents(normalizedPath ?? string.Empty);
            if (!contents.Exists)
                throw new FileStoreException($"The directory '{path}' does not exist.");

            var results = new List<IFileStoreEntry>();
            CollectEntries(normalizedPath, includeSubDirectories, results, cancellationToken);
            return Task.FromResult((IEnumerable<IFileStoreEntry>)results);
        }

        private void CollectEntries(string path, bool includeSubDirectories, List<IFileStoreEntry> results, CancellationToken cancellationToken)
        {
            var contents = _fileProvider.GetDirectoryContents(path ?? string.Empty);
            if (!contents.Exists)
                return;

            var directories = new List<IFileStoreEntry>();
            var files = new List<IFileStoreEntry>();

            foreach (var entry in contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entryPath = string.IsNullOrEmpty(path) ? entry.Name : $"{path}/{entry.Name}";
                var storeEntry = new FileProviderStoreEntry(this.NormalizePath(entryPath), entry);
                if (entry.IsDirectory)
                    directories.Add(storeEntry);
                else
                    files.Add(storeEntry);
            }

            results.AddRange(directories);
            results.AddRange(files);

            if (includeSubDirectories)
            {
                foreach (var directory in directories)
                    CollectEntries(directory.Path, true, results, cancellationToken);
            }
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var normalizedPath = this.NormalizePath(path);
            var fileInfo = _fileProvider.GetFileInfo(normalizedPath);
            if (!fileInfo.Exists || fileInfo.IsDirectory)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return Task.FromResult(fileInfo.CreateReadStream());
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync<bool>();
        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();
        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default) => ThrowReadOnlyAsync();

        private static Task ThrowReadOnlyAsync()
        {
            throw new FileStoreException("FileProviderStore is read-only.");
        }

        private static Task<T> ThrowReadOnlyAsync<T>()
        {
            throw new FileStoreException("FileProviderStore is read-only.");
        }

        private sealed class VirtualDirectoryFileInfo : IFileInfo
        {
            public VirtualDirectoryFileInfo(string path)
            {
                Name = string.IsNullOrEmpty(path)
                    ? string.Empty
                    : path.Substring(path.LastIndexOf('/') + 1);
                LastModified = DateTimeOffset.UtcNow;
            }

            public bool Exists => true;
            public long Length => -1;
            public string PhysicalPath => null;
            public string Name { get; }
            public DateTimeOffset LastModified { get; }
            public bool IsDirectory => true;
            public Stream CreateReadStream() => throw new NotSupportedException();
        }
    }
}
