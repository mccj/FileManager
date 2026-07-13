using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class PwaApplicationBuilderExtensions
    {
        /// <summary>挂载 PWA 离线友好文件管理 UI。</summary>
        public static void UsePwaFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "pwa";

        internal static void ConfigurePwaRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Pwa");
        }

        private static Assembly GetExecutingAssembly() => typeof(PwaApplicationBuilderExtensions).Assembly;
    }

    public sealed class PwaFileManagerUiModule : IUiModule
    {
        public PwaFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => PwaApplicationBuilderExtensions.ConfigurePwaRoutes(builder);
    }
}
