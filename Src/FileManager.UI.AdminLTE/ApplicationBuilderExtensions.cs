using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class AdminLTEApplicationBuilderExtensions
    {
        /// <summary>挂载 AdminLTE 风格文件管理 UI。</summary>
        public static void UseAdminLTEFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "adminlte";
        }

        internal static void ConfigureAdminLTERoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "AdminLTE");
        }

        private static Assembly GetExecutingAssembly() => typeof(AdminLTEApplicationBuilderExtensions).Assembly;
    }

    public sealed class AdminLTEFileManagerUiModule : IUiModule
    {
        public AdminLTEFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => AdminLTEApplicationBuilderExtensions.ConfigureAdminLTERoutes(builder);
    }
}
