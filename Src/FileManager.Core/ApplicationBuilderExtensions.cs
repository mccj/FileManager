using System;
using System.Threading.Tasks;
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
        public static FileServicesOptions AddPermissionHandle(this FileServicesOptions app, Func<System.IServiceProvider, FileStorage.IFileStoreEntry, Task<FileStorage.Permission>> func)
        {
            app.Services.AddSingleton<FileStorage.IPermissions>((sp) =>
            {
                return new FileStorage.DefaultPermissions(f => func(sp, f));
            });

            return app;
        }
        public static FileServicesOptions AddPermissionHandle(this FileServicesOptions app, Func<FileStorage.IFileStoreEntry, Task<FileStorage.Permission>> func)
        {
            return AddPermissionHandle(app, (sp, fs) =>
            {
                return func(fs);
            });
        }
    }
}
