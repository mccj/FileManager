using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage.Ftp
{
    public class FtpStore : IFileStore
    {
        private readonly IAsyncFtpClient _ftpClient;

        public FtpStore(IAsyncFtpClient ftpClient)
        {
            _ftpClient = ftpClient;
        }

        public FtpStore(string host, int port, string user, string pass)
            : this(CreateClient(host, port, user, pass))
        {
        }

        private static AsyncFtpClient CreateClient(string host, int port, string user, string pass)
        {
            var client = new AsyncFtpClient(host, user, pass, port);
            client.Encoding = System.Text.Encoding.UTF8;
            return client;
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            if (await _ftpClient.FileExists(path))
            {
                var fileInfo = (await _ftpClient.GetListing(path)).FirstOrDefault();
                return new FtpStoreEntry(path, fileInfo);
            }
            return null;
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            if (await _ftpClient.DirectoryExists(path))
            {
                var fileModifiedTime = await _ftpClient.GetModifiedTime(path);
                return new FtpStoreEntry(path, new FtpListItem { FullName = path, Size = -1, Modified = fileModifiedTime });
            }
            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            if (!await _ftpClient.DirectoryExists(path))
            {
                return new IFileStoreEntry[] { };
            }

            var listItems = await _ftpClient.GetListing(path);

            var results = listItems.Select(f =>
            {
                var filePath = this.NormalizePath(f.FullName);
                return new FtpStoreEntry(filePath, f);
            });

            return results;
        }

        public async Task<bool> TryCreateDirectoryAsync(string path)
        {
            if (await _ftpClient.DirectoryExists(path))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            await _ftpClient.CreateDirectory(path);

            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path)
        {
            if (!await _ftpClient.FileExists(path))
                return false;

            await _ftpClient.DeleteFile(path);

            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path)
        {
            if (!await _ftpClient.DirectoryExists(path))
                return false;

            await _ftpClient.DeleteDirectory(path);

            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath)
        {
            if (!await _ftpClient.FileExists(oldPath))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            if (await _ftpClient.FileExists(newPath))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            await _ftpClient.MoveFile(oldPath, newPath);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath)
        {
            if (!await _ftpClient.FileExists(srcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (await _ftpClient.FileExists(dstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await Copy(srcPath, dstPath);
        }

        public async Task<Stream> GetFileStreamAsync(string path)
        {
            if (!await _ftpClient.FileExists(path))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return await _ftpClient.OpenRead(path);
        }

        public async Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return await GetFileStreamAsync(fileStoreEntry.Path);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            if (!overwrite && await _ftpClient.FileExists(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (await _ftpClient.DirectoryExists(path))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            var physicalDirectoryPath = Path.GetDirectoryName(path);
            await _ftpClient.CreateDirectory(physicalDirectoryPath);

            await _ftpClient.UploadStream(inputStream, path);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath)
        {
            if (!await _ftpClient.DirectoryExists(oldPath))
                throw new FileStoreException($"Cannot move Directory '{oldPath}' because it does not exist.");

            if (await _ftpClient.DirectoryExists(newPath) || await _ftpClient.FileExists(newPath))
                throw new FileStoreException($"Cannot move Directory because the new path '{newPath}' already exists.");

            await _ftpClient.MoveDirectory(oldPath, newPath);
        }

        private async Task Copy(string sourceFileName, string destFileName)
        {
            using (var readStream = await _ftpClient.OpenRead(sourceFileName))
            {
                await _ftpClient.UploadStream(readStream, destFileName);
            }
        }
    }
}
