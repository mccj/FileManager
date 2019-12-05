using System.Collections.Generic;
using System.Threading.Tasks;
#if !NETFULL
//using Microsoft.AspNetCore.StaticFiles;
#else
#endif
namespace FileManager
{
    public interface IFileManagerProvider
    {
        Result<ResultInfo> Initiate();
        Result<ResultInfo[]> SeekFolder(string path, string search);
        Result<ResultInfo> GetInfo(string path);
        Result<ResultInfo[]> ReadFolder(string path);
        Result<ResultInfo> AddFolder(string path, string name);
#if !NETFULL
        Task<Result<List<ResultInfo>>> Upload(string path, IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files);
#else
        //Task<Result<List<ResultInfo>>> Upload(string path, IEnumerable<IFormFile> files);
#endif
        Result<ResultInfo> Rename(string old, string @new);
        Result<ResultInfo> Move(string old, string @new);
        Result<ResultInfo> Copy(string source, string target);
        Result<ResultInfo> SaveFile(string path, string content);
        Result<ResultInfo> Delete(string path);
        byte[] ReadFile(string path);
        byte[] GetImage(string path, bool thumbnail);
        Result<ResultInfo> Summarize();
    }
}