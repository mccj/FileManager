using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager.FileStorage.Standard
{
    public class FileProviderStore : IFileStore
    {
        private readonly IFileProvider _fileProvider;

        public FileProviderStore(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public Task<IFileStoreEntry> GetFileInfoAsync(string path)
        {
            var fileInfo = _fileProvider.GetFileInfo(path);
            if (fileInfo.Exists)
            {
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, fileInfo));
            }

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IFileStoreEntry> GetDirectoryInfoAsync(string path)
        {
            var directoryInfo = _fileProvider.GetDirectoryContents(path);

            if (directoryInfo.Exists)
            {
                return Task.FromResult<IFileStoreEntry>(new FileSystemStoreEntry(path, new StoreDirInfo(path)));
            }

            return Task.FromResult<IFileStoreEntry>(null);
        }

        public Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false)
        {
            var results = _fileProvider.GetDirectoryContents(path);
            if (!results.Exists)
            {
                return Task.FromResult(Enumerable.Empty<IFileStoreEntry>());
            }
            var _results = results.Select(f => new FileSystemStoreEntry(this.Combine(path, f.Name), f));
            return Task.FromResult((IEnumerable<IFileStoreEntry>)_results);
        }

        public Task<bool> TryCreateDirectoryAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task<bool> TryDeleteFileAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task<bool> TryDeleteDirectoryAsync(string path)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task MoveFileAsync(string oldPath, string newPath)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task CopyFileAsync(string srcPath, string dstPath)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task<Stream> GetFileStreamAsync(string path)
        {
            var stream = _fileProvider.GetFileInfo(path).CreateReadStream();
            return Task.FromResult<Stream>(stream);
        }

        public Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry)
        {
            return GetFileStreamAsync(fileStoreEntry.Path);
        }

        public Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false)
        {
            throw new FileStoreException($"不支持.");
        }

        public Task MoveDirectoryAsync(string oldPath, string newPath)
        {
            throw new FileStoreException($"不支持.");
        }
    }
    class StoreDirInfo : IFileInfo
    {
        private string path;

        public StoreDirInfo(string path)
        {
            this.path = path;
        }

        public bool Exists => true;

        public long Length => -1;

        public string PhysicalPath => path;

        public string Name =>System.IO.Path.GetDirectoryName(path);

        public DateTimeOffset LastModified => System.DateTimeOffset.Now;

        public bool IsDirectory => true;

        public Stream CreateReadStream()
        {
            return null;
        }
    }
}
