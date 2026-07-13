using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class MobileApplicationBuilderExtensions
    {
        /// <summary>挂载移动端响应式 UI。</summary>
        public static void UseMobileFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "mobile";
        }

        internal static void ConfigureMobileRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "Mobile");
        }

        private static Assembly GetExecutingAssembly() => typeof(MobileApplicationBuilderExtensions).Assembly;
    }

    public sealed class MobileFileManagerUiModule : IUiModule
    {
        public MobileFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => MobileApplicationBuilderExtensions.ConfigureMobileRoutes(builder);
    }
}
