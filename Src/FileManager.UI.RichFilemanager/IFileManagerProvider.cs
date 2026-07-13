using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.AspNetCore.Http;
#endif

namespace FileManager
{
    public interface IFileManagerProvider
    {
        Task<Result<ResultInfo>> InitiateAsync(CancellationToken cancellationToken = default);
        Task<Result<ResultInfo[]>> SeekFolderAsync(string path, string search, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> GetInfoAsync(string path, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo[]>> ReadFolderAsync(string path, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> AddFolderAsync(string path, string name, CancellationToken cancellationToken = default);
#if !NETFULL
        Task<Result<List<ResultInfo>>> UploadAsync(string path, IEnumerable<IFormFile> files, CancellationToken cancellationToken = default);
#endif
        Task<Result<ResultInfo>> RenameAsync(string old, string @new, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> MoveAsync(string old, string @new, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> CopyAsync(string source, string target, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> SaveFileAsync(string path, string content, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> DeleteAsync(string path, CancellationToken cancellationToken = default);
        Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default);
        Task<Result<ResultInfo>> SummarizeAsync(CancellationToken cancellationToken = default);
    }
}
