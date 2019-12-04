using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    public static class ApplicationBuilderExtensions1111
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, System.Action<FileServicesOptions> action = null)
        {
            var r = new FileServicesOptions() { Services = services };
            if (action == null)
                AddWebRootFileProviderReadOnly(r);
            else
                action?.Invoke(r);

            return services;
        }
        public static IApplicationBuilder UseFileManagerUI(this IApplicationBuilder app, PathString pathMatch, System.Action<FileManagerOptions> action = null)
        {
            //var rr = app.ApplicationServices.GetService<e1>();



            //var option = new FileServerOptions
            //{
            //    //FileProvider = app.ApplicationServices.GetService<MyCompositeFileProvider>(),
            //    RequestPath = pathMatch,
            //    EnableDefaultFiles = true,
            //    EnableDirectoryBrowsing = true
            //};
            ////option.StaticFileOptions.DefaultContentType = "";
            //option.StaticFileOptions.ServeUnknownFileTypes = true;

            //app.UseFileServer(option);

            var r = new FileManagerOptions() { PathMatch = pathMatch, ApplicationBuilder = app };
            if (action == null)
                UseDefaultFileManagerUI(r);
            else
                action?.Invoke(r);

            r.Fun();

            return app;
        }

        public static void UseDefaultFileManagerUI(this FileManagerOptions app)
        {
            app.Fun = new System.Action(() =>
            {
                var fileStore = app.ApplicationBuilder.ApplicationServices.GetService<FileStorage.IFileStore>();
                var option = new FileServerOptions
                {
                    FileProvider = new FileStorage.StoreFileProvider(fileStore),
                    RequestPath = app.PathMatch,
                    EnableDefaultFiles = true,
                    EnableDirectoryBrowsing = true
                };
                option.StaticFileOptions.ServeUnknownFileTypes = true;

                app.ApplicationBuilder.UseFileServer(option);
            });

            //return app;
        }
        public static FileServicesOptions AddFileProviderReadOnly(this FileServicesOptions app, System.Func<System.IServiceProvider, Microsoft.Extensions.FileProviders.IFileProvider> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return new FileStorage.FileProviderStore(func(sp));
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
        public static FileServicesOptions AddFileProviderRootPhysicalPathReadOnly(this FileServicesOptions app, PathString path)
        {
            return AddFileProviderPhysicalPathReadOnly(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.ContentRootPath + path;
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
        public static FileServicesOptions AddFileProviderPhysicalPathReadOnly(this FileServicesOptions app, string path)
        {
            return AddFileProviderPhysicalPathReadOnly(app, sp =>
            {
                return path;
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
        public static FileServicesOptions AddRootPhysicalFilePath(this FileServicesOptions app, PathString path)
        {
            return AddPhysicalFilePath(app, sp =>
            {
                var env = sp.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
                return env.ContentRootPath + path;
            });
        }
        public static FileServicesOptions AddPhysicalFilePath(this FileServicesOptions app, System.Func<System.IServiceProvider, string> func)
        {
            return AddFileStore(app, sp =>
            {
                return new FileStorage.FileSystemStore(func(sp));
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

    public class FileServicesOptions
    {
        public IServiceCollection Services { get; set; }
    }
    public class FileManagerOptions
    {
        public PathString PathMatch { get; set; }
        public IApplicationBuilder ApplicationBuilder { get; set; }
        public/* protected internal*/ System.Action Fun { get; set; }
    }
}
