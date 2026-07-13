using FileManager.FileStorage;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.SqlBlob
{
    public sealed class SqlBlobStore : IFileStore
    {
        private readonly string _connectionString;

        public SqlBlobStore(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            _connectionString = connectionString;
            EnsureSchema();
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT length(content), last_modified_utc FROM fm_nodes WHERE path = $path AND is_directory = 0";
            command.Parameters.AddWithValue("$path", path);
            using var reader = command.ExecuteReader();
            if (!reader.Read())
                return Task.FromResult<IFileStoreEntry>(null);

            return Task.FromResult<IFileStoreEntry>(CreateEntry(path, false, reader.GetInt64(0), ParseTimestamp(reader.GetString(1))));
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            using var connection = OpenConnection();
            if (IsDirectory(connection, path) || HasChildren(connection, path))
                return Task.FromResult<IFileStoreEntry>(CreateEntry(path, true, 0, DateTimeOffset.UtcNow));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            using var connection = OpenConnection();
            if (!IsDirectory(connection, path) && !HasChildren(connection, path))
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            using var command = connection.CreateCommand();
            command.CommandText = string.IsNullOrEmpty(path)
                ? "SELECT path, is_directory, CASE WHEN is_directory = 1 THEN 0 ELSE length(content) END AS content_length, last_modified_utc FROM fm_nodes"
                : "SELECT path, is_directory, CASE WHEN is_directory = 1 THEN 0 ELSE length(content) END AS content_length, last_modified_utc FROM fm_nodes WHERE path = $path OR path LIKE $like ESCAPE '\\'";
            if (!string.IsNullOrEmpty(path))
            {
                command.Parameters.AddWithValue("$path", path);
                command.Parameters.AddWithValue("$like", EscapeLike(prefix) + "%");
            }

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                cancellationToken.ThrowIfCancellationRequested();
                var nodePath = reader.GetString(0);
                if (nodePath.Equals(path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDirectChild(path, prefix, nodePath, out var childName))
                    continue;

                if (entries.ContainsKey(childName))
                    continue;

                var isDirectory = reader.GetInt64(1) == 1;
                entries[childName] = CreateEntry(
                    this.Combine(path, childName),
                    isDirectory,
                    isDirectory ? 0 : reader.GetInt64(2),
                    ParseTimestamp(reader.GetString(3)));
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
            using var connection = OpenConnection();
            if (IsFile(connection, path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            EnsureParentDirectories(connection, path);
            return Task.FromResult(UpsertNode(connection, path, true, null));
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM fm_nodes WHERE path = $path AND is_directory = 0";
            command.Parameters.AddWithValue("$path", path);
            return Task.FromResult(command.ExecuteNonQuery() > 0);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM fm_nodes WHERE path = $path OR path LIKE $like ESCAPE '\\'";
            command.Parameters.AddWithValue("$path", path);
            command.Parameters.AddWithValue("$like", EscapeLike(path.TrimEnd('/') + "/") + "%");
            command.ExecuteNonQuery();
            return Task.FromResult(true);
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            using var connection = OpenConnection();
            if (!IsFile(connection, oldPath))
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (IsFile(connection, newPath) || IsDirectory(connection, newPath))
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            EnsureParentDirectories(connection, newPath);
            using var command = connection.CreateCommand();
            command.CommandText = "UPDATE fm_nodes SET path = $newPath WHERE path = $oldPath AND is_directory = 0";
            command.Parameters.AddWithValue("$newPath", newPath);
            command.Parameters.AddWithValue("$oldPath", oldPath);
            if (command.ExecuteNonQuery() == 0)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            await Task.CompletedTask;
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            using var connection = OpenConnection();
            if (!IsFile(connection, srcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (IsFile(connection, dstPath) || IsDirectory(connection, dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            using var select = connection.CreateCommand();
            select.CommandText = "SELECT content, last_modified_utc FROM fm_nodes WHERE path = $path AND is_directory = 0";
            select.Parameters.AddWithValue("$path", srcPath);
            using var reader = select.ExecuteReader();
            if (!reader.Read())
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            EnsureParentDirectories(connection, dstPath);
            UpsertNode(connection, dstPath, false, (byte[])reader[0]);
            await Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT content FROM fm_nodes WHERE path = $path AND is_directory = 0";
            command.Parameters.AddWithValue("$path", path);
            var content = command.ExecuteScalar() as byte[];
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
            using var connection = OpenConnection();
            if (!overwrite && IsFile(connection, path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (IsDirectory(connection, path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            EnsureParentDirectories(connection, path);
            UpsertNode(connection, path, false, memory.ToArray());
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
            using var connection = OpenConnection();
            using var command = connection.CreateCommand();
            command.CommandText = @"
CREATE TABLE IF NOT EXISTS fm_nodes (
  path TEXT PRIMARY KEY,
  is_directory INTEGER NOT NULL,
  content BLOB,
  last_modified_utc TEXT NOT NULL
);";
            command.ExecuteNonQuery();
            UpsertNode(connection, string.Empty, true, null);
        }

        private SqliteConnection OpenConnection()
        {
            var connection = new SqliteConnection(_connectionString);
            connection.Open();
            return connection;
        }

        private static bool UpsertNode(SqliteConnection connection, string path, bool isDirectory, byte[] content)
        {
            using var command = connection.CreateCommand();
            command.CommandText = @"
INSERT INTO fm_nodes(path, is_directory, content, last_modified_utc)
VALUES ($path, $isDirectory, $content, $modified)
ON CONFLICT(path) DO UPDATE SET
  is_directory = excluded.is_directory,
  content = excluded.content,
  last_modified_utc = excluded.last_modified_utc;";
            command.Parameters.AddWithValue("$path", path);
            command.Parameters.AddWithValue("$isDirectory", isDirectory ? 1 : 0);
            command.Parameters.AddWithValue("$content", (object)content ?? DBNull.Value);
            command.Parameters.AddWithValue("$modified", DateTimeOffset.UtcNow.ToString("O"));
            return command.ExecuteNonQuery() > 0;
        }

        private static void EnsureParentDirectories(SqliteConnection connection, string path)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1)
                return;

            var current = string.Empty;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];
                UpsertNode(connection, current, true, null);
            }
        }

        private static bool IsDirectory(SqliteConnection connection, string path)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM fm_nodes WHERE path = $path AND is_directory = 1";
            command.Parameters.AddWithValue("$path", path);
            return command.ExecuteScalar() != null;
        }

        private static bool IsFile(SqliteConnection connection, string path)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1 FROM fm_nodes WHERE path = $path AND is_directory = 0";
            command.Parameters.AddWithValue("$path", path);
            return command.ExecuteScalar() != null;
        }

        private static bool HasChildren(SqliteConnection connection, string path)
        {
            using var command = connection.CreateCommand();
            if (string.IsNullOrEmpty(path))
            {
                command.CommandText = "SELECT 1 FROM fm_nodes WHERE path <> '' LIMIT 1";
                return command.ExecuteScalar() != null;
            }

            command.CommandText = "SELECT 1 FROM fm_nodes WHERE path LIKE $like ESCAPE '\\' AND path <> $path LIMIT 1";
            command.Parameters.AddWithValue("$path", path);
            command.Parameters.AddWithValue("$like", EscapeLike(path.TrimEnd('/') + "/") + "%");
            return command.ExecuteScalar() != null;
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

        private static string EscapeLike(string value) => value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");

        private static DateTimeOffset ParseTimestamp(string value) => DateTimeOffset.Parse(value);

        private static SqlBlobStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new SqlBlobStoreEntry(path ?? string.Empty, isDirectory, length, modified);
    }
}
