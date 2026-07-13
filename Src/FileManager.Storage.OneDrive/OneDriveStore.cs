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

namespace FileManager.Storage.OneDrive
{
    public sealed class OneDriveStore : IFileStore
    {
        private const string GraphBaseUrl = "https://graph.microsoft.com/v1.0/me/drive";

        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public OneDriveStore(string accessToken, HttpClient httpClient = null)
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
            var item = await TryGetItemAsync(path, cancellationToken);
            if (item == null || item.IsFolder)
                return null;

            return CreateEntry(path, false, item.Size, item.LastModified);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                return CreateEntry(path, true, 0, DateTimeOffset.UtcNow);

            var item = await TryGetItemAsync(path, cancellationToken);
            if (item != null && item.IsFolder)
                return CreateEntry(path, true, 0, item.LastModified);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in await ListChildrenAsync(path, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childPath = this.Combine(path, item.Name);
                entries[item.Name] = CreateEntry(childPath, item.IsFolder, item.IsFolder ? 0 : item.Size, item.LastModified);
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

            var parentPath = GetParentPath(path);
            var folderName = System.IO.Path.GetFileName(path);
            var url = string.IsNullOrEmpty(parentPath)
                ? $"{GraphBaseUrl}/root/children"
                : $"{GraphBaseUrl}/root:/{EscapePath(parentPath)}:/children";

            using var request = CreateRequest(HttpMethod.Post, url);
            request.Content = new StringContent(JsonSerializer.Serialize(new
            {
                name = folderName,
                folder = new { },
                conflictBehavior = "fail"
            }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            if (await GetFileInfoAsync(path, cancellationToken) == null)
                return false;

            using var request = CreateRequest(HttpMethod.Delete, $"{GraphBaseUrl}/root:/{EscapePath(path)}");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
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

            using var request = CreateRequest(HttpMethod.Delete, $"{GraphBaseUrl}/root:/{EscapePath(path)}");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var item = await TryGetItemAsync(oldPath, cancellationToken);
            if (item == null || item.IsFolder)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");
            if (await GetFileInfoAsync(newPath, cancellationToken) != null || await GetDirectoryInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");

            await MoveItemAsync(item.Id, newPath, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var item = await TryGetItemAsync(srcPath, cancellationToken);
            if (item == null || item.IsFolder)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (await GetFileInfoAsync(dstPath, cancellationToken) != null || await GetDirectoryInfoAsync(dstPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            var parentPath = GetParentPath(dstPath);
            var fileName = System.IO.Path.GetFileName(dstPath);
            var parentUrl = string.IsNullOrEmpty(parentPath)
                ? $"{GraphBaseUrl}/root"
                : $"{GraphBaseUrl}/root:/{EscapePath(parentPath)}";

            using var request = CreateRequest(HttpMethod.Post, $"{parentUrl}:/{EscapePath(fileName)}:/copy");
            request.Content = new StringContent(JsonSerializer.Serialize(new { parentReference = new { path = "/drive/root" + (string.IsNullOrEmpty(parentPath) ? string.Empty : "/" + parentPath) } }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            using var request = CreateRequest(HttpMethod.Get, $"{GraphBaseUrl}/root:/{EscapePath(path)}:/content");
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
            var method = overwrite ? HttpMethod.Put : HttpMethod.Put;
            using var request = CreateRequest(method, $"{GraphBaseUrl}/root:/{EscapePath(path)}:/content");
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
            var item = await TryGetItemAsync(oldPath, cancellationToken);
            if (item == null || !item.IsFolder)
                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");
            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

            await MoveItemAsync(item.Id, newPath, cancellationToken);
        }

        private async Task MoveItemAsync(string itemId, string destinationPath, CancellationToken cancellationToken)
        {
            var parentPath = GetParentPath(destinationPath);
            var name = System.IO.Path.GetFileName(destinationPath);
            using var request = CreateRequest(new HttpMethod("PATCH"), $"{GraphBaseUrl}/items/{itemId}");
            request.Content = new StringContent(JsonSerializer.Serialize(new
            {
                parentReference = new { path = "/drive/root" + (string.IsNullOrEmpty(parentPath) ? string.Empty : "/" + parentPath) },
                name
            }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private async Task<GraphItem> TryGetItemAsync(string path, CancellationToken cancellationToken)
        {
            using var request = CreateRequest(HttpMethod.Get, $"{GraphBaseUrl}/root:/{EscapePath(path)}");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            await EnsureSuccessAsync(response, cancellationToken);
            return ParseItem(await response.Content.ReadAsStringAsync());
        }

        private async Task<IReadOnlyList<GraphItem>> ListChildrenAsync(string path, CancellationToken cancellationToken)
        {
            var url = string.IsNullOrEmpty(path)
                ? $"{GraphBaseUrl}/root/children"
                : $"{GraphBaseUrl}/root:/{EscapePath(path)}:/children";

            using var request = CreateRequest(HttpMethod.Get, url);
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var results = new List<GraphItem>();
            foreach (var value in document.RootElement.GetProperty("value").EnumerateArray())
                results.Add(ParseItem(value.GetRawText()));

            return results;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return request;
        }

        private static GraphItem ParseItem(string json)
        {
            using var document = JsonDocument.Parse(json);
            return ParseItem(document.RootElement);
        }

        private static GraphItem ParseItem(JsonElement element)
        {
            return new GraphItem
            {
                Id = element.GetProperty("id").GetString(),
                Name = element.GetProperty("name").GetString(),
                IsFolder = element.TryGetProperty("folder", out _),
                Size = element.TryGetProperty("size", out var size) ? size.GetInt64() : 0,
                LastModified = element.TryGetProperty("lastModifiedDateTime", out var modified)
                    ? DateTimeOffset.Parse(modified.GetString())
                    : DateTimeOffset.UtcNow
            };
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new FileStoreException($"Microsoft Graph request failed ({(int)response.StatusCode}): {body}");
            }
        }

        private static string EscapePath(string path) => path.Replace("'", "''");

        private static string GetParentPath(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        private static OneDriveStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new OneDriveStoreEntry(path ?? string.Empty, isDirectory, length, modified);

        private sealed class GraphItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsFolder { get; set; }
            public long Size { get; set; }
            public DateTimeOffset LastModified { get; set; }
        }
    }
}
