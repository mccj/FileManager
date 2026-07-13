using System;

namespace FileManager.FileStorage
{
    /// <summary>
    /// 虚拟文件存储中的文件或目录项。
    /// </summary>
    public interface IFileStoreEntry
    {
        /// <summary>在存储内的完整路径。</summary>
        string Path { get; }

        /// <summary>名称（不含路径）。</summary>
        string Name { get; }

        /// <summary>所在目录的路径。</summary>
        string DirectoryPath { get; }

        /// <summary>文件大小（字节）；目录时为 0。</summary>
        long Length { get; }

        /// <summary>最后修改时间（UTC）。</summary>
        DateTimeOffset LastModifiedUtc { get; }

        /// <summary>是否为目录。</summary>
        bool IsDirectory { get; }
    }
}
