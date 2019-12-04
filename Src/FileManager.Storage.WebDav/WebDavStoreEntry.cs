using System;

namespace FileManager.FileStorage.WebDav
{
    public class WebDavStoreEntry : IFileStoreEntry
    {
        private readonly global::WebDav.WebDavResource _fileInfo;
        private readonly string _path;

        internal WebDavStoreEntry(string path, global::WebDav.WebDavResource fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public string Path => _path;
        public string Name => _fileInfo.DisplayName ?? Uri.UnescapeDataString(System.IO.Path.GetFileName(_fileInfo.Uri?.Trim('/', '\\')));
        public string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public long Length => _fileInfo.ContentLength ?? -1;
        public bool IsDirectory => _fileInfo.ContentLength == null || _fileInfo.ContentLength <= -1;
        public DateTimeOffset LastModifiedUtc => _fileInfo.LastModifiedDate ?? System.DateTime.MinValue;
        //public DateTime? CreationTimeUtc => _fileInfo.Created.ToUniversalTime();
        //public string Extension => System.IO.Path.GetExtension(_fileInfo.Name);
    }
}
