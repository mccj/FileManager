using FileManager.FileStorage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.InMemory
{
    public sealed class InMemoryStore : IFileStore
    {
        private sealed class FileData
        {
            public byte[] Bytes;
            public DateTimeOffset LastModifiedUtc;
        }

        private readonly ConcurrentDictionary<string, FileData> _files =
            new ConcurrentDictionary<string, FileData>(StringComparer.OrdinalIgnoreCase);

        private readonly ConcurrentDictionary<string, byte> _directories =
            new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);

        public InMemoryStore()
        {
            _directories[string.Empty] = 0;
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (_files.TryGetValue(path, out var file))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, file.Bytes.LongLength, file.LastModifiedUtc));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (_directories.ContainsKey(path) || HasChildren(path))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (!_directories.ContainsKey(path) && !HasChildren(path))
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            foreach (var directory in _directories.Keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsDirectChild(path, prefix, directory, out var childName))
                    continue;

                if (!entries.ContainsKey(childName))
                    entries[childName] = CreateEntry(this.Combine(path, childName), true, 0, DateTimeOffset.UtcNow);
            }

            foreach (var filePath in _files.Keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsDirectChild(path, prefix, filePath, out var childName))
                    continue;

                if (entries.ContainsKey(childName))
                    continue;

                var file = _files[filePath];
                entries[childName] = CreateEntry(this.Combine(path, childName), false, file.Bytes.LongLength, file.LastModifiedUtc);
            }

            if (includeSubDirectories)
            {
                foreach (var entry in entries.Values.Where(e => e.IsDirectory).ToArray())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    foreach (var nested in GetDirectoryContentAsync(entry.Path, true, cancellationToken).GetAwaiter().GetResult())
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
            if (_files.ContainsKey(path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            return Task.FromResult(_directories.TryAdd(path, 0));
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            return Task.FromResult(_files.TryRemove(path, out _));
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var prefix = path.TrimEnd('/') + "/";
            foreach (var filePath in _files.Keys.Where(k => k.Equals(path, StringComparison.OrdinalIgnoreCase) || k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                _files.TryRemove(filePath, out _);

            foreach (var directory in _directories.Keys.Where(k => k.Equals(path, StringComparison.OrdinalIgnoreCase) || k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToArray())
                _directories.TryRemove(directory, out _);

            return Task.FromResult(true);
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (!_files.TryRemove(oldPath, out var file))
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (_files.ContainsKey(newPath))
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            EnsureParentDirectories(newPath);
            _files[newPath] = file;
            await Task.CompletedTask;
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            if (!_files.TryGetValue(srcPath, out var source))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (_files.ContainsKey(dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            EnsureParentDirectories(dstPath);
            _files[dstPath] = new FileData
            {
                Bytes = source.Bytes.ToArray(),
                LastModifiedUtc = DateTimeOffset.UtcNow
            };
            await Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (!_files.TryGetValue(path, out var file))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return Task.FromResult<Stream>(new MemoryStream(file.Bytes, writable: false));
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (!overwrite && _files.ContainsKey(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            using (var memory = new MemoryStream())
            {
                await inputStream.CopyToAsync(memory, 81920, cancellationToken);
                EnsureParentDirectories(path);
                _files[path] = new FileData
                {
                    Bytes = memory.ToArray(),
                    LastModifiedUtc = DateTimeOffset.UtcNow
                };
            }
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

        private void EnsureParentDirectories(string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1)
                return;

            var current = string.Empty;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];
                _directories.TryAdd(current, 0);
            }
        }

        private bool HasChildren(string path)
        {
            path = path ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            return _directories.Keys.Any(k => IsDescendant(path, prefix, k) && GetChildName(path, prefix, k) != null)
                || _files.Keys.Any(k => IsDescendant(path, prefix, k) && GetChildName(path, prefix, k) != null);
        }

        private static bool IsDescendant(string path, string prefix, string candidate)
        {
            if (string.IsNullOrEmpty(path))
                return !string.IsNullOrEmpty(candidate);
            return candidate.Equals(path, StringComparison.OrdinalIgnoreCase)
                || candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                || _StartsWithFile(prefix, candidate);
        }

        private static bool _StartsWithFile(string prefix, string candidate)
        {
            return candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDirectChild(string path, string prefix, string candidate, out string childName)
        {
            childName = GetChildName(path, prefix, candidate);
            return childName != null;
        }

        private static string GetChildName(string path, string prefix, string candidate)
        {
            if (string.IsNullOrEmpty(candidate))
                return null;

            if (!string.IsNullOrEmpty(path) && candidate.Equals(path, StringComparison.OrdinalIgnoreCase))
                return null;

            if (!string.IsNullOrEmpty(path))
            {
                if (!candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return null;
                candidate = candidate.Substring(prefix.Length);
            }

            if (string.IsNullOrEmpty(candidate))
                return null;

            var slash = candidate.IndexOf('/');
            if (slash >= 0)
                candidate = candidate.Substring(0, slash);

            return string.IsNullOrEmpty(candidate) ? null : candidate;
        }

        private static InMemoryStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset lastModifiedUtc)
        {
            return new InMemoryStoreEntry(path ?? string.Empty, isDirectory, length, lastModifiedUtc);
        }
    }
}
