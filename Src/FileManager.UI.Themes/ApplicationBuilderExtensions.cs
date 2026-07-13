using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class ThemesApplicationBuilderExtensions
    {
        /// <summary>挂载主题切换演示 UI。</summary>
        public static void UseThemesFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "themes";
        }

        internal static void ConfigureThemesRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Themes");
        }

        private static Assembly GetExecutingAssembly() => typeof(ThemesApplicationBuilderExtensions).Assembly;
    }

    public sealed class ThemesFileManagerUiModule : IUiModule
    {
        public ThemesFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => ThemesApplicationBuilderExtensions.ConfigureThemesRoutes(builder);
    }
}
