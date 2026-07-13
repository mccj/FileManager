using Amazon.S3;
using Amazon.S3.Model;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.AwsS3
{
    public sealed class AwsS3Store : IFileStore
    {
        private const string DirectoryMarkerSuffix = "/.dir";

        private readonly IAmazonS3 _client;
        private readonly string _bucketName;

        public AwsS3Store(IAmazonS3 client, string bucketName)
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
                var response = await _client.GetObjectMetadataAsync(_bucketName, path, cancellationToken);
                return new AwsS3StoreEntry(path, false, response.ContentLength, (response.LastModified ?? DateTime.Now).ToUniversalTime());
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            if (await HasDirectoryContentAsync(path, cancellationToken) || await DirectoryMarkerExistsAsync(path, cancellationToken))
                return new AwsS3StoreEntry(path, true, 0, DateTimeOffset.UtcNow);

            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            string continuationToken = null;

            do
            {
                var request = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    Delimiter = "/",
                    ContinuationToken = continuationToken
                };
                var response = await _client.ListObjectsV2Async(request, cancellationToken);

                foreach (var commonPrefix in response.CommonPrefixes)
                {
                    var name = GetNameFromPrefix(commonPrefix, prefix);
                    if (!string.IsNullOrEmpty(name) && !entries.ContainsKey(name))
                        entries[name] = new AwsS3StoreEntry(this.Combine(path, name), true, 0, DateTimeOffset.UtcNow);
                }

                foreach (var s3Object in response.S3Objects)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (s3Object.Key.EndsWith(DirectoryMarkerSuffix, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var relativeName = string.IsNullOrEmpty(prefix)
                        ? s3Object.Key
                        : s3Object.Key.Substring(prefix.Length);
                    if (relativeName.Contains("/"))
                        continue;

                    if (!entries.ContainsKey(relativeName))
                    {
                        entries[relativeName] = new AwsS3StoreEntry(
                            this.Combine(path, relativeName),
                            false,
                            s3Object.Size ?? 0,
                            (s3Object.LastModified ?? DateTime.Now).ToUniversalTime());
                    }
                }

                continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
            }
            while (!string.IsNullOrEmpty(continuationToken));

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

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = path.TrimEnd('/') + DirectoryMarkerSuffix,
                InputStream = new MemoryStream(Array.Empty<byte>())
            };
            await _client.PutObjectAsync(request, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            await _client.DeleteObjectAsync(_bucketName, path, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path) ?? string.Empty;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var deleted = false;
            string continuationToken = null;

            do
            {
                var response = await _client.ListObjectsV2Async(new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = prefix,
                    ContinuationToken = continuationToken
                }, cancellationToken);

                foreach (var s3Object in response.S3Objects)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await _client.DeleteObjectAsync(_bucketName, s3Object.Key, cancellationToken);
                    deleted = true;
                }

                continuationToken = response.IsTruncated == true ? response.NextContinuationToken : null;
            }
            while (!string.IsNullOrEmpty(continuationToken));

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

            await _client.CopyObjectAsync(new CopyObjectRequest
            {
                SourceBucket = _bucketName,
                SourceKey = srcPath,
                DestinationBucket = _bucketName,
                DestinationKey = dstPath
            }, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = this.NormalizePath(path);
            try
            {
                var response = await _client.GetObjectAsync(_bucketName, path, cancellationToken);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");
            }
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

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = path,
                InputStream = inputStream
            };
            await _client.PutObjectAsync(request, cancellationToken);
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

            try
            {
                await _client.GetObjectMetadataAsync(_bucketName, path.TrimEnd('/') + DirectoryMarkerSuffix, cancellationToken);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
        }

        private async Task<bool> HasDirectoryContentAsync(string path, CancellationToken cancellationToken)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var response = await _client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = prefix,
                MaxKeys = 1
            }, cancellationToken);

            return response.S3Objects.Count > 0 || response.CommonPrefixes.Count > 0;
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
