using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 本地文件系统目录存储注册扩展。
    /// </summary>
    public  static partial class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddPhysicalFilePath(this FileServicesOptions app, System.Func<System.IServiceProvider, string> func)
        {
            return app.AddFileStore( sp =>
            {
                return new FileStorage.Standard.FileSystemStore(func(sp));
            });
        }
        /// <summary>注册物理路径为存储根（可读写）。</summary>
        public static FileServicesOptions AddRootPhysicalFilePath(this FileServicesOptions app, Microsoft.AspNetCore.Http.PathString path)
        {
            return AddPhysicalFilePath(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.ContentRootPath + path;
            });
        }

        public static FileServicesOptions AddPhysicalFilePath(this FileServicesOptions app, string path)
        {
            return AddPhysicalFilePath(app, sp =>
            {
                return path;
            });
        }


    }
}
