//using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
#if !NETFULL
using Microsoft.AspNetCore.StaticFiles;
#else
#endif
namespace FileManager
{
    public class FileManagerController
    {
        private readonly IFileManagerProvider _fileManager;
        private readonly FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();

        public FileManagerController(IFileManagerProvider fileManager)
        {
            this._fileManager = fileManager;
        }

        public ActionResult Index(string mode, string path, string name,

#if !NETFULL
           IEnumerable<Microsoft.AspNetCore.Http.IFormFile> files,
#else
           /*IEnumerable<IFormFile> files,*/
#endif
            string old, string @new, string source, string target, string content, bool thumbnail, string @string)
        {
            try
            {
                if (mode == null)
                {
                    return null;
                }

                if (!string.IsNullOrWhiteSpace(path) && path.StartsWith("/"))
                    path = path.Substring(1);
                if (!string.IsNullOrWhiteSpace(@new) && @new.StartsWith("/"))
                    @new = @new == "/" ? string.Empty : @new.Substring(1);
                if (!string.IsNullOrWhiteSpace(source) && source.StartsWith("/"))
                    source = source.Substring(1);
                if (!string.IsNullOrWhiteSpace(target) && target.StartsWith("/"))
                    target = target.Substring(1);


                switch (mode.ToLower(CultureInfo.CurrentCulture))
                {
                    case "initiate":
                        return Json(_fileManager.Initiate());
                    case "getinfo":
                        return Json(_fileManager.GetInfo(path));
                    case "readfolder":
                        return Json(_fileManager.ReadFolder(path));
                    case "addfolder":
                        return Json(_fileManager.AddFolder(path, name));
#if !NETFULL
                    case "upload":
                        return Json(_fileManager.Upload(path, files).Result);
#else
                    //case "upload":
                    //    return Json(_fileManager.Upload(path, files).Result);
#endif
                    case "rename":
                        return Json(_fileManager.Rename(old, @new));
                    case "move":
                        return Json(_fileManager.Move(old, @new));
                    case "copy":
                        return Json(_fileManager.Copy(source, target));
                    case "savefile":
                        return Json(_fileManager.SaveFile(path, content));
                    case "delete":
                        return Json(_fileManager.Delete(path));
                    case "download":
                        return this.Download(path);
                    case "getimage":
                        return this.GetImage(path, thumbnail);
                    case "readfile":
                        return this.ReadFile(path);
                    case "summarize":
                        return Json(_fileManager.Summarize());
                    case "seekfolder":
                        return Json(_fileManager.SeekFolder(path, @string));
                }

                throw new Exception("Unknown Request!");
            }
            catch (Exception e)
            {
                // returns all unhandeled exceptions and returns them in JSON format with 500.
                // Issue #314
                return new JsonResult(e.Message, contentType: "application/json")
                {
                    StatusCode = 500
                    //StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError,
                };
            }
        }
        private FileResult ReadFile(string path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            byte[] fileBytes = _fileManager.ReadFile(path);

            var cd = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = fileName
            };
            //Response.Headers.Add("Content-Disposition", cd.ToString());
            var contentType = getMimeMapping(fileName);
            return new FileResult(fileBytes, contentType) { ContentDisposition = cd };
        }
        private FileResult Download(string path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            byte[] fileBytes = _fileManager.ReadFile(path);

            return new FileResult(fileBytes, "application/x-msdownload", fileName);
        }
        public FileResult GetImage(string path, bool thumbnail)
        {
            var fileName = System.IO.Path.GetFileName(path);
            byte[] fileBytes = _fileManager.GetImage(path, thumbnail);

            var cd = new System.Net.Mime.ContentDisposition
            {
                Inline = true,
                FileName = fileName
            };
            //Response.Headers.Add("Content-Disposition", cd.ToString());
            var contentType = getMimeMapping(fileName);
            return new FileResult(fileBytes, contentType/*"image/*"*/) { ContentDisposition = cd };
        }
        private string getMimeMapping(string fileName)
        {
            var fileExt = System.IO.Path.GetExtension(fileName);
            if (fileExtensionContentTypeProvider.Mappings.ContainsKey(fileExt))
                return fileExtensionContentTypeProvider.Mappings[fileExt];

            else
                return "application/x-msdownload";
        }
        private JsonResult Json(object dynamic)
        {
            var value = Newtonsoft.Json.JsonConvert.SerializeObject(dynamic, new Newtonsoft.Json.JsonSerializerSettings
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                //ContractResolver = new LowercaseContractResolver()
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
            this.FileBytes = fileBytes;
            if (!string.IsNullOrWhiteSpace(fileName))
                this.ContentDisposition = new System.Net.Mime.ContentDisposition { FileName = fileName };
            this.ContentType = contentType;
        }
        public byte[] FileBytes { get; set; }
        //public string FileName { get; set; }

    }
    public class JsonResult : ActionResult
    {
        public JsonResult() { }
        public JsonResult(string data, string contentType = "application/json")
        {
            this.Data = data;
            this.ContentType = contentType;
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
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
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
        [Newtonsoft.Json.JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Created { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(UnixTimestampConverter))]
        public DateTime? Modified { get; set; }
        [Newtonsoft.Json.JsonConverter(typeof(UnixTimestampConverter))]
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
    public class UnixTimestampConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime) || objectType == typeof(TimeSpan);
        }
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DateTime time)
            {
                var unixTimestamp = GetUnixTimestamp(time);
                writer.WriteValue(unixTimestamp);
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
        private Int32 GetUnixTimestamp(DateTime dt)
        {
            return (Int32)(dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}
