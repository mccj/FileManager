using AspNetCoreDashboard.Abstractions;
using FileManager.FileStorage;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.Extensions.DependencyInjection;
#endif

namespace FileManager.UI.Common
{
    public static class RestFileApiRoutes
    {
        public static IUiModuleRegistration MapRestFileApi(this IUiModuleRegistration builder)
        {
            return builder
                .MapCommand("/api/entries", HandleListAsync)
                .MapCommand("/api/download", HandleDownloadAsync)
                .MapCommand("/api/upload", HandleUploadAsync)
                .MapCommand("/api/mkdir", HandleMkdirAsync)
                .MapCommand("/api/delete", HandleDeleteAsync)
                .MapCommand("/api/rename", HandleRenameAsync);
        }

        public static IUiModuleRegistration MapEmbeddedUiContent(
            this IUiModuleRegistration builder,
            Assembly assembly,
            string contentFolderName,
            string indexFileName = "index.html")
        {
            var contentNs = $"{assembly.GetName().Name}.Content.{contentFolderName}";
            return builder
                .MapEmbeddedUi(assembly, contentNs)
                .MapFallbackToIndex($"{contentNs}.{indexFileName}");
        }

        public static IUiModuleRegistration MapShellConfig(this IUiModuleRegistration builder, object config)
        {
            return builder.MapCommand("/shell-config.json", async context =>
            {
                await context.Response.JsonAsync(config);
            });
        }

        private static async Task HandleListAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var api = CreateApi(context);
            var result = await api.ListAsync(path, CancellationToken.None);
            await context.Response.JsonAsync(result);
        }

        private static async Task HandleDownloadAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var api = CreateApi(context);
            var download = await api.DownloadAsync(path, CancellationToken.None);
            await context.Response.FileAsync(download.Stream, "application/octet-stream", Path.GetFileName(download.Path));
        }

        private static async Task HandleUploadAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var files = await context.Request.GetFilesAsync("file");
            var file = files.FirstOrDefault();
            if (file == null)
                throw new FileStoreException("Upload file is required.");

            var api = CreateApi(context);
            await using var stream = await file.OpenReadStreamAsync();
            var result = await api.UploadAsync(path, file.FileName, stream, CancellationToken.None);
            await context.Response.JsonAsync(result);
        }

        private static async Task HandleMkdirAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var api = CreateApi(context);
            var result = await api.MkdirAsync(path, CancellationToken.None);
            await context.Response.JsonAsync(result);
        }

        private static async Task HandleDeleteAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var api = CreateApi(context);
            var result = await api.DeleteAsync(path, CancellationToken.None);
            await context.Response.JsonAsync(result);
        }

        private static async Task HandleRenameAsync(UiLegacyContext context)
        {
            var path = await context.Request.GetQueryAsync("path");
            var name = await context.Request.GetQueryAsync("name");
            var api = CreateApi(context);
            var result = await api.RenameAsync(path, name, CancellationToken.None);
            await context.Response.JsonAsync(result);
        }

        private static RestFileApi CreateApi(UiLegacyContext context)
        {
            var fileStore = context.Services.GetService<IFileStore>();
            var permissions = context.Services.GetService<DefaultPermissionHandle>();
            return new RestFileApi(fileStore, permissions);
        }
    }
}
