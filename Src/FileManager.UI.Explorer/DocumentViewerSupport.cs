using Microsoft.Extensions.Caching.Memory;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileManager
{
    internal static class DocumentViewerSupport
    {
        private const string ErrorCacheKeyPrefix = "DocumentViewerError:";

        public static string GetDocumentType(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.TrimStart('.').ToLowerInvariant();
            if (string.IsNullOrEmpty(ext))
                return "office";

            if (ext == "pdf")
                return "pdf";

            if (ext == "xpz" || ext == "xps" || ext == "oxps")
                return "xpz";

            return "office";
        }

        public static long GetDocumentVersion(DocumentViewerCacheInfo cacheInfo)
        {
            if (cacheInfo == null)
                return 0;

            return cacheInfo.FileLength ^ cacheInfo.LastModifiedUtc.UtcTicks;
        }

        public static string GetFilePath(DocumentViewerCacheInfo cacheInfo)
        {
            return string.IsNullOrEmpty(cacheInfo.Path)
                ? cacheInfo.FileName
                : cacheInfo.Path + "/" + cacheInfo.FileName;
        }

        public static void SetLastError(IMemoryCache memoryCache, string stateId, string message)
        {
            if (memoryCache == null || string.IsNullOrWhiteSpace(stateId))
                return;

            memoryCache.Set(ErrorCacheKeyPrefix + stateId, message ?? string.Empty, TimeSpan.FromMinutes(30));
        }

        public static string GetLastError(IMemoryCache memoryCache, string stateId)
        {
            if (memoryCache == null || string.IsNullOrWhiteSpace(stateId))
                return null;

            return memoryCache.TryGetValue(ErrorCacheKeyPrefix + stateId, out string message) ? message : null;
        }

        public static async Task WriteDocumentStreamAsync(
            UiLegacyContext context,
            Stream stream,
            string etag,
            string fileName,
            string contentType,
            string gleamTechRange)
        {
            context.Response.ContentType = contentType;
            context.Response.SetHeader("ETag", $"\"{etag}\"");
            context.Response.SetHeader("Accept-Ranges", "bytes");
            context.Response.SetHeader("Content-Disposition", $"inline; filename=\"{fileName}\"");

            if (!stream.CanSeek)
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(stream);
                return;
            }

            var streamLength = stream.Length;

            if (!string.IsNullOrEmpty(gleamTechRange))
            {
                if (await TryWriteGleamTechRangeAsync(context, stream, streamLength, gleamTechRange))
                    return;
            }

            if (await TryWriteHttpRangeResponseAsync(context, stream, streamLength))
                return;

            context.Response.StatusCode = 200;
            stream.Position = 0;
            await context.Response.WriteAsync(stream);
        }

        private static async Task<bool> TryWriteGleamTechRangeAsync(
            UiLegacyContext context,
            Stream stream,
            long streamLength,
            string gleamTechRange)
        {
            var rangeSplit = gleamTechRange.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            var ranges = new long[rangeSplit.Length];
            for (var i = 0; i < rangeSplit.Length; i++)
                long.TryParse(rangeSplit[i], out ranges[i]);

            if (ranges.Length == 1)
            {
                var streamStart = streamLength + ranges[0];
                var streamEnd = streamLength;
                var streamCount = streamEnd - streamStart;
                if (streamCount <= 0)
                    return false;

                context.Response.StatusCode = 206;
                context.Response.SetHeader("Content-Range", $"bytes {streamStart}-{(streamEnd - 1)}/{streamLength}");
                var bytes = new byte[streamCount];
                stream.Position = streamStart;
                await stream.ReadAsync(bytes, 0, Convert.ToInt32(streamCount));
                await context.Response.WriteAsync(bytes);
                return true;
            }

            if (ranges.Length == 2)
            {
                var streamStart = ranges[0];
                var streamEnd = ranges[1];
                var streamCount = streamEnd - streamStart;
                if (streamCount <= 0)
                    return false;

                context.Response.StatusCode = 206;
                context.Response.SetHeader("Content-Range", $"bytes {streamStart}-{(streamEnd - 1)}/{streamLength}");
                var bytes = new byte[streamCount];
                stream.Position = streamStart;
                await stream.ReadAsync(bytes, 0, Convert.ToInt32(streamCount));
                await context.Response.WriteAsync(bytes);
                return true;
            }

            return false;
        }

        private static async Task<bool> TryWriteHttpRangeResponseAsync(
            UiLegacyContext context,
            Stream stream,
            long streamLength)
        {
            var rangeHeader = context.Request.GetHeader("Range");
            if (string.IsNullOrEmpty(rangeHeader) || !rangeHeader.StartsWith("bytes=", StringComparison.OrdinalIgnoreCase))
                return false;

            var rangeValue = rangeHeader.Substring("bytes=".Length).Trim();
            var dashIndex = rangeValue.IndexOf('-');
            if (dashIndex < 0)
                return false;

            var startText = rangeValue.Substring(0, dashIndex);
            var endText = rangeValue.Substring(dashIndex + 1);

            long start;
            long end;
            if (string.IsNullOrEmpty(startText))
            {
                if (!long.TryParse(endText, out var suffixLength) || suffixLength <= 0)
                    return false;

                start = Math.Max(0, streamLength - suffixLength);
                end = streamLength - 1;
            }
            else
            {
                if (!long.TryParse(startText, out start))
                    return false;

                end = string.IsNullOrEmpty(endText)
                    ? streamLength - 1
                    : long.Parse(endText);

                if (end >= streamLength)
                    end = streamLength - 1;
            }

            if (start < 0 || start > end)
                return false;

            var count = end - start + 1;
            context.Response.StatusCode = 206;
            context.Response.SetHeader("Content-Range", $"bytes {start}-{end}/{streamLength}");
            stream.Position = start;
            var buffer = new byte[count];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            await context.Response.WriteAsync(buffer);
            return true;
        }
    }
}
