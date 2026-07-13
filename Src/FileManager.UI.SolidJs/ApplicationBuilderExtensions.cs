using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class SolidJsApplicationBuilderExtensions
    {
        /// <summary>挂载 Solid.js REST UI。</summary>
        public static void UseSolidJsFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "solidjs";

        internal static void ConfigureSolidJsRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "SolidJs");
        }

        private static Assembly GetExecutingAssembly() => typeof(SolidJsApplicationBuilderExtensions).Assembly;
    }

    public sealed class SolidJsFileManagerUiModule : IUiModule
    {
        public SolidJsFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => SolidJsApplicationBuilderExtensions.ConfigureSolidJsRoutes(builder);
    }
}
