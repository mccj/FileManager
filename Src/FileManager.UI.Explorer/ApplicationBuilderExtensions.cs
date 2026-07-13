using AspNetCoreDashboard.Abstractions;
using FileManager.FileStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using IAppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;

namespace FileManager
{
    /// <summary>
    /// FileExplorer 富文件管理 UI 的路由与 API 配置。
    /// </summary>
    public static class FileExplorerApplicationBuilderExtensions
    {
        /// <summary>选择挂载 FileExplorer UI 模块。</summary>
        public static void UseFileExplorerUI(this FileManagerOptions app)
        {
            app.UiModuleKind = "explorer";
        }
        private static string getMimeMapping(string fileName) => ExplorerContentTypeProvider.GetMimeMapping(fileName);
        internal static void ConfigureFileExplorerRoutes(IUiModuleRegistration builder)
        {
            var assembly = GetExecutingAssembly();
            var contentNs = GetContentFolderNamespace("FileExplorer");

            builder.MapGet("", ctx => ctx.RedirectAsync(ctx.Path.TrimEnd('/') + "/"))
                   .MapEmbeddedUi(assembly, contentNs)
                   .MapGet("/Resource/{*path}", ctx => ServeEmbeddedResource(ctx, assembly, contentNs))
                   .MapFallbackToIndex(GetContentResourceName("FileExplorer", "index.html"));
            builder.MapCommand("/localization", async context =>
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
            });
            builder.MapCommand("/localization.json", async context =>
            {
                var display = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("display") : context.Request.GetQuery("display");

                var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.Languages", $"Languages.Viewer-{display ?? "en"}.json"));

                var json = new System.IO.StreamReader(stream).ReadToEnd();
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json);
            });

            builder.MapCommand("/Filemanager/ListFolder", async context =>
             { // 列出文件夹内容（文件与目录）
                 var folderParameters = await context.Request.GetBodyModelBinderAsync<ListFolderParameters>();

                 if (!ExplorerRoutePermissions.TryParsePath(context.Services,folderParameters?.Path, out var path))
                     throw new FileStoreException("Invalid explorer path.");
                 var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                 var permissions = context.Services.GetService<FileStorage.DefaultPermissionHandle>();
                 await ExplorerRoutePermissions.Create(context.Services).EnsureListAsync(path);
                 var fileStoreEntries = await fileStore.GetDirectoryContentAsync(path);

                 var readOnlyPermissions = !fileStore.Supports(FileStoreCapabilityFlags.Write);
                 var folderPermissionTasks = fileStoreEntries.Where(ff => ff.IsDirectory).Select(async ff =>
                 {
                     var permission = readOnlyPermissions
                         ? FileStoreCapabilitiesExtensions.GetReadOnlyFolderPermissions()
                         : await permissions.GetPermissionAsync(ff);
                     return new object[] { ff.Name, permission, null, true, null, ff.LastModifiedUtc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK") };
                 });
                 var folders = await Task.WhenAll(folderPermissionTasks);
                 var files = fileStoreEntries.Where(ff => !ff.IsDirectory).Select(ff => new object[] { ff.Name, ff.Length, null, ff.LastModifiedUtc.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK") }).ToArray();

                 // 文件列：name, size, systemType, dateModified
                 // 文件夹列：name, permissions, fileTypes, size, systemType, dateModified
                 //permissions:1073774591,26755,1,2,7,11,19,35,67,24707,
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
             });
            builder.MapCommand("/Filemanager/ExpandFolder", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<ListFolderParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);
                await guard.EnsureListAsync(path);
                var rr11 = await f.GetDirectoryContentAsync(path);

                // 文件列：name, size, systemType, dateModified
                // 文件夹列：name, permissions, fileTypes, size, systemType, dateModified
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
            });
            builder.MapCommand("/Filemanager/Delete", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<DeleteParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        var itemPath = ExplorerRoutePermissions.ItemPath(path, item);
                        await guard.EnsureDeleteAsync(itemPath);
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
            });
            builder.MapCommand("/Filemanager/Rename", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<RenameParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    await guard.EnsureRenameAsync(ExplorerRoutePermissions.ItemPath(path, rr.ItemName));
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
            });
            builder.MapCommand("/Filemanager/Create", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CreateParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    await guard.EnsureMkdirAsync(ExplorerRoutePermissions.ItemPath(path, rr.ItemName));
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
            });
            builder.MapCommand("/Filemanager/Copy", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CopyParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);

                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.TargetPath, out var pathTarget);


                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        await guard.EnsureCopyAsync(ExplorerRoutePermissions.ItemPath(path, item), pathTarget);
                        if (item.EndsWith("\\"))
                        {
                            var dirName = item.TrimEnd('\\');
                            await f.CopyDirectoryAsync(f.Combine(path, dirName), f.Combine(pathTarget, dirName));
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

            });
            builder.MapCommand("/Filemanager/Move", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<CopyParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);

                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.TargetPath, out var pathTarget);


                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    foreach (var item in rr.ItemNames)
                    {
                        await guard.EnsureMoveAsync(ExplorerRoutePermissions.ItemPath(path, item), pathTarget);
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

            });
            builder.MapCommand("/Filemanager/AddToZip", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<AddToZipParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    var zipPath = string.IsNullOrEmpty(path) ? rr.ZipFileName : path + "/" + rr.ZipFileName;
                    await guard.EnsureUploadAsync(path);
                    foreach (var item in rr.ItemNames)
                        await guard.EnsureDownloadAsync(ExplorerRoutePermissions.ItemPath(path, item));
                    var zipExists = await f.GetFileInfoAsync(zipPath);
                    if (zipExists == null)
                    {
                        using (var zipStream = new System.IO.MemoryStream())
                        {
                            await ZipArchiveHelper.CreateZipAsync(f, path, rr.ItemNames, zipStream);
                            zipStream.Position = 0;
                            await f.CreateFileFromStreamAsync(zipPath, zipStream, overwrite: false);
                        }
                    }
                    else
                    {
                        await ZipArchiveHelper.AddItemsToZipAsync(f, path, rr.ItemNames, zipPath);
                    }

                    await context.Response.JsonAsync(new { Success = true, Result = rr.ItemNames });
                }
                catch (Exception ex)
                {
                    await context.Response.JsonAsync(new
                    {
                        Success = false,
                        Result = new { Message = ex.Message }
                    });
                }
            });
            builder.MapCommand("/Filemanager/ExtractAll", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<ExtractAllParameters>();
                ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var guard = ExplorerRoutePermissions.Create(context.Services);

                try
                {
                    var zipPath = path + "/" + rr.ItemName.TrimEnd('\\');
                    var destinationPath = string.IsNullOrWhiteSpace(rr.TargetPath)
                        ? path
                        : ExplorerRoutePermissions.ParsePath(context.Services, rr.TargetPath);

                    await guard.EnsureExtractAsync(zipPath, destinationPath);
                    await ZipArchiveHelper.ExtractAllAsync(f, zipPath, destinationPath);
                    await context.Response.JsonAsync(new { Success = true, Result = rr.ItemName });
                }
                catch (Exception ex)
                {
                    await context.Response.JsonAsync(new
                    {
                        Success = false,
                        Result = new { Message = ex.Message }
                    });
                }
            });

            builder.MapCommand("/Filemanager/GetThumbnail", async context =>
            {
                var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                var maxSize = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("maxSize") : context.Request.GetQuery("maxSize");
                ExplorerRoutePermissions.TryParsePath(context.Services,mpath, out var path);
                var f = context.Services.GetService<FileStorage.IFileStore>();
                var inStream = await f.GetFileStreamAsync(path + "/" + fileName);

                context.Response.ContentType = getMimeMapping(fileName);

                if (int.TryParse(maxSize, out int wwww))
                {
                    try
                    {
                        var sdfs = SixLabors.ImageSharp.Image.DetectFormat(inStream);
                        using (var image = SixLabors.ImageSharp.Image.Load(inStream))
                        {
                            var w = image.Width > image.Height ? wwww : wwww * image.Width / image.Height;
                            var h = image.Width > image.Height ? wwww * image.Height / image.Width : wwww;
                            image.Mutate(x => x
                                .Resize(Math.Min(w, image.Width), Math.Min(h, image.Height))
                            );
                            using (var ms = new System.IO.MemoryStream())
                            {
                                image.SaveAsPng(ms);
                                var bytes = ms.ToArray();
                                await context.Response.WriteAsync(bytes);
                            }
                        }

                    }
                    catch
                    {
                    }
                }



                //await context.Response.CopyToAsync(rr11);
            });
            builder.MapCommand(new[] { "/Filemanager/Preview", "/Filemanager/Preview/(?<fileName>.*)" }, async context =>
             {
                 var previewerType = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("previewerType") : context.Request.GetQuery("previewerType");
                 var stateId = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("stateId") : context.Request.GetQuery("stateId");
                 var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                 var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                 var urlfileName = context.UriMatch.Result("${fileName}");

                 ExplorerRoutePermissions.TryParsePath(context.Services,mpath, out var path);

                 if (previewerType.Equals("ImageViewer", StringComparison.OrdinalIgnoreCase))
                 {
                     var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.PreviewHtml", previewerType + ".html"));
                     var htmlSource = new System.IO.StreamReader(stream).ReadToEnd();

                     var imgUrl = $"../Filemanager/GetImage?stateId={Uri.EscapeDataString(stateId)}&path={Uri.EscapeDataString(mpath)}&fileName={Uri.EscapeDataString(fileName)}&vary=635881258701436895";
                    var html = htmlSource.Replace("{${imgurl}$}", imgUrl).Replace("{${imgalt}$}", fileName);
                     context.Response.ContentType = "text/html";
                     await context.Response.WriteAsync(html);
                 }
                 else if (previewerType.Equals("TextViewer", StringComparison.OrdinalIgnoreCase))
                 {
                     var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.PreviewHtml", previewerType + ".html"));
                     var htmlSource = new System.IO.StreamReader(stream).ReadToEnd();
                     var textUrl = $"../Filemanager/Download?stateId={Uri.EscapeDataString(stateId)}&path={Uri.EscapeDataString(mpath)}&openInBrowser=true&fileName={Uri.EscapeDataString(fileName)}";
                     var language = ExplorerHighlightLanguages.GetHighlightLanguage(fileName);
                     var html = htmlSource
                         .Replace("{${texturl}$}", textUrl)
                         .Replace("{${title}$}", fileName)
                         .Replace("{${language}$}", language);
                     context.Response.ContentType = "text/html; charset=utf-8";
                     await context.Response.WriteAsync(html);
                 }
                 else if (previewerType.Equals("DocumentViewer", StringComparison.OrdinalIgnoreCase))
                 {
                     var stream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.PreviewHtml", previewerType + ".html"));
                     var htmlSource = new System.IO.StreamReader(stream).ReadToEnd();

                     context.Response.ContentType = "text/html";
                     if (previewerType.Equals("DocumentViewer", StringComparison.OrdinalIgnoreCase))
                     {
                         var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                         var fileEntry = await fileStore.GetFileInfoAsync(path + "/" + fileName);
                         var cacheInfoKey = BuildDocumentViewerCacheKey(stateId, path, fileName);
                         var memoryCache = context.Services.GetService<IMemoryCache>();
                         memoryCache?.Set(cacheInfoKey, new DocumentViewerCacheInfo
                         {
                             StateId = stateId,
                             Path = path,
                             FileName = fileName,
                             FileLength = fileEntry?.Length ?? 0,
                             LastModifiedUtc = fileEntry?.LastModifiedUtc ?? DateTimeOffset.UtcNow
                         }, TimeSpan.FromHours(2));

                        var documentType = DocumentViewerSupport.GetDocumentType(fileName);
                         var html = htmlSource
                         .Replace("{${stateId}$}", stateId)
                         .Replace("{${cacheInfoKey}$}", cacheInfoKey)
                         .Replace("{${documentType}$}", documentType)
                         .Replace("{${custom}$}", Newtonsoft.Json.JsonConvert.SerializeObject(
                             new
                             {
                                 permissions = 262143,
                                 showToolbar = true,
                                 showSidePane = true,
                                 singlePageRotation = false,

                                 downloadUrl = $"../../Filemanager/Download?stateId={Uri.EscapeDataString(stateId)}&path={Uri.EscapeDataString(mpath)}&openInBrowser=false&fileName={Uri.EscapeDataString(fileName)}",
                                 downloadAsPdfUrl = $"../../DocumentViewer/DownloadAsPdf?stateId={stateId}&cacheInfoKey={cacheInfoKey}",
                             }
                         ).Replace("\"", "\\\""));
                         await context.Response.WriteAsync(html);
                     }
                     else
                     {
                         var html = htmlSource;//.Replace("{${imgurl}$}", $"../Filemanager/GetImage?stateId={stateId}&path={mpath}&fileName={fileName}&vary=635881258701436895").Replace("{${imgalt}$}", fileName);
                         await context.Response.WriteAsync(html);
                     }
                 }
                 else
                 {
                     var f = context.Services.GetService<FileStorage.IFileStore>();
                     var rr11 = await f.GetFileStreamAsync(path + "/" + fileName);

                     await context.Response.FileAsync(rr11, getMimeMapping(fileName));
                 }

             });
            builder.MapCommand(new[] { "/Filemanager/GetImage", "/Filemanager/Download", "/Filemanager/Download/(?<fileName>.*)" }, async context =>
            {
                var mpath = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("path") : context.Request.GetQuery("path");
                var fileName = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("fileName") : context.Request.GetQuery("fileName");
                var fileName2 = context.UriMatch.Result("${fileName}");

                var openInBrowser = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("openInBrowser") : context.Request.GetQuery("openInBrowser");

                var fpath = "";
                if (!string.IsNullOrWhiteSpace(mpath))
                {
                    ExplorerRoutePermissions.TryParsePath(context.Services,mpath, out var path);

                    fpath = path + "/" + fileName;
                }
                else
                {
                    ExplorerPathParser.TryParseDocumentViewerPath(fileName2, out var path);
                    fpath = path;
                }

                var f = context.Services.GetService<FileStorage.IFileStore>();
                await ExplorerRoutePermissions.Create(context.Services).EnsureDownloadAsync(fpath);
                var rr11 = await f.GetFileStreamAsync(fpath);
                bool.TryParse(openInBrowser, out bool result);
                if (result)
                {
                    context.Response.SetHeader("Content-Disposition", $"inline; filename=\"{fileName}\"");
                    await context.Response.FileAsync(rr11, getMimeMapping(fileName));
                }
                else
                {
                    await context.Response.FileAsync(rr11, "application/octet-stream", fileName);
                }
            });
            builder.MapCommand("/Filemanager/DownloadAsZip", async context =>
            {
                try
                {
                    var parametersJson = context.Request.Method == "POST"
                        ? await context.Request.GetFormValueAsync("parameters")
                        : context.Request.GetQuery("parameters");
                    var rr = string.IsNullOrWhiteSpace(parametersJson)
                        ? null
                        : Newtonsoft.Json.JsonConvert.DeserializeObject<AddToZipParameters>(parametersJson);
                    ExplorerRoutePermissions.TryParsePath(context.Services,rr?.Path, out var path);
                    var f = context.Services.GetService<FileStorage.IFileStore>();

                    context.Response.ContentType = "application/zip";
                    context.Response.SetHeader("Content-Disposition", $"attachment; filename=\"{rr?.ZipFileName ?? "download.zip"}\"");
                    using (var zipStream = new System.IO.MemoryStream())
                    {
                        await ZipArchiveHelper.CreateZipAsync(f, path, rr.ItemNames, zipStream);
                        zipStream.Position = 0;
                        await context.Response.WriteAsync(zipStream);
                    }
                }
                catch (Exception ex)
                {
                    var html = @"<!DOCTYPE html>
<html>
    <head>
        <title>Download Error</title>
        <script type=""text/javascript"">
            if (parent && parent.fileManager) {
                var exceptionResult = {
  ""Message"": """ + ex.Message.Replace("\"", "\\\"") + @"""
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
                }
            });

            builder.MapCommand("/Fileuploader/Begin", async context =>
            {
                var f = context.Services.GetService<FileStorage.IFileStore>();
                IMemoryCache memoryCache = context.Services.GetService<IMemoryCache>();
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderBegin>();

                memoryCache.Set(rr.UploadId, rr, new TimeSpan(2, 0, 0));

                var validations = new Dictionary<string, object>();
                foreach (var ff in rr.Validations)
                {
                    _ = await f.GetFileInfoAsync(ff.Value.Name);
                    validations[ff.Key] = new { Action = 0 };
                }

                var r = new
                {
                    Success = true,
                    Result = validations
                };
                await context.Response.JsonAsync(r);
            });
            builder.MapCommand("/Fileuploader/SendStream", async context =>
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

                    IMemoryCache memoryCache = context.Services.GetService<IMemoryCache>();
                    var rr = memoryCache.Get<UploaderBegin>(uploadId);

                    var f = context.Services.GetService<FileStorage.IFileStore>();

                    ExplorerRoutePermissions.TryParsePath(context.Services,rr?.CustomParameters?.FileManagerPath, out var ppath);

                    var safeName = FileStorage.VirtualPath.ValidateFileName(name);
                    var path = ExplorerPathParser.CombinePath(ppath, safeName);
                    await ExplorerRoutePermissions.Create(context.Services).EnsureUploadAsync(ppath);
                    using (var body = await context.Request.OpenBodyAsync())
                    {
                        await f.CreateFileFromStreamAsync(path, body);
                    }

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
            });
            builder.MapCommand("/Fileuploader/End", async context =>
            {
                IMemoryCache memoryCache = context.Services.GetService<IMemoryCache>();
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderEnd>();

                memoryCache.Remove(rr.UploadId);

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
            });
            builder.MapCommand("/Fileuploader/Skip", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderSkip>();

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
            });
            builder.MapCommand("/Fileuploader/Cancel", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<UploaderSkip>();

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
            });
            builder.MapCommand("/Fileuploader/KeepSessionAlive", async context =>
            {
                var rr = await context.Request.GetBodyModelBinderAsync<ParametersBaseB>();

                var r = new
                {
                    Result = true,
                    Success = true
                };
                await context.Response.JsonAsync(r);
            });

            builder.MapCommand("/Fileuploader/Fail", async context =>
            {
                IMemoryCache memoryCache = context.Services.GetService<IMemoryCache>();

                var rr = await context.Request.GetBodyModelBinderAsync<UploaderFail>();
                memoryCache.Remove(rr.UploadId);

                var r = new
                {
                    Success = true
                };
                await context.Response.JsonAsync(r);
            });

            builder.MapCommand("/DocumentViewer/PrepareDocument", async context =>
            {
                var parameters = await context.Request.GetBodyModelBinderAsync<CacheParameters>();
                var memoryCache = context.Services.GetService<IMemoryCache>();
                var stateId = parameters?.StateId;

                if (memoryCache == null || string.IsNullOrWhiteSpace(parameters?.CacheInfoKey))
                {
                    DocumentViewerSupport.SetLastError(memoryCache, stateId, "Document preview session is invalid.");
                    await context.Response.JsonAsync(new
                    {
                        Success = false,
                        Result = new { Message = "Document preview session is invalid." }
                    });
                    return;
                }

                if (!memoryCache.TryGetValue(parameters.CacheInfoKey, out DocumentViewerCacheInfo cacheInfo) || cacheInfo == null)
                {
                    DocumentViewerSupport.SetLastError(memoryCache, stateId, "Document preview session has expired. Refresh the page and try again.");
                    await context.Response.JsonAsync(new
                    {
                        Success = false,
                        Result = new { Message = "SessionExpired" }
                    });
                    return;
                }

                try
                {
                    var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                    var filePath = DocumentViewerSupport.GetFilePath(cacheInfo);
                    await ExplorerRoutePermissions.Create(context.Services).EnsureDownloadAsync(filePath);
                    var fileEntry = await fileStore.GetFileInfoAsync(filePath);
                    if (fileEntry == null)
                    {
                        var message = $"The file '{filePath}' does not exist.";
                        DocumentViewerSupport.SetLastError(memoryCache, stateId, message);
                        await context.Response.JsonAsync(new
                        {
                            Success = false,
                            Result = new { Message = message }
                        });
                        return;
                    }

                    cacheInfo.FileLength = fileEntry.Length;
                    cacheInfo.LastModifiedUtc = fileEntry.LastModifiedUtc;
                    memoryCache.Set(parameters.CacheInfoKey, cacheInfo, TimeSpan.FromHours(2));

                    await context.Response.JsonAsync(new
                    {
                        Success = true,
                        Result = DocumentViewerSupport.GetDocumentVersion(cacheInfo)
                    });
                }
                catch (FileStoreException ex)
                {
                    DocumentViewerSupport.SetLastError(memoryCache, stateId, ex.Message);
                    await context.Response.JsonAsync(new
                    {
                        Success = false,
                        Result = new { Message = ex.Message }
                    });
                }
            });
            builder.MapCommand("/DocumentViewer/DownloadDocument", async context =>
            {
                var stateId = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("stateId") : context.Request.GetQuery("stateId");
                var cacheInfoKey = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("cacheInfoKey") : context.Request.GetQuery("cacheInfoKey");
                var v = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("v") : context.Request.GetQuery("v");
                var gleamTechRange = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("_") : context.Request.GetQuery("_");

                var memoryCache = context.Services.GetService<IMemoryCache>();
                if (memoryCache?.TryGetValue(cacheInfoKey, out DocumentViewerCacheInfo cacheInfo) == true && cacheInfo != null)
                {
                    try
                    {
                        var filePath = DocumentViewerSupport.GetFilePath(cacheInfo);
                        await ExplorerRoutePermissions.Create(context.Services).EnsureDownloadAsync(filePath);
                        var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                        var stream = await fileStore.GetFileStreamAsync(filePath);
                        await DocumentViewerSupport.WriteDocumentStreamAsync(
                            context,
                            stream,
                            v,
                            cacheInfo.FileName,
                            getMimeMapping(cacheInfo.FileName),
                            gleamTechRange);
                    }
                    catch (FileStoreException ex)
                    {
                        DocumentViewerSupport.SetLastError(memoryCache, stateId, ex.Message);
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync(ex.Message);
                    }

                    return;
                }

                var filename = System.IO.Path.GetFileName(System.IO.Path.ChangeExtension(cacheInfoKey, ".xpz"));
                var demoStream = assembly.GetManifestResourceStreamIgnoreCase(GetContentResourceName("FileExplorer.tmpPdf", filename));
                if (demoStream == null)
                {
                    DocumentViewerSupport.SetLastError(memoryCache, stateId, "Document preview data was not found.");
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Document preview data was not found.");
                    return;
                }

                await DocumentViewerSupport.WriteDocumentStreamAsync(
                    context,
                    demoStream,
                    v,
                    filename,
                    "application/octet-stream",
                    gleamTechRange);
            });
            builder.MapCommand("/DocumentViewer/GetLastError", async context =>
           {
               var parameters = await context.Request.GetBodyModelBinderAsync<ParametersBaseB>();
               var memoryCache = context.Services.GetService<IMemoryCache>();
               var message = DocumentViewerSupport.GetLastError(memoryCache, parameters?.StateId)
                   ?? "Downloading file part error occurred. The document could not be loaded.";

               await context.Response.JsonAsync(new
               {
                   Success = true,
                   Result = new
                   {
                       Message = message
                   }
               });
           });
            builder.MapCommand("/DocumentViewer/DownloadAsPdf", async context =>
            {
                var stateId = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("stateId") : context.Request.GetQuery("stateId");
                var cacheInfoKey = context.Request.Method == "POST" ? await context.Request.GetFormValueAsync("cacheInfoKey") : context.Request.GetQuery("cacheInfoKey");

                var memoryCache = context.Services.GetService<IMemoryCache>();
                if (memoryCache?.TryGetValue(cacheInfoKey, out DocumentViewerCacheInfo cacheInfo) == true && cacheInfo != null)
                {
                    var fileStore = context.Services.GetService<FileStorage.IFileStore>();
                    var filePath = string.IsNullOrEmpty(cacheInfo.Path)
                        ? cacheInfo.FileName
                        : cacheInfo.Path + "/" + cacheInfo.FileName;
                    var stream = await fileStore.GetFileStreamAsync(filePath);
                    context.Response.ContentType = getMimeMapping(cacheInfo.FileName);
                    context.Response.SetHeader("Content-Disposition", $"inline; filename=\"{cacheInfo.FileName}\"");
                    await context.Response.WriteAsync(stream);
                    return;
                }

                context.Response.ContentType = "application/pdf";
                await context.Response.WriteAsync("");
            });
        }
        private static async Task ServeEmbeddedResource(IUiContext ctx, Assembly assembly, string contentNamespace)
        {
            var path = ctx.GetRouteValue("path");
            if (string.IsNullOrWhiteSpace(path))
            {
                ctx.StatusCode = 404;
                return;
            }

            var resourceName = EmbeddedResourcePathHelper.ToResourceName(contentNamespace, path);
            var stream = EmbeddedResourceCache.OpenReadStream(assembly, resourceName)
                ?? assembly.GetManifestResourceStreamIgnoreCase(resourceName);
            if (stream == null)
            {
                // ToResourceName converts '-' to '_', but some embedded assets keep hyphens (e.g. theme-gray).
                var literalName = $"{contentNamespace}.{path.Replace('/', '.').Replace('\\', '.')}";
                stream = assembly.GetManifestResourceStreamIgnoreCase(literalName);
            }

            if (stream == null)
            {
                ctx.StatusCode = 404;
                return;
            }

            await ctx.WriteStreamAsync(stream, getMimeMapping(path));
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

        private static string BuildDocumentViewerCacheKey(string stateId, string path, string fileName)
        {
            var key = $"{stateId}|{path}|{fileName}";
            var hash = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(key))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
            return $"{stateId}/{hash}.json";
        }
    }

    internal sealed class DocumentViewerCacheInfo
    {
        public string StateId { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public long FileLength { get; set; }
        public DateTimeOffset LastModifiedUtc { get; set; }
    }

    public class ParametersBaseB
    {
        [JsonProperty("stateId")]
        public string StateId { get; set; }
    }
    public class CacheParameters : ParametersBaseB
    {
        [JsonProperty("cacheInfoKey")]
        public string CacheInfoKey { get; set; }
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
    public class ExtractAllParameters : DeleteParameters
    {
        [JsonProperty("itemName")]
        public string ItemName { get; set; }

        [JsonProperty("targetPath")]
        public string TargetPath { get; set; }
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


