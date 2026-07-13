using Cassandra;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Cassandra
{
    public sealed class CassandraStore : IFileStore
    {
        private readonly ISession _session;

        public CassandraStore(string contactPoints, string keyspace)
        {
            if (string.IsNullOrWhiteSpace(contactPoints))
                throw new ArgumentException("Contact points are required.", nameof(contactPoints));
            if (string.IsNullOrWhiteSpace(keyspace))
                throw new ArgumentException("Keyspace is required.", nameof(keyspace));

            var cluster = Cluster.Builder()
                .AddContactPoints(contactPoints.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray())
                .Build();
            _session = cluster.Connect(keyspace);
            EnsureSchema();
        }

        public CassandraStore(ISession session)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            EnsureSchema();
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var row = _session.Execute(new SimpleStatement(
                "SELECT content, last_modified_utc FROM fm_nodes WHERE path = ? AND is_directory = false",
                path)).FirstOrDefault();
            if (row == null)
                return Task.FromResult<IFileStoreEntry>(null);

            var content = row.GetValue<byte[]>("content");
            var length = content?.LongLength ?? 0;
            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, length, row.GetValue<DateTimeOffset>("last_modified_utc")));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (IsDirectory(path) || HasChildren(path))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (!IsDirectory(path) && !HasChildren(path))
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            var rows = _session.Execute(new SimpleStatement("SELECT path, is_directory, content, last_modified_utc FROM fm_nodes"));
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var nodePath = row.GetValue<string>("path");
                if (nodePath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDirectChild(path, prefix, nodePath, out var childName))
                    continue;

                if (entries.ContainsKey(childName))
                    continue;

                var isDirectory = row.GetValue<bool>("is_directory");
                var content = row.GetValue<byte[]>("content");
                entries[childName] = CreateEntry(
                    this.Combine(path, childName),
                    isDirectory,
                    isDirectory ? 0 : content?.LongLength ?? 0,
                    row.GetValue<DateTimeOffset>("last_modified_utc"));
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
            if (IsFile(path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            EnsureParentDirectories(path);
            return Task.FromResult(UpsertNode(path, true, null));
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var applied = _session.Execute(new SimpleStatement(
                "DELETE FROM fm_nodes WHERE path = ? IF is_directory = false",
                path)).First().GetValue<bool>("[applied]");
            return Task.FromResult(applied);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var prefix = path.TrimEnd('/') + "/";
            var rows = _session.Execute(new SimpleStatement("SELECT path FROM fm_nodes"));
            foreach (var row in rows)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var nodePath = row.GetValue<string>("path");
                if (nodePath.Equals(path, StringComparison.OrdinalIgnoreCase) || nodePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    _session.Execute(new SimpleStatement("DELETE FROM fm_nodes WHERE path = ?", nodePath));
            }

            return Task.FromResult(true);
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (!IsFile(oldPath))
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (IsFile(newPath) || IsDirectory(newPath))
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            var row = _session.Execute(new SimpleStatement(
                "SELECT content, last_modified_utc FROM fm_nodes WHERE path = ? AND is_directory = false",
                oldPath)).FirstOrDefault();
            if (row == null)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            EnsureParentDirectories(newPath);
            UpsertNode(newPath, false, row.GetValue<byte[]>("content"));
            _session.Execute(new SimpleStatement("DELETE FROM fm_nodes WHERE path = ?", oldPath));
            await Task.CompletedTask;
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            if (!IsFile(srcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (IsFile(dstPath) || IsDirectory(dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            var row = _session.Execute(new SimpleStatement(
                "SELECT content FROM fm_nodes WHERE path = ? AND is_directory = false",
                srcPath)).FirstOrDefault();
            if (row == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            EnsureParentDirectories(dstPath);
            UpsertNode(dstPath, false, row.GetValue<byte[]>("content"));
            await Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var row = _session.Execute(new SimpleStatement(
                "SELECT content FROM fm_nodes WHERE path = ? AND is_directory = false",
                path)).FirstOrDefault();
            var content = row?.GetValue<byte[]>("content");
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
            if (!overwrite && IsFile(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (IsDirectory(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            EnsureParentDirectories(path);
            UpsertNode(path, false, memory.ToArray());
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

        private void EnsureSchema()
        {
            _session.Execute(@"
CREATE TABLE IF NOT EXISTS fm_nodes (
  path text PRIMARY KEY,
  is_directory boolean,
  content blob,
  last_modified_utc timestamp
)");
            UpsertNode(string.Empty, true, null);
        }

        private bool UpsertNode(string path, bool isDirectory, byte[] content)
        {
            _session.Execute(new SimpleStatement(
                "INSERT INTO fm_nodes (path, is_directory, content, last_modified_utc) VALUES (?, ?, ?, ?)",
                path, isDirectory, content, DateTimeOffset.UtcNow));
            return true;
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
                UpsertNode(current, true, null);
            }
        }

        private bool IsDirectory(string path)
        {
            var row = _session.Execute(new SimpleStatement(
                "SELECT is_directory FROM fm_nodes WHERE path = ?",
                path)).FirstOrDefault();
            return row != null && row.GetValue<bool>("is_directory");
        }

        private bool IsFile(string path)
        {
            var row = _session.Execute(new SimpleStatement(
                "SELECT is_directory FROM fm_nodes WHERE path = ?",
                path)).FirstOrDefault();
            return row != null && !row.GetValue<bool>("is_directory");
        }

        private bool HasChildren(string path)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            foreach (var row in _session.Execute(new SimpleStatement("SELECT path FROM fm_nodes")))
            {
                var nodePath = row.GetValue<string>("path");
                if (string.IsNullOrEmpty(path))
                {
                    if (!string.IsNullOrEmpty(nodePath))
                        return true;
                }
                else if (!nodePath.Equals(path, StringComparison.OrdinalIgnoreCase) && nodePath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDirectChild(string path, string prefix, string candidate, out string childName)
        {
            childName = null;
            if (string.IsNullOrEmpty(candidate))
                return false;
            if (!string.IsNullOrEmpty(path) && candidate.Equals(path, StringComparison.OrdinalIgnoreCase))
                return false;

            var relative = string.IsNullOrEmpty(prefix) ? candidate : candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? candidate.Substring(prefix.Length) : null;
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

        private static CassandraStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new CassandraStoreEntry(path ?? string.Empty, isDirectory, length, modified);
    }
}
