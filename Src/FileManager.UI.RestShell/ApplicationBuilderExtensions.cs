using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class RestShellApplicationBuilderExtensions
    {
        /// <summary>挂载 REST Shell 通用前端 UI。</summary>
        public static void UseRestShellFileManagerUI(this FileManagerOptions app, string theme = "default")
        {
            app.UiModuleKind = "restshell";
            app.RestShellTheme = theme;
        }

        internal static void ConfigureRestShellRoutes(IUiModuleRegistration builder, string theme)
        {
            var assembly = GetExecutingAssembly();
            var themeInfo = RestShellThemes.Resolve(theme);

            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapShellConfig(new
                   {
                       theme,
                       title = themeInfo.Title,
                       accentColor = themeInfo.AccentColor,
                       features = themeInfo.Features
                   })
                   .MapEmbeddedUiContent(assembly, "RestShell");
        }

        private static Assembly GetExecutingAssembly() => typeof(RestShellApplicationBuilderExtensions).Assembly;
    }

    public sealed class RestShellFileManagerUiModule : IUiModule
    {
        public RestShellFileManagerUiModule(string pathPrefix, string theme)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
            Theme = string.IsNullOrWhiteSpace(theme) ? "default" : theme;
        }

        public string PathPrefix { get; }
        public string Theme { get; }

        public void Configure(IUiModuleRegistration builder) => RestShellApplicationBuilderExtensions.ConfigureRestShellRoutes(builder, Theme);
    }
}
