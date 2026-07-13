using AspNetCoreDashboard.Abstractions;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.Extensions.DependencyInjection;
#endif

namespace FileManager
{
    public static class ElFinderApplicationBuilderExtensions
    {
        /// <summary>挂载 elFinder UI。</summary>
        public static void UseElFinderUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "elfinder";
        }

        internal static void ConfigureElFinderRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            var contentNs = GetContentFolderNamespace("ElFinder");

            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapCommand("/connector", HandleConnectorAsync)
                   .MapEmbeddedUi(assembly, contentNs)
                   .MapFallbackToIndex(GetContentResourceName("ElFinder", "index.html"));
        }

        private static async Task HandleConnectorAsync(UiLegacyContext context)
        {
            var request = context.Request;
            var cancellationToken = CancellationToken.None;
            var cmd = request.Method == "POST"
                ? (await request.GetFormValuesAsync("cmd"))?.FirstOrDefault()
                : await request.GetQueryAsync("cmd");
            var target = request.Method == "POST"
                ? (await request.GetFormValuesAsync("target"))?.FirstOrDefault()
                : await request.GetQueryAsync("target");
            var name = request.Method == "POST"
                ? (await request.GetFormValuesAsync("name"))?.FirstOrDefault()
                : await request.GetQueryAsync("name");
            var sources = request.Method == "POST"
                ? (await request.GetFormValuesAsync("targets"))?.FirstOrDefault()
                : await request.GetQueryAsync("targets");
            var dst = request.Method == "POST"
                ? (await request.GetFormValuesAsync("dst"))?.FirstOrDefault()
                : await request.GetQueryAsync("dst");
            var content = request.Method == "POST"
                ? (await request.GetFormValuesAsync("content"))?.FirstOrDefault()
                : await request.GetQueryAsync("content");
            var cutValue = request.Method == "POST"
                ? (await request.GetFormValuesAsync("cut"))?.FirstOrDefault()
                : await request.GetQueryAsync("cut");
            var cut = string.Equals(cutValue, "1", StringComparison.Ordinal) || string.Equals(cutValue, "true", StringComparison.OrdinalIgnoreCase);
            var initValue = request.Method == "POST"
                ? (await request.GetFormValuesAsync("init"))?.FirstOrDefault()
                : await request.GetQueryAsync("init");
            var init = string.Equals(initValue, "1", StringComparison.Ordinal) || string.Equals(initValue, "true", StringComparison.OrdinalIgnoreCase);
            var treeValue = request.Method == "POST"
                ? (await request.GetFormValuesAsync("tree"))?.FirstOrDefault()
                : await request.GetQueryAsync("tree");
            var tree = string.Equals(treeValue, "1", StringComparison.Ordinal) || string.Equals(treeValue, "true", StringComparison.OrdinalIgnoreCase);

            IReadOnlyList<IUiFormFile> uiFiles = request.Method == "POST" ? await request.GetFilesAsync("upload[]") : null;
            if (uiFiles == null || uiFiles.Count == 0)
                uiFiles = request.Method == "POST" ? await request.GetFilesAsync("upload") : null;
            var files = uiFiles?.ToList();

            var fileStore = context.Services.GetService<IFileStore>();
            var permissions = context.Services.GetService<DefaultPermissionHandle>();
            var guard = new FileStorePermissionGuard(fileStore, permissions);
            var connector = new ElFinderConnector(fileStore, guard);
            var uploadStreams = new List<Stream>();
            var uploadNames = new List<string>();
            if (files != null)
            {
                foreach (var file in files)
                {
                    uploadStreams.Add(await file.OpenReadStreamAsync());
                    uploadNames.Add(file.FileName);
                }
            }

            var result = await connector.ExecuteAsync(
                cmd,
                target,
                name,
                oldName: null,
                newName: name,
                source: sources,
                targetPath: dst,
                content,
                uploadStreams,
                uploadNames,
                cut,
                init,
                tree,
                cancellationToken);

            if (result is ElFinderFileDownload download)
            {
                await context.Response.FileAsync(
                    download.Stream,
                    "application/octet-stream",
                    Path.GetFileName(download.Path));
                return;
            }

            await context.Response.JsonAsync(connector.Serialize(result), "application/json; charset=utf-8");
        }

        private static Assembly GetExecutingAssembly() => typeof(ElFinderApplicationBuilderExtensions).Assembly;

        private static string GetContentFolderNamespace(string folderName) =>
            $"{GetExecutingAssembly().GetName().Name}.Content.{folderName}";

        private static string GetContentResourceName(string folderName, string fileName) =>
            $"{GetContentFolderNamespace(folderName)}.{fileName}";
    }
}
