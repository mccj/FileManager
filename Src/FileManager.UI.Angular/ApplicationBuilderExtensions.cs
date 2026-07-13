using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class AngularApplicationBuilderExtensions
    {
        /// <summary>挂载 Angular REST 文件管理 UI。</summary>
        public static void UseAngularFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "angular";
        }

        internal static void ConfigureAngularRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Angular");
        }

        private static Assembly GetExecutingAssembly() => typeof(AngularApplicationBuilderExtensions).Assembly;
    }

    public sealed class AngularFileManagerUiModule : IUiModule
    {
        public AngularFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => AngularApplicationBuilderExtensions.ConfigureAngularRoutes(builder);
    }
}
