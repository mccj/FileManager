using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
#if NET8_0_OR_GREATER
using AspNetCoreDashboard;
using AspNetCoreDashboard.Abstractions;
#endif

namespace FileManager
{
    public static class ApplicationBuilderExtensions
    {
        public static IServiceCollection AddFileManager(this IServiceCollection services, System.Action<FileServicesOptions> action = null)
        {
            services.AddSingleton<FileStorage.DefaultPermissionHandle>();

            var r = new FileServicesOptions() { Services = services };
            if (action == null)
                r.AddWebRootFileProviderReadOnly();
            else
                action?.Invoke(r);

#if NET8_0_OR_GREATER
            services.AddUiModules();
#endif
            return services;
        }

        public static IApplicationBuilder UseFileManagerUI(this IApplicationBuilder app, PathString pathMatch, System.Action<FileManagerOptions> action = null)
        {
            var r = new FileManagerOptions() { PathMatch = pathMatch, ApplicationBuilder = app };
            if (action == null)
                UseDefaultFileManagerUI(r);
            else
                action?.Invoke(r);

            if (!string.IsNullOrEmpty(r.UiModuleKind))
            {
#if NET8_0_OR_GREATER
                r.Fun = () => MountUiModule(r);
#else
                throw new System.PlatformNotSupportedException("UI modules require a .NET 8+ ASP.NET Core host with AspNetCoreDashboard 3.6.0.");
#endif
            }

            r.Fun();
            return app;
        }

#if NET8_0_OR_GREATER
        private static void MountUiModule(FileManagerOptions options)
        {
            IUiModule module = options.UiModuleKind switch
            {
                "explorer" => new FileExplorerUiModule(options.PathMatch.Value),
                "richfilemanager" => CreateRichFilemanagerModule(options.PathMatch.Value),
                _ => throw new System.InvalidOperationException($"Unknown UI module kind: {options.UiModuleKind}")
            };

            options.ApplicationBuilder.UseUiModule(module);
        }

        private static IUiModule CreateRichFilemanagerModule(string pathPrefix)
        {
            var moduleType = System.Type.GetType("FileManager.RichFilemanagerUiModule, FileManager.UI.RichFilemanager", throwOnError: true);
            return (IUiModule)System.Activator.CreateInstance(moduleType, pathPrefix);
        }
#endif

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
