using System;
using FileManager.FileStorage;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace FileManager.Storage.MongoDB
{
    public class MongoDBStoreEntry : IFileStoreEntry
    {
        private readonly string _path;
        private GridFSFileInfo _fileInfo;

        public MongoDBStoreEntry(string path, GridFSFileInfo fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path => _path;
        public string Name => _fileInfo.Filename;//?? Uri.UnescapeDataString(System.IO.Path.GetFileName(_fileInfo.Uri?.Trim('/', '\\')));
        public string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public long Length => _fileInfo.Length;
        public bool IsDirectory => _fileInfo.Length > 0;
        public DateTimeOffset LastModifiedUtc => _fileInfo.UploadDateTime;
        //public DateTime? CreationTimeUtc => _fileInfo.Created.ToUniversalTime();
        //public string Extension => System.IO.Path.GetExtension(_fileInfo.Name);
    }
}
