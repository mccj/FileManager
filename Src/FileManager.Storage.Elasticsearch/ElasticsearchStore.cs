using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Elasticsearch
{
    public sealed class ElasticsearchStore : IFileStore
    {
        private readonly ElasticsearchClient _client;
        private readonly string _indexName;

        public ElasticsearchStore(Uri endpoint, string indexName = "fm_nodes")
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));

            _indexName = indexName;
            var settings = new ElasticsearchClientSettings(endpoint);
            _client = new ElasticsearchClient(settings);
            EnsureIndex();
        }

        public ElasticsearchStore(ElasticsearchClient client, string indexName = "fm_nodes")
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentException("Index name is required.", nameof(indexName));
            _indexName = indexName;
            EnsureIndex();
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var response = await _client.GetAsync<NodeDocument>(_indexName, path, cancellationToken);
            if (!response.Found || response.Source == null || response.Source.IsDirectory)
                return null;

            return CreateEntry(path, false, response.Source.Content?.LongLength ?? 0, response.Source.LastModifiedUtc);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (await IsDirectoryAsync(path, cancellationToken) || await HasChildrenAsync(path, cancellationToken))
                return CreateEntry(path, true, 0, DateTimeOffset.UtcNow);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (!await IsDirectoryAsync(path, cancellationToken) && !await HasChildrenAsync(path, cancellationToken))
                return Enumerable.Empty<IFileStoreEntry>();

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            SearchResponse<NodeDocument> response;
            if (string.IsNullOrEmpty(prefix))
            {
                response = await _client.SearchAsync<NodeDocument>(s => s
                    .Indices(_indexName)
                    .Size(10000)
                    .Query(q => q.MatchAll(new MatchAllQuery())), cancellationToken);
            }
            else
            {
                response = await _client.SearchAsync<NodeDocument>(s => s
                    .Indices(_indexName)
                    .Size(10000)
                    .Query(q => q.Prefix(p => p.Field(f => f.Path).Value(prefix))), cancellationToken);
            }

            foreach (var hit in response.Documents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (hit.Path.Equals(path, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!IsDirectChild(path, prefix, hit.Path, out var childName))
                    continue;

                if (entries.ContainsKey(childName))
                    continue;

                entries[childName] = CreateEntry(
                    this.Combine(path, childName),
                    hit.IsDirectory,
                    hit.IsDirectory ? 0 : hit.Content?.LongLength ?? 0,
                    hit.LastModifiedUtc);
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
            if (await GetFileInfoAsync(path, cancellationToken) != null)
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");
            if (await IsDirectoryAsync(path, cancellationToken))
                return false;

            await EnsureParentDirectoriesAsync(path, cancellationToken);
            await IndexNodeAsync(path, true, null, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var response = await _client.DeleteAsync<NodeDocument>(new Id(path), d => d.Index(_indexName), cancellationToken);
            return response.Result == Result.Deleted;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");

            var prefix = path.TrimEnd('/') + "/";
            var response = await _client.SearchAsync<NodeDocument>(s => s
                .Indices(_indexName)
                .Size(10000)
                .Query(q => q.Bool(b => b
                    .Should(
                        sh => sh.Term(t => t.Field(f => f.Path).Value(path)),
                        sh => sh.Prefix(p => p.Field(f => f.Path).Value(prefix))))), cancellationToken);

            foreach (var hit in response.Hits)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _client.DeleteAsync<NodeDocument>(hit.Id, d => d.Index(_indexName), cancellationToken);
            }

            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var existing = await _client.GetAsync<NodeDocument>(_indexName, oldPath, cancellationToken);
            if (!existing.Found || existing.Source == null || existing.Source.IsDirectory)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (await GetFileInfoAsync(newPath, cancellationToken) != null || await IsDirectoryAsync(newPath, cancellationToken))
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            await EnsureParentDirectoriesAsync(newPath, cancellationToken);
            await IndexNodeAsync(newPath, false, existing.Source.Content, cancellationToken);
            await _client.DeleteAsync<NodeDocument>(new Id(oldPath), d => d.Index(_indexName), cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var existing = await _client.GetAsync<NodeDocument>(_indexName, srcPath, cancellationToken);
            if (!existing.Found || existing.Source == null || existing.Source.IsDirectory)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (await GetFileInfoAsync(dstPath, cancellationToken) != null || await IsDirectoryAsync(dstPath, cancellationToken))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await EnsureParentDirectoriesAsync(dstPath, cancellationToken);
            await IndexNodeAsync(dstPath, false, existing.Source.Content, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var response = await _client.GetAsync<NodeDocument>(_indexName, path, cancellationToken);
            if (!response.Found || response.Source == null || response.Source.IsDirectory || response.Source.Content == null)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return new MemoryStream(response.Source.Content, writable: false);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (!overwrite && await GetFileInfoAsync(path, cancellationToken) != null)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            if (await IsDirectoryAsync(path, cancellationToken))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            await EnsureParentDirectoriesAsync(path, cancellationToken);
            await IndexNodeAsync(path, false, memory.ToArray(), cancellationToken);
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

        private void EnsureIndex()
        {
            var exists = _client.Indices.ExistsAsync(_indexName).GetAwaiter().GetResult();
            if (!exists.Exists)
            {
                _client.Indices.CreateAsync(_indexName).GetAwaiter().GetResult();
                IndexNodeAsync(string.Empty, true, null, CancellationToken.None).GetAwaiter().GetResult();
            }
        }

        private async Task<bool> IsDirectoryAsync(string path, CancellationToken cancellationToken)
        {
            var response = await _client.GetAsync<NodeDocument>(_indexName, path, cancellationToken);
            return response.Found && response.Source != null && response.Source.IsDirectory;
        }

        private async Task<bool> HasChildrenAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path))
            {
                var count = await _client.CountAsync(c => c.Indices(_indexName), cancellationToken);
                return count.Count > 1;
            }

            var prefix = path.TrimEnd('/') + "/";
            var response = await _client.SearchAsync<NodeDocument>(s => s
                .Indices(_indexName)
                .Size(1)
                .Query(q => q.Prefix(p => p.Field(f => f.Path).Value(prefix))), cancellationToken);
            return response.Documents.Any(d => !d.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
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
                if (!await IsDirectoryAsync(current, cancellationToken))
                    await IndexNodeAsync(current, true, null, cancellationToken);
            }
        }

        private async Task IndexNodeAsync(string path, bool isDirectory, byte[] content, CancellationToken cancellationToken)
        {
            var document = new NodeDocument
            {
                Path = path,
                IsDirectory = isDirectory,
                Content = content,
                LastModifiedUtc = DateTimeOffset.UtcNow
            };
            await _client.IndexAsync(document, i => i.Index(_indexName).Id(path), cancellationToken);
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

        private static ElasticsearchStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new ElasticsearchStoreEntry(path ?? string.Empty, isDirectory, length, modified);

        internal sealed class NodeDocument
        {
            public string Path { get; set; }
            public bool IsDirectory { get; set; }
            public byte[] Content { get; set; }
            public DateTimeOffset LastModifiedUtc { get; set; }
        }
    }
}
