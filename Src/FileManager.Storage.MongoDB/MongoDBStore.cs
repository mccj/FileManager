using FileManager.FileStorage;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.Storage.MongoDB
{
    public class MongoDBStore : IFileStore
    {
        private readonly GridFSBucket _bucket;
        //private readonly global::MongoDB.Driver.Linq.IMongoQueryable<GridFSFileInfo> _fsFilesInfo;
        private readonly string _dirName = ".dir______";//文件夹标记
        //public MongoDBStore(MongoClientSettings settings) : this(new MongoClient(settings))
        //{
        //}
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
            //_fsFilesInfo = _bucket.Database.GetCollection<GridFSFileInfo>("fs.files").AsQueryable();
        }
        public MongoDBStore(GridFSBucket bucket)
        {
            _bucket = bucket;
        }
        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            path = GetPhysicalPath(path);
            var filesInfo = await getGridFSFileInfoAsync(path);
            if (filesInfo != null)
            {
                return new MongoDBStoreEntry(path, filesInfo);
            }
            return null;
        }
        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            path = GetPhysicalPath(path);
            var dirpath = path + "/" + _dirName;
            var filesInfo = await getGridFSFileInfoAsync(dirpath);
            if (filesInfo == null)
            {
                var filesInfos = await getGridFSDirectoryInfoAsync(dirpath);
                if (filesInfos.Any())
                {
                    await TryCreateDirectoryAsync(path);
                }
                else
                {
                    return null;
                }
            }

            return await Task.FromResult(new MongoDBStoreEntry(path, new GridFSFileInfo(new global::MongoDB.Bson.BsonDocument(new Dictionary<string, object> {
                { "filename", path  } ,
                {"length",-1 },
                { "uploadDate",System.DateTime.MinValue}
            }))));
            //return null;
        }
        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            try
            {
                path = GetPhysicalPath(path);
                //var ssss = _fsFilesInfo.Where(f => f.Filename.StartsWith(path)).ToArray();
                var gridFSFileInfos = await getGridFSDirectoryInfoAsync(path);
                var results = 转换当前目录(path, gridFSFileInfos);
                //var results = ssss.Where(f => 是否当前目录(path, f.Filename)).Select(f => new MongoDBStoreEntry(path, f)).ToArray();
                return await Task.FromResult(results);
            }
            catch (Exception ex)
            {

                throw;
            }

            //Builders<GridFSFileInfo>.Filter.Regex(x => x.Filename, new global::MongoDB.Bson.BsonRegularExpression(""));
            //using (var cursor = await _bucket.FindAsync(filter))
            //{
            //    var results = (await cursor.ToListAsync()).Select(f => new MongoDBStoreEntry(path, f));
            //    return results;
            //}
            //return await Task.FromResult(Enumerable.Empty<IFileStoreEntry>());
        }
        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            path = GetPhysicalPath(path);
            var dirpath = path + "/" + _dirName;
            var fileInfo = await getGridFSFileInfoAsync(dirpath);
            if (fileInfo != null)
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            var response = await _bucket.OpenUploadStreamAsync(dirpath);
            await response.CloseAsync();
            return response != null;
        }
        public async Task<bool> TryDeleteFileAsync(string path)
        {
            path = GetPhysicalPath(path);
            var fileInfo = await getGridFSFileInfoAsync(path);
            await _bucket.DeleteAsync(fileInfo.Id);

            return true;
        }
        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            path = GetPhysicalPath(path);

            var fileInfos = await getGridFSDirectoryInfoAsync(path);

            foreach (var item in fileInfos)
            {
                await _bucket.DeleteAsync(item.Id);
            }

            return true;
        }
        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            var physicalOldPath = GetPhysicalPath(oldPath);
            var physicalNewPath = GetPhysicalPath(newPath);

            var fileInfo = await getGridFSFileInfoAsync(physicalOldPath);
            await _bucket.RenameAsync(fileInfo.Id, physicalNewPath);
        }
        public async Task CopyFileAsync(string srcPath, string dstPath)
        {
            var physicalSrcPath = GetPhysicalPath(srcPath);
            if (await getGridFSFileInfoAsync(physicalSrcPath) == null)
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var physicalDstPath = GetPhysicalPath(dstPath);
            if (await getGridFSFileInfoAsync(physicalDstPath) != null)
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");


            var stream = await _bucket.OpenDownloadStreamByNameAsync(physicalSrcPath);
            var response = await _bucket.UploadFromStreamAsync(physicalDstPath, stream);
            if (response == null)
            {
                throw new FileStoreException($"Cannot copy file '{srcPath}'.");
            }
        }
        public async Task<Stream> GetFileStreamAsync(string path)
        {
            var requestUri = GetPhysicalPath(path);
            var streamResponse = await _bucket.OpenDownloadStreamByNameAsync(requestUri);
            if (streamResponse == null)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return streamResponse;
        }
        public async Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return await GetFileStreamAsync(fileStoreEntry.Path);
        }
        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            path = GetPhysicalPath(path);
            var filter = Builders<GridFSFileInfo>.Filter.And(Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, path));
            using (var cursor = await _bucket.FindAsync(filter))
            {
                if (cursor.Any())
                    throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
            }

            var response = await _bucket.UploadFromStreamAsync(path, inputStream);
            if (response == null)
            {
                throw new FileStoreException($"Cannot create file '{path}'.");
            }
        }

        //public Task<bool> IsDirectory(string path)
        //{
        //    //var result = _ftpClient.FileExists(path);
        //    //return Task.FromResult(!result);
        //    var result = _ftpClient.DirectoryExists(path);
        //    return Task.FromResult(result);

        //}

        //public bool DirectoryExists(string newPath)
        //{
        //    return _ftpClient.DirectoryExists(newPath);
        //}

        public Task MoveDirectoryAsync(string oldPath, string newPath)
        {
            throw new System.NotImplementedException();
            //var physicalSrcPath = GetPhysicalPath(oldPath);
            //var physicalDstPath = GetPhysicalPath(newPath);

            //var response = await _ftpClient.Move(physicalSrcPath, physicalDstPath);

            //if (!response.IsSuccessful)
            //{
            //    throw new FileStoreException($"Cannot copy Directory '{oldPath}'.");
            //}
        }

        //public bool FileExists(string newPath)
        //{
        //    return _ftpClient.FileExists(newPath);
        //}

        //public void CopyDirectoryAsync(string oldPath, string newPath)
        //{
        //    DirectoryCopy(oldPath, newPath);
        //}
        private string GetPhysicalPath(string path)
        {
            return this.NormalizePath(path);
            //path = this.NormalizePath(path);

            //var physicalPath = string.IsNullOrEmpty(path) ? _fileSystemPath : Path.Combine(_fileSystemPath, path);

            //// Verify that the resulting path is inside the root file system path.
            //var pathIsAllowed = Path.GetFullPath(physicalPath).StartsWith(_fileSystemPath, StringComparison.OrdinalIgnoreCase);
            //if (!pathIsAllowed)
            //{
            //    throw new FileStoreException($"The path '{path}' resolves to a physical path outside the file system store root.");
            //}

            //return physicalPath;
        }
        private async Task<IEnumerable<GridFSFileInfo>> getGridFSDirectoryInfoAsync(string path)
        {
            var results = _bucket.Database.GetCollection<GridFSFileInfo>("fs.files")
                .AsQueryable().Where(f => f.Filename.StartsWith(path)).ToArray();

            return await Task.FromResult(results);
            //var filter = Builders<GridFSFileInfo>.Filter.Gte(x => x.Filename, path);
            //using (var cursor = await _bucket.FindAsync(filter))
            //{
            //    var results = await cursor.ToListAsync();
            //    return results;
            //}
        }
        private async Task<GridFSFileInfo> getGridFSFileInfoAsync(string path)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, path);
            using (var cursor = await _bucket.FindAsync(filter))
            {
                var results = (await cursor.ToListAsync())?.FirstOrDefault();
                return results;
            }
        }
        private bool 是否当前目录(string path, string subpath)
        {
            var l = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Length + 1;
            return
                subpath.StartsWith(path, StringComparison.OrdinalIgnoreCase) &&
                subpath.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Length == l;
        }
        private IEnumerable<IFileStoreEntry> 转换当前目录(string path, IEnumerable<GridFSFileInfo> fileInfos)
        {
            var ddddf = new Dictionary<string, IFileStoreEntry>();
            var l = path.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Length;

            foreach (var item in fileInfos.Where(f => f.Filename.StartsWith(path)))
            {
                var rr = item.Filename.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries).Skip(l).ToArray();
                var fileName = rr.FirstOrDefault();
                if (rr.Length > 1 && !ddddf.ContainsKey(fileName))
                {
                    ddddf.Add(fileName, new MongoDBStoreEntry(path, new GridFSFileInfo(new global::MongoDB.Bson.BsonDocument(new Dictionary<string, object> {
                        { "filename", path + "/" + fileName } ,
                        {"length",-1 },
                        { "uploadDate",System.DateTime.MinValue}
                    }))));
                }
                else if (fileName == _dirName)
                {//文件夹标记
                    continue;
                }
                else if (!ddddf.ContainsKey(fileName))
                {
                    ddddf.Add(fileName, new MongoDBStoreEntry(path, item));
                }
            }
            return ddddf.Values.ToArray();
        }
        //private void DirectoryCopy(string sourceDirName, string destDirName)
        //{
        //    //// Get the subdirectories for the specified directory.
        //    //var dir = _ftpClient.GetListingAsync(sourceDirName, FluentFTP.FtpListOption.ForceList);

        //    //if (!dir.Exists)
        //    //{
        //    //    throw new DirectoryNotFoundException(
        //    //        "Source directory does not exist or could not be found: "
        //    //        + sourceDirName);
        //    //}

        //    //var dirs = dir.GetDirectories();
        //    //// If the destination directory doesn't exist, create it.
        //    //if (!Directory.Exists(destDirName))
        //    //{
        //    //    Directory.CreateDirectory(destDirName);
        //    //}

        //    //// Get the files in the directory and copy them to the new location.
        //    //var files = dir.GetFiles();
        //    //foreach (var file in files)
        //    //{
        //    //    var temppath = Path.Combine(destDirName, file.Name);
        //    //    file.CopyTo(temppath, false);
        //    //}

        //    //// If copying subdirectories, copy them and their contents to new location.
        //    //foreach (var subdir in dirs)
        //    //{
        //    //    var temppath = Path.Combine(destDirName, subdir.Name);
        //    //    DirectoryCopy(subdir.FullName, temppath);
        //    //}
        //}
    }
}
