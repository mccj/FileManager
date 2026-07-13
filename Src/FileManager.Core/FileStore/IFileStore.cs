using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileManager.FileStorage
{
    /// <summary>
    /// 虚拟文件存储的通用抽象。
    /// </summary>
    /// <remarks>
    /// 路径使用正斜杠 (<c>/</c>) 分隔，相对于虚拟存储根目录；不支持绝对路径、盘符以及 <c>../</c> 上级遍历。
    /// 大小写是否敏感由具体存储实现决定（例如 Windows 文件系统不区分大小写，Linux 与 Azure Blob 区分大小写）。
    /// </remarks>
    public interface IFileStore
    {
        /// <summary>获取指定路径的文件信息。</summary>
        Task<IFileStoreEntry> GetFileInfoAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>获取指定路径的目录信息。</summary>
        Task<IFileStoreEntry> GetDirectoryInfoAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>获取目录下的文件与子目录列表。</summary>
        /// <param name="path">目录路径；为 <c>null</c> 时表示根目录。</param>
        /// <param name="includeSubDirectories">是否递归包含子目录中的项。</param>
        Task<IEnumerable<IFileStoreEntry>> GetDirectoryContentAsync(string path = null, bool includeSubDirectories = false, CancellationToken cancellationToken = default);

        /// <summary>尝试创建目录；目录已存在时返回 <c>false</c>。</summary>
        Task<bool> TryCreateDirectoryAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>尝试删除文件。</summary>
        Task<bool> TryDeleteFileAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>尝试删除目录。</summary>
        Task<bool> TryDeleteDirectoryAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>将文件移动到新路径。</summary>
        Task MoveFileAsync(string oldPath, string newPath, CancellationToken cancellationToken = default);

        /// <summary>将文件复制到新路径。</summary>
        Task CopyFileAsync(string srcPath, string dstPath, CancellationToken cancellationToken = default);

        /// <summary>按路径打开文件只读流。</summary>
        Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default);

        /// <summary>按文件项打开文件只读流。</summary>
        Task<Stream> GetFileStreamAsync(IFileStoreEntry fileStoreEntry, CancellationToken cancellationToken = default);

        /// <summary>从输入流创建或覆盖文件。</summary>
        Task CreateFileFromStreamAsync(string path, Stream inputStream, bool overwrite = false, CancellationToken cancellationToken = default);

        /// <summary>将目录移动到新路径。</summary>
        Task MoveDirectoryAsync(string oldPath, string newPath, CancellationToken cancellationToken = default);
    }
}
