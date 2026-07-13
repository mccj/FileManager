using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Dropbox
{
    public sealed class DropboxStore : IFileStore
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public DropboxStore(string accessToken, HttpClient httpClient = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));
            _accessToken = accessToken;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            try
            {
                var metadata = await GetMetadataAsync(path, cancellationToken);
                if (metadata == null || metadata.IsFolder)
                    return null;

                return CreateEntry(path, false, metadata.Size, metadata.ServerModified);
            }
            catch (FileStoreException)
            {
                return null;
            }
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                return CreateEntry(path, true, 0, DateTimeOffset.UtcNow);

            try
            {
                var metadata = await GetMetadataAsync(path, cancellationToken);
                if (metadata != null && metadata.IsFolder)
                    return CreateEntry(path, true, 0, metadata.ServerModified);

                return null;
            }
            catch (FileStoreException)
            {
                return null;
            }
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in await ListFolderAsync(path, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childPath = this.Combine(path, item.Name);
                entries[item.Name] = CreateEntry(childPath, item.IsFolder, item.IsFolder ? 0 : item.Size, item.ServerModified);
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
            if (await GetDirectoryInfoAsync(path, cancellationToken) != null)
                return false;

            await PostApiAsync("https://api.dropboxapi.com/2/files/create_folder_v2",
                JsonSerializer.Serialize(new { path = ToDropboxPath(path), autorename = false }), cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (await GetFileInfoAsync(path, cancellationToken) == null)
                return false;

            await PostApiAsync("https://api.dropboxapi.com/2/files/delete_v2",
                JsonSerializer.Serialize(new { path = ToDropboxPath(path) }), cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                throw new FileStoreException("Cannot delete the root directory.");
            if (await GetDirectoryInfoAsync(path, cancellationToken) == null)
                return false;

            await PostApiAsync("https://api.dropboxapi.com/2/files/delete_v2",
                JsonSerializer.Serialize(new { path = ToDropboxPath(path) }), cancellationToken);
            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (await GetFileInfoAsync(oldPath, cancellationToken) == null)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (await GetFileInfoAsync(newPath, cancellationToken) != null || await GetDirectoryInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            await PostApiAsync("https://api.dropboxapi.com/2/files/move_v2",
                JsonSerializer.Serialize(new { from_path = ToDropboxPath(oldPath), to_path = ToDropboxPath(newPath), autorename = false }), cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            if (await GetFileInfoAsync(srcPath, cancellationToken) == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (await GetFileInfoAsync(dstPath, cancellationToken) != null || await GetDirectoryInfoAsync(dstPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await PostApiAsync("https://api.dropboxapi.com/2/files/copy_v2",
                JsonSerializer.Serialize(new { from_path = ToDropboxPath(srcPath), to_path = ToDropboxPath(dstPath), autorename = false }), cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://content.dropboxapi.com/2/files/download");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(new { path = ToDropboxPath(path) }));
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (!response.IsSuccessStatusCode)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var bytes = await response.Content.ReadAsByteArrayAsync();
            return new MemoryStream(bytes, writable: false);
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
            if (await GetDirectoryInfoAsync(path, cancellationToken) != null)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            using var memory = new MemoryStream();
            await inputStream.CopyToAsync(memory, 81920, cancellationToken);
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://content.dropboxapi.com/2/files/upload");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Add("Dropbox-API-Arg", JsonSerializer.Serialize(new
            {
                path = ToDropboxPath(path),
                mode = overwrite ? "overwrite" : "add",
                autorename = false,
                mute = false
            }));
            request.Content = new ByteArrayContent(memory.ToArray());
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
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

            await PostApiAsync("https://api.dropboxapi.com/2/files/move_v2",
                JsonSerializer.Serialize(new { from_path = ToDropboxPath(oldPath), to_path = ToDropboxPath(newPath), autorename = false }), cancellationToken);
        }

        private async Task<DropboxMetadata> GetMetadataAsync(string path, CancellationToken cancellationToken)
        {
            var json = await PostApiAsync("https://api.dropboxapi.com/2/files/get_metadata",
                JsonSerializer.Serialize(new { path = ToDropboxPath(path), include_deleted = false }), cancellationToken);
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            return new DropboxMetadata
            {
                Name = root.GetProperty("name").GetString(),
                IsFolder = root.GetProperty(".tag").GetString() == "folder",
                Size = root.TryGetProperty("size", out var size) ? size.GetInt64() : 0,
                ServerModified = root.TryGetProperty("server_modified", out var modified)
                    ? DateTimeOffset.Parse(modified.GetString())
                    : DateTimeOffset.UtcNow
            };
        }

        private async Task<IReadOnlyList<DropboxMetadata>> ListFolderAsync(string path, CancellationToken cancellationToken)
        {
            var results = new List<DropboxMetadata>();
            var hasMore = true;
            string cursor = null;

            while (hasMore)
            {
                string json;
                if (cursor == null)
                {
                    json = await PostApiAsync("https://api.dropboxapi.com/2/files/list_folder",
                        JsonSerializer.Serialize(new { path = ToDropboxPath(path), recursive = false, include_deleted = false }), cancellationToken);
                }
                else
                {
                    json = await PostApiAsync("https://api.dropboxapi.com/2/files/list_folder/continue",
                        JsonSerializer.Serialize(new { cursor }), cancellationToken);
                }

                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;
                foreach (var entry in root.GetProperty("entries").EnumerateArray())
                {
                    results.Add(new DropboxMetadata
                    {
                        Name = entry.GetProperty("name").GetString(),
                        IsFolder = entry.GetProperty(".tag").GetString() == "folder",
                        Size = entry.TryGetProperty("size", out var size) ? size.GetInt64() : 0,
                        ServerModified = entry.TryGetProperty("server_modified", out var modified)
                            ? DateTimeOffset.Parse(modified.GetString())
                            : DateTimeOffset.UtcNow
                    });
                }

                hasMore = root.GetProperty("has_more").GetBoolean();
                cursor = hasMore ? root.GetProperty("cursor").GetString() : null;
            }

            return results;
        }

        private async Task<string> PostApiAsync(string url, string body, CancellationToken cancellationToken)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new FileStoreException($"Dropbox API request failed ({(int)response.StatusCode}): {body}");
            }
        }

        private static string ToDropboxPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            return "/" + path.Replace('\\', '/').Trim('/');
        }

        private static DropboxStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new DropboxStoreEntry(path ?? string.Empty, isDirectory, length, modified);

        private sealed class DropboxMetadata
        {
            public string Name { get; set; }
            public bool IsFolder { get; set; }
            public long Size { get; set; }
            public DateTimeOffset ServerModified { get; set; }
        }
    }
}
