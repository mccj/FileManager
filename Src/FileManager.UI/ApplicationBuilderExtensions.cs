using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    public static class ApplicationBuilderExtensions1111
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, System.Action<FileServicesOptions> action = null)
        {
            services.AddSingleton<FileStorage.DefaultPermissionHandle>();

            var r = new FileServicesOptions() { Services = services };
            if (action == null)
                r.AddWebRootFileProviderReadOnly();
            else
                action?.Invoke(r);

            return services;
        }
        public static IApplicationBuilder UseFileManagerUI(this IApplicationBuilder app, PathString pathMatch, System.Action<FileManagerOptions> action = null)
        {
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
        }
    }
}
