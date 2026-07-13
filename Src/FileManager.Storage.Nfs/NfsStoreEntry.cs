using System;
using FileManager.FileStorage;

namespace FileManager.Storage.Nfs
{
    public sealed class NfsStoreEntry : IFileStoreEntry
    {
        private readonly IFileStoreEntry _inner;

        public NfsStoreEntry(IFileStoreEntry inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            Path = _inner.Path;
            Name = _inner.Name;
            DirectoryPath = _inner.DirectoryPath;
            Length = _inner.Length;
            LastModifiedUtc = _inner.LastModifiedUtc;
            IsDirectory = _inner.IsDirectory;
        }

        public string Path { get; }
        public string Name { get; }
        public string DirectoryPath { get; }
        public long Length { get; }
        public DateTimeOffset LastModifiedUtc { get; }
        public bool IsDirectory { get; }
    }
}
