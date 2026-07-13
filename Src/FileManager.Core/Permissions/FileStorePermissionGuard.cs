using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    public sealed class FileStorePermissionGuard
    {
        private readonly IFileStore _fileStore;
        private readonly DefaultPermissionHandle _permissions;

        public FileStorePermissionGuard(IFileStore fileStore, DefaultPermissionHandle permissions)
        {
            _fileStore = fileStore;
            _permissions = permissions;
        }

        public bool StoreSupportsWrite()
        {
            return _fileStore.Supports(FileStoreCapabilityFlags.Write);
        }

        public async Task<bool> CanWriteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            if (!StoreSupportsWrite())
                return false;

            var directory = await GetDirectoryEntryAsync(path, cancellationToken);
            if (directory == null)
                return false;

            var granted = await _permissions.GetPermissionAsync(directory);
            var writeFlags = Permission.Upload | Permission.Create | Permission.Delete | Permission.Rename;
            return (granted & writeFlags) != 0;
        }

        public async Task EnsureListAsync(string path, CancellationToken cancellationToken = default)
        {
            var directory = await GetDirectoryEntryAsync(path, cancellationToken);
            if (directory == null)
                throw new FileStoreException($"Directory '{path ?? string.Empty}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(directory);
            if ((granted & (Permission.ListFiles | Permission.ListSubFolders)) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureDownloadAsync(string path, CancellationToken cancellationToken = default)
        {
            var file = await _fileStore.GetFileInfoAsync(path, cancellationToken);
            if (file == null)
                throw new FileStoreException($"The file '{path}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(file);
            if ((granted & (Permission.Download | Permission.Preview)) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureUploadAsync(string directoryPath, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var directory = await GetDirectoryEntryAsync(directoryPath, cancellationToken);
            if (directory == null)
                throw new FileStoreException($"Directory '{directoryPath ?? string.Empty}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(directory);
            if ((granted & (Permission.Upload | Permission.Create)) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureMkdirAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var parentPath = GetParentPath(path);
            var parent = await GetDirectoryEntryAsync(parentPath, cancellationToken);
            if (parent == null)
                throw new FileStoreException($"Parent directory '{parentPath}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(parent);
            if ((granted & Permission.Create) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureDeleteAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var entry = await GetEntryAsync(path, cancellationToken);
            if (entry == null)
                throw new FileStoreException($"Path '{path}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(entry);
            if ((granted & Permission.Delete) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureRenameAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var entry = await GetEntryAsync(path, cancellationToken);
            if (entry == null)
                throw new FileStoreException($"Path '{path}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(entry);
            if ((granted & (Permission.Rename | Permission.Cut)) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureCopyAsync(string itemPath, string targetDirectoryPath, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var entry = await GetEntryAsync(itemPath, cancellationToken);
            if (entry == null)
                throw new FileStoreException($"Path '{itemPath}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(entry);
            if ((granted & (Permission.Copy | Permission.Download)) == 0)
                throw new FileStoreException("Access denied.");

            await EnsureUploadAsync(targetDirectoryPath, cancellationToken);
        }

        public async Task EnsureMoveAsync(string itemPath, string targetDirectoryPath, CancellationToken cancellationToken = default)
        {
            await EnsureRenameAsync(itemPath, cancellationToken);
            await EnsureUploadAsync(targetDirectoryPath, cancellationToken);
        }

        public async Task EnsureEditAsync(string filePath, CancellationToken cancellationToken = default)
        {
            EnsureStoreWritable();
            var file = await _fileStore.GetFileInfoAsync(filePath, cancellationToken);
            if (file == null)
                throw new FileStoreException($"The file '{filePath}' does not exist.");

            var granted = await _permissions.GetPermissionAsync(file);
            if ((granted & (Permission.Edit | Permission.Upload)) == 0)
                throw new FileStoreException("Access denied.");
        }

        public async Task EnsureExtractAsync(string archivePath, string destinationDirectoryPath, CancellationToken cancellationToken = default)
        {
            await EnsureDownloadAsync(archivePath, cancellationToken);
            var granted = await _permissions.GetPermissionAsync(await _fileStore.GetFileInfoAsync(archivePath, cancellationToken));
            if ((granted & Permission.Extract) == 0 && (granted & Permission.Download) == 0)
                throw new FileStoreException("Access denied.");
            await EnsureUploadAsync(destinationDirectoryPath, cancellationToken);
        }

        private async Task<IFileStoreEntry> GetEntryAsync(string path, CancellationToken cancellationToken)
        {
            return await _fileStore.GetFileInfoAsync(path, cancellationToken)
                ?? (IFileStoreEntry)await _fileStore.GetDirectoryInfoAsync(path, cancellationToken);
        }

        private void EnsureStoreWritable()
        {
            if (!StoreSupportsWrite())
                throw new FileStoreException("Storage is read-only.");
        }

        private async Task<IFileStoreEntry> GetDirectoryEntryAsync(string path, CancellationToken cancellationToken)
        {
            path = VirtualPath.Normalize(path) ?? string.Empty;
            return await _fileStore.GetDirectoryInfoAsync(path, cancellationToken);
        }

        private static string GetParentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            var slash = path.LastIndexOf('/');
            return slash < 0 ? string.Empty : path.Substring(0, slash);
        }
    }
}
