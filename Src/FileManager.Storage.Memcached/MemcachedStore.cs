using Enyim.Caching;
using Enyim.Caching.Configuration;
using Enyim.Caching.Memcached;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Memcached
{
    public sealed class MemcachedStore : IFileStore
    {
        private const string DirectorySetKey = "fm:dirs";
        private const string FileKeyPrefix = "fm:file:";

        private readonly IMemcachedClient _client;

        public MemcachedStore(IMemcachedClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            var directories = GetDirectorySet();
            directories.Add(string.Empty);
            SaveDirectorySet(directories);
        }

        public MemcachedStore(string host, int port = 11211)
            : this(CreateClient(host, port))
        {
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var key = FileKeyPrefix + path;
            var content = _client.Get<byte[]>(key);
            if (content == null)
                return Task.FromResult<IFileStoreEntry>(null);

            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, content.LongLength, DateTimeOffset.UtcNow));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var directories = GetDirectorySet();
            if (directories.Contains(path) || HasChildren(path))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (!GetDirectorySet().Contains(path) && !HasChildren(path))
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            foreach (var directoryPath in GetDirectorySet())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsDirectChild(path, prefix, directoryPath, out var childName))
                    continue;
                if (!entries.ContainsKey(childName))
                    entries[childName] = CreateEntry(this.Combine(path, childName), true, 0, DateTimeOffset.UtcNow);
            }

            foreach (var filePath in EnumerateKnownFilePaths())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsDirectChild(path, prefix, filePath, out var childName))
                    continue;
                if (entries.ContainsKey(childName))
                    continue;

                var content = _client.Get<byte[]>(FileKeyPrefix + filePath);
                var length = content?.LongLength ?? 0;
                entries[childName] = CreateEntry(this.Combine(path, childName), false, length, DateTimeOffset.UtcNow);
            }

            if (includeSubDirectories)
            {
                foreach (var directory in entries.Values.Where(e => e.IsDirectory).ToArray())
                {
                    foreach (var nested in GetDirectoryContentAsync(directory.Path, true, cancellationToken).GetAwaiter().GetResult())
                    {
                        if (!entries.ContainsKey(nested.Path))
                            entries[nested.Path] = nested;
                    }
                }
            }

            return Task.FromResult(entries.Values.AsEnumerable());
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (_client.Get<byte[]>(FileKeyPrefix + path) != null)
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            EnsureParentDirectories(path);
            var directories = GetDirectorySet();
            var added = directories.Add(path);
            SaveDirectorySet(directories);
            return Task.FromResult(added);
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var removed = _client.Remove(FileKeyPrefix + path);
            if (removed)
                UntrackFilePath(path);
            return Task.FromResult(removed);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var prefix = path.TrimEnd('/') + "/";
            foreach (var filePath in EnumerateKnownFilePaths().Where(p => p.Equals(path, StringComparison.OrdinalIgnoreCase) || p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
            {
                _client.Remove(FileKeyPrefix + filePath);
                UntrackFilePath(filePath);
            }

            var directories = GetDirectorySet();
            foreach (var directory in directories.Where(p => p.Equals(path, StringComparison.OrdinalIgnoreCase) || p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                directories.Remove(directory);
            SaveDirectorySet(directories);

            return Task.FromResult(true);
        }

        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var oldKey = FileKeyPrefix + oldPath;
            var newKey = FileKeyPrefix + newPath;
            var content = _client.Get<byte[]>(oldKey);
            if (content == null)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (_client.Get<byte[]>(newKey) != null)
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            EnsureParentDirectories(newPath);
            _client.Store(StoreMode.Set, newKey, content);
            _client.Remove(oldKey);
            UntrackFilePath(oldPath);
            TrackFilePath(newPath);
            return Task.CompletedTask;
        }

        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var srcKey = FileKeyPrefix + srcPath;
            var dstKey = FileKeyPrefix + dstPath;
            var content = _client.Get<byte[]>(srcKey);
            if (content == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (_client.Get<byte[]>(dstKey) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            EnsureParentDirectories(dstPath);
            _client.Store(StoreMode.Set, dstKey, content);
            TrackFilePath(dstPath);
            return Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var content = _client.Get<byte[]>(FileKeyPrefix + path);
            if (content == null)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return Task.FromResult<Stream>(new MemoryStream(content, writable: false));
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var key = FileKeyPrefix + path;
            if (!overwrite && _client.Get<byte[]>(key) != null)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (GetDirectorySet().Contains(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            EnsureParentDirectories(path);
            _client.Store(StoreMode.Set, key, memory.ToArray());
            TrackFilePath(path);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (await GetDirectoryInfoAsync(oldPath, cancellationToken) == null)
                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");
            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

            await this.CopyDirectoryAsync(oldPath, newPath, cancellationToken);
            await TryDeleteDirectoryAsync(oldPath, cancellationToken);
        }

        private static IMemcachedClient CreateClient(string host, int port)
        {
            var loggerFactory = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;
            var configuration = new MemcachedClientConfiguration(loggerFactory, new MemcachedClientOptions());
            configuration.AddServer(host, port);
            return new MemcachedClient(loggerFactory, configuration);
        }

        private HashSet<string> GetDirectorySet()
        {
            var stored = _client.Get<string>(DirectorySetKey);
            if (string.IsNullOrEmpty(stored))
                return new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            return new HashSet<string>(stored.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        }

        private void SaveDirectorySet(HashSet<string> directories)
        {
            _client.Store(StoreMode.Set, DirectorySetKey, string.Join("\n", directories));
        }

        private IEnumerable<string> EnumerateKnownFilePaths()
        {
            var fileIndex = _client.Get<string>(FileKeyPrefix + "__index__");
            if (string.IsNullOrEmpty(fileIndex))
                return Enumerable.Empty<string>();

            return fileIndex.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private void TrackFilePath(string path)
        {
            var paths = new HashSet<string>(EnumerateKnownFilePaths(), StringComparer.OrdinalIgnoreCase) { path };
            _client.Store(StoreMode.Set, FileKeyPrefix + "__index__", string.Join("\n", paths));
        }

        private void UntrackFilePath(string path)
        {
            var paths = new HashSet<string>(EnumerateKnownFilePaths(), StringComparer.OrdinalIgnoreCase);
            if (!paths.Remove(path))
                return;

            _client.Store(StoreMode.Set, FileKeyPrefix + "__index__", string.Join("\n", paths));
        }

        private void EnsureParentDirectories(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1)
                return;

            var directories = GetDirectorySet();
            var current = string.Empty;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];
                directories.Add(current);
            }
            SaveDirectorySet(directories);
        }

        private bool HasChildren(string path)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            if (GetDirectorySet().Any(directory => IsDirectChild(path, prefix, directory, out _)))
                return true;

            return EnumerateKnownFilePaths().Any(filePath => IsDirectChild(path, prefix, filePath, out _));
        }

        private static bool IsDirectChild(string path, string prefix, string candidate, out string childName)
        {
            childName = null;
            if (string.IsNullOrEmpty(candidate))
                return false;
            if (!string.IsNullOrEmpty(path) && candidate.Equals(path, StringComparison.OrdinalIgnoreCase))
                return false;

            var relative = string.IsNullOrEmpty(prefix)
                ? candidate
                : candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? candidate.Substring(prefix.Length) : null;
            if (string.IsNullOrEmpty(relative))
                return false;

            var slash = relative.IndexOf('/');
            if (slash >= 0)
                relative = relative.Substring(0, slash);
            if (string.IsNullOrEmpty(relative))
                return false;

            childName = relative;
            return true;
        }

        private static MemcachedStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new MemcachedStoreEntry(path ?? string.Empty, isDirectory, length, modified);
    }
}
