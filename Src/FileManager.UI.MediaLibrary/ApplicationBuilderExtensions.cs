using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class MediaLibraryApplicationBuilderExtensions
    {
        /// <summary>挂载媒体库风格 UI。</summary>
        public static void UseMediaLibraryFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "medialibrary";
        }

        internal static void ConfigureMediaLibraryRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "MediaLibrary");
        }

        private static Assembly GetExecutingAssembly() => typeof(MediaLibraryApplicationBuilderExtensions).Assembly;
    }

    public sealed class MediaLibraryFileManagerUiModule : IUiModule
    {
        public MediaLibraryFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => MediaLibraryApplicationBuilderExtensions.ConfigureMediaLibraryRoutes(builder);
    }
}
