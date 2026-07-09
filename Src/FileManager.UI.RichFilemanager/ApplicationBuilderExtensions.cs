using AspNetCoreDashboard.Abstractions;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.Extensions.DependencyInjection;
#endif

namespace FileManager
{
    public static class RichFilemanagerApplicationBuilderExtensions
    {
        public static void UseRichFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "richfilemanager";
        }

        internal static void ConfigureRichFilemanagerRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            var contentNs = GetContentFolderNamespace("RichFilemanager");

            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapEmbeddedUi(assembly, contentNs)
                   .MapFallbackToIndex(GetContentResourceName("RichFilemanager", "index.html"));

            builder.MapCommand("/connectors/php/filemanager.php", context =>
            {
                var mode = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("mode")?.Result?.FirstOrDefault() : context.Request.GetQuery("mode");
                var path = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("path")?.Result?.FirstOrDefault() : context.Request.GetQuery("path");
                var name = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("name")?.Result?.FirstOrDefault() : context.Request.GetQuery("name");
                var old = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("old")?.Result?.FirstOrDefault() : context.Request.GetQuery("old");
                var @new = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("new")?.Result?.FirstOrDefault() : context.Request.GetQuery("new");
                var source = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("source")?.Result?.FirstOrDefault() : context.Request.GetQuery("source");
                var target = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("target")?.Result?.FirstOrDefault() : context.Request.GetQuery("target");
                var content = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("content")?.Result?.FirstOrDefault() : context.Request.GetQuery("content");
                var thumbnail = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("thumbnail")?.Result?.FirstOrDefault() : context.Request.GetQuery("thumbnail");
                var @string = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("string")?.Result?.FirstOrDefault() : context.Request.GetQuery("string");
#if NET8_0_OR_GREATER
                IReadOnlyList<IUiFormFile> uiFiles = context.Request.Method == "POST" ? context.Request.GetFilesAsync("files")?.Result : null;
                IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files = uiFiles?.Select(f => new UiFormFileAdapter(f)).ToList();
#else
                IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files = null;
#endif
                bool.TryParse(thumbnail, out var _thumbnail);
                var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                var controller = new FileManagerController(fileStore);

                var r = controller.Index(mode, path, name, files,
                    old, @new, source, target, content, _thumbnail, @string);

                context.Response.ContentType = r.ContentType;
                context.Response.StatusCode = r.StatusCode;
                if (r.ContentDisposition != null)
                    context.Response.SetHeader("Content-Disposition", r.ContentDisposition.ToString());

                if (r is JsonResult jsonResult)
                    return context.Response.WriteAsync(jsonResult.Data);
                if (r is FileResult fileResult)
                    return context.Response.WriteAsync(fileResult.FileBytes);
                return Task.CompletedTask;
            });
        }

        private static System.Reflection.Assembly GetExecutingAssembly()
        {
            return typeof(FileManagerController).GetTypeInfo().Assembly;
        }

        private static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return $"{GetContentFolderNamespace(contentFolder)}.{resourceName}";
        }

        private static string GetContentFolderNamespace(string contentFolder)
        {
            var assemblyName = GetExecutingAssembly().GetName().Name;
            return $"{assemblyName}.Content.{contentFolder}";
        }
    }
}
