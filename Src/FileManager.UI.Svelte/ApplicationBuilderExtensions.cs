using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class SvelteApplicationBuilderExtensions
    {
        /// <summary>挂载 Svelte REST UI。</summary>
        public static void UseSvelteFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "svelte";
        }

        internal static void ConfigureSvelteRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Svelte");
        }

        private static Assembly GetExecutingAssembly() => typeof(SvelteApplicationBuilderExtensions).Assembly;
    }

    public sealed class SvelteFileManagerUiModule : IUiModule
    {
        public SvelteFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => SvelteApplicationBuilderExtensions.ConfigureSvelteRoutes(builder);
    }
}
