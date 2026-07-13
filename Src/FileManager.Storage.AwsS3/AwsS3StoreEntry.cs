using FileManager.FileStorage;
using System;

namespace FileManager.Storage.AwsS3
{
    public sealed class AwsS3StoreEntry : IFileStoreEntry
    {
        public AwsS3StoreEntry(string path, bool isDirectory, long length, DateTimeOffset lastModifiedUtc)
        {
            Path = path ?? string.Empty;
            IsDirectory = isDirectory;
            Length = isDirectory ? 0 : length;
            LastModifiedUtc = lastModifiedUtc;
            Name = System.IO.Path.GetFileName(Path.TrimEnd('/'));
            DirectoryPath = IsDirectory
                ? Path
                : (string.IsNullOrEmpty(Path) ? string.Empty : Path.Substring(0, Math.Max(0, Path.Length - Name.Length)).TrimEnd('/'));
        }

        public string Path { get; }
        public string Name { get; }
        public string DirectoryPath { get; }
        public long Length { get; }
        public DateTimeOffset LastModifiedUtc { get; }
        public bool IsDirectory { get; }
    }
}
