using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddFileStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.IFileStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        public static FileServicesOptions AddFileStore(this FileServicesOptions app, FileStorage.IFileStore fileStore)
        {
            return AddFileStore(app, sp =>
            {
                return fileStore;
            });
        }
    }
}
