using AspNetCoreDashboard.Abstractions;
using Newtonsoft.Json;
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
        public IServiceProvider Services => _ctx.Services ?? FileManagerOwinServices.Provider;
        public System.Text.RegularExpressions.Match UriMatch => _ctx.RouteMatch;
    }

    internal sealed class UiLegacyRequest
    {
        private readonly IUiContext _ctx;

        public UiLegacyRequest(IUiContext ctx) => _ctx = ctx;

        public string Method => _ctx.Method;

        public Task<string> GetFormValueAsync(string name) => _ctx.GetFormValueAsync(name);

        public string GetQuery(string name) => _ctx.GetQueryAsync(name).GetAwaiter().GetResult();

        public async Task<T> GetBodyModelBinderAsync<T>()
        {
            using (var stream = await _ctx.OpenRequestBodyAsync())
            using (var reader = new StreamReader(stream))
            {
                var json = await reader.ReadToEndAsync();
                return string.IsNullOrWhiteSpace(json)
                    ? default
                    : JsonConvert.DeserializeObject<T>(json);
            }
        }

        public Stream Body => _ctx.OpenRequestBodyAsync().GetAwaiter().GetResult();

        public string GetHeader(string name) => _ctx.GetRequestHeader(name);

        public Task<IReadOnlyList<IUiFormFile>> GetFilesAsync(string name)
        {
            return GetFilesAsyncCore(name);
        }

        public Task<IReadOnlyList<string>> GetFormValuesAsync(string name)
        {
            return GetFormValuesAsyncCore(name);
        }

        private async Task<IReadOnlyList<IUiFormFile>> GetFilesAsyncCore(string name)
        {
            var file = await _ctx.GetFormFileAsync(name);
            if (file == null)
                return Array.Empty<IUiFormFile>();
            return new[] { file };
        }

        private async Task<IReadOnlyList<string>> GetFormValuesAsyncCore(string name)
        {
            var value = await _ctx.GetFormValueAsync(name);
            if (string.IsNullOrEmpty(value))
                return Array.Empty<string>();
            return new[] { value };
        }
    }

    internal sealed class UiLegacyResponse
    {
        private readonly IUiContext _ctx;
        private string _contentType;

        public UiLegacyResponse(IUiContext ctx) => _ctx = ctx;

        public string ContentType
        {
            get => _contentType;
            set => _contentType = value;
        }

        public int StatusCode
        {
            get => _ctx.StatusCode;
            set => _ctx.StatusCode = value;
        }

        public Task JsonAsync(object data) => _ctx.WriteJsonAsync(data);

        public Task WriteAsync(string content) =>
            _ctx.WriteAsync(content ?? string.Empty, _contentType ?? "text/plain");

        public Task WriteAsync(byte[] content)
        {
            if (content == null || content.Length == 0)
                return WriteAsync(string.Empty);

            var stream = new MemoryStream(content, writable: false);
            return _ctx.WriteStreamAsync(stream, _contentType ?? "application/octet-stream");
        }

        public Task WriteAsync(Stream stream) =>
            _ctx.WriteStreamAsync(stream, _contentType ?? "application/octet-stream");

        public Task WriteAsync(object data) =>
            WriteAsync(data?.ToString() ?? string.Empty);

        public Task FileAsync(Stream fileStream, string contentType, string fileDownloadName = null)
        {
            _contentType = contentType;
            if (!string.IsNullOrWhiteSpace(fileDownloadName))
                _ctx.SetResponseHeader("Content-Disposition", "attachment;filename=" + Uri.EscapeDataString(fileDownloadName));
            return _ctx.WriteStreamAsync(fileStream, contentType);
        }

        public Task FileAsync(byte[] fileContents, string contentType, string fileDownloadName)
        {
            _contentType = contentType;
            if (!string.IsNullOrWhiteSpace(fileDownloadName))
                _ctx.SetResponseHeader("Content-Disposition", "attachment;filename=" + Uri.EscapeDataString(fileDownloadName));
            var stream = new MemoryStream(fileContents ?? Array.Empty<byte>(), writable: false);
            return _ctx.WriteStreamAsync(stream, contentType);
        }

        public void SetHeader(string name, string value) => _ctx.SetResponseHeader(name, value);
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

        public static IUiModuleRegistration MapCommand(this IUiModuleRegistration registration, string[] patterns, Func<UiLegacyContext, Task> handler)
        {
            foreach (var pattern in patterns)
                registration.MapCommand(pattern, handler);
            return registration;
        }
    }

    internal static class AssemblyResourceExtensions
    {
        public static Stream GetManifestResourceStreamIgnoreCase(this System.Reflection.Assembly assembly, string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
                return stream;

            var match = assembly.GetManifestResourceNames()
                .FirstOrDefault(n => string.Equals(n, resourceName, StringComparison.OrdinalIgnoreCase));
            return match == null ? null : assembly.GetManifestResourceStream(match);
        }
    }
}
