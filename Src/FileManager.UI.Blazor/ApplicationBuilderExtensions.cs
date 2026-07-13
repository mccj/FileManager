using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class BlazorApplicationBuilderExtensions
    {
        /// <summary>挂载 Blazor REST 文件管理 UI。</summary>
        public static void UseBlazorFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "blazor";
        }

        internal static void ConfigureBlazorRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Blazor");
        }

        private static Assembly GetExecutingAssembly() => typeof(BlazorApplicationBuilderExtensions).Assembly;
    }

    public sealed class BlazorFileManagerUiModule : IUiModule
    {
        public BlazorFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => BlazorApplicationBuilderExtensions.ConfigureBlazorRoutes(builder);
    }
}
