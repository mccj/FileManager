using FileManager.FileStorage;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.Storage.Sftp
{
    public sealed class SftpStore : IFileStore
    {
        private readonly ConnectionInfo _connectionInfo;
        private readonly string _root;

        public SftpStore(string host, int port, string user, string password, string root = "/")
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host is required.", nameof(host));
            _connectionInfo = new ConnectionInfo(host, port, user, new PasswordAuthenticationMethod(user, password));
            _root = NormalizeRoot(root);
        }

        public SftpStore(ConnectionInfo connectionInfo, string root = "/")
        {
            _connectionInfo = connectionInfo ?? throw new ArgumentNullException(nameof(connectionInfo));
            _root = NormalizeRoot(root);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path);
                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath) || client.GetAttributes(remotePath).IsDirectory)
                    return null;

                var file = client.Get(remotePath);
                return (IFileStoreEntry)new SftpStoreEntry(path, file);
            });
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path) ?? string.Empty;
                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath) || !client.GetAttributes(remotePath).IsDirectory)
                    return null;

                var file = client.Get(remotePath);
                return (IFileStoreEntry)new SftpStoreEntry(path, file);
            });
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path) ?? string.Empty;
                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath) || !client.GetAttributes(remotePath).IsDirectory)
                    return Enumerable.Empty<IFileStoreEntry>();

                var entries = client.ListDirectory(remotePath)
                    .Where(f => f.Name != "." && f.Name != "..")
                    .Select(f => new SftpStoreEntry(ToVirtualPath(f.FullName), f))
                    .Cast<IFileStoreEntry>()
                    .ToList();

                if (includeSubDirectories)
                {
                    var all = new Dictionary<string, IFileStoreEntry>(StringComparer.OrdinalIgnoreCase);
                    foreach (var entry in entries)
                        all[entry.Path] = entry;
                    foreach (var directory in entries.Where(e => e.IsDirectory).ToArray())
                    {
                        foreach (var nested in await GetDirectoryContentAsync(directory.Path, true, cancellationToken))
                        {
                            if (!all.ContainsKey(nested.Path))
                                all[nested.Path] = nested;
                        }
                    }

                    return all.Values;
                }

                return entries;
            });
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path);
                var remotePath = ToRemotePath(path);
                if (client.Exists(remotePath) && !client.GetAttributes(remotePath).IsDirectory)
                    throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

                if (client.Exists(remotePath))
                    return false;

                client.CreateDirectory(remotePath);
                return true;
            });
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path);
                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath) || client.GetAttributes(remotePath).IsDirectory)
                    return false;

                client.DeleteFile(remotePath);
                return true;
            });
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path) ?? string.Empty;
                if (string.IsNullOrEmpty(path))
                    throw new FileStoreException("Cannot delete the root directory.");

                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath))
                    return false;

                DeleteDirectoryRecursive(client, remotePath);
                return true;
            });
        }

        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                oldPath = this.NormalizePath(oldPath);
                newPath = this.NormalizePath(newPath);
                var oldRemote = ToRemotePath(oldPath);
                var newRemote = ToRemotePath(newPath);
                if (!client.Exists(oldRemote) || client.GetAttributes(oldRemote).IsDirectory)
                    throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");
                if (client.Exists(newRemote))
                    throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

                EnsureParentDirectory(client, newPath);
                client.RenameFile(oldRemote, newRemote);
                await Task.CompletedTask;
            });
        }

        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                srcPath = this.NormalizePath(srcPath);
                dstPath = this.NormalizePath(dstPath);
                var srcRemote = ToRemotePath(srcPath);
                var dstRemote = ToRemotePath(dstPath);
                if (!client.Exists(srcRemote) || client.GetAttributes(srcRemote).IsDirectory)
                    throw new FileStoreException($"The file '{srcPath}' does not exist.");
                if (client.Exists(dstRemote))
                    throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

                EnsureParentDirectory(client, dstPath);
                using var read = client.OpenRead(srcRemote);
                using var write = client.Create(dstRemote);
                await read.CopyToAsync(write, 81920, cancellationToken);
            });
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path);
                var remotePath = ToRemotePath(path);
                if (!client.Exists(remotePath) || client.GetAttributes(remotePath).IsDirectory)
                    throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

                var stream = client.OpenRead(remotePath);
                var memory = new MemoryStream();
                await stream.CopyToAsync(memory, 81920, cancellationToken);
                memory.Position = 0;
                stream.Dispose();
                return (Stream)memory;
            });
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                path = this.NormalizePath(path);
                var remotePath = ToRemotePath(path);
                if (!overwrite && client.Exists(remotePath))
                    throw new FileStoreException($"Cannot create file '{path}' because it already exists.");
                if (client.Exists(remotePath) && client.GetAttributes(remotePath).IsDirectory)
                    throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

                EnsureParentDirectory(client, path);
                using var output = client.Create(remotePath);
                await inputStream.CopyToAsync(output, 81920, cancellationToken);
            });
        }

        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            return WithClient(async client =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                oldPath = this.NormalizePath(oldPath);
                newPath = this.NormalizePath(newPath);
                var oldRemote = ToRemotePath(oldPath);
                var newRemote = ToRemotePath(newPath);
                if (!client.Exists(oldRemote) || !client.GetAttributes(oldRemote).IsDirectory)
                    throw new FileStoreException($"Cannot move directory '{oldPath}' because it does not exist.");
                if (client.Exists(newRemote))
                    throw new FileStoreException($"Cannot move directory because the new path '{newPath}' already exists.");

                client.RenameFile(oldRemote, newRemote);
                await Task.CompletedTask;
            });
        }

        private static void DeleteDirectoryRecursive(SftpClient client, string remotePath)
        {
            foreach (var entry in client.ListDirectory(remotePath))
            {
                if (entry.Name == "." || entry.Name == "..")
                    continue;
                if (entry.IsDirectory)
                    DeleteDirectoryRecursive(client, entry.FullName);
                else
                    client.DeleteFile(entry.FullName);
            }

            client.DeleteDirectory(remotePath);
        }

        private void EnsureParentDirectory(SftpClient client, string virtualPath)
        {
            var parent = GetParentVirtualPath(virtualPath);
            if (string.IsNullOrEmpty(parent))
                return;

            var remoteParent = ToRemotePath(parent);
            if (!client.Exists(remoteParent))
                client.CreateDirectory(remoteParent);
        }

        private string ToRemotePath(string virtualPath)
        {
            virtualPath = this.NormalizePath(virtualPath) ?? string.Empty;
            if (string.IsNullOrEmpty(virtualPath))
                return _root;

            return _root.TrimEnd('/') + "/" + virtualPath.Replace('\\', '/');
        }

        private string ToVirtualPath(string remotePath)
        {
            remotePath = remotePath?.Replace('\\', '/') ?? string.Empty;
            if (remotePath.StartsWith(_root, StringComparison.OrdinalIgnoreCase))
                remotePath = remotePath.Substring(_root.Length).TrimStart('/');
            return this.NormalizePath(remotePath) ?? string.Empty;
        }

        private static string GetParentVirtualPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;
            var index = path.LastIndexOf('/');
            return index < 0 ? string.Empty : path.Substring(0, index);
        }

        private static string NormalizeRoot(string root)
        {
            root = (root ?? "/").Replace('\\', '/');
            if (!root.StartsWith("/"))
                root = "/" + root;
            return root.TrimEnd('/');
        }

        private Task<T> WithClient<T>(Func<SftpClient, Task<T>> action)
        {
            return Task.Run(() =>
            {
                using var client = new SftpClient(_connectionInfo);
                client.Connect();
                try
                {
                    return action(client).GetAwaiter().GetResult();
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect();
                }
            });
        }

        private Task WithClient(Func<SftpClient, Task> action)
        {
            return WithClient(async client =>
            {
                await action(client);
                return true;
            });
        }
    }
}
