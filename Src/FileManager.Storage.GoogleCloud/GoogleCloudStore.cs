using FileManager.FileStorage;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.GoogleCloud
{
    public sealed class GoogleCloudStore : IFileStore
    {
        private const string DirectoryMarkerSuffix = "/.dir";

        private readonly StorageClient _client;
        private readonly string _bucketName;

        public GoogleCloudStore(StorageClient client, string bucketName)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("Bucket name is required.", nameof(bucketName));
            _bucketName = bucketName;
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            try
            {
                var obj = await _client.GetObjectAsync(_bucketName, path, cancellationToken: cancellationToken);
                return new GoogleCloudStoreEntry(path, false, (long?)obj.Size ?? 0, obj.UpdatedDateTimeOffset ?? DateTimeOffset.UtcNow);
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (await HasDirectoryContentAsync(path, cancellationToken) || await DirectoryMarkerExistsAsync(path, cancellationToken))
                return new GoogleCloudStoreEntry(path, true, 0, DateTimeOffset.UtcNow);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var options = new ListObjectsOptions { Delimiter = "/" };
            var response = _client.ListObjects(_bucketName, prefix, options);

            foreach (var page in response.AsRawResponses())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (page.Prefixes != null)
                {
                    foreach (var commonPrefix in page.Prefixes)
                    {
                        var name = GetNameFromPrefix(commonPrefix, prefix);
                        if (!string.IsNullOrEmpty(name) && !entries.ContainsKey(name))
                            entries[name] = new GoogleCloudStoreEntry(this.Combine(path, name), true, 0, DateTimeOffset.UtcNow);
                    }
                }

                if (page.Items == null)
                    continue;

                foreach (var obj in page.Items)
                {
                    if (obj.Name.EndsWith(DirectoryMarkerSuffix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var relativeName = string.IsNullOrEmpty(prefix) ? obj.Name : obj.Name.Substring(prefix.Length);
                    if (relativeName.Contains("/"))
                        continue;

                    if (!entries.ContainsKey(relativeName))
                    {
                        entries[relativeName] = new GoogleCloudStoreEntry(
                            this.Combine(path, relativeName),
                            false,
                            (long?)obj.Size ?? 0,
                            obj.UpdatedDateTimeOffset ?? DateTimeOffset.UtcNow);
                    }
                }
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
            if (await DirectoryMarkerExistsAsync(path, cancellationToken))
                return false;

            using var stream = new MemoryStream(Array.Empty<byte>());
            await _client.UploadObjectAsync(_bucketName, path.TrimEnd('/') + DirectoryMarkerSuffix, "application/octet-stream", stream, cancellationToken: cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            try
            {
                await _client.DeleteObjectAsync(_bucketName, path, cancellationToken: cancellationToken);
                return true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var deleted = false;

            await foreach (var obj in _client.ListObjectsAsync(_bucketName, prefix).WithCancellation(cancellationToken))
            {
                await _client.DeleteObjectAsync(_bucketName, obj.Name, cancellationToken: cancellationToken);
                deleted = true;
            }

            try
            {
                await _client.DeleteObjectAsync(_bucketName, prefix.TrimEnd('/') + DirectoryMarkerSuffix, cancellationToken: cancellationToken);
                deleted = true;
            }
            catch (Google.GoogleApiException ex) when (ex.HttpStatusCode == System.Net.HttpStatusCode.NotFound)
            {
            }

            return deleted;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            await CopyFileAsync(oldPath, newPath, cancellationToken);
            await TryDeleteFileAsync(oldPath, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            if (await GetFileInfoAsync(srcPath, cancellationToken) == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");
            if (await GetFileInfoAsync(dstPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await _client.CopyObjectAsync(_bucketName, srcPath, _bucketName, dstPath, cancellationToken: cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var stream = new MemoryStream();
            await _client.DownloadObjectAsync(_bucketName, path, stream, cancellationToken: cancellationToken);
            stream.Position = 0;
            return stream;
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

            await _client.UploadObjectAsync(_bucketName, path, "application/octet-stream", inputStream, cancellationToken: cancellationToken);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await GetDirectoryInfoAsync(oldPath, cancellationToken) == null)
                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");
            if (await GetDirectoryInfoAsync(newPath, cancellationToken) != null || await GetFileInfoAsync(newPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

            await this.CopyDirectoryAsync(oldPath, newPath, cancellationToken);
            await TryDeleteDirectoryAsync(oldPath, cancellationToken);
        }

        private async Task<bool> DirectoryMarkerExistsAsync(string path, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(path))
                return true;

            return await GetFileInfoAsync(path.TrimEnd('/') + DirectoryMarkerSuffix, cancellationToken) != null;
        }

        private async Task<bool> HasDirectoryContentAsync(string path, CancellationToken cancellationToken)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var options = new ListObjectsOptions { Delimiter = "/" };
            foreach (var page in _client.ListObjects(_bucketName, prefix, options).AsRawResponses())
            {
                cancellationToken.ThrowIfCancellationRequested();
                if ((page.Prefixes != null && page.Prefixes.Count > 0) || (page.Items != null && page.Items.Count > 0))
                    return true;
            }

            await Task.CompletedTask;
            return false;
        }

        private static string GetNameFromPrefix(string prefixValue, string parentPrefix)
        {
            var relative = string.IsNullOrEmpty(parentPrefix)
                ? prefixValue
                : prefixValue.Substring(parentPrefix.Length);
            return relative.TrimEnd('/').Split('/').FirstOrDefault();
        }
    }
}
