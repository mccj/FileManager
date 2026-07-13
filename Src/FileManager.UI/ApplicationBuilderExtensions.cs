using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
#if NET8_0_OR_GREATER
using AspNetCoreDashboard;
using AspNetCoreDashboard.Abstractions;
#endif

namespace FileManager
{
    /// <summary>
    /// ASP.NET Core 宿主集成：注册 FileManager 服务与 UI 模块。
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// 注册 FileManager 核心服务。未提供配置时默认挂载只读 WebRoot 存储。
        /// </summary>
        public static IServiceCollection AddFileManager(this IServiceCollection services, System.Action<FileServicesOptions> action = null)
        {
            services.AddSingleton<FileStorage.DefaultPermissionHandle>();
            services.AddMemoryCache();

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

        /// <summary>
        /// 在指定路径前缀挂载文件管理 UI 模块（FileExplorer、Rich Filemanager、REST Shell 等）。
        /// </summary>
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
                throw new System.PlatformNotSupportedException("UI modules require a .NET 8+ ASP.NET Core host with AspNetCoreDashboard 1.0.0-beta-20.");
#endif
            }

            r.Fun();
            return app;
        }

#if NET8_0_OR_GREATER
        private static void MountUiModule(FileManagerOptions options)
        {
            if (RestShellUi.IsShellKind(options.UiModuleKind))
            {
                var theme = RestShellUi.ResolveTheme(options.UiModuleKind, options.RestShellTheme);
                options.ApplicationBuilder.UseUiModule(new RestShellFileManagerUiModule(options.PathMatch.Value, theme));
                return;
            }

            IUiModule module = options.UiModuleKind switch
            {
                "explorer" => new FileExplorerUiModule(options.PathMatch.Value),
                "richfilemanager" => CreateOptionalUiModule("FileManager.RichFilemanagerUiModule, FileManager.UI.RichFilemanager", "FileManager.UI.RichFilemanager", options.PathMatch.Value),
                "elfinder" => CreateOptionalUiModule("FileManager.ElFinderUiModule, FileManager.UI.ElFinder", "FileManager.UI.ElFinder", options.PathMatch.Value),
                "adminlte" => CreateOptionalUiModule("FileManager.AdminLTEFileManagerUiModule, FileManager.UI.AdminLTE", "FileManager.UI.AdminLTE", options.PathMatch.Value),
                "filepicker" => CreateOptionalUiModule("FileManager.FilePickerUiModule, FileManager.UI.FilePicker", "FileManager.UI.FilePicker", options.PathMatch.Value),
                "tinymce" => CreateOptionalUiModule("FileManager.TinyMceFileManagerUiModule, FileManager.UI.TinyMce", "FileManager.UI.TinyMce", options.PathMatch.Value),
                "ckeditor" => CreateOptionalUiModule("FileManager.CkEditorFileManagerUiModule, FileManager.UI.CkEditor", "FileManager.UI.CkEditor", options.PathMatch.Value),
                "pwa" => CreateOptionalUiModule("FileManager.PwaFileManagerUiModule, FileManager.UI.Pwa", "FileManager.UI.Pwa", options.PathMatch.Value),
                _ => throw new System.InvalidOperationException($"Unknown UI module kind: {options.UiModuleKind}")
            };

            options.ApplicationBuilder.UseUiModule(module);
        }

        private static IUiModule CreateOptionalUiModule(string typeName, string assemblyName, string pathPrefix)
        {
            var moduleType = System.Type.GetType(typeName, throwOnError: false);
            if (moduleType == null)
            {
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (string.Equals(assembly.GetName().Name, assemblyName, System.StringComparison.Ordinal))
                    {
                        moduleType = assembly.GetType(typeName.Split(',')[0].Trim(), throwOnError: false);
                        break;
                    }
                }
            }

            if (moduleType == null)
                throw new System.InvalidOperationException(
                    $"Unable to load UI module '{typeName}'. Reference the {assemblyName} project/package and call the matching Use*UI() extension.");

            return (IUiModule)System.Activator.CreateInstance(moduleType, pathPrefix);
        }
#endif

        /// <summary>挂载 ASP.NET Core 内置目录浏览 UI（<c>UseFileServer</c>）。</summary>
        /// <summary>挂载 ASP.NET Core 默认目录浏览 UI。</summary>
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
