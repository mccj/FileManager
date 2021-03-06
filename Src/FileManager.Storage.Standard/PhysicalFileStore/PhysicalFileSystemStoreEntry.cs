using System;
using System.IO;

namespace FileManager.FileStorage.Standard
{
    public class PhysicalSystemStoreEntry : IFileStoreEntry
    {
        private readonly FileSystemInfo _directoryInfo;
        private readonly string _path;

        internal PhysicalSystemStoreEntry(string path, FileSystemInfo directoryInfo)
        {
            _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public virtual string Path => _path;
        public virtual string Name => _directoryInfo.Name;
        public virtual string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public virtual DateTimeOffset LastModifiedUtc => _directoryInfo.LastWriteTimeUtc;
        public virtual long Length => -1;
        public virtual bool IsDirectory => true;
        //public virtual DateTime CreationTimeUtc => _directoryInfo.CreationTimeUtc;
        //public virtual string Extension => _directoryInfo.Extension;
    }
    public class PhysicalFileSystemStoreEntry : PhysicalSystemStoreEntry
    {
        private readonly FileInfo _fileInfo;
        //private readonly string _path;

        internal PhysicalFileSystemStoreEntry(string path, FileInfo fileInfo) : base(path, fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }

        public override long Length => _fileInfo.Length;
        public override bool IsDirectory => false;
    }
}
