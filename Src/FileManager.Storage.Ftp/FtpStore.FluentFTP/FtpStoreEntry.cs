using System;

namespace FileManager.FileStorage.Ftp
{
    public class FtpStoreEntry : IFileStoreEntry
    {
        private readonly FluentFTP.FtpListItem _fileInfo;
        private readonly string _path;

        internal FtpStoreEntry(string path, FluentFTP.FtpListItem fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path => _path;
        public string Name => _fileInfo.Name;
        public string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public long Length => _fileInfo.Size;
        public bool IsDirectory => _fileInfo.Type == FluentFTP.FtpFileSystemObjectType.Directory;
        public DateTimeOffset LastModifiedUtc => _fileInfo.Modified.ToUniversalTime();
        //public DateTime? CreationTimeUtc => _fileInfo.Created.ToUniversalTime();
        //public string Extension => System.IO.Path.GetExtension(_fileInfo.Name);
    }
}
