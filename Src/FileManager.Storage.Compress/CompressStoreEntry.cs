using FileManager.FileStorage;
using System;
using System.Linq;

namespace FileManager.Storage.Compress
{
    public class CompressStoreEntry : IFileStoreEntry
    {
        private readonly SharpCompress.Common.IEntry _fileInfo;
        private readonly string _path;

        internal CompressStoreEntry(string path, SharpCompress.Common.IEntry fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path => _path;
        public string Name => 名称转换(_fileInfo.Key);
        public string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public long Length => _fileInfo.Size;
        public bool IsDirectory => _fileInfo.IsDirectory;
        public DateTimeOffset LastModifiedUtc => _fileInfo.LastModifiedTime?.ToUniversalTime() ?? DateTimeOffset.MinValue;
        //public DateTime? CreationTimeUtc => _fileInfo.Created.ToUniversalTime();
        //public string Extension => System.IO.Path.GetExtension(_fileInfo.Name);
        private string 名称转换(string path)
        {
            return System.IO.Path.GetFileName(path.Trim('/'));
        }
    }
}
