using FileManager.FileStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
#if !NETFULL
using Microsoft.AspNetCore.Http;
#endif

namespace FileManager
{
    public class FileManagerController
    {
        private readonly IFileManagerProvider _fileManager;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        public FileManagerController(IFileStore fileStore, DefaultPermissionHandle permissions = null)
        {
            if (fileStore == null)
                throw new ArgumentNullException(nameof(fileStore));
            _fileManager = new FileManagerProvider(fileStore, permissions);
        }

        public async Task<ActionResult> IndexAsync(
            string mode,
            string path,
            string name,
#if !NETFULL
            IEnumerable<IFormFile> files,
#else
            IEnumerable<object> files,
#endif
            string old,
            string @new,
            string source,
            string target,
            string content,
            bool thumbnail,
            string @string,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (mode == null)
                    return null;

                path = TrimLeadingSlash(path);
                @new = TrimLeadingSlash(@new);
                source = TrimLeadingSlash(source);
                target = TrimLeadingSlash(target);

                switch (mode.ToLower(CultureInfo.InvariantCulture))
                {
                    case "initiate":
                        return Json(await _fileManager.InitiateAsync(cancellationToken));
                    case "getinfo":
                        return Json(await _fileManager.GetInfoAsync(path, cancellationToken));
                    case "readfolder":
                        return Json(await _fileManager.ReadFolderAsync(path, cancellationToken));
                    case "addfolder":
                        return Json(await _fileManager.AddFolderAsync(path, name, cancellationToken));
#if !NETFULL
                    case "upload":
                        return Json(await _fileManager.UploadAsync(path, files, cancellationToken));
#endif
                    case "rename":
                        return Json(await _fileManager.RenameAsync(old, @new, cancellationToken));
                    case "move":
                        return Json(await _fileManager.MoveAsync(old, @new, cancellationToken));
                    case "copy":
                        return Json(await _fileManager.CopyAsync(source, target, cancellationToken));
                    case "savefile":
                        return Json(await _fileManager.SaveFileAsync(path, content, cancellationToken));
                    case "delete":
                        return Json(await _fileManager.DeleteAsync(path, cancellationToken));
                    case "download":
                        return await DownloadAsync(path, cancellationToken);
                    case "getimage":
                        return await GetImageAsync(path, thumbnail, cancellationToken);
                    case "readfile":
                        return await ReadFileAsync(path, cancellationToken);
                    case "summarize":
                        return Json(await _fileManager.SummarizeAsync(cancellationToken));
                    case "seekfolder":
                        return Json(await _fileManager.SeekFolderAsync(path, @string, cancellationToken));
                }

                throw new InvalidOperationException("Unknown request mode.");
            }
            catch (FileStoreException ex)
            {
                return new JsonResult(ex.Message, contentType: "application/json") { StatusCode = 400 };
            }
            catch (Exception)
            {
                return new JsonResult("An unexpected error occurred.", contentType: "application/json") { StatusCode = 500 };
            }
        }

        private async Task<FileResult> ReadFileAsync(string path, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(path);
            var stream = await _fileManager.OpenReadAsync(path, cancellationToken);
            var contentType = GetMimeMapping(fileName);
            return new FileResult(stream, contentType)
            {
                ContentDisposition = new System.Net.Mime.ContentDisposition { Inline = true, FileName = fileName }
            };
        }

        private async Task<FileResult> DownloadAsync(string path, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(path);
            var stream = await _fileManager.OpenReadAsync(path, cancellationToken);
            return new FileResult(stream, "application/octet-stream", fileName);
        }

        private Task<FileResult> GetImageAsync(string path, bool thumbnail, CancellationToken cancellationToken)
        {
            return ReadFileAsync(path, cancellationToken);
        }

        private static string TrimLeadingSlash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;
            if (value == "/")
                return string.Empty;
            return value.StartsWith("/") ? value.Substring(1) : value;
        }

        private string GetMimeMapping(string fileName)
        {
            var fileExt = Path.GetExtension(fileName);
            if (fileExtensionContentTypeProvider.Mappings.TryGetValue(fileExt, out var mime))
                return mime;
            return "application/octet-stream";
        }

        private JsonResult Json(object dynamic)
        {
            var value = JsonConvert.SerializeObject(dynamic, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            });
            return new JsonResult(value, contentType: "application/json");
        }
    }

    public abstract class ActionResult
    {
        public string ContentType { get; set; }
        public int StatusCode { get; set; } = 200;
        public System.Net.Mime.ContentDisposition ContentDisposition { get; set; }
    }

    public class FileResult : ActionResult
    {
        public FileResult(byte[] fileBytes, string contentType, string fileName = null)
        {
            FileBytes = fileBytes;
            if (!string.IsNullOrWhiteSpace(fileName))
                ContentDisposition = new System.Net.Mime.ContentDisposition { FileName = fileName };
            ContentType = contentType;
        }

        public FileResult(Stream fileStream, string contentType, string fileName = null)
        {
            FileStream = fileStream;
            if (!string.IsNullOrWhiteSpace(fileName))
                ContentDisposition = new System.Net.Mime.ContentDisposition { FileName = fileName };
            ContentType = contentType;
        }

        public byte[] FileBytes { get; set; }
        public Stream FileStream { get; set; }
    }

    public class JsonResult : ActionResult
    {
        public JsonResult() { }
        public JsonResult(string data, string contentType = "application/json")
        {
            Data = data;
            ContentType = contentType;
        }
        public string Data { get; set; }
    }

    public class Result<T>
    {
        public T Data { get; set; }
        public List<Error> Errors { get; set; }
    }

    public class Error
    {
        public int Code { get; set; }
        public string Title { get; set; }
        public Meta Meta { get; set; }
    }

    public class Meta
    {
        public List<string> Arguments { get; set; }
    }

    public class ResultInfo
    {
        public string Id { get; set; }
        [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public InfoType Type { get; set; }
        public Attributes Attributes { get; set; }
    }

    public enum InfoType
    {
        initiate,
        summary,
        file,
        folder
    }

    public class Attributes
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool? Readable { get; set; }
        public bool? Writable { get; set; }
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; set; }
        [JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Modified { get; set; }
        [JsonConverter(typeof(UnixTimestampConverter))]
        public TimeSpan? Timestamp { get; set; }
        public string Extension { get; set; }
        public long? Size { get; set; }
        public Config Config { get; set; }
        public int Files { get; set; }
        public int Folders { get; set; }
        public int SizeLimit { get; set; }
        public int Height { get; set; }
        public int Width { get; set; }
    }

    public class Config
    {
        public Security Security { get; set; }
    }

    public class Security
    {
        public bool ReadOnly { get; set; }
        public Extensions Extensions { get; set; }
    }

    public class Extensions
    {
        public bool IgnoreCase { get; set; }
        public Policy Policy { get; set; }
        public string[] Restrictions { get; set; }
    }

    public enum Policy
    {
        ALLOW_LIST,
        DISALLOW_LIST
    }

    public class UnixTimestampConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(TimeSpan);
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime time)
            {
                writer.WriteValue(GetUnixTimestamp(time));
            }
            else if (value is TimeSpan timeSpan)
            {
                writer.WriteValue(timeSpan.TotalSeconds);
            }
            else
            {
                serializer.Serialize(writer, value);
            }
        }

        private static int GetUnixTimestamp(DateTime dt)
        {
            return (int)(dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
