using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class ReactApplicationBuilderExtensions
    {
        /// <summary>挂载 React REST 文件管理 UI。</summary>
        public static void UseReactFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "react";
        }

        internal static void ConfigureReactRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "React");
        }

        private static Assembly GetExecutingAssembly() => typeof(ReactApplicationBuilderExtensions).Assembly;
    }

    public sealed class ReactFileManagerUiModule : IUiModule
    {
        public ReactFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => ReactApplicationBuilderExtensions.ConfigureReactRoutes(builder);
    }
}
