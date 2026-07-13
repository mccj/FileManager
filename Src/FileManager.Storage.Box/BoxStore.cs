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



namespace FileManager.Storage.Box

{

    public sealed class BoxStore : IFileStore

    {

        private const string ApiBaseUrl = "https://api.box.com/2.0";

        private const string UploadBaseUrl = "https://upload.box.com/api/2.0";



        private readonly HttpClient _httpClient;

        private readonly string _accessToken;



        public BoxStore(string accessToken, HttpClient httpClient = null)

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

                var resolved = await ResolvePathAsync(path, cancellationToken);

                if (resolved == null || resolved.IsFolder)

                    return null;



                return CreateEntry(path, false, resolved.Size, resolved.ModifiedAt);

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

                var resolved = await ResolvePathAsync(path, cancellationToken);

                if (resolved != null && resolved.IsFolder)

                    return CreateEntry(path, true, 0, resolved.ModifiedAt);



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

            var folderId = await GetFolderIdAsync(path, cancellationToken);

            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);



            foreach (var item in await ListFolderItemsAsync(folderId, cancellationToken))

            {

                cancellationToken.ThrowIfCancellationRequested();

                var childPath = this.Combine(path, item.Name);

                entries[item.Name] = CreateEntry(childPath, item.IsFolder, item.IsFolder ? 0 : item.Size, item.ModifiedAt);

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



            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var parentPath = segments.Length > 1

                ? string.Join("/", segments.Take(segments.Length - 1))

                : string.Empty;

            var parentId = await GetFolderIdAsync(parentPath, cancellationToken);

            var name = segments[segments.Length - 1];



            await PostJsonAsync($"{ApiBaseUrl}/folders",

                JsonSerializer.Serialize(new

                {

                    name,

                    parent = new { id = parentId }

                }), cancellationToken);

            return true;

        }



        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            var resolved = await ResolvePathAsync(path, cancellationToken);

            if (resolved == null || resolved.IsFolder)

                return false;



            await DeleteAsync($"{ApiBaseUrl}/files/{resolved.Id}", cancellationToken);

            return true;

        }



        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path) ?? string.Empty;

            if (string.IsNullOrEmpty(path))

                throw new FileStoreException("Cannot delete the root directory.");



            var resolved = await ResolvePathAsync(path, cancellationToken);

            if (resolved == null || !resolved.IsFolder)

                return false;



            await DeleteAsync($"{ApiBaseUrl}/folders/{resolved.Id}?recursive=true", cancellationToken);

            return true;

        }



        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            oldPath = this.NormalizePath(oldPath);

            newPath = this.NormalizePath(newPath);

            var source = await ResolvePathAsync(oldPath, cancellationToken);

            if (source == null || source.IsFolder)

                throw new FileStoreException($"The file '{oldPath}' does not exist.");

            if (await GetFileInfoAsync(newPath, cancellationToken) != null || await GetDirectoryInfoAsync(newPath, cancellationToken) != null)

                throw new FileStoreException($"Cannot move file because the destination path '{newPath}' already exists.");



            var (parentId, name) = await GetParentAndNameAsync(newPath, cancellationToken);

            await PutJsonAsync($"{ApiBaseUrl}/files/{source.Id}",

                JsonSerializer.Serialize(new { name, parent = new { id = parentId } }), cancellationToken);

        }



        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            srcPath = this.NormalizePath(srcPath);

            dstPath = this.NormalizePath(dstPath);

            var source = await ResolvePathAsync(srcPath, cancellationToken);

            if (source == null || source.IsFolder)

                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (await GetFileInfoAsync(dstPath, cancellationToken) != null || await GetDirectoryInfoAsync(dstPath, cancellationToken) != null)

                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");



            var (parentId, name) = await GetParentAndNameAsync(dstPath, cancellationToken);

            await PostJsonAsync($"{ApiBaseUrl}/files/{source.Id}/copy",

                JsonSerializer.Serialize(new { name, parent = new { id = parentId } }), cancellationToken);

        }



        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            path = this.NormalizePath(path);

            var resolved = await ResolvePathAsync(path, cancellationToken);

            if (resolved == null || resolved.IsFolder)

                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");



            using var request = CreateRequest(HttpMethod.Get, $"{ApiBaseUrl}/files/{resolved.Id}/content");

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

            var existing = await ResolvePathAsync(path, cancellationToken);

            if (!overwrite && existing != null && !existing.IsFolder)

                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (existing != null && existing.IsFolder)

                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");



            using var memory = new MemoryStream();

            await inputStream.CopyToAsync(memory, 81920, cancellationToken);

            var content = memory.ToArray();



            if (existing != null && overwrite)

            {

                await UploadFileContentAsync(existing.Id, content, cancellationToken);

                return;

            }



            var (parentId, name) = await GetParentAndNameAsync(path, cancellationToken);

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(JsonSerializer.Serialize(new { name, parent = new { id = parentId } }), Encoding.UTF8, "application/json"), "attributes");

            form.Add(new ByteArrayContent(content), "file", name);



            using var request = new HttpRequestMessage(HttpMethod.Post, $"{UploadBaseUrl}/files/content");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            request.Content = form;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

        }



        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)

        {

            cancellationToken.ThrowIfCancellationRequested();

            oldPath = this.NormalizePath(oldPath);

            newPath = this.NormalizePath(newPath);

            var source = await ResolvePathAsync(oldPath, cancellationToken);

            if (source == null || !source.IsFolder)

                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");

            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)

                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");



            var (parentId, name) = await GetParentAndNameAsync(newPath, cancellationToken);

            await PutJsonAsync($"{ApiBaseUrl}/folders/{source.Id}",

                JsonSerializer.Serialize(new { name, parent = new { id = parentId } }), cancellationToken);

        }



        private async Task UploadFileContentAsync(string fileId, byte[] content, CancellationToken cancellationToken)

        {

            using var form = new MultipartFormDataContent();

            form.Add(new StringContent(JsonSerializer.Serialize(new { name = "file" }), Encoding.UTF8, "application/json"), "attributes");

            form.Add(new ByteArrayContent(content), "file", "file");



            using var request = new HttpRequestMessage(HttpMethod.Post, $"{UploadBaseUrl}/files/{fileId}/content");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            request.Content = form;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

        }



        private async Task<string> GetFolderIdAsync(string path, CancellationToken cancellationToken)

        {

            path = path ?? string.Empty;

            if (string.IsNullOrEmpty(path))

                return "0";



            var resolved = await ResolvePathAsync(path, cancellationToken);

            if (resolved == null || !resolved.IsFolder)

                throw new FileStoreException($"The directory '{path}' does not exist.");



            return resolved.Id;

        }



        private async Task<(string ParentId, string Name)> GetParentAndNameAsync(string path, CancellationToken cancellationToken)

        {

            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length == 0)

                throw new FileStoreException("Invalid path.");



            var name = segments[segments.Length - 1];

            var parentPath = segments.Length > 1

                ? string.Join("/", segments.Take(segments.Length - 1))

                : string.Empty;

            var parentId = await GetFolderIdAsync(parentPath, cancellationToken);

            return (parentId, name);

        }



        private async Task<BoxItem> ResolvePathAsync(string path, CancellationToken cancellationToken)

        {

            if (string.IsNullOrEmpty(path))

                return new BoxItem { Id = "0", Name = string.Empty, IsFolder = true, ModifiedAt = DateTimeOffset.UtcNow };



            var segments = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            var folderId = "0";

            BoxItem current = null;



            for (var i = 0; i < segments.Length; i++)

            {

                var segment = segments[i];

                var items = await ListFolderItemsAsync(folderId, cancellationToken);

                current = items.FirstOrDefault(item => item.Name.Equals(segment, StringComparison.OrdinalIgnoreCase));

                if (current == null)

                    return null;



                var isLast = i == segments.Length - 1;

                if (isLast)

                    return current;



                if (!current.IsFolder)

                    return null;



                folderId = current.Id;

            }



            return current;

        }



        private async Task<IReadOnlyList<BoxItem>> ListFolderItemsAsync(string folderId, CancellationToken cancellationToken)

        {

            var results = new List<BoxItem>();

            var offset = 0;

            const int limit = 1000;



            while (true)

            {

                var json = await GetJsonAsync($"{ApiBaseUrl}/folders/{folderId}/items?limit={limit}&offset={offset}", cancellationToken);

                using var document = JsonDocument.Parse(json);

                var root = document.RootElement;

                foreach (var entry in root.GetProperty("entries").EnumerateArray())

                {

                    var type = entry.GetProperty("type").GetString();

                    results.Add(new BoxItem

                    {

                        Id = entry.GetProperty("id").GetString(),

                        Name = entry.GetProperty("name").GetString(),

                        IsFolder = type == "folder",

                        Size = type == "file" && entry.TryGetProperty("size", out var size) ? size.GetInt64() : 0,

                        ModifiedAt = entry.TryGetProperty("modified_at", out var modified)

                            ? DateTimeOffset.Parse(modified.GetString())

                            : DateTimeOffset.UtcNow

                    });

                }



                var total = root.GetProperty("total_count").GetInt32();

                offset += limit;

                if (offset >= total)

                    break;

            }



            return results;

        }



        private async Task<string> GetJsonAsync(string url, CancellationToken cancellationToken)

        {

            using var request = CreateRequest(HttpMethod.Get, url);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadAsStringAsync();

        }



        private async Task<string> PostJsonAsync(string url, string body, CancellationToken cancellationToken)

        {

            using var request = CreateRequest(HttpMethod.Post, url);

            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadAsStringAsync();

        }



        private async Task<string> PutJsonAsync(string url, string body, CancellationToken cancellationToken)

        {

            using var request = CreateRequest(HttpMethod.Put, url);

            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

            return await response.Content.ReadAsStringAsync();

        }



        private async Task DeleteAsync(string url, CancellationToken cancellationToken)

        {

            using var request = CreateRequest(HttpMethod.Delete, url);

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            await EnsureSuccessAsync(response, cancellationToken);

        }



        private HttpRequestMessage CreateRequest(HttpMethod method, string url)

        {

            var request = new HttpRequestMessage(method, url);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return request;

        }



        private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)

        {

            if (!response.IsSuccessStatusCode)

            {

                var body = await response.Content.ReadAsStringAsync();

                throw new FileStoreException($"Box API request failed ({(int)response.StatusCode}): {body}");

            }

        }



        private static BoxStoreEntry CreateEntry(string path, bool isDirectory, long length, DateTimeOffset modified)

            => new BoxStoreEntry(path ?? string.Empty, isDirectory, length, modified);



        private sealed class BoxItem

        {

            public string Id { get; set; }

            public string Name { get; set; }

            public bool IsFolder { get; set; }

            public long Size { get; set; }

            public DateTimeOffset ModifiedAt { get; set; }

        }

    }

}

