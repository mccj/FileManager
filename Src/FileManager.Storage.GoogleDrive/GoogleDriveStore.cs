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

namespace FileManager.Storage.GoogleDrive
{
    public sealed class GoogleDriveStore : IFileStore
    {
        private const string DriveApiBase = "https://www.googleapis.com/drive/v3";
        private const string UploadApiBase = "https://www.googleapis.com/upload/drive/v3";

        private readonly HttpClient _httpClient;
        private readonly string _accessToken;

        public GoogleDriveStore(string accessToken, HttpClient httpClient = null)
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
            var item = await TryResolveItemAsync(path, cancellationToken);
            if (item == null || item.IsFolder)
                return null;

            return CreateEntry(path, false, item.Size, item.Modified);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (string.IsNullOrEmpty(path))
                return CreateEntry(path, true, 0, DateTimeOffset.UtcNow);

            var item = await TryResolveItemAsync(path, cancellationToken);
            if (item != null && item.IsFolder)
                return CreateEntry(path, true, 0, item.Modified);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var parentId = await ResolveFolderIdAsync(path, cancellationToken);
            if (parentId == null)
                return Enumerable.Empty<IFileStoreEntry>();

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in await ListChildrenAsync(parentId, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var childPath = this.Combine(path, item.Name);
                entries[item.Name] = CreateEntry(childPath, item.IsFolder, item.IsFolder ? 0 : item.Size, item.Modified);
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
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var folderName = System.IO.Path.GetFileName(path);

            using var request = CreateRequest(HttpMethod.Post, $"{DriveApiBase}/files");
            request.Content = new StringContent(JsonSerializer.Serialize(new
            {
                name = folderName,
                mimeType = "application/vnd.google-apps.folder",
                parents = new[] { parentId }
            }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var item = await TryResolveItemAsync(path, cancellationToken);
            if (item == null || item.IsFolder)
                return false;

            using var request = CreateRequest(HttpMethod.Delete, $"{DriveApiBase}/files/{item.Id}");
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

            var item = await TryResolveItemAsync(path, cancellationToken);
            if (item == null || !item.IsFolder)
                return false;

            using var request = CreateRequest(HttpMethod.Delete, $"{DriveApiBase}/files/{item.Id}");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var item = await TryResolveItemAsync(oldPath, cancellationToken);
            if (item == null || item.IsFolder)
                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            var parentPath = GetParentPath(newPath);
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var name = System.IO.Path.GetFileName(newPath);

            using var request = CreateRequest(new HttpMethod("PATCH"), $"{DriveApiBase}/files/{item.Id}?addParents={parentId}&removeParents={item.ParentId}");
            request.Content = new StringContent(JsonSerializer.Serialize(new { name }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            var item = await TryResolveItemAsync(srcPath, cancellationToken);
            if (item == null || item.IsFolder)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var parentPath = GetParentPath(dstPath);
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var name = System.IO.Path.GetFileName(dstPath);

            using var request = CreateRequest(HttpMethod.Post, $"{DriveApiBase}/files/{item.Id}/copy");
            request.Content = new StringContent(JsonSerializer.Serialize(new { name, parents = new[] { parentId } }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var item = await TryResolveItemAsync(path, cancellationToken);
            if (item == null || item.IsFolder)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            using var request = CreateRequest(HttpMethod.Get, $"{DriveApiBase}/files/{item.Id}?alt=media");
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
            var parentPath = GetParentPath(path);
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var fileName = System.IO.Path.GetFileName(path);

            if (overwrite)
            {
                var existing = await TryResolveItemAsync(path, cancellationToken);
                if (existing != null)
                {
                    using var updateRequest = CreateRequest(new HttpMethod("PATCH"), $"{UploadApiBase}/files/{existing.Id}?uploadType=media");
                    updateRequest.Content = new ByteArrayContent(memory.ToArray());
                    updateRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    using var updateResponse = await _httpClient.SendAsync(updateRequest, cancellationToken);
                    await EnsureSuccessAsync(updateResponse, cancellationToken);
                    return;
                }
            }

            using var request = CreateRequest(HttpMethod.Post, $"{UploadApiBase}/files?uploadType=multipart");
            var metadata = JsonSerializer.Serialize(new { name = fileName, parents = new[] { parentId } });
            var boundary = "fm_boundary_" + Guid.NewGuid().ToString("N");
            var body = BuildMultipartBody(boundary, metadata, memory.ToArray());
            request.Content = new ByteArrayContent(body);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/related");
            request.Content.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            var item = await TryResolveItemAsync(oldPath, cancellationToken);
            if (item == null || !item.IsFolder)
                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");
            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

            var parentPath = GetParentPath(newPath);
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var name = System.IO.Path.GetFileName(newPath);

            using var request = CreateRequest(new HttpMethod("PATCH"), $"{DriveApiBase}/files/{item.Id}?addParents={parentId}&removeParents={item.ParentId}");
            request.Content = new StringContent(JsonSerializer.Serialize(new { name }), Encoding.UTF8, "application/json");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);
        }

        private async Task<DriveItem> TryResolveItemAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path))
                return new DriveItem { Id = "root", Name = string.Empty, IsFolder = true, ParentId = null, Size = 0, Modified = DateTimeOffset.UtcNow };

            var parentPath = GetParentPath(path);
            var parentId = await ResolveFolderIdAsync(parentPath, cancellationToken) ?? "root";
            var name = System.IO.Path.GetFileName(path);
            var query = $"name = '{EscapeQuery(name)}' and '{parentId}' in parents and trashed = false";
            using var request = CreateRequest(HttpMethod.Get, $"{DriveApiBase}/files?q={Uri.EscapeDataString(query)}&fields=files(id,name,mimeType,size,modifiedTime,parents)");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            foreach (var file in document.RootElement.GetProperty("files").EnumerateArray())
                return ParseItem(file);

            return null;
        }

        private async Task<string> ResolveFolderIdAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path))
                return "root";

            var item = await TryResolveItemAsync(path, cancellationToken);
            return item != null && item.IsFolder ? item.Id : null;
        }

        private async Task<IReadOnlyList<DriveItem>> ListChildrenAsync(string parentId, CancellationToken cancellationToken)
        {
            var query = $"'{parentId}' in parents and trashed = false";
            using var request = CreateRequest(HttpMethod.Get, $"{DriveApiBase}/files?q={Uri.EscapeDataString(query)}&fields=files(id,name,mimeType,size,modifiedTime,parents)");
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessAsync(response, cancellationToken);

            var results = new List<DriveItem>();
            using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            foreach (var file in document.RootElement.GetProperty("files").EnumerateArray())
                results.Add(ParseItem(file));

            return results;
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            return request;
        }

        private static DriveItem ParseItem(JsonElement element)
        {
            var mimeType = element.TryGetProperty("mimeType", out var mime) ? mime.GetString() : null;
            return new DriveItem
            {
                Id = element.GetProperty("id").GetString(),
                Name = element.GetProperty("name").GetString(),
                IsFolder = mimeType == "application/vnd.google-apps.folder",
                Size = element.TryGetProperty("size", out var size) ? size.GetInt64() : 0,
                Modified = element.TryGetProperty("modifiedTime", out var modified)
                    ? DateTimeOffset.Parse(modified.GetString())
                    : DateTimeOffset.UtcNow,
                ParentId = element.TryGetProperty("parents", out var parents) && parents.GetArrayLength() > 0
                    ? parents[0].GetString()
                    : null
            };
        }

        private static byte[] BuildMultipartBody(string boundary, string metadataJson, byte[] content)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8, 1024, leaveOpen: true);
            writer.Write("--" + boundary + "\r\n");
            writer.Write("Content-Type: application/json; charset=UTF-8\r\n\r\n");
            writer.Write(metadataJson + "\r\n");
            writer.Write("--" + boundary + "\r\n");
            writer.Write("Content-Type: application/octet-stream\r\n\r\n");
            writer.Flush();
            stream.Write(content, 0, content.Length);
            writer.Write("\r\n--" + boundary + "--\r\n");
            writer.Flush();
            return stream.ToArray();
        }

        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw new FileStoreException($"Google Drive API request failed ({(int)response.StatusCode}): {body}");
            }
        }

        private static string EscapeQuery(string value) => value.Replace("'", "\\'");

        private static string GetParentPath(string path)
        {
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }

        private static GoogleDriveStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)
            => new GoogleDriveStoreEntry(path ?? string.Empty, isDirectory, length, modified);

        private sealed class DriveItem
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool IsFolder { get; set; }
            public long Size { get; set; }
            public DateTimeOffset Modified { get; set; }
            public string ParentId { get; set; }
        }
    }
}
