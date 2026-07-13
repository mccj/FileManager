using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// FileManager 核心服务注册扩展（存储实例与权限处理）。
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>注册自定义 <see cref="FileStorage.IFileStore"/> 工厂。</summary>
        public static FileServicesOptions AddFileStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.IFileStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }

        /// <summary>注册固定的 <see cref="FileStorage.IFileStore"/> 实例。</summary>
        public static FileServicesOptions AddFileStore(this FileServicesOptions app, FileStorage.IFileStore fileStore)
        {
            return AddFileStore(app, sp =>
            {
                return fileStore;
            });
        }

        /// <summary>注册按文件项解析权限的处理器。</summary>
        public static FileServicesOptions AddPermissionHandle(this FileServicesOptions app, Func<System.IServiceProvider, FileStorage.IFileStoreEntry, Task<FileStorage.Permission>> func)
        {
            app.Services.AddSingleton<FileStorage.IPermissions>((sp) =>
            {
                return new FileStorage.DefaultPermissions(f => func(sp, f));
            });

            return app;
        }

        /// <summary>注册按文件项解析权限的处理器（不依赖 <see cref="IServiceProvider"/>）。</summary>
        public static FileServicesOptions AddPermissionHandle(this FileServicesOptions app, Func<FileStorage.IFileStoreEntry, Task<FileStorage.Permission>> func)
        {
            return AddPermissionHandle(app, (sp, fs) =>
            {
                return func(fs);
            });
        }

        /// <summary>注册演示用全权限处理器（本地示例默认允许读写、上传、预览等）。</summary>
        public static FileServicesOptions AddFullAccessPermissionHandle(this FileServicesOptions app)
        {
            return app.AddPermissionHandle(entry =>
            {
                if (entry.IsDirectory)
                {
                    return Task.FromResult(
                        FileStorage.Permission.ListSubFolders | FileStorage.Permission.ListFiles | FileStorage.Permission.Download
                        | FileStorage.Permission.Create | FileStorage.Permission.Upload | FileStorage.Permission.Delete
                        | FileStorage.Permission.Rename | FileStorage.Permission.Copy | FileStorage.Permission.Compress
                        | FileStorage.Permission.Extract | FileStorage.Permission.Preview);
                }

                return Task.FromResult(
                    FileStorage.Permission.Download | FileStorage.Permission.Preview | FileStorage.Permission.Delete
                    | FileStorage.Permission.Rename | FileStorage.Permission.Copy | FileStorage.Permission.Edit);
            });
        }
    }
}
