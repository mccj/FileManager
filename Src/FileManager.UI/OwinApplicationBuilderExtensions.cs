#if NET48
using AspNetCoreDashboard.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Owin;
using System;
using System.IO;

namespace FileManager
{
    public sealed class FileManagerOwinOptions
    {
        public string UiModuleKind { get; set; } = "explorer";

        public void UseFileExplorerUI()
        {
            UiModuleKind = "explorer";
        }
    }

    public static class OwinApplicationBuilderExtensions
    {
        public static IAppBuilder AddFileManager(this IAppBuilder app, Action<FileServicesOptions> configure = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddFileManager(configure);
            FileManagerOwinServices.Provider = services.BuildServiceProvider();
            return app;
        }

        public static IAppBuilder UseFileManager(this IAppBuilder app, string pathPrefix, string physicalPath)
        {
            var root = ResolveContentPath(physicalPath);
            Directory.CreateDirectory(root);
            return UseFileManager(app, pathPrefix, f => f.AddFileProviderPhysicalPathReadOnly(root), o => o.UseFileExplorerUI());
        }

        public static IAppBuilder UseFileManager(
            IAppBuilder app,
            string pathPrefix,
            Action<FileServicesOptions> configure,
            Action<FileManagerOwinOptions> uiConfigure = null)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));

            app.AddFileManager(configure);

            var uiOptions = new FileManagerOwinOptions();
            uiConfigure?.Invoke(uiOptions);

            var module = CreateUiModule(uiOptions.UiModuleKind, pathPrefix);
            app.AddUiModules()
               .AddModule(module);
            app.UseUiModules();
            return app;
        }

        public static IAppBuilder UseFileManagerUI(
            IAppBuilder app,
            string pathPrefix,
            Action<FileManagerOwinOptions> uiConfigure = null)
        {
            if (FileManagerOwinServices.Provider == null)
                throw new InvalidOperationException("Call AddFileManager or UseFileManager with storage configuration before UseFileManagerUI.");

            var uiOptions = new FileManagerOwinOptions();
            uiConfigure?.Invoke(uiOptions);

            var module = CreateUiModule(uiOptions.UiModuleKind, pathPrefix);
            app.AddUiModules()
               .AddModule(module);
            app.UseUiModules();
            return app;
        }

        private static IUiModule CreateUiModule(string uiModuleKind, string pathPrefix)
        {
            switch (uiModuleKind)
            {
                case "explorer":
                    return new FileExplorerUiModule(pathPrefix);
                default:
                    throw new InvalidOperationException($"Unknown UI module kind: {uiModuleKind}");
            }
        }

        public static string ResolveContentPath(string physicalPath)
        {
            if (string.IsNullOrWhiteSpace(physicalPath))
                throw new ArgumentNullException(nameof(physicalPath));

            return Path.IsPathRooted(physicalPath)
                ? physicalPath
                : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, physicalPath));
        }
    }
}
#endif
