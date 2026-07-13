using FileManager.FileStorage;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.InfluxDb
{
    public sealed class InfluxDbStore : IFileStore
    {
        private const string Measurement = "fm_nodes";
        private const string LengthField = "length";
        private const string ContentField = "content";
        private const string PathTag = "path";
        private const string IsDirectoryTag = "is_directory";

        private readonly InfluxDBClient _client;
        private readonly string _org;
        private readonly string _bucket;

        public InfluxDbStore(string url, string token, string org, string bucket)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("URL is required.", nameof(url));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token is required.", nameof(token));
            if (string.IsNullOrWhiteSpace(org))
                throw new ArgumentException("Organization is required.", nameof(org));
            if (string.IsNullOrWhiteSpace(bucket))
                throw new ArgumentException("Bucket is required.", nameof(bucket));

            _org = org;
            _bucket = bucket;
            _client = new InfluxDBClient(url, token);
            EnsureRootDirectoryAsync().GetAwaiter().GetResult();
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var node = await GetNodeAsync(path, cancellationToken);
            if (node == null || node.IsDirectory)
                return null;

            return CreateEntry(path, false, node.Length, node.LastModifiedUtc);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var node = await GetNodeAsync(path, cancellationToken);
            if (node != null && node.IsDirectory)
                return CreateEntry(path, true, 0, node.LastModifiedUtc);

            if (await HasChildrenAsync(path, cancellationToken))
                return CreateEntry(path, true, 0, DateTimeOffset.UtcNow);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (await GetDirectoryInfoAsync(path, cancellationToken) == null)
                return Enumerable.Empty<IFileStoreEntry>();

            var nodes = await GetAllNodesAsync(cancellationToken);
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            foreach (var node in nodes)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (node.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDirectChild(path, prefix, node.Path, out var childName))
                    continue;

                if (entries.ContainsKey(childName))
                    continue;

                entries[childName] = CreateEntry(
                    this.Combine(path, childName),
                    node.IsDirectory,
                    node.IsDirectory ? 0 : node.Length,
                    node.LastModifiedUtc);
            }

            if (includeSubDirectories)
            {
                foreach (var directory in entries.Values.Where(e => e.IsDirectory).ToArray())
                {
                    foreach (var nested in await GetDirectoryContentAsync(directory.Path, true, cancellationToken))
                    {
                        if (!entries.ContainsKey(nested.Path))
                            entries[nested.Path] = nested;
                    }
                }
            }

            return entries.Values;
        }

        public async Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var existing = await GetNodeAsync(path, cancellationToken);
            if (existing != null && !existing.IsDirectory)
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            await EnsureParentDirectoriesAsync(path, cancellationToken);
            if (existing != null && existing.IsDirectory)
                return false;

            await WriteNodeAsync(path, true, null, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var existing = await GetNodeAsync(path, cancellationToken);
            if (existing == null || existing.IsDirectory)
                return false;

            await DeleteNodeAsync(path, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var nodes = await GetAllNodesAsync(cancellationToken);
            var prefix = path.TrimEnd('/') + "/";
            foreach (var node in nodes.Where(n => n.Path.Equals(path, StringComparison.OrdinalIgnoreCase) || n.Path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                await DeleteNodeAsync(node.Path, cancellationToken);

            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var existing = await GetNodeAsync(oldPath, cancellationToken);
            if (existing == null || existing.IsDirectory)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            if (await GetNodeAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            var content = await GetFileContentAsync(oldPath, cancellationToken);
            await EnsureParentDirectoriesAsync(newPath, cancellationToken);
            await WriteNodeAsync(newPath, false, content, cancellationToken);
            await DeleteNodeAsync(oldPath, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var existing = await GetNodeAsync(srcPath, cancellationToken);
            if (existing == null || existing.IsDirectory)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (await GetNodeAsync(dstPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            var content = await GetFileContentAsync(srcPath, cancellationToken);
            await EnsureParentDirectoriesAsync(dstPath, cancellationToken);
            await WriteNodeAsync(dstPath, false, content, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var content = await GetFileContentAsync(path, cancellationToken);
            if (content == null)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return new MemoryStream(content, writable: false);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var existing = await GetNodeAsync(path, cancellationToken);
            if (!overwrite && existing != null && !existing.IsDirectory)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (existing != null && existing.IsDirectory)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            await EnsureParentDirectoriesAsync(path, cancellationToken);
            await WriteNodeAsync(path, false, memory.ToArray(), cancellationToken);
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

        private async Task EnsureRootDirectoryAsync()
        {
            var existing = await GetNodeAsync(string.Empty, CancellationToken.None);
            if (existing == null)
                await WriteNodeAsync(string.Empty, true, null, CancellationToken.None);
        }

        private async Task EnsureParentDirectoriesAsync(string path, CancellationToken cancellationToken)
        {
            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length <= 1)
                return;

            var current = string.Empty;
            for (var i = 0; i < segments.Length - 1; i++)
            {
                current = string.IsNullOrEmpty(current) ? segments[i] : current + "/" + segments[i];
                var existing = await GetNodeAsync(current, cancellationToken);
                if (existing == null)
                    await WriteNodeAsync(current, true, null, cancellationToken);
            }
        }

        private async Task WriteNodeAsync(string path, bool isDirectory, byte[] content, CancellationToken cancellationToken)
        {
            await DeleteNodeAsync(path, cancellationToken);
            var point = PointData.Measurement(Measurement)
                .Tag(PathTag, path)
                .Tag(IsDirectoryTag, isDirectory ? "true" : "false")
                .Field(LengthField, isDirectory ? 0L : (content?.LongLength ?? 0L))
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            if (!isDirectory && content != null)
                point = point.Field(ContentField, Convert.ToBase64String(content));

            await _client.GetWriteApiAsync().WritePointAsync(point, _bucket, _org, cancellationToken);
        }

        private async Task DeleteNodeAsync(string path, CancellationToken cancellationToken)
        {
            var predicate = $"_measurement=\"{Measurement}\" AND {PathTag}=\"{EscapePredicateValue(path)}\"";
            await _client.GetDeleteApi().Delete(DateTime.MinValue, DateTime.UtcNow.AddMinutes(1), predicate, _bucket, _org, cancellationToken);
        }

        private async Task<NodeRecord> GetNodeAsync(string path, CancellationToken cancellationToken)
        {
            var flux = BuildNodeQuery($"r[\"{PathTag}\"] == \"{EscapeFluxValue(path)}\"");
            var tables = await _client.GetQueryApi().QueryAsync(flux, _org, cancellationToken);
            return ParseNodes(tables).FirstOrDefault(n => n.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<byte[]> GetFileContentAsync(string path, CancellationToken cancellationToken)
        {
            var flux = BuildNodeQuery($"r[\"{PathTag}\"] == \"{EscapeFluxValue(path)}\" and r[\"_field\"] == \"{ContentField}\"");
            var tables = await _client.GetQueryApi().QueryAsync(flux, _org, cancellationToken);
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var value = record.GetValue()?.ToString();
                    if (!string.IsNullOrEmpty(value))
                        return Convert.FromBase64String(value);
                }
            }

            return null;
        }

        private async Task<IReadOnlyList<NodeRecord>> GetAllNodesAsync(CancellationToken cancellationToken)
        {
            var flux = BuildNodeQuery($"r[\"_field\"] == \"{LengthField}\"");
            var tables = await _client.GetQueryApi().QueryAsync(flux, _org, cancellationToken);
            return ParseNodes(tables);
        }

        private async Task<bool> HasChildrenAsync(string path, CancellationToken cancellationToken)
        {
            var nodes = await GetAllNodesAsync(cancellationToken);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            return nodes.Any(node => IsDirectChild(path, prefix, node.Path, out _));
        }

        private string BuildNodeQuery(string filterExpression)
        {
            return
                $"from(bucket: \"{EscapeFluxValue(_bucket)}\") " +
                $"|> range(start: 0) " +
                $"|> filter(fn: (r) => r[\"_measurement\"] == \"{Measurement}\" and {filterExpression}) " +
                "|> group(columns: [\"path\", \"is_directory\"]) " +
                "|> last()";
        }

        private static IReadOnlyList<NodeRecord> ParseNodes(IEnumerable<InfluxDB.Client.Core.Flux.Domain.FluxTable> tables)
        {
            var nodes = new Dictionary<string, NodeRecord>(StringComparer.OrdinalIgnoreCase);
            foreach (var table in tables)
            {
                foreach (var record in table.Records)
                {
                    var path = record.GetValueByKey(PathTag)?.ToString();
                    if (string.IsNullOrEmpty(path))
                        continue;

                    var isDirectory = string.Equals(record.GetValueByKey(IsDirectoryTag)?.ToString(), "true", StringComparison.OrdinalIgnoreCase);
                    var length = 0L;
                    if (record.GetValue() != null && long.TryParse(record.GetValue().ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedLength))
                        length = parsedLength;

                    var modified = record.GetTime().HasValue
                        ? record.GetTime().Value.ToDateTimeOffset()
                        : DateTimeOffset.UtcNow;
                    nodes[path] = new NodeRecord(path, isDirectory, length, modified);
                }
            }

            return nodes.Values.ToList();
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

        private static string EscapeFluxValue(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static string EscapePredicateValue(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static InfluxDbStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new InfluxDbStoreEntry(path ?? string.Empty, isDirectory, length, modified);

        private sealed class NodeRecord
        {
            public NodeRecord(string path, bool isDirectory, long length, DateTimeOffset lastModifiedUtc)
            {
                Path = path;
                IsDirectory = isDirectory;
                Length = length;
                LastModifiedUtc = lastModifiedUtc;
            }

            public string Path { get; }
            public bool IsDirectory { get; }
            public long Length { get; }
            public DateTimeOffset LastModifiedUtc { get; }
        }
    }
}
