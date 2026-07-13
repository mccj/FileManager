using System;
using FileManager.FileStorage;
using Renci.SshNet.Sftp;

namespace FileManager.Storage.Sftp
{
    public sealed class SftpStoreEntry : IFileStoreEntry
    {
        public SftpStoreEntry(string path, ISftpFile file)
        {
            Path = path ?? string.Empty;
            IsDirectory = file.IsDirectory;
            Length = IsDirectory ? 0 : file.Length;
            LastModifiedUtc = file.LastWriteTimeUtc;
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
