using FileManager.FileStorage;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.MongoDB
{
    public class MongoDBStore : IFileStore
    {
        private readonly GridFSBucket _bucket;
        private readonly string _dirName = ".dir______";

        public MongoDBStore(string url) : this(new MongoClient(url))
        {
        }

        public MongoDBStore(MongoUrl url) : this(new MongoClient(url))
        {
        }

        public MongoDBStore(MongoClient client, string databaseName = "MongoDBFilesStore")
        {
            var database = client.GetDatabase(databaseName);
            _bucket = new GridFSBucket(database);
        }

        public MongoDBStore(GridFSBucket bucket)
        {
            _bucket = bucket;
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var filesInfo = await GetGridFsFileInfoAsync(path, cancellationToken);
            return filesInfo == null ? null : new MongoDBStoreEntry(path, filesInfo);
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var dirPath = path + "/" + _dirName;
            var filesInfo = await GetGridFsFileInfoAsync(dirPath, cancellationToken);
            if (filesInfo == null)
            {
                var filesInfos = await GetGridFsDirectoryInfoAsync(path, cancellationToken);
                if (!filesInfos.Any())
                    return null;

                await TryCreateDirectoryAsync(path, cancellationToken);
            }

            return new MongoDBStoreEntry(path, new GridFSFileInfo(new global::MongoDB.Bson.BsonDocument(new Dictionary<string, object>
            {
                { "filename", path },
                { "length", -1 },
                { "uploadDate", DateTime.MinValue }
            })));
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var gridFsFileInfos = await GetGridFsDirectoryInfoAsync(path, cancellationToken);
            return MapDirectoryEntries(path, gridFsFileInfos);
        }

        public async Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var dirPath = path + "/" + _dirName;
            var fileInfo = await GetGridFsFileInfoAsync(dirPath, cancellationToken);
            if (fileInfo != null)
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            var response = await _bucket.OpenUploadStreamAsync(dirPath, cancellationToken: cancellationToken);
            await response.CloseAsync(cancellationToken);
            return response != null;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var fileInfo = await GetGridFsFileInfoAsync(path, cancellationToken);
            await _bucket.DeleteAsync(fileInfo.Id, cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            var fileInfos = await GetGridFsDirectoryInfoAsync(path, cancellationToken);
            foreach (var item in fileInfos)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await _bucket.DeleteAsync(item.Id, cancellationToken);
            }

            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalOldPath = GetPhysicalPath(oldPath);
            var physicalNewPath = GetPhysicalPath(newPath);
            var fileInfo = await GetGridFsFileInfoAsync(physicalOldPath, cancellationToken);
            await _bucket.RenameAsync(fileInfo.Id, physicalNewPath, cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalSrcPath = GetPhysicalPath(srcPath);
            if (await GetGridFsFileInfoAsync(physicalSrcPath, cancellationToken) == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var physicalDstPath = GetPhysicalPath(dstPath);
            if (await GetGridFsFileInfoAsync(physicalDstPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            using (var stream = await _bucket.OpenDownloadStreamByNameAsync(physicalSrcPath, cancellationToken: cancellationToken))
            {
                var response = await _bucket.UploadFromStreamAsync(physicalDstPath, stream, cancellationToken: cancellationToken);
                if (response == null)
                    throw new FileStoreException($"Cannot copy file '{srcPath}'.");
            }
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var streamResponse = await _bucket.OpenDownloadStreamByNameAsync(requestUri, cancellationToken: cancellationToken);
            if (streamResponse == null)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return streamResponse;
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            path = GetPhysicalPath(path);
            if (!overwrite && await GetGridFsFileInfoAsync(path, cancellationToken) != null)
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            var response = await _bucket.UploadFromStreamAsync(path, inputStream, cancellationToken: cancellationToken);
            if (response == null)
                throw new FileStoreException($"Cannot create file '{path}'.");
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalOldPath = GetPhysicalPath(oldPath);
            var physicalNewPath = GetPhysicalPath(newPath);

            if (await GetDirectoryInfoAsync(physicalOldPath, cancellationToken) == null)
                throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");

            if (await GetDirectoryInfoAsync(physicalNewPath, cancellationToken) != null || await GetFileInfoAsync(physicalNewPath, cancellationToken) != null)
                throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

            var prefix = physicalOldPath.TrimEnd('/');
            var files = await GetGridFsDirectoryInfoAsync(prefix, cancellationToken);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!IsPathPrefix(prefix, file.Filename))
                    continue;

                var suffix = file.Filename.Length > prefix.Length
                    ? file.Filename.Substring(prefix.Length)
                    : string.Empty;
                var newFilename = physicalNewPath.TrimEnd('/') + suffix;
                await _bucket.RenameAsync(file.Id, newFilename, cancellationToken);
            }
        }

        private string GetPhysicalPath(string path) => this.NormalizePath(path);

        private async Task<IEnumerable<GridFSFileInfo>> GetGridFsDirectoryInfoAsync(string path, CancellationToken cancellationToken)
        {
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";
            var filter = string.IsNullOrEmpty(path)
                ? Builders<GridFSFileInfo>.Filter.Empty
                : Builders<GridFSFileInfo>.Filter.Or(
                    Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, path),
                    Builders<GridFSFileInfo>.Filter.Regex(x => x.Filename, new global::MongoDB.Bson.BsonRegularExpression("^" + System.Text.RegularExpressions.Regex.Escape(prefix))));

            using (var cursor = await _bucket.FindAsync(filter, cancellationToken: cancellationToken))
            {
                return await cursor.ToListAsync(cancellationToken);
            }
        }

        private async Task<GridFSFileInfo> GetGridFsFileInfoAsync(string path, CancellationToken cancellationToken)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, path);
            using (var cursor = await _bucket.FindAsync(filter, cancellationToken: cancellationToken))
            {
                return (await cursor.ToListAsync(cancellationToken))?.FirstOrDefault();
            }
        }

        private static bool IsPathPrefix(string prefix, string filename)
        {
            return string.Equals(prefix, filename, StringComparison.OrdinalIgnoreCase)
                || filename.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
        }

        private IEnumerable<IFileStoreEntry> MapDirectoryEntries(string path, IEnumerable<GridFSFileInfo> fileInfos)
        {
            var entries = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
            var depth = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var prefix = string.IsNullOrEmpty(path) ? string.Empty : path.TrimEnd('/') + "/";

            foreach (var item in fileInfos.Where(f => string.IsNullOrEmpty(path) || IsPathPrefix(path, f.Filename)))
            {
                var relativeSegments = item.Filename.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(depth).ToArray();
                var fileName = relativeSegments.FirstOrDefault();
                if (string.IsNullOrEmpty(fileName) || fileName == _dirName)
                    continue;

                if (relativeSegments.Length > 1 && !entries.ContainsKey(fileName))
                {
                    entries.Add(fileName, new MongoDBStoreEntry(path, new GridFSFileInfo(new global::MongoDB.Bson.BsonDocument(new Dictionary<string, object>
                    {
                        { "filename", string.IsNullOrEmpty(path) ? fileName : path + "/" + fileName },
                        { "length", -1 },
                        { "uploadDate", DateTime.MinValue }
                    }))));
                }
                else if (!entries.ContainsKey(fileName))
                {
                    entries.Add(fileName, new MongoDBStoreEntry(path, item));
                }
            }

            return entries.Values.ToArray();
        }
    }
}
