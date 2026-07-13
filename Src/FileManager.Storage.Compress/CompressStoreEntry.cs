using FileManager.FileStorage;
using SharpCompress.Common;
using System;

namespace FileManager.Storage.Compress
{
    public class CompressStoreEntry : IFileStoreEntry
    {
        private readonly IEntry _fileInfo;
        private readonly string _path;
        private readonly bool _isDirectoryOverride;

        internal CompressStoreEntry(string path, IEntry fileInfo)
        {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            _path = path ?? throw new ArgumentNullException(nameof(path));
        }

        internal CompressStoreEntry(string path, bool isDirectory)
        {
            _path = path ?? throw new ArgumentNullException(nameof(path));
            _isDirectoryOverride = isDirectory;
        }

        public string Path => _path;
        public string Name => PathHelper.GetEntryName(_path);
        public string DirectoryPath => _path.Substring(0, _path.Length - Name.Length).TrimEnd('/');
        public long Length => _fileInfo?.Size ?? -1;
        public bool IsDirectory => _fileInfo?.IsDirectory ?? _isDirectoryOverride;
        public DateTimeOffset LastModifiedUtc => _fileInfo?.LastModifiedTime?.ToUniversalTime() ?? DateTimeOffset.MinValue;
    }

    internal static class PathHelper
    {
        public static string GetEntryName(string path)
        {
            return System.IO.Path.GetFileName(path.Trim('/'));
        }
    }
}
