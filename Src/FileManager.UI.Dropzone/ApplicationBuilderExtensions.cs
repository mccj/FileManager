using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class DropzoneApplicationBuilderExtensions
    {
        /// <summary>挂载 Dropzone 拖拽上传 UI。</summary>
        public static void UseDropzoneFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "dropzone";
        }

        internal static void ConfigureDropzoneRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Dropzone");
        }

        private static Assembly GetExecutingAssembly() => typeof(DropzoneApplicationBuilderExtensions).Assembly;
    }

    public sealed class DropzoneFileManagerUiModule : IUiModule
    {
        public DropzoneFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => DropzoneApplicationBuilderExtensions.ConfigureDropzoneRoutes(builder);
    }
}
