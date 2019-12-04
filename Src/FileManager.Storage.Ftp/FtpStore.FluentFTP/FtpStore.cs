using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage.Ftp
{
    public class FtpStore : IFileStore
    {
        private readonly FluentFTP.IFtpClient _ftpClient;
        public FtpStore(FluentFTP.IFtpClient ftpClient/*, string fileSystemPath*/)
        {
            _ftpClient = ftpClient;

            //FtpTrace.LogToConsole = true;

            //FtpTrace.LogUserName = false;   // hide FTP user names
            //FtpTrace.LogPassword = false;   // hide FTP passwords
            //FtpTrace.LogIP = false; 	// hide FTP server IP addresses
        }
        public FtpStore(/*string fileSystemPath,*/ string host, int port, string user, string pass) : this(new FluentFTP.FtpClient(host, port, user, pass) { Encoding = System.Text.Encoding.UTF8 }/*, fileSystemPath*/)
        {
        }
        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            if (await _ftpClient.FileExistsAsync(path))
            {
                //var fileInfo = await _ftpClient.GetObjectInfoAsync(path);
                //var fileInfo = await _ftpClient.GetFilePermissionsAsync(path);

                //var fileSize = await _ftpClient.GetFileSizeAsync(path);
                //var fileModifiedTime = await _ftpClient.GetModifiedTimeAsync(path);

                var fileInfo = (await _ftpClient.GetListingAsync(path)).FirstOrDefault();
                return new FtpStoreEntry(path, fileInfo);

            }
            return null;
        }
        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            if (await _ftpClient.DirectoryExistsAsync(path))
            {
                //var fileInfo = await _ftpClient.GetObjectInfoAsync(path);
                //var fileInfo = await _ftpClient.GetFilePermissionsAsync(path);

                //var fileSize = await _ftpClient.GetFileSizeAsync(path);
                var fileModifiedTime = await _ftpClient.GetModifiedTimeAsync(path);
                return new FtpStoreEntry(path, new FluentFTP.FtpListItem { FullName = path, Size = -1, Modified = fileModifiedTime });
            }
            return null;
        }
        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            if (!await _ftpClient.DirectoryExistsAsync(path))
            {
                return new IFileStoreEntry[] { };
            }

            // Add directories.
            var listItems = await _ftpClient
                     .GetListingAsync(path);

            var results = listItems.Select(f =>
                     {
                         var filePath = this.NormalizePath(f.FullName);
                         return new FtpStoreEntry(filePath, f);
                     });

            return (IEnumerable<IFileStoreEntry>)results;
        }
        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            if (await _ftpClient.DirectoryExistsAsync(path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            //if (_ftpClient.DirectoryExists(path))
            //    return Task.FromResult(false);

            await _ftpClient.CreateDirectoryAsync(path);

            return true;
        }
        public async Task<bool> TryDeleteFileAsync(string path)
        {
            if (!await _ftpClient.FileExistsAsync(path))
                return false;

            await _ftpClient.DeleteFileAsync(path);

            return true;
        }
        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (!await _ftpClient.DirectoryExistsAsync(path))
                return false;

            await _ftpClient.DeleteDirectoryAsync(path);

            return true;
        }
        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            if (!await _ftpClient.FileExistsAsync(oldPath))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            if (await _ftpClient.FileExistsAsync(newPath) || _ftpClient.FileExists(newPath))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            await _ftpClient.MoveFileAsync(oldPath, newPath);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath)
        {
            if (!await _ftpClient.FileExistsAsync(srcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (await _ftpClient.FileExistsAsync(dstPath) || _ftpClient.FileExists(dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await Copy(srcPath, dstPath);
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            if (!await _ftpClient.FileExistsAsync(path))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            var stream = await _ftpClient.OpenReadAsync(path);

            return stream;
        }
        public async Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return await GetFileStreamAsync(fileStoreEntry.Path);
        }
        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            if (!overwrite && await _ftpClient.FileExistsAsync(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (await _ftpClient.DirectoryExistsAsync(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            // Create directory path if it doesn't exist.
            var physicalDirectoryPath = Path.GetDirectoryName(path);
            await _ftpClient.CreateDirectoryAsync(physicalDirectoryPath);

            await _ftpClient.UploadAsync(inputStream, path);

            //using (var outputStream = _ftpClient.OpenWrite(path))
            //{
            //    await inputStream.CopyToAsync(outputStream);
            //    outputStream.Close();
            //}
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

        public async Task MoveDirectoryAsync(string oldPath, string newPath)
        {
            if (!await _ftpClient.DirectoryExistsAsync(oldPath))
                throw new FileStoreException($"Cannot move Directory '{oldPath}' because it does not exist.");

            if (await _ftpClient.DirectoryExistsAsync(newPath) || _ftpClient.FileExists(newPath))
                throw new FileStoreException($"Cannot move Directory because the new path '{newPath}' already exists.");

            await _ftpClient.MoveDirectoryAsync(oldPath, newPath);
        }

        //public bool FileExists(string newPath)
        //{
        //    return _ftpClient.FileExists(newPath);
        //}

        //public void CopyDirectoryAsync(string oldPath, string newPath)
        //{
        //    DirectoryCopy(oldPath, newPath);
        //}
        private async Task Copy(string sourceFileName, string destFileName)
        {
            using (var readStream = await _ftpClient.OpenReadAsync(sourceFileName))
            {
                await _ftpClient.UploadAsync(readStream, destFileName);
                //using (var writeStream = _ftpClient.OpenWrite(destFileName))
                //{

                //    readStream.CopyToAsync(writeStream).Wait();
                //}
            }
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
