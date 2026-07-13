using FileManager.FileStorage;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.UI.Common
{
    public sealed class RestFileApi
    {
        private readonly IFileStore _fileStore;
        private readonly FileStorePermissionGuard _guard;

        public RestFileApi(IFileStore fileStore, DefaultPermissionHandle permissions)
        {
            _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
            if (permissions == null)
                throw new ArgumentNullException(nameof(permissions));
            _guard = new FileStorePermissionGuard(fileStore, permissions);
        }

        public async Task<object> ListAsync(string path, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path) ?? string.Empty;
            await _guard.EnsureListAsync(path, cancellationToken);
            var entries = await _fileStore.GetDirectoryContentAsync(path, includeSubDirectories: false, cancellationToken);
            return new
            {
                path,
                canWrite = await _guard.CanWriteDirectoryAsync(path, cancellationToken),
                entries = entries.Select(e => new
                {
                    e.Name,
                    e.Path,
                    e.IsDirectory,
                    e.Length,
                    modifiedUtc = e.LastModifiedUtc
                }).OrderBy(e => !e.IsDirectory).ThenBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            };
        }

        public async Task<RestFileDownload> DownloadAsync(string path, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path);
            await _guard.EnsureDownloadAsync(path, cancellationToken);
            var stream = await _fileStore.GetFileStreamAsync(path, cancellationToken);
            return new RestFileDownload(path, stream);
        }

        public async Task<object> UploadAsync(string path, string fileName, Stream content, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path) ?? string.Empty;
            await _guard.EnsureUploadAsync(path, cancellationToken);
            fileName = VirtualPath.ValidateFileName(fileName);
            var targetPath = _fileStore.Combine(path, fileName);
            await _fileStore.CreateFileFromStreamAsync(targetPath, content, overwrite: true, cancellationToken);
            return new { path = targetPath };
        }

        public async Task<object> MkdirAsync(string path, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path);
            await _guard.EnsureMkdirAsync(path, cancellationToken);
            await _fileStore.TryCreateDirectoryAsync(path, cancellationToken);
            return new { path };
        }

        public async Task<object> DeleteAsync(string path, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path);
            await _guard.EnsureDeleteAsync(path, cancellationToken);
            if (await _fileStore.GetDirectoryInfoAsync(path, cancellationToken) != null)
                await _fileStore.TryDeleteDirectoryAsync(path, cancellationToken);
            else
                await _fileStore.TryDeleteFileAsync(path, cancellationToken);
            return new { path };
        }

        public async Task<object> RenameAsync(string path, string newName, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path);
            await _guard.EnsureRenameAsync(path, cancellationToken);
            newName = VirtualPath.ValidateFileName(newName);
            var parent = GetParentPath(path);
            var newPath = _fileStore.Combine(parent, newName);
            if (await _fileStore.GetDirectoryInfoAsync(path, cancellationToken) != null)
                await _fileStore.MoveDirectoryAsync(path, newPath, cancellationToken);
            else
                await _fileStore.MoveFileAsync(path, newPath, cancellationToken);
            return new { path = newPath };
        }

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }
    }

    public sealed class RestFileDownload
    {
        public RestFileDownload(string path, Stream stream)
        {
            Path = path;
            Stream = stream;
        }

        public string Path { get; }
        public Stream Stream { get; }
    }
}
