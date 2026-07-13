using FileManager.FileStorage;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Redis
{
    public sealed class RedisStore : IFileStore
    {
        private const string DirectorySetKey = "fm:dirs";
        private const string FileKeyPrefix = "fm:file:";

        private readonly IConnectionMultiplexer _connection;
        private readonly IDatabase _database;

        public RedisStore(IConnectionMultiplexer connection, int database = -1)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _database = _connection.GetDatabase(database);
            _database.SetAdd(DirectorySetKey, string.Empty);
        }

        public RedisStore(string connectionString, int database = -1)
            : this(ConnectionMultiplexer.Connect(connectionString), database)
        {
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var key = FileKeyPrefix + path;
            if (!_database.KeyExists(key))
                return Task.FromResult<IFileStoreEntry>(null);

            var length = (long)_database.StringLength(key);
            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, length, DateTimeOffset.UtcNow));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (_database.SetContains(DirectorySetKey, path) || HasChildren(path))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (!_database.SetContains(DirectorySetKey, path) && !HasChildren(path))
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var server = GetServer();

            foreach (var directory in _database.SetMembers(DirectorySetKey))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var directoryPath = directory.ToString();
                if (!IsDirectChild(path, prefix, directoryPath, out var childName))
                    continue;
                if (!entries.ContainsKey(childName))
                    entries[childName] = CreateEntry(this.Combine(path, childName), true, 0, DateTimeOffset.UtcNow);
            }

            foreach (var key in server.Keys(pattern: FileKeyPrefix + (string.IsNullOrEmpty(prefix) ? "*" : prefix + "*"), pageSize: 500))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var filePath = key.ToString().Substring(FileKeyPrefix.Length);
                if (!IsDirectChild(path, prefix, filePath, out var childName))
                    continue;
                if (entries.ContainsKey(childName))
                    continue;

                var length = (long)_database.StringLength(key);
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
            if (_database.KeyExists(FileKeyPrefix + path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            EnsureParentDirectories(path);
            return Task.FromResult(_database.SetAdd(DirectorySetKey, path));
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            return Task.FromResult(_database.KeyDelete(FileKeyPrefix + path));
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var prefix = path.TrimEnd('/') + "/";
            var server = GetServer();
            foreach (var key in server.Keys(pattern: FileKeyPrefix + prefix + "*", pageSize: 500))
                _database.KeyDelete(key);

            foreach (var directory in _database.SetMembers(DirectorySetKey).Select(v => v.ToString()).Where(p => p.Equals(path, StringComparison.OrdinalIgnoreCase) || p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                _database.SetRemove(DirectorySetKey, directory);

            return Task.FromResult(true);
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var oldKey = FileKeyPrefix + oldPath;
            var newKey = FileKeyPrefix + newPath;
            if (!_database.KeyExists(oldKey))
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (_database.KeyExists(newKey))
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            EnsureParentDirectories(newPath);
            var value = await _database.StringGetAsync(oldKey);
            await _database.StringSetAsync(newKey, value);
            await _database.KeyDeleteAsync(oldKey);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var srcKey = FileKeyPrefix + srcPath;
            var dstKey = FileKeyPrefix + dstPath;
            if (!_database.KeyExists(srcKey))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (_database.KeyExists(dstKey))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            EnsureParentDirectories(dstPath);
            var value = await _database.StringGetAsync(srcKey);
            await _database.StringSetAsync(dstKey, value);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var value = await _database.StringGetAsync(FileKeyPrefix + path);
            if (value.IsNullOrEmpty)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return new MemoryStream((byte[])value, writable: false);
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
            if (!overwrite && _database.KeyExists(key))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (_database.SetContains(DirectorySetKey, path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            EnsureParentDirectories(path);
            await _database.StringSetAsync(key, memory.ToArray());
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

        private IServer GetServer()
        {
            var endpoint = _connection.GetEndPoints().First();
            return _connection.GetServer(endpoint);
        }

        private void EnsureParentDirectories(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1)
                return;

            var current = string.Empty;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];
                _database.SetAdd(DirectorySetKey, current);
            }
        }

        private bool HasChildren(string path)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            if (_database.SetMembers(DirectorySetKey).Any(v => IsDirectChild(path, prefix, v.ToString(), out _)))
                return true;

            var server = GetServer();
            return server.Keys(pattern: FileKeyPrefix + (string.IsNullOrEmpty(prefix) ? "*" : prefix + "*"), pageSize: 1).Any();
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

        private static RedisStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new RedisStoreEntry(path ?? string.Empty, isDirectory, length, modified);
    }
}
