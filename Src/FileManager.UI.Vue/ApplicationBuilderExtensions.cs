using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class VueApplicationBuilderExtensions
    {
        /// <summary>挂载 Vue REST 文件管理 UI。</summary>
        public static void UseVueFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "vue";
        }

        internal static void ConfigureVueRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Vue");
        }

        private static Assembly GetExecutingAssembly() => typeof(VueApplicationBuilderExtensions).Assembly;
    }

    public sealed class VueFileManagerUiModule : IUiModule
    {
        public VueFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => VueApplicationBuilderExtensions.ConfigureVueRoutes(builder);
    }
}
