using AspNetCoreDashboard.Abstractions;
using FileManager.UI.Common;
using System;
using System.Reflection;

namespace FileManager
{
    public static class SdkDemoApplicationBuilderExtensions
    {
        /// <summary>挂载 SDK 演示 UI。</summary>
        public static void UseSdkDemoFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "sdkdemo";
        }

        internal static void ConfigureSdkDemoRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapRestFileApi()
                   .MapEmbeddedUiContent(assembly, "SdkDemo");
        }

        private static Assembly GetExecutingAssembly() => typeof(SdkDemoApplicationBuilderExtensions).Assembly;
    }

    public sealed class SdkDemoFileManagerUiModule : IUiModule
    {
        public SdkDemoFileManagerUiModule(string pathPrefix)
        {
            if (string.IsNullOrWhiteSpace(pathPrefix))
                throw new ArgumentNullException(nameof(pathPrefix));
            PathPrefix = pathPrefix.StartsWith("/", StringComparison.Ordinal) ? pathPrefix : "/" + pathPrefix;
        }

        public string PathPrefix { get; }

        public void Configure(IUiModuleRegistration builder) => SdkDemoApplicationBuilderExtensions.ConfigureSdkDemoRoutes(builder);
    }
}
