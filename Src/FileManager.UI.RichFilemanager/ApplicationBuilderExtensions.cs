using AspNetCoreDashboard.Abstractions;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.Extensions.DependencyInjection;
#endif

namespace FileManager
{
    public static class RichFilemanagerApplicationBuilderExtensions
    {
        /// <summary>挂载 Rich Filemanager UI。</summary>
        public static void UseRichFileManagerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "richfilemanager";
        }

        internal static void ConfigureRichFilemanagerRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            var contentNs = GetContentFolderNamespace("RichFilemanager");

            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapCommand("/connectors/php/filemanager.php", HandleConnectorAsync)
                   .MapEmbeddedUi(assembly, contentNs)
                   .MapFallbackToIndex(GetContentResourceName("RichFilemanager", "index.html"));
        }

        private static async Task HandleConnectorAsync(UiLegacyContext context)
        {
            var request = context.Request;
            var cancellationToken = CancellationToken.None;
            var mode = request.Method == "POST"
                ? (await request.GetFormValuesAsync("mode"))?.FirstOrDefault()
                : await request.GetQueryAsync("mode");
            var path = request.Method == "POST"
                ? (await request.GetFormValuesAsync("path"))?.FirstOrDefault()
                : await request.GetQueryAsync("path");
            var name = request.Method == "POST"
                ? (await request.GetFormValuesAsync("name"))?.FirstOrDefault()
                : await request.GetQueryAsync("name");
            var old = request.Method == "POST"
                ? (await request.GetFormValuesAsync("old"))?.FirstOrDefault()
                : await request.GetQueryAsync("old");
            var @new = request.Method == "POST"
                ? (await request.GetFormValuesAsync("new"))?.FirstOrDefault()
                : await request.GetQueryAsync("new");
            var source = request.Method == "POST"
                ? (await request.GetFormValuesAsync("source"))?.FirstOrDefault()
                : await request.GetQueryAsync("source");
            var target = request.Method == "POST"
                ? (await request.GetFormValuesAsync("target"))?.FirstOrDefault()
                : await request.GetQueryAsync("target");
            var content = request.Method == "POST"
                ? (await request.GetFormValuesAsync("content"))?.FirstOrDefault()
                : await request.GetQueryAsync("content");
            var thumbnail = request.Method == "POST"
                ? (await request.GetFormValuesAsync("thumbnail"))?.FirstOrDefault()
                : await request.GetQueryAsync("thumbnail");
            var @string = request.Method == "POST"
                ? (await request.GetFormValuesAsync("string"))?.FirstOrDefault()
                : await request.GetQueryAsync("string");
#if NET8_0_OR_GREATER
            IReadOnlyList<IUiFormFile> uiFiles = request.Method == "POST" ? await request.GetFilesAsync("files") : null;
            IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files = uiFiles?.Select(f => new UiFormFileAdapter(f)).ToList();
#else
            IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files = null;
#endif
            bool.TryParse(thumbnail, out var thumbnailValue);
            var fileStore = context.Services.GetService<IFileStore>();
            var permissions = context.Services.GetService<DefaultPermissionHandle>();
            var controller = new FileManagerController(fileStore, permissions);

            var result = await controller.IndexAsync(
                mode, path, name, files, old, @new, source, target, content, thumbnailValue, @string, cancellationToken);

            context.Response.ContentType = result.ContentType;
            context.Response.StatusCode = result.StatusCode;
            if (result.ContentDisposition != null)
                context.Response.SetHeader("Content-Disposition", result.ContentDisposition.ToString());

            if (result is JsonResult jsonResult)
            {
                await context.Response.WriteAsync(jsonResult.Data);
                return;
            }

            if (result is FileResult fileResult)
            {
                if (fileResult.FileStream != null)
                    await context.Response.WriteAsync(fileResult.FileStream);
                else
                    await context.Response.WriteAsync(fileResult.FileBytes);
            }
        }

        private static Assembly GetExecutingAssembly()
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
