using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class TinyMceApplicationBuilderExtensions
    {
        /// <summary>挂载 TinyMCE 编辑器文件选择 UI。</summary>
        public static void UseTinyMceFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "tinymce";
        }

        internal static void ConfigureTinyMceRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "TinyMce");
        }

        private static Assembly GetExecutingAssembly() => typeof(TinyMceApplicationBuilderExtensions).Assembly;
    }

    public sealed class TinyMceFileManagerUiModule : IUiModule
    {
        public TinyMceFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => TinyMceApplicationBuilderExtensions.ConfigureTinyMceRoutes(builder);
    }
}
