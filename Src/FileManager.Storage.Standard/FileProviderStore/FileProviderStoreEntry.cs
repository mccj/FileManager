using Microsoft.Extensions.FileProviders;
using System;

namespace FileManager.FileStorage.Standard
{
    public class FileProviderStoreEntry : IFileStoreEntry
    {
        private readonly IFileInfo _fileInfo;
        private readonly string _path;

        internal FileProviderStoreEntry(string path, IFileInfo fileInfo)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }

        public string Path => _path;
        public string Name => _fileInfo.Name;
        public string DirectoryPath
        {
            get
            {
                var lastSlash = _path.LastIndexOf('/');
                return lastSlash < 0 ? string.Empty : _path.Substring(0, lastSlash);
            }
        }
        public DateTimeOffset LastModifiedUtc => _fileInfo.LastModified.UtcDateTime;
        public long Length => _fileInfo.Length;
        public bool IsDirectory => _fileInfo.IsDirectory;
    }
}
