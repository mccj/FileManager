using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage.WebDav
{
    public class WebDavStore : IFileStore
    {
        private readonly global::WebDav.IWebDavClient _ftpClient;

        public WebDavStore(global::WebDav.IWebDavClient ftpClient)
        {
            _ftpClient = ftpClient;
        }

        public WebDavStore(global::WebDav.WebDavClientParams clientParams) : this(new global::WebDav.WebDavClient(clientParams))
        {
        }

        public WebDavStore(System.Uri url, string user, string pass) : this(new global::WebDav.WebDavClientParams { BaseAddress = url, Credentials = new System.Net.NetworkCredential(user, pass) })
        {
        }

        public async Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var filesInfo = await _ftpClient.Propfind(requestUri);
            if (filesInfo.IsSuccessful && filesInfo.Resources.FirstOrDefault()?.ContentLength >= 0)
                return new WebDavStoreEntry(requestUri, filesInfo.Resources.FirstOrDefault());
            return null;
        }

        public async Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var filesInfo = await _ftpClient.Propfind(requestUri);
            if (filesInfo.IsSuccessful && filesInfo.Resources.FirstOrDefault()?.ContentLength == null)
                return new WebDavStoreEntry(requestUri, filesInfo.Resources.FirstOrDefault());
            return null;
        }

        public async Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var filesInfo = await _ftpClient.Propfind(requestUri);
            if (filesInfo.IsSuccessful)
                return filesInfo.Resources.Skip(1).Select(f => new WebDavStoreEntry(requestUri, f));
            return Enumerable.Empty<IFileStoreEntry>();
        }

        public async Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var response = await _ftpClient.Mkcol(requestUri);
            return response.IsSuccessful;
        }

        public async Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var response = await _ftpClient.Delete(requestUri);
            return response.IsSuccessful;
        }

        public async Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var response = await _ftpClient.Delete(requestUri);
            return response.IsSuccessful;
        }

        public async Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalOldPath = GetPhysicalPath(oldPath);
            var physicalNewPath = GetPhysicalPath(newPath);
            var response = await _ftpClient.Move(physicalOldPath, physicalNewPath);
            if (!response.IsSuccessful)
                throw new FileStoreException($"Cannot move file '{oldPath}'.");
        }

        public async Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalSrcPath = GetPhysicalPath(srcPath);
            var physicalDstPath = GetPhysicalPath(dstPath);
            var response = await _ftpClient.Move(physicalSrcPath, physicalDstPath);
            if (!response.IsSuccessful)
                throw new FileStoreException($"Cannot copy file '{srcPath}'.");
        }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var streamResponse = await _ftpClient.GetRawFile(requestUri);
            if (!streamResponse.IsSuccessful)
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return streamResponse.Stream;
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestUri = GetPhysicalPath(path);
            var response = await _ftpClient.PutFile(requestUri, inputStream);
            if (!response.IsSuccessful)
                throw new FileStoreException($"Cannot create file '{path}'.");
        }

        public async Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalSrcPath = GetPhysicalPath(oldPath);
            var physicalDstPath = GetPhysicalPath(newPath);
            var response = await _ftpClient.Move(physicalSrcPath, physicalDstPath);
            if (!response.IsSuccessful)
                throw new FileStoreException($"Cannot move directory '{oldPath}'.");
        }

        private string GetPhysicalPath(string path)
        {
            return this.NormalizePath(path);
        }
    }
}
