using AspNetCoreDashboard;
using AspNetCoreDashboard.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
//using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using IAppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;

namespace FileManager
{
    /// <summary>
    /// https://demos.gleamtech.com/filevista/
    /// 
    /// ZipEngine
    /// SharpCompress
    /// AWSSDK.S3
    /// FluentScheduler
    /// UAParser
    /// DiagramEngine
    /// EmailEngine
    /// ImagingEngine
    /// PortableEngine
    /// PresentationEngine
    /// ProjectManagementEngine
    /// SpreadsheetEngine
    /// WordProcessingEngine
    /// GraphicsEngine
    /// VideoEngine
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        public static void UseFileExplorerUI(this FileManagerOptions app)
        {
            app.Fun = new System.Action(() =>
            {
                app.ApplicationBuilder.UseFileManager(app.PathMatch);
            });
        }
        private static IAppBuilder UseFileManager(this IAppBuilder app, string pathMatch, IEnumerable<IDashboardAuthorizationFilter> authorization = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
            //if (provider == null) throw new ArgumentNullException(nameof(provider));

            //var services = app.ApplicationServices;
            //var options = /*options ?? services.GetService<DashboardOptions>() ??*/ new DashboardOptions() { Authorization = authorizationFilters };
            var routes = /*services.GetService<RouteCollection>()??*/ new RouteCollection();

            initRoute(routes, app.ApplicationServices);
            //app.Map(new PathString(pathMatch), x => x.UseMiddleware<AspNetCoreFileManagerMiddleware>(/*storage, */options, routes));
            app.UseFileManager(pathMatch, routes, authorization);
            return app;
        }
        private static IAppBuilder UseFileManager(this IAppBuilder app, string pathMatch, RouteCollection routes, IEnumerable<IDashboardAuthorizationFilter> authorization = null)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
            if (routes == null) throw new ArgumentNullException(nameof(routes));

            app.UseMapDashboard(pathMatch, routes, authorization);
            return app;
        }
        private static FileExtensionContentTypeProvider fileExtensionContentTypeProvider = new FileExtensionContentTypeProvider();
        private static string getMimeMapping(string fileName)
        {
            var fileExt = System.IO.Path.GetExtension(fileName);
            if (fileExtensionContentTypeProvider.Mappings.ContainsKey(fileExt))
                return fileExtensionContentTypeProvider.Mappings[fileExt];

            else
                return "application/x-msdownload";
        }
        private static void initRoute(RouteCollection routes, IServiceProvider applicationServices)
        {
            var assembly = GetExecutingAssembly();
            routes.Add("", new RedirectDispatcher((uriMatch) => uriMatch.Value + "/"));
            routes.Add("/", new EmbeddedResourceDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, assembly, GetContentResourceName("FileExplorer", "index.html")));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.js)", "application/javascript", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.css)", "text/css", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.png)", "image/png", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.gif)", "image/gif", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.ttf)", "application/font-sfnt", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.woff)", "application/font-woff", GetContentFolderNamespace("FileExplorer"));
            routes.AddEmbeddedResource(assembly, "/Resource/webviewer/(?<path>.+\\.html)", "text/html", GetContentFolderNamespace("FileExplorer.webviewer"));
            routes.AddEmbeddedResource(assembly, "/Resource/(?<path>.+\\.swf)", "application/x-shockwave-flash", GetContentFolderNamespace("FileExplorer"));
            routes.AddCommand("/localization", async context =>
            {
                var display = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("display") : context.Request.GetQuery("display");
                var format = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("format") : context.Request.GetQuery("format");


                var streamCn = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.Languages", "Languages.FileUltimate-cn.xml"));
                var streamEn = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.Languages", "Languages.FileUltimate-en.xml"));

                System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
                xmlDocument.Load(streamEn);
                var dd = xmlDocument.SelectNodes("/Language/Body/EntryGroup/Entry");
                var dds = dd.OfType<System.Xml.XmlNode>().Select(f => new { Key = f.Attributes["key"]?.Value, Value = f.InnerText }).ToArray();

                System.Xml.XmlDocument xmlDocument2 = new System.Xml.XmlDocument();
                xmlDocument2.Load(streamCn);
                var dd2 = xmlDocument2.SelectNodes("/Language/Body/EntryGroup/Entry");
                var dds2 = dd2.OfType<System.Xml.XmlNode>().Select(f => new { Key = f.Attributes["key"]?.Value, Value = f.InnerText }).Select(f => new { f.Key, Value = f.Value == "{NotTranslated}" ? dds.FirstOrDefault(ff => ff.Key == f.Key)?.Value : f.Value }).ToDictionary(f => f.Key, f => f.Value);

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(dds2);

                if (!string.IsNullOrWhiteSpace(format))
                {
                    var ss111 = System.Globalization.CultureInfo.GetCultureInfo(format);
                    //var json2 = Newtonsoft.Json.JsonConvert.SerializeObject(ss111);
                }

                var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer", "localizationSource.js"));
                var javascriptSource = new System.IO.StreamReader(stream).ReadToEnd();
                var javascript = javascriptSource.Replace("{${lang}$}", display).Replace("{${culture}$}", format ?? "en-US").Replace("\"{${entries}$}\"", json);
                context.Response.ContentType = "application/javascript";
                await context.Response.WriteAsync(javascript);
                return true;
            });
            routes.AddCommand("/localization.json", async context =>
            {
                var display = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("display") : context.Request.GetQuery("display");

                var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.Languages", $"Languages.Viewer-{display ?? "en"}.json"));

                var json = new System.IO.StreamReader(stream).ReadToEnd();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
                return true;
            });

            routes.AddCommand("/Filemanager/ListFolder", async context =>
             {//读取压缩包的内容
                 var rr = await context.Request.GetBodyModelBinderAsync<ListFolderParameters>();
                 var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                 var drive = dsfsdf.Result("${drive}");
                 var path = dsfsdf.Result("${path}");
                 var f = applicationServices.GetService<FileStorage.IFileStore>();
                 var rr11 = await f.GetDirectoryContentAsync(path);



                 var folders = rr11.Where(ff => ff.IsDirectory).Select(ff => new object[] { ff.Name, null, null, true, null, ff.LastModifiedUtc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK") }).ToArray();
                 var files = rr11.Where(ff => !ff.IsDirectory).Select(ff => new object[] { ff.Name, ff.Length, null, ff.LastModifiedUtc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK") }).ToArray();

                 //文件  ：name,size,systemType,dateModified
                 //文件夹：name,permissions,fileTypes,size,systemType,dateModified

                 var r = new
                 {
                     Success = true,
                     Result = new
                     {
                         Files = files,
                         // new[] {
                         //    new object[] { "favicon.ico", 32038, "1111111111", "2019-11-29T00:21:09.6324208Z" },
                         //    new object[] { "aa.ico", 32038, null, "2019-11-29T00:21:09.6324208Z" },
                         //},
                         Folders = folders
                         //new[] {
                         //    new object[] { "css", "111111111111111", "2222222222222", "33333333", "4444444444","2019-11-29T00:21:09.6324208Z" },
                         //    new object[] { "js", null, null, false,null, "2019-11-29T00:21:09.6324208Z" },
                         //    new object[] { "lib", null, null, false, null, "2019-11-29T00:21:09.6324208Z" }
                         //}
                     }
                 };

                 await context.Response.JsonAsync(r);


                 //                           //var time = context.Request.GetQuery("time");
                 //                           var mode = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("mode") : context.Request.GetQuery("mode");
                 //               var path = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                 //               var name = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("name") : context.Request.GetQuery("name");
                 //               var old = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("old") : context.Request.GetQuery("old");
                 //               var @new = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("new") : context.Request.GetQuery("new");
                 //               var source = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("source") : context.Request.GetQuery("source");
                 //               var target = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("target") : context.Request.GetQuery("target");
                 //               var content = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("content") : context.Request.GetQuery("content");
                 //               var thumbnail = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("thumbnail") : context.Request.GetQuery("thumbnail");
                 //               var @string = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("string") : context.Request.GetQuery("string");
                 //#if !NETFULL
                 //                           var files = context.Request.Method == "POST" ? context.Request.GetFilesAsync("files")?.Result : null;
                 //#else
                 //                           //var files = context.Request.Method == "POST" ? context.Request.GetFilesAsync("files")?.Result : null;
                 //#endif
                 //                           //using (var reader = new System.IO.StreamReader(context.Request.Body, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4 * 1024, leaveOpen: true))
                 //                           //{
                 //                           //    var text = reader.ReadToEndAsync().Result;
                 //                           //}

                 //                           //string mode, string path, string name, List<IFormFile> files, string old, string @new, string source, string target, string content, bool thumbnail, string @string
                 //                           bool.TryParse(thumbnail, out var _thumbnail);
                 //               var r = controller.Index(mode, path, name,
                 //#if !NETFULL
                 //                       files,
                 //#else
                 //                       /*files,*/
                 //#endif
                 //                       old, @new, source, target, content, _thumbnail, @string);

                 //               context.Response.ContentType = r.ContentType;
                 //               context.Response.StatusCode = r.StatusCode;
                 //               if (r.ContentDisposition != null)
                 //                   context.Response.SetHeader("Content-Disposition", r.ContentDisposition.ToString());

                 //               if (r is JsonResult jsonResult)
                 //               {
                 //                   context.Response.WriteAsync(jsonResult.Data);
                 //               }
                 //               if (r is FileResult fileResult)
                 //               {
                 //                   context.Response.WriteAsync(fileResult.FileBytes);
                 //               }
                 return true;
             });
            routes.AddCommand("/Filemanager/ExpandFolder", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<ListFolderParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");
                var f = applicationServices.GetService<FileStorage.IFileStore>();
                var rr11 = await f.GetDirectoryContentAsync(path);

                //文件  ：name,size,systemType,dateModified
                //文件夹：name,permissions,fileTypes,size,systemType,dateModified
                var folders = rr11.Where(ff => ff.IsDirectory).Select(ff => new object[] { ff.Name, null, null, true, null, ff.LastModifiedUtc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK") }).ToArray();

                var r = new
                {
                    Success = true,
                    Result = new
                    {
                        Folders = folders
                    }
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Filemanager/Delete", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<DeleteParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");
                var f = applicationServices.GetService<FileStorage.IFileStore>();

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        if (item.EndsWith("\\"))
                        {
                            await f.TryDeleteDirectoryAsync(path + "/" + item);
                        }
                        else
                        {
                            await f.TryDeleteFileAsync(path + "/" + item);
                        }
                    }
                    var r = new
                    {
                        Success = true,
                        Result = rr.ItemNames
                    };
                    await context.Response.JsonAsync(r);
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };
                    await context.Response.JsonAsync(r);
                }
                return true;
            });
            routes.AddCommand("/Filemanager/Rename", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<RenameParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");
                var f = applicationServices.GetService<FileStorage.IFileStore>();

                try
                {
                    if (rr.ItemName.EndsWith("\\"))
                    {
                        await f.MoveDirectoryAsync(path + "/" + rr.ItemName, path + "/" + rr.ItemNewName);
                    }
                    else
                    {
                        await f.MoveFileAsync(path + "/" + rr.ItemName, path + "/" + rr.ItemNewName);
                    }

                    var r = new
                    {
                        Success = true,
                        Result = rr.ItemNewName
                    };
                    await context.Response.JsonAsync(r);
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };
                    await context.Response.JsonAsync(r);
                }
                return true;
            });
            routes.AddCommand("/Filemanager/Create", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CreateParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");
                var f = applicationServices.GetService<FileStorage.IFileStore>();

                try
                {
                    if (rr.ItemName.EndsWith("\\"))
                    {
                        var rr11 = await f.TryCreateDirectoryAsync(path + "/" + rr.ItemName);
                        if (rr11)
                        {
                            var r = new
                            {
                                Success = true,
                                Result = rr.ItemName
                            };
                            await context.Response.JsonAsync(r);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };
                    await context.Response.JsonAsync(r);
                }
                return true;
            });
            routes.AddCommand("/Filemanager/Copy", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CopyParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");

                var dsfsdfTarget = System.Text.RegularExpressions.Regex.Match(rr?.TargetPath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var driveTarget = dsfsdfTarget.Result("${drive}");
                var pathTarget = dsfsdfTarget.Result("${path}");


                var f = applicationServices.GetService<FileStorage.IFileStore>();

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        if (item.EndsWith("\\"))
                        {
                            throw new Exception($"功能未实现.");
                            //await f.MoveDirectoryAsync(path + "/" + item, pathTarget + "/" + item);
                        }
                        else
                        {
                            await f.CopyFileAsync(path + "/" + item, pathTarget + "/" + item);
                        }
                    }
                    var r = new
                    {
                        Success = true,
                        Result = rr.ItemNames
                    };
                    await context.Response.JsonAsync(r);
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };
                    await context.Response.JsonAsync(r);
                }

                return true;
            });
            routes.AddCommand("/Filemanager/Move", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CopyParameters>();
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.Path, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");

                var dsfsdfTarget = System.Text.RegularExpressions.Regex.Match(rr?.TargetPath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var driveTarget = dsfsdfTarget.Result("${drive}");
                var pathTarget = dsfsdfTarget.Result("${path}");


                var f = applicationServices.GetService<FileStorage.IFileStore>();

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        if (item.EndsWith("\\"))
                        {
                            await f.MoveDirectoryAsync(path + "/" + item, pathTarget + "/" + item);
                        }
                        else
                        {
                            await f.MoveFileAsync(path + "/" + item, pathTarget + "/" + item);
                        }
                    }
                    var r = new
                    {
                        Success = true,
                        Result = rr.ItemNames
                    };
                    await context.Response.JsonAsync(r);
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };
                    await context.Response.JsonAsync(r);
                }

                return true;
            });
            routes.AddCommand("/Filemanager/AddToZip", async context =>
            {
                var r = new
                {
                    Success = false,
                    Result = new
                    {
                        Message = "暂时不支持此功能"
                    }
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Filemanager/ExtractAll", async context =>
            {
                var r = new
                {
                    Success = false,
                    Result = new
                    {
                        Message = "暂时不支持此功能"
                    }
                };
                await context.Response.JsonAsync(r);
                return true;
            });

            routes.AddCommand("/Filemanager/GetThumbnail", async context =>
            {
                var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                var maxSize = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("maxSize") : context.Request.GetQuery("maxSize");
                var dsfsdf = System.Text.RegularExpressions.Regex.Match(mpath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                var drive = dsfsdf.Result("${drive}");
                var path = dsfsdf.Result("${path}");
                var f = applicationServices.GetService<FileStorage.IFileStore>();
                var rr11 = await f.GetFileStreamAsync(path + "/" + fileName);

                context.Response.ContentType = getMimeMapping(fileName);

                if (int.TryParse(maxSize, out int wwww))
                {
                    try
                    {
                        var imgS = new System.Drawing.Bitmap(rr11);
                        var img = imgS.GetThumbnailImage(Math.Min(wwww, imgS.Width), Math.Min(wwww, imgS.Height), () => true, IntPtr.Zero);
                        var ms = new System.IO.MemoryStream();
                        img.Save(context.Response.Body, System.Drawing.Imaging.ImageFormat.Png);
                        return true;
                    }
                    catch (Exception ex)
                    {
                    }
                }



                //await context.Response.CopyToAsync(rr11);
                return true;
            });
            routes.AddCommand(new[] { "/Filemanager/Preview", "/Filemanager/Preview/(?<fileName>.*)" }, async context =>
             {
                 var previewerType = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("previewerType") : context.Request.GetQuery("previewerType");
                 var stateId = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("stateId") : context.Request.GetQuery("stateId");
                 var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                 var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                 var urlfileName = context.UriMatch.Result("${fileName}");

                 var dsfsdf = System.Text.RegularExpressions.Regex.Match(mpath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                 var drive = dsfsdf.Result("${drive}");
                 var path = dsfsdf.Result("${path}");

                 if (previewerType.Equals("ImageViewer", StringComparison.OrdinalIgnoreCase))
                 {
                     var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.PreviewHtml", previewerType + ".html"));
                     var htmlSource = new System.IO.StreamReader(stream).ReadToEnd();

                     var html = htmlSource.Replace("{${imgurl}$}", $"../Filemanager/GetImage?stateId={stateId}&path={mpath}&fileName={fileName}&vary=635881258701436895").Replace("{${imgalt}$}", fileName);
                     context.Response.ContentType = "text/html";
                     await context.Response.WriteAsync(html);
                 }
                 else if (previewerType.Equals("DocumentViewer", StringComparison.OrdinalIgnoreCase))
                 {
                     var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.PreviewHtml", previewerType + ".html"));
                     var htmlSource = new System.IO.StreamReader(stream).ReadToEnd();

                     var html = htmlSource;//.Replace("{${imgurl}$}", $"../Filemanager/GetImage?stateId={stateId}&path={mpath}&fileName={fileName}&vary=635881258701436895").Replace("{${imgalt}$}", fileName);
                     context.Response.ContentType = "text/html";
                     await context.Response.WriteAsync(html);
                 }
                 else
                 {
                     var f = applicationServices.GetService<FileStorage.IFileStore>();
                     var rr11 = await f.GetFileStreamAsync(path + "/" + fileName);

                     await context.Response.FileAsync(rr11, getMimeMapping(fileName));
                 }

                 return true;
             });
            routes.AddCommand(new[] { "/Filemanager/GetImage", "/Filemanager/Download/(?<fileName>.*)" }, async context =>
            {
                var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                var fileName2 = context.UriMatch.Result("${fileName}");

                var openInBrowser = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("openInBrowser") : context.Request.GetQuery("openInBrowser");

                var fpath = "";
                if (!string.IsNullOrWhiteSpace(mpath))
                {
                    var dsfsdf = System.Text.RegularExpressions.Regex.Match(mpath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                    var drive = dsfsdf.Result("${drive}");
                    var path = dsfsdf.Result("${path}");

                    fpath = path + "/" + fileName;
                }
                else
                {
                    var dsfsdf = System.Text.RegularExpressions.Regex.Match(fileName2, "(?<stateId>\\S*?)/(?<drive>.+?)/(?<path>.*)");
                    var drive = dsfsdf.Result("${drive}");
                    var path = dsfsdf.Result("${path}");
                    fpath = path;
                }

                var f = applicationServices.GetService<FileStorage.IFileStore>();
                var rr11 = await f.GetFileStreamAsync(fpath);
                bool.TryParse(openInBrowser, out bool result);
                if (result)
                {
                    await context.Response.FileAsync(rr11, getMimeMapping(fileName));
                }
                else
                {
                    await context.Response.FileAsync(rr11, "application/octet-stream", fileName);
                }
                return true;
            });
            routes.AddCommand("/Filemanager/DownloadAsZip", async context =>
            {
                var html = @"<!DOCTYPE html>
<html>
    <head>
        <title>Download Error</title>
        <script type=""text/javascript"">
            if (parent && parent.fileManager) {
                var exceptionResult = {
  ""Message"": ""暂时不支持此功能""
};
                parent.fileManager.messageBox.showError(
                    {
                        title: ""Action Error"",
                        message: exceptionResult.Message,
                        detailsConfig: {
                            data: exceptionResult.Type ? exceptionResult : null,
                            type: ""json""
                        }
                    }
                );
            }
            if (parent === self)
                window.close();
        </script>        
    </head>
    <body>
    </body>
</html>";

                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
                return true;
            });
            routes.AddCommand("/Filemanager/DownloadAsPdf", async context =>
            {
                var html = @"<!DOCTYPE html>
<html>
    <head>
        <title>Download Error</title>
        <script type=""text/javascript"">
            if (parent && parent.fileManager) {
                var exceptionResult = {
  ""Message"": ""暂时不支持此功能""
};
                parent.fileManager.messageBox.showError(
                    {
                        title: ""Action Error"",
                        message: exceptionResult.Message,
                        detailsConfig: {
                            data: exceptionResult.Type ? exceptionResult : null,
                            type: ""json""
                        }
                    }
                );
            }
            if (parent === self)
                window.close();
        </script>        
    </head>
    <body>
    </body>
</html>";

                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(html);
                return true;
            });

            routes.AddCommand("/Fileuploader/Begin", async context =>
            {
                var f = applicationServices.GetService<FileStorage.IFileStore>();
                IMemoryCache memoryCache = applicationServices.GetService<IMemoryCache>();
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderBegin>();

                memoryCache.Set(rr.UploadId, rr, new TimeSpan(2, 0, 0));

                var r = new
                {
                    Success = true,
                    Result = rr.Validations.ToDictionary(ff => ff.Key, ff =>
                    {
                        var rrr = f.GetFileInfoAsync(ff.Value.Name).Result;
                        return new
                        {
                            Action = 0,//Action = 0:Accept(接受)，1:Reject(拒绝)，2:ConfirmReplace(确认替换)
                                       //Data = new { ItemType = 0 },//ItemType = 1:Folder，2:File
                                       //ActionData = "拒绝原因"
                                       //ActionData = new
                                       //{
                                       //    ExistingFileDate = System.DateTimeOffset.Now,
                                       //    ExistingFileName = ff.Value.Name,
                                       //    ExistingFileSize = rrr.Length,
                                       //    NewFileName = "timg(2).jpg"
                                       //}
                        };
                    })
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Fileuploader/SendStream", async context =>
            {
                try
                {
                    var stateId = context.Request.GetQuery("stateId");
                    var uploadId = context.Request.GetQuery("uploadId");
                    var itemId = context.Request.GetQuery("itemId");
                    var size = context.Request.GetQuery("size");
                    var name = context.Request.GetQuery("name");
                    var dateModified = context.Request.GetQuery("dateModified");
                    var chunk = context.Request.GetQuery("chunk");
                    var chunks = context.Request.GetQuery("chunks");

                    IMemoryCache memoryCache = applicationServices.GetService<IMemoryCache>();
                    var rr = memoryCache.Get<UploaderBegin>(uploadId);

                    var f = applicationServices.GetService<FileStorage.IFileStore>();

                    var dsfsdf = System.Text.RegularExpressions.Regex.Match(rr?.CustomParameters?.FileManagerPath, "\\[(?<drive>.*)\\]:(?<path>.*)");
                    var drive = dsfsdf.Result("${drive}");
                    var ppath = dsfsdf.Result("${path}");

                    var path = ppath + "\\" + name;
                    await f.CreateFileFromStreamAsync(path, context.Request.Body);

                    var r = new
                    {
                        Success = true
                    };
                    await context.Response.JsonAsync(r);
                }
                catch (Exception ex)
                {
                    var r = new
                    {
                        Success = false,
                        Result = new
                        {
                            Message = ex.Message
                        }
                    };

                    await context.Response.JsonAsync(r);
                }
                return true;
            });
            routes.AddCommand("/Fileuploader/End", async context =>
            {
                IMemoryCache memoryCache = applicationServices.GetService<IMemoryCache>();
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderEnd>();

                memoryCache.Remove(rr.UploadId);

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Fileuploader/Skip", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderSkip>();

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Fileuploader/Cancel", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderSkip>();

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/Fileuploader/KeepSessionAlive", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<ParametersBaseB>();

                var r = new
                {
                    Result = true,
                    Success = true
                };
                await context.Response.JsonAsync(r);
                return true;
            });

            routes.AddCommand("/Fileuploader/Fail", async context =>
            {
                IMemoryCache memoryCache = applicationServices.GetService<IMemoryCache>();

                var rr = await context.Request.GetBodyModelBinderAsync<UploaderFail>();
                memoryCache.Remove(rr.UploadId);

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
                return true;
            });

            routes.AddCommand("/DocumentViewer/PrepareDocument", async context =>
            {
                var r = new
                {
                    Success = true,
                    Result = 637100357324610856
                };

                await context.Response.JsonAsync(r);
                return true;
            });
            routes.AddCommand("/DocumentViewer/DownloadDocument", async context =>
            {
                var stateId = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("stateId") : context.Request.GetQuery("stateId");
                var cacheInfoKey = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("cacheInfoKey") : context.Request.GetQuery("cacheInfoKey");
                var v = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("v") : context.Request.GetQuery("v");
                var _range = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("_") : context.Request.GetQuery("_");
                var range = context.Request.GetHeader("Range");
                var ifRange = context.Request.GetHeader("If-Range");

                var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.tmpPdf", "z03xrq-1rv5896.xpz"));

                context.Response.ContentType = "application/octet-stream";
                context.Response.SetHeader("ETag", "\"637094457471508903\"");
                context.Response.SetHeader("Content-Disposition", "inline; filename=\"z03xrq-1rv5896.xpz\"");
                context.Response.StatusCode = 206;

                var s = _range.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(f => { int.TryParse(f, out var iii); return iii; }).ToArray();
                if (s.Length != 2)
                {
                    context.Response.SetHeader("Content-Range", "bytes 6445488-6445509/6445510");
                    var count = 6445509 - 6445488 + 1;
                    var bydes = new byte[count];
                    stream.Position = 6445488;
                    stream.Read(bydes, 0, count);
                    await context.Response.WriteAsync(bydes);
                }
                else
                {
                    context.Response.SetHeader("Content-Range", "bytes " + s[0] + "-" + (s[1] - 1) + "/" + stream.Length);
                    var count = s[1] - s[0];
                    var bydes = new byte[count];
                    stream.Position = s[0];
                    stream.Read(bydes, 0, count);
                    await context.Response.WriteAsync(bydes);
                }


                return true;
            });
            routes.AddCommand("/DocumentViewer/GetLastError", async context =>
           {
               var rr = await context.Request.GetBodyModelBinderAsync<ParametersBaseB>();

               //var r = new
               //{
               //    Success = false,
               //    Result = new
               //    {
               //        Message = "暂时不支持此功能"
               //    }
               //};
               //await context.Response.JsonAsync(r);
               return await System.Threading.Tasks.Task.FromResult(true);
           });
        }
        private static System.Reflection.Assembly GetExecutingAssembly()
        {
            return typeof(ParametersBaseB).GetTypeInfo().Assembly;
        }
        private static string GetContentResourceName(string contentFolder, string resourceName)
        {
            return $"{GetContentFolderNamespace(contentFolder)}.{resourceName}";
        }
        private static string GetContentFolderNamespace(string contentFolder)
        {
            var assemblyName = GetExecutingAssembly().GetName().Name;//"FileManager.UI.Explorer";//typeof(ApplicationBuilderExtensions).Namespace
            return $"{assemblyName}.Content.{contentFolder}";
        }
    }
    public static class DashboardResponse_Eeeeeeeeeeeeeeeeeeeeeee
    {
        public static void Json(this DashboardResponse response, object data/*, JsonSerializerSettings serializerSettings = null*/)
        {
            JsonAsync(response, data).Wait();
        }
        public static async System.Threading.Tasks.Task JsonAsync(this DashboardResponse response, object data/*, JsonSerializerSettings serializerSettings = null*/)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            response.ContentType = "application/json";
            await response.WriteAsync(json);
        }
        public static async System.Threading.Tasks.Task FileAsync(this DashboardResponse response, System.IO.Stream fileStream, string contentType, string fileDownloadName = null)
        {
            response.ContentType = contentType;
            if (!string.IsNullOrWhiteSpace(fileDownloadName))
                response.SetHeader("Content-Disposition", "attachment;" + "filename=" + Uri.EscapeDataString(fileDownloadName));
            await response.CopyToAsync(fileStream);
        }
        public static async System.Threading.Tasks.Task FileAsync(this DashboardResponse response, byte[] fileContents, string contentType, string fileDownloadName)
        {
            response.ContentType = contentType;
            if (!string.IsNullOrWhiteSpace(fileDownloadName))
                response.SetHeader("Content-Disposition", "attachment;" + "filename=" + Uri.EscapeDataString(fileDownloadName));
            await response.WriteAsync(fileContents);
        }
    }

    public class ParametersBaseB
    {
        [JsonProperty("stateId")]
        public string StateId { get; set; }
    }
    public class ParametersBase : ParametersBaseB
    {
        [JsonProperty("path")]
        public string Path { get; set; }
    }
    public class ListFolderParameters : ParametersBase
    {
        [JsonProperty("isRefresh")]
        public bool IsRefresh { get; set; }
    }
    public class PreviewParameters : ParametersBase
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("maxSize")]
        public string MaxSize { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
    public class GetThumbnailParameters : PreviewParameters
    {
        [JsonProperty("previewerType")]
        public string PreviewerType { get; set; }
    }
    public class DeleteParameters : ParametersBase
    {
        [JsonProperty("itemNames")]
        public string[] ItemNames { get; set; }
    }
    public class AddToZipParameters : DeleteParameters
    {
        [JsonProperty("zipFileName")]
        public string ZipFileName { get; set; }
    }
    public class CopyParameters : DeleteParameters
    {
        [JsonProperty("targetPath")]
        public string TargetPath { get; set; }
    }
    public class CreateParameters : ParametersBase
    {
        [JsonProperty("itemName")]
        public string ItemName { get; set; }
    }
    public class RenameParameters : CreateParameters
    {
        [JsonProperty("itemNewName")]
        public string ItemNewName { get; set; }
    }
    public class Result
    {

        [JsonProperty("Folders")]
        public object[][] Folders { get; set; }

        [JsonProperty("Files")]
        public object[][] Files { get; set; }
    }
    public class ListFolderResponse
    {

        [JsonProperty("Success")]
        public bool Success { get; set; }

        [JsonProperty("Result")]
        public Result Result { get; set; }
    }





    public class Validation
    {

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Size")]
        public int Size { get; set; }
    }

    public class CustomParameters
    {

        [JsonProperty("fileManagerStateId")]
        public string FileManagerStateId { get; set; }

        [JsonProperty("fileManagerPath")]
        public string FileManagerPath { get; set; }
    }
    public class UploaderEnd : ParametersBaseB
    {
        [JsonProperty("uploadId")]
        public string UploadId { get; set; }
    }

    public class UploaderBegin : UploaderEnd
    {
        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("validations")]
        public Dictionary<string, Validation> Validations { get; set; }

        [JsonProperty("customParameters")]
        public CustomParameters CustomParameters { get; set; }
    }
    public class UploaderFail : UploaderEnd
    {
        [JsonProperty("itemId")]
        public string ItemId { get; set; }
        [JsonProperty("clientError")]
        public string ClientError { get; set; }
    }
    public class UploaderSkip : UploaderEnd
    {
        [JsonProperty("itemIds")]
        public string[] ItemIds { get; set; }
    }
}


