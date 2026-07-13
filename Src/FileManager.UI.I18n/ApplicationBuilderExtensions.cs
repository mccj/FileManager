using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class I18nApplicationBuilderExtensions
    {
        /// <summary>挂载多语言演示 UI。</summary>
        public static void UseI18nFileManagerUI(this FileManagerOptions app) => app.UiModuleKind = "i18n";

        internal static void ConfigureI18nRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "I18n");
        }

        private static Assembly GetExecutingAssembly() => typeof(I18nApplicationBuilderExtensions).Assembly;
    }

    public sealed class I18nFileManagerUiModule : IUiModule
    {
        public I18nFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix)) throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }
        public string PathPrefix { get; }
        public void Configure(IUiModuleRegistration builder) => I18nApplicationBuilderExtensions.ConfigureI18nRoutes(builder);
    }
}
