using FluentFTP;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (await _ftpClient.FileExists(path, token: cancellationToken))
            {
                var fileInfo = (await _ftpClient.GetListing(path, token: cancellationToken)).FirstOrDefault();
                return new FtpStoreEntry(path, fileInfo);
            }
            return null;
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (await _ftpClient.DirectoryExists(path, token: cancellationToken))
            {
                var fileModifiedTime = await _ftpClient.GetModifiedTime(path, token: cancellationToken);
                return new FtpStoreEntry(path, new FtpListItem { FullName = path, Size = -1, Modified = fileModifiedTime });
            }
            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (!await _ftpClient.DirectoryExists(path, token: cancellationToken))
                return new IFileStoreEntry[] { };

            var listItems = await _ftpClient.GetListing(path, token: cancellationToken);
            return listItems.Select(f =>
            {
                var filePath = this.NormalizePath(f.FullName);
                return new FtpStoreEntry(filePath, f);
            });
        }

        public async Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (await _ftpClient.DirectoryExists(path, token: cancellationToken))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            await _ftpClient.CreateDirectory(path, token: cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (!await _ftpClient.FileExists(path, token: cancellationToken))
                return false;

            await _ftpClient.DeleteFile(path, token: cancellationToken);
            return true;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (!await _ftpClient.DirectoryExists(path, token: cancellationToken))
                return false;

            await _ftpClient.DeleteDirectory(path, token: cancellationToken);
            return true;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (!await _ftpClient.FileExists(oldPath, token: cancellationToken))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            if (await _ftpClient.FileExists(newPath, token: cancellationToken))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            await _ftpClient.MoveFile(oldPath, newPath, token: cancellationToken);
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            srcPath = this.NormalizePath(srcPath);
            dstPath = this.NormalizePath(dstPath);
            if (!await _ftpClient.FileExists(srcPath, token: cancellationToken))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            if (await _ftpClient.FileExists(dstPath, token: cancellationToken))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            await Copy(srcPath, dstPath, cancellationToken);
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (!await _ftpClient.FileExists(path, token: cancellationToken))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return await _ftpClient.OpenRead(path, token: cancellationToken);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            path = this.NormalizePath(path);
            if (!overwrite && await _ftpClient.FileExists(path, token: cancellationToken))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (await _ftpClient.DirectoryExists(path, token: cancellationToken))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            var parentPath = GetParentVirtualPath(path);
            if (!string.IsNullOrEmpty(parentPath))
                await _ftpClient.CreateDirectory(parentPath, token: cancellationToken);

            await _ftpClient.UploadStream(inputStream, path, token: cancellationToken);
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            oldPath = this.NormalizePath(oldPath);
            newPath = this.NormalizePath(newPath);
            if (!await _ftpClient.DirectoryExists(oldPath, token: cancellationToken))
                throw new FileStoreException($"Cannot move Directory '{oldPath}' because it does not exist.");

            if (await _ftpClient.DirectoryExists(newPath, token: cancellationToken) || await _ftpClient.FileExists(newPath, token: cancellationToken))
                throw new FileStoreException($"Cannot move Directory because the new path '{newPath}' already exists.");

            await _ftpClient.MoveDirectory(oldPath, newPath, token: cancellationToken);
        }

        private async Task Copy(string sourceFileName, string destFileName, CancellationToken cancellationToken)
        {
            using (var readStream = await _ftpClient.OpenRead(sourceFileName, token: cancellationToken))
            {
                await _ftpClient.UploadStream(readStream, destFileName, token: cancellationToken);
            }
        }

        private static string GetParentVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var index = path.LastIndexOf('/');
            return index < 0 ? null : path.Substring(0, index);
        }
    }
}
