using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManagerWebUI.FileStorage
{
    public class FtpStore : IFileStore
    {
        private readonly FluentFTP.IFtpClient _ftpClient;
        public FtpStore(FluentFTP.IFtpClient ftpClient, string fileSystemPath)
        {
            _ftpClient = ftpClient;
        }
        public FtpStore(string fileSystemPath, string host, int port, string user, string pass) : this(new FluentFTP.FtpClient(host, port, user, pass) { Encoding = System.Text.Encoding.UTF8 }, fileSystemPath)
        {
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            if (_ftpClient.FileExists(path))
            {
                var fileInfo = _ftpClient.GetFilePermissions(path);
                return Task.FromResult<IFileStoreEntry>(new FtpStoreEntry(path, fileInfo));

            }
            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            if (_ftpClient.DirectoryExists(path))
            {
                var directoryInfo = _ftpClient.GetObjectInfo(path);
                return Task.FromResult<IFileStoreEntry>(new FtpStoreEntry(path, directoryInfo));

            }

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null)
        {
            if (!_ftpClient.DirectoryExists(path))
            {
                return Task.FromResult<IEnumerable<IFileStoreEntry>>(new IFileStoreEntry[] { });
            }

            // Add directories.
            var results = _ftpClient
                     .GetListing(path)
                     .Select(f =>
                     {
                         var filePath = this.NormalizePath(path);
                         return new FtpStoreEntry(filePath, f);
                     });

            return Task.FromResult((IEnumerable<IFileStoreEntry>)results);
        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            if (_ftpClient.DirectoryExists(path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            //if (_ftpClient.DirectoryExists(path))
            //    return Task.FromResult(false);

            _ftpClient.CreateDirectory(path);

            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            if (!_ftpClient.FileExists(path))
                return Task.FromResult(false);

            _ftpClient.DeleteFile(path);

            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (!_ftpClient.DirectoryExists(path))
                return Task.FromResult(false);

            _ftpClient.DeleteDirectory(path);

            return Task.FromResult(true);
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            if (!_ftpClient.FileExists(oldPath))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            if (_ftpClient.FileExists(newPath) || _ftpClient.FileExists(newPath))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            _ftpClient.MoveFile(oldPath, newPath);

            return Task.CompletedTask;
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            if (!_ftpClient.FileExists(srcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (_ftpClient.FileExists(dstPath) || _ftpClient.FileExists(dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            Copy(srcPath, dstPath);
            return Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            if (!_ftpClient.FileExists(path))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var stream = _ftpClient.OpenReadAsync(path);

            return stream;
        }

        public Task CreateFileFromStream(string path, Stream inputStream, bool overwrite = false)
        {
            if (!overwrite && _ftpClient.FileExists(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (_ftpClient.DirectoryExists(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            // Create directory path if it doesn't exist.
            var physicalDirectoryPath = Path.GetDirectoryName(path);
            _ftpClient.CreateDirectory(physicalDirectoryPath);

            _ftpClient.Upload(inputStream, path);
            return Task.CompletedTask;

            //using (var outputStream = _ftpClient.OpenWrite(path))
            //{
            //    await inputStream.CopyToAsync(outputStream);
            //    outputStream.Close();
            //}
        }

        public Task<bool> IsDirectory(string path)
        {
            //var result = _ftpClient.FileExists(path);
            //return Task.FromResult(!result);
            var result = _ftpClient.DirectoryExists(path);
            return Task.FromResult(result);

        }

        public bool DirectoryExists(string newPath)
        {
            return _ftpClient.DirectoryExists(newPath);
        }

        public void MoveDirectoryAsync(string oldPath, string newPath)
        {
            _ftpClient.MoveDirectory(oldPath, newPath);
        }

        public bool FileExists(string newPath)
        {
            return _ftpClient.FileExists(newPath);
        }

        public void CopyDirectoryAsync(string oldPath, string newPath)
        {
            DirectoryCopy(oldPath, newPath);
        }
        private void Copy(string sourceFileName, string destFileName)
        {
            using (var readStream = _ftpClient.OpenRead(sourceFileName))
            {
                _ftpClient.Upload(readStream, destFileName);
                //using (var writeStream = _ftpClient.OpenWrite(destFileName))
                //{

                //    readStream.CopyToAsync(writeStream).Wait();
                //}
            }
        }
        private void DirectoryCopy(string sourceDirName, string destDirName)
        {
            //// Get the subdirectories for the specified directory.
            //var dir = _ftpClient.GetListingAsync(sourceDirName, FluentFTP.FtpListOption.ForceList);

            //if (!dir.Exists)
            //{
            //    throw new DirectoryNotFoundException(
            //        "Source directory does not exist or could not be found: "
            //        + sourceDirName);
            //}

            //var dirs = dir.GetDirectories();
            //// If the destination directory doesn't exist, create it.
            //if (!Directory.Exists(destDirName))
            //{
            //    Directory.CreateDirectory(destDirName);
            //}

            //// Get the files in the directory and copy them to the new location.
            //var files = dir.GetFiles();
            //foreach (var file in files)
            //{
            //    var temppath = Path.Combine(destDirName, file.Name);
            //    file.CopyTo(temppath, false);
            //}

            //// If copying subdirectories, copy them and their contents to new location.
            //foreach (var subdir in dirs)
            //{
            //    var temppath = Path.Combine(destDirName, subdir.Name);
            //    DirectoryCopy(subdir.FullName, temppath);
            //}
        }

    }
}
