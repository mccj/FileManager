using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 
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
