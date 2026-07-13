using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class NextJsApplicationBuilderExtensions
    {
        /// <summary>挂载 Next.js REST UI。</summary>
        public static void UseNextJsFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "nextjs";
        }

        internal static void ConfigureNextJsRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "NextJs");
        }

        private static Assembly GetExecutingAssembly() => typeof(NextJsApplicationBuilderExtensions).Assembly;
    }

    public sealed class NextJsFileManagerUiModule : IUiModule
    {
        public NextJsFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => NextJsApplicationBuilderExtensions.ConfigureNextJsRoutes(builder);
    }
}
