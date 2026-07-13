using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class Bootstrap5ApplicationBuilderExtensions
    {
        /// <summary>挂载 Bootstrap 5 风格 UI。</summary>
        public static void UseBootstrap5FileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "bootstrap5";

        internal static void ConfigureBootstrap5Routes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Bootstrap5");
        }

        private static Assembly GetExecutingAssembly() => typeof(Bootstrap5ApplicationBuilderExtensions).Assembly;
    }

    public sealed class Bootstrap5FileManagerUiModule : IUiModule
    {
        public Bootstrap5FileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => Bootstrap5ApplicationBuilderExtensions.ConfigureBootstrap5Routes(builder);
    }
}
