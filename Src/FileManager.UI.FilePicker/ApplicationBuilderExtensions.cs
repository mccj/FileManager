using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class FilePickerApplicationBuilderExtensions
    {
        /// <summary>挂载文件选择器 UI。</summary>
        public static void UseFilePickerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "filepicker";
        }

        internal static void ConfigureFilePickerRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "FilePicker", "picker.html");
        }

        private static Assembly GetExecutingAssembly() => typeof(FilePickerApplicationBuilderExtensions).Assembly;
    }

    public sealed class FilePickerUiModule : IUiModule
    {
        public FilePickerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => FilePickerApplicationBuilderExtensions.ConfigureFilePickerRoutes(builder);
    }
}
