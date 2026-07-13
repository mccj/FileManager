using Microsoft.Extensions.FileProviders.Physical;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage.Standard
{
    public class FileSystemStore : IFileStore
    {
        private readonly string _fileSystemPath;

        public FileSystemStore(string fileSystemPath)
        {
            _fileSystemPath = Path.GetFullPath(fileSystemPath);
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);
            var fileInfo = new PhysicalFileInfo(new FileInfo(physicalPath));

            if (fileInfo.Exists)
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path ?? string.Empty, fileInfo));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);
            var directoryInfo = new PhysicalDirectoryInfo(new DirectoryInfo(physicalPath));

            if (directoryInfo.Exists)
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path ?? string.Empty, directoryInfo));

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);
            var results = new List<IFileStoreEntry>();

            if (!Directory.Exists(physicalPath))
                return Task.FromResult((IEnumerable<IFileStoreEntry>)results);

            var searchOption = includeSubDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            results.AddRange(
                Directory
                    .GetDirectories(physicalPath, "*", searchOption)
                    .Select(f =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var fileSystemInfo = new PhysicalDirectoryInfo(new DirectoryInfo(f));
                        var fileRelativePath = f.Substring(_fileSystemPath.Length);
                        var filePath = this.NormalizePath(fileRelativePath);
                        return new FileSystemStoreEntry(filePath, fileSystemInfo);
                    }));

            results.AddRange(
                Directory
                    .GetFiles(physicalPath, "*", searchOption)
                    .Select(f =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var fileSystemInfo = new PhysicalFileInfo(new FileInfo(f));
                        var fileRelativePath = f.Substring(_fileSystemPath.Length);
                        var filePath = this.NormalizePath(fileRelativePath);
                        return new FileSystemStoreEntry(filePath, fileSystemInfo);
                    }));

            return Task.FromResult((IEnumerable<IFileStoreEntry>)results);
        }

        public Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);

            if (File.Exists(physicalPath))
                throw new FileStoreException($"Cannot create directory because the path '{path}' already exists and is a file.");

            if (Directory.Exists(physicalPath))
                return Task.FromResult(false);

            Directory.CreateDirectory(physicalPath);
            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);

            if (!File.Exists(physicalPath))
                return Task.FromResult(false);

            File.Delete(physicalPath);
            return Task.FromResult(true);
        }

        public Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);

            if (!Directory.Exists(physicalPath))
                return Task.FromResult(false);

            Directory.Delete(physicalPath, recursive: true);
            return Task.FromResult(true);
        }

        public Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalOldPath = GetPhysicalPath(oldPath);

            if (!File.Exists(physicalOldPath))
                throw new FileStoreException($"Cannot move file '{oldPath}' because it does not exist.");

            var physicalNewPath = GetPhysicalPath(newPath);

            if (File.Exists(physicalNewPath) || Directory.Exists(physicalNewPath))
                throw new FileStoreException($"Cannot move file because the new path '{newPath}' already exists.");

            File.Move(physicalOldPath, physicalNewPath);
            return Task.CompletedTask;
        }

        public Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalSrcPath = GetPhysicalPath(srcPath);

            if (!File.Exists(physicalSrcPath))
                throw new FileStoreException($"The file '{srcPath}' does not exist.");

            var physicalDstPath = GetPhysicalPath(dstPath);

            if (File.Exists(physicalDstPath) || Directory.Exists(physicalDstPath))
                throw new FileStoreException($"Cannot copy file because the destination path '{dstPath}' already exists.");

            File.Copy(physicalSrcPath, physicalDstPath);
            return Task.CompletedTask;
        }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalPath = GetPhysicalPath(path);

            if (!File.Exists(physicalPath))
                throw new FileStoreException($"Cannot get file stream because the file '{path}' does not exist.");

            return Task.FromResult<Stream>(File.OpenRead(physicalPath));
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default)
        {
            return GetFileStreamAsync(fileStoreEntry.Path, cancellationToken);
        }

        public async Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var physicalPath = GetPhysicalPath(path);

            if (!overwrite && File.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists.");

            if (Directory.Exists(physicalPath))
                throw new FileStoreException($"Cannot create file '{path}' because it already exists as a directory.");

            var physicalDirectoryPath = Path.GetDirectoryName(physicalPath);
            Directory.CreateDirectory(physicalDirectoryPath);

            var fileInfo = new FileInfo(physicalPath);
            using (var outputStream = fileInfo.Create())
            {
                await inputStream.CopyToAsync(outputStream, 81920, cancellationToken);
            }
        }

        public Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var physicalOldPath = GetPhysicalPath(oldPath);

            if (!Directory.Exists(physicalOldPath))
                throw new FileStoreException($"Cannot move Directory '{oldPath}' because it does not exist.");

            var physicalNewPath = GetPhysicalPath(newPath);

            if (Directory.Exists(physicalNewPath) || File.Exists(physicalNewPath))
                throw new FileStoreException($"Cannot move Directory because the new path '{newPath}' already exists.");

            Directory.Move(physicalOldPath, physicalNewPath);
            return Task.CompletedTask;
        }

        private string GetPhysicalPath(string path)
        {
            path = this.NormalizePath(path);
            var physicalPath = String.IsNullOrEmpty(path) ? _fileSystemPath : Path.Combine(_fileSystemPath, path);
            var fullPath = Path.GetFullPath(physicalPath);
            var pathIsAllowed = fullPath.StartsWith(_fileSystemPath, StringComparison.OrdinalIgnoreCase);
            if (!pathIsAllowed)
                throw new FileStoreException($"The path '{path}' resolves to a physical path outside the file system store root.");

            return fullPath;
        }
    }
}
