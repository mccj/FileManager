using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class PreactApplicationBuilderExtensions
    {
        /// <summary>挂载 Preact REST UI。</summary>
        public static void UsePreactFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "preact";

        internal static void ConfigurePreactRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Preact");
        }

        private static Assembly GetExecutingAssembly() => typeof(PreactApplicationBuilderExtensions).Assembly;
    }

    public sealed class PreactFileManagerUiModule : IUiModule
    {
        public PreactFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => PreactApplicationBuilderExtensions.ConfigurePreactRoutes(builder);
    }
}
