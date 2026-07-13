using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// FileManager 服务注册选项；在 <c>AddFileManager</c> 回调中配置存储与权限。
    /// </summary>
    public class FileServicesOptions
    {
        /// <summary>ASP.NET Core 服务集合。</summary>
        public IServiceCollection Services { get; set; }
    }
}
