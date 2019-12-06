using FileManager.FileStorage;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.Storage.MongoDB
{
    public class MongoDBStore : IFileStore
    {
        private readonly GridFSBucket _bucket;
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
        }
        public MongoDBStore(GridFSBucket bucket)
        {
            _bucket = bucket;
        }
        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            var requestUri = GetPhysicalPath(path);
            var filesInfo = await _bucket.OpenDownloadStreamByNameAsync(requestUri);
            if (filesInfo != null)
            {
                return new MongoDBStoreEntry(requestUri, new GridFSFileInfo(filesInfo.FileInfo.Metadata));
            }
            return null;
        }
        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            var requestUri = GetPhysicalPath(path);

            var filesInfo = await _bucket.FindAsync(requestUri);
            if (filesInfo != null)
            {
                return new MongoDBStoreEntry(requestUri, filesInfo.FirstOrDefault());
            }
            //        var filter = Builders<GridFSFileInfo>.Filter.And(
            //Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, "securityvideo"));

            //        using (var cursor = await _bucket.FindAsync(filter))
            //        {
            //            var results = (await cursor.ToListAsync()).Select(f => new MongoDBStoreEntry(requestUri, f));
            //            return results;
            //        }
            return null;
        }
        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            var requestUri = GetPhysicalPath(path);
            //var filesInfo = await _bucket.FindAsync(requestUri);

            var filter = Builders<GridFSFileInfo>.Filter.And(
    Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, "securityvideo"));

            using (var cursor = await _bucket.FindAsync(filter))
            {
                var results = (await cursor.ToListAsync()).Select(f => new MongoDBStoreEntry(requestUri, f));
                return results;
            }
            //return Enumerable.Empty<IFileStoreEntry>();
        }
        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            throw new System.NotImplementedException();
            //var requestUri = GetPhysicalPath(path);
            //var response = await _ftpClient.Mkcol(requestUri);

            //return response.IsSuccessful;
        }
        public async Task<bool> TryDeleteFileAsync(string path)
        {
            var requestUri = GetPhysicalPath(path);
            await _bucket.DeleteAsync(requestUri);

            return true;
        }
        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            throw new System.NotImplementedException();
            //var requestUri = GetPhysicalPath(path);
            //var response = await _ftpClient.Delete(requestUri);

            //return response.IsSuccessful;
        }
        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            var physicalOldPath = GetPhysicalPath(oldPath);
            var physicalNewPath = GetPhysicalPath(newPath);

            await _bucket.RenameAsync(physicalOldPath, physicalNewPath);
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            throw new System.NotImplementedException();
            //var physicalSrcPath = GetPhysicalPath(srcPath);
            //var physicalDstPath = GetPhysicalPath(dstPath);

            //var response = await _ftpClient.Move(physicalSrcPath, physicalDstPath);

            //if (!response.IsSuccessful)
            //{
            //    throw new FileStoreException($"Cannot copy file '{srcPath}'.");
            //}
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
            var requestUri = GetPhysicalPath(path);
            var response = await _bucket.UploadFromStreamAsync(requestUri, inputStream);
            if (response != null)
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
