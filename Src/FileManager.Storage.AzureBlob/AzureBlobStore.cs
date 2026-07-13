using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.AzureBlob
{
    public sealed class AzureBlobStore : IFileStore
    {
        private const string DirectoryMarkerSuffix = "/.dir";

        private readonly BlobContainerClient _container;

        public AzureBlobStore(BlobContainerClient container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public AzureBlobStore(string connectionString, string containerName)
            : this(new BlobContainerClient(connectionString, containerName))
        {
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var client = _container.GetBlobClient(path);
            try
            {
                var properties = await client.GetPropertiesAsync(cancellationToken: cancellationToken);
                return new AzureBlobStoreEntry(path, false, properties.Value.ContentLength, properties.Value.LastModified);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (await HasDirectoryContentAsync(path, cancellationToken) || await DirectoryMarkerExistsAsync(path, cancellationToken))
                return new AzureBlobStoreEntry(path, true, 0, DateTimeOffset.UtcNow);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);

            await foreach (var item in _container.GetBlobsByHierarchyAsync(BlobTraits.Metadata, BlobStates.None, "/", prefix, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.IsPrefix)
                {
                    var name = GetNameFromPrefix(item.Prefix, prefix);
                    if (!string.IsNullOrEmpty(name) && !entries.ContainsKey(name))
                        entries[name] = new AzureBlobStoreEntry(this.Combine(path, name), true, 0, DateTimeOffset.UtcNow);
                    continue;
                }

                var blobName = item.Blob.Name;
                if (blobName.EndsWith(DirectoryMarkerSuffix, StringComparison.OrdinalIgnoreCase))
                    continue;

                var relativeName = string.IsNullOrEmpty(prefix)
                    ? blobName
                    : blobName.Substring(prefix.Length);
                var slash = relativeName.IndexOf('/');
                if (slash >= 0)
                {
                    var dirName = relativeName.Substring(0, slash);
                    if (!entries.ContainsKey(dirName))
                        entries[dirName] = new AzureBlobStoreEntry(this.Combine(path, dirName), true, 0, item.Blob.Properties.LastModified ?? DateTimeOffset.UtcNow);
                    continue;
                }

                if (!entries.ContainsKey(relativeName))
                {
                    entries[relativeName] = new AzureBlobStoreEntry(
                        this.Combine(path, relativeName),
                        false,
                        item.Blob.Properties.ContentLength ?? 0,
                        item.Blob.Properties.LastModified ?? DateTimeOffset.UtcNow);
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

            var marker = _container.GetBlobClient(path.TrimEnd('/') + DirectoryMarkerSuffix);
            await marker.UploadAsync(BinaryData.FromBytes(Array.Empty<byte>()), overwrite: true, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var response = await _container.GetBlobClient(path).DeleteIfExistsAsync(cancellationToken: cancellationToken);
            return response.Value;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var deleted = false;

            await foreach (var page in _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, prefix: prefix, cancellationToken: cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _container.DeleteBlobIfExistsAsync(page.Name, cancellationToken: cancellationToken);
                deleted = true;
            }

            var marker = _container.GetBlobClient(prefix.TrimEnd('/') + DirectoryMarkerSuffix);
            if (await marker.DeleteIfExistsAsync(cancellationToken: cancellationToken))
                deleted = true;

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
            var source = _container.GetBlobClient(srcPath);
            if (!await source.ExistsAsync(cancellationToken))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var destination = _container.GetBlobClient(dstPath);
            if (await destination.ExistsAsync(cancellationToken))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await destination.StartCopyFromUriAsync(source.Uri, cancellationToken: cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var client = _container.GetBlobClient(path);
            if (!await client.ExistsAsync(cancellationToken))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var response = await client.DownloadStreamingAsync(cancellationToken: cancellationToken);
            return response.Value.Content;
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            var client = _container.GetBlobClient(path);
            if (!overwrite && await client.ExistsAsync(cancellationToken))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            await client.UploadAsync(inputStream, overwrite: overwrite, cancellationToken);
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

            return await _container.GetBlobClient(path.TrimEnd('/') + DirectoryMarkerSuffix).ExistsAsync(cancellationToken);
        }

        private async Task<bool> HasDirectoryContentAsync(string path, CancellationToken cancellationToken)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            await foreach (var _ in _container.GetBlobsByHierarchyAsync(BlobTraits.None, BlobStates.None, delimiter: "/", prefix: prefix, cancellationToken: cancellationToken))
                return true;

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
