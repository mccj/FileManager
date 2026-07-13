using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class SpaApplicationBuilderExtensions
    {
        /// <summary>挂载 SPA REST 文件管理 UI。</summary>
        public static void UseSpaFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "spa";
        }

        internal static void ConfigureSpaRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Spa");
        }

        private static Assembly GetExecutingAssembly() => typeof(SpaApplicationBuilderExtensions).Assembly;
    }
}
