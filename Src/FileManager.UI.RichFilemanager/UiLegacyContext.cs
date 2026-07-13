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
        public IServiceProvider Services => _ctx.Services;
    }

    internal sealed class UiLegacyRequest
    {
        private readonly IUiContext _ctx;

        public UiLegacyRequest(IUiContext ctx) => _ctx = ctx;

        public string Method => _ctx.Method;

        public Task<string> GetFormValueAsync(string name) => _ctx.GetFormValueAsync(name);

        public Task<string> GetQueryAsync(string name) => _ctx.GetQueryAsync(name);

        public string GetQuery(string name) => GetQueryAsync(name).GetAwaiter().GetResult();

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

        public Task<IReadOnlyList<IUiFormFile>> GetFilesAsync(string name) =>
            GetFilesAsyncCore(name);

        public Task<IReadOnlyList<string>> GetFormValuesAsync(string name) =>
            GetFormValuesAsyncCore(name);

        private async Task<IReadOnlyList<IUiFormFile>> GetFilesAsyncCore(string name)
        {
            var file = await _ctx.GetFormFileAsync(name);
            return file == null ? Array.Empty<IUiFormFile>() : new[] { file };
        }

        private async Task<IReadOnlyList<string>> GetFormValuesAsyncCore(string name)
        {
            var value = await _ctx.GetFormValueAsync(name);
            return string.IsNullOrEmpty(value) ? Array.Empty<string>() : new[] { value };
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

        public Task WriteAsync(object data) => _ctx.WriteAsync(data?.ToString() ?? string.Empty, _contentType ?? "text/plain");

        public Task WriteAsync(Stream stream) =>
            _ctx.WriteStreamAsync(stream, _contentType ?? "application/octet-stream");

        public Task WriteAsync(byte[] content)
        {
            if (content == null || content.Length == 0)
                return WriteAsync(string.Empty);
            var stream = new MemoryStream(content, writable: false);
            return _ctx.WriteStreamAsync(stream, _contentType ?? "application/octet-stream");
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
    }

#if NET8_0_OR_GREATER
    internal sealed class UiFormFileAdapter : Microsoft.AspNetCore.Http.IFormFile
    {
        private readonly IUiFormFile _file;

        public UiFormFileAdapter(IUiFormFile file) => _file = file;

        public string ContentType => _file.ContentType;
        public string ContentDisposition => null;
        public Microsoft.AspNetCore.Http.IHeaderDictionary Headers => new Microsoft.AspNetCore.Http.HeaderDictionary();
        public long Length => -1;
        public string Name => "files";
        public string FileName => _file.FileName;

        public async Task CopyToAsync(Stream target, CancellationToken cancellationToken = default)
        {
            await using var stream = await _file.OpenReadStreamAsync();
            await stream.CopyToAsync(target, 81920, cancellationToken);
        }

        public async Task CopyTo(Stream target)
        {
            await using var stream = await _file.OpenReadStreamAsync();
            await stream.CopyToAsync(target);
        }

        public async Task<Stream> OpenReadStreamAsync() => await _file.OpenReadStreamAsync();

        public Stream OpenReadStream() => OpenReadStreamAsync().GetAwaiter().GetResult();
    }
#endif
}
