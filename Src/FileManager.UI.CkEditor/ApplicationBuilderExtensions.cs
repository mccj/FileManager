using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class CkEditorApplicationBuilderExtensions
    {
        /// <summary>挂载 CKEditor 5 文件选择 UI。</summary>
        public static void UseCkEditorFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "ckeditor";

        internal static void ConfigureCkEditorRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "CkEditor");
        }

        private static Assembly GetExecutingAssembly() => typeof(CkEditorApplicationBuilderExtensions).Assembly;
    }

    public sealed class CkEditorFileManagerUiModule : IUiModule
    {
        public CkEditorFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => CkEditorApplicationBuilderExtensions.ConfigureCkEditorRoutes(builder);
    }
}
