using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace FileManager
{
    /// <summary>
    /// IFileProvider 适配为只读存储的注册扩展。
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddFileProviderReadOnly(this FileServicesOptions app, System.Func<System.IServiceProvider, Microsoft.Extensions.FileProviders.IFileProvider> func)
        {
            app.AddFileStore((sp) =>
            {
                return new FileStorage.Standard.FileProviderStore(func(sp));
            });

            return app;
        }
        /// <summary>注册 ASP.NET Core WebRoot 为只读存储。</summary>
        public static FileServicesOptions AddWebRootFileProviderReadOnly(this FileServicesOptions app)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var legacyEnv = sp.GetService<IHostingEnvironment>();
                return legacyEnv?.WebRootFileProvider;
            });
        }
        /// <summary>注册物理路径为只读存储。</summary>
        public static FileServicesOptions AddFileProviderPhysicalPathReadOnly(this FileServicesOptions app, System.Func<System.IServiceProvider, string> func)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(func(sp));
                return fileProvider;
            });
        }
        /// <summary>注册 ASP.NET Core ContentRoot 为只读存储。</summary>
        public static FileServicesOptions AddContentRootFileProviderReadOnly(this FileServicesOptions app)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var legacyEnv = sp.GetService<IHostingEnvironment>();
                return legacyEnv?.ContentRootFileProvider;
            });
        }
        public static FileServicesOptions AddFileProviderReadOnly(this FileServicesOptions app, Microsoft.Extensions.FileProviders.IFileProvider fileProvider)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                return fileProvider;
            });
        }

        public static FileServicesOptions AddFileProviderRootPhysicalPathReadOnly(this FileServicesOptions app, Microsoft.AspNetCore.Http.PathString path)
        {
            return AddFileProviderPhysicalPathReadOnly(app, sp =>
            {
                var legacyEnv = sp.GetService<IHostingEnvironment>();
                return legacyEnv?.ContentRootPath + path;
            });
        }
        /// <summary>注册物理路径为只读存储。</summary>
        public static FileServicesOptions AddFileProviderPhysicalPathReadOnly(this FileServicesOptions app, string path)
        {
            return AddFileProviderPhysicalPathReadOnly(app, sp =>
            {
                return path;
            });
        }
    }
}
