using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 
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
        public static FileServicesOptions AddWebRootFileProviderReadOnly(this FileServicesOptions app)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.WebRootFileProvider;
            });
        }
        public static FileServicesOptions AddFileProviderPhysicalPathReadOnly(this FileServicesOptions app, System.Func<System.IServiceProvider, string> func)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(func(sp));
                return fileProvider;
            });
        }
        public static FileServicesOptions AddContentRootFileProviderReadOnly(this FileServicesOptions app)
        {
            return AddFileProviderReadOnly(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.ContentRootFileProvider;
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
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.ContentRootPath + path;
            });
        }
        public static FileServicesOptions AddFileProviderPhysicalPathReadOnly(this FileServicesOptions app, string path)
        {
            return AddFileProviderPhysicalPathReadOnly(app, sp =>
            {
                return path;
            });
        }
    }
}
