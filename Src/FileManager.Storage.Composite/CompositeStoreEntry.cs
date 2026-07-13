using FileManager.FileStorage;
using System;

namespace FileManager.Storage.Composite
{
    public sealed class CompositeStoreEntry : IFileStoreEntry
    {
        private readonly IFileStoreEntry _inner;

        public CompositeStoreEntry(string compositePath, IFileStoreEntry inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Path = compositePath ?? string.Empty;
            Name = _inner.Name;
            DirectoryPath = GetDirectoryPath(compositePath, Name);
            Length = _inner.Length;
            LastModifiedUtc = _inner.LastModifiedUtc;
            IsDirectory = _inner.IsDirectory;
        }

        public CompositeStoreEntry(string driveName, bool isDriveRoot)
        {
            if (!isDriveRoot)
                throw new ArgumentException("Use the inner-entry constructor for nested paths.", nameof(isDriveRoot));

            Path = driveName ?? throw new ArgumentNullException(nameof(driveName));
            Name = driveName;
            DirectoryPath = string.Empty;
            Length = 0;
            LastModifiedUtc = DateTimeOffset.UtcNow;
            IsDirectory = true;
        }

        public static CompositeStoreEntry CreateRoot()
        {
            return new CompositeStoreEntry(isCompositeRoot: true);
        }

        private CompositeStoreEntry(bool isCompositeRoot)
        {
            if (!isCompositeRoot)
                throw new ArgumentException("Invalid composite root marker.", nameof(isCompositeRoot));

            Path = string.Empty;
            Name = string.Empty;
            DirectoryPath = string.Empty;
            Length = 0;
            LastModifiedUtc = DateTimeOffset.UtcNow;
            IsDirectory = true;
        }

        public string Path { get; }
        public string Name { get; }
        public string DirectoryPath { get; }
        public long Length { get; }
        public DateTimeOffset LastModifiedUtc { get; }
        public bool IsDirectory { get; }

        private static string GetDirectoryPath(string path, string name)
        {
            if (string.IsNullOrEmpty(path))
                return string.Empty;
            if (path.Length <= name.Length)
                return string.Empty;
            return path.Substring(0, path.Length - name.Length).TrimEnd('/');
        }
    }
}
