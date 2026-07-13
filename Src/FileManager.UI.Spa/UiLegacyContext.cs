using AspNetCoreDashboard.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileManager
{
    internal sealed class UiLegacyContext
    {
        private readonly IUiContext _ctx;

        public UiLegacyContext(IUiContext ctx)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            Request = new UiLegacyRequest(ctx);
            Response = new UiLegacyResponse(ctx);
        }

        public UiLegacyRequest Request { get; }
        public UiLegacyResponse Response { get; }
        public IServiceProvider Services => _ctx.Services;
    }

    internal sealed class UiLegacyRequest
    {
        private readonly IUiContext _ctx;

        public UiLegacyRequest(IUiContext ctx) => _ctx = ctx;

        public string Method => _ctx.Method;

        public Task<string> GetQueryAsync(string name) => _ctx.GetQueryAsync(name);

        public Task<IReadOnlyList<IUiFormFile>> GetFilesAsync(string name) =>
            GetFilesAsyncCore(name);

        private async Task<IReadOnlyList<IUiFormFile>> GetFilesAsyncCore(string name)
        {
            var file = await _ctx.GetFormFileAsync(name);
            return file == null ? Array.Empty<IUiFormFile>() : new[] { file };
        }
    }

    internal sealed class UiLegacyResponse
    {
        private readonly IUiContext _ctx;

        public UiLegacyResponse(IUiContext ctx) => _ctx = ctx;

        public Task JsonAsync(object data) => _ctx.WriteJsonAsync(data);

        public Task FileAsync(Stream stream, string contentType, string fileDownloadName = null)
        {
            if (!string.IsNullOrWhiteSpace(fileDownloadName))
                _ctx.SetResponseHeader("Content-Disposition", "attachment;filename=" + Uri.EscapeDataString(fileDownloadName));
            return _ctx.WriteStreamAsync(stream, contentType);
        }
    }

    internal static class UiModuleRegistrationExtensions
    {
        public static IUiModuleRegistration MapCommand(this IUiModuleRegistration registration, string pattern, Func<UiLegacyContext, Task> handler)
        {
            Task Handler(IUiContext ctx) => handler(new UiLegacyContext(ctx));
            registration.MapGet(pattern, Handler);
            registration.MapPost(pattern, Handler);
            return registration;
        }
    }
}
