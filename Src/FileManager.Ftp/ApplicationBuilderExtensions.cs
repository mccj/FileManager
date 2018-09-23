using AspNetCoreDashboard.Dashboard;
using System;
using System.Collections.Generic;
#if !NETFULL
using Microsoft.Extensions.DependencyInjection;
using IAppBuilder = Microsoft.AspNetCore.Builder.IApplicationBuilder;
#else
using IAppBuilder = Owin.IAppBuilder;
#endif
namespace FileManager
{
    /// <summary>
    /// https://demo.kodcloud.com/
    /// https://github.com/oyejorge/gpFinder
    /// 
    /// </summary>
    public static class ApplicationBuilder2Extensions
    {
        public static IAppBuilder UseFileManagerForFtp(this IAppBuilder app,
            string pathMatch,
            //string fileSystemPath, 
            string host, int port, string user, string pass,
            string webRootPath = null,
            string webPath = "",
            bool publicPath = false,
            string[] allowedExtensions = null,
            IEnumerable<IDashboardAuthorizationFilter> authorization = null)
            {
                if (app == null) throw new ArgumentNullException(nameof(app));
                if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));

                var Provider = new FileManagerWebUI.FileStorage.FtpStore(webRootPath, host, port, user, pass);
                app.UseFileManager(pathMatch, Provider, "", webPath, publicPath, allowedExtensions, authorization);
                return app;
            }
        //        //#if !NETFULL
        //        //        public static IAppBuilder UseFileManager(this IAppBuilder app,
        //        //           string pathMatch,
        //        //           Microsoft.Extensions.FileProviders.IFileProvider fileProvider,
        //        //           string webRootPath = null,
        //        //           string webPath = "",
        //        //           bool publicPath = false,
        //        //           string[] allowedExtensions = null,
        //        //           IEnumerable<IDashboardAuthorizationFilter> authorization = null)
        //        //        {
        //        //            if (app == null) throw new ArgumentNullException(nameof(app));
        //        //            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
        //        //            //if (string.IsNullOrWhiteSpace(webRootPath))
        //        //            //{
        //        //            //    var env = app.ApplicationServices.GetService<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
        //        //            //    webRootPath = env.WebRootPath;
        //        //            //}
        //        //            var Provider = new FileManagerProvider(fileProvider, webRootPath, webPath, publicPath, allowedExtensions);
        //        //            app.UseFileManager(pathMatch, Provider, authorization);
        //        //            return app;
        //        //        }
        //        //#endif
        //        public static IAppBuilder UseFileManager(this IAppBuilder app,
        //            string pathMatch,
        //            IFileManagerProvider provider,
        //            IEnumerable<IDashboardAuthorizationFilter> authorization = null)
        //        {
        //            if (app == null) throw new ArgumentNullException(nameof(app));
        //            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
        //            if (provider == null) throw new ArgumentNullException(nameof(provider));

        //            //var services = app.ApplicationServices;
        //            //var options = /*options ?? services.GetService<DashboardOptions>() ??*/ new DashboardOptions() { Authorization = authorizationFilters };
        //            var routes = /*services.GetService<RouteCollection>()??*/ new RouteCollection();

        //            initRoute(routes, provider);
        //            //app.Map(new PathString(pathMatch), x => x.UseMiddleware<AspNetCoreFileManagerMiddleware>(/*storage, */options, routes));
        //            app.UseFileManager(pathMatch, routes, authorization);
        //            return app;
        //        }
        //        public static IAppBuilder UseFileManager(this IAppBuilder app,
        //            string pathMatch,
        //            RouteCollection routes,
        //            IEnumerable<IDashboardAuthorizationFilter> authorization = null)
        //        {
        //            if (app == null) throw new ArgumentNullException(nameof(app));
        //            if (pathMatch == null) throw new ArgumentNullException(nameof(pathMatch));
        //            if (routes == null) throw new ArgumentNullException(nameof(routes));

        //            app.UseMapDashboard(pathMatch, authorization, routes);
        //            return app;
        //        }

        //        private static void initRoute(RouteCollection routes, IFileManagerProvider fileManagerProvider)
        //        {
        //            var controller = new FileManagerController(fileManagerProvider);

        //            //application/json
        //            //text/html
        //            //application/javascript
        //            //text/css
        //            routes.Add("/", new EmbeddedResourceDispatcher(System.Net.Mime.MediaTypeNames.Text.Html, GetExecutingAssembly(), GetContentResourceName("RichFilemanager", "index.html")));
        //            //routes.Add("/js[0-9]+", new CombinedResourceDispatcher("application/javascript", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager/js"), new[] { "" }));
        //            //routes.Add("/css[0-9]+", new CombinedResourceDispatcher("text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager/js"), new[] { "" }));
        //            routes.AddEmbeddedResource("/src/css/(?<path>.+\\.css)", "text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.src.css"));
        //            routes.AddEmbeddedResource("/src/js/(?<path>.+\\.js)", "application/javascript", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.src.js"));
        //            routes.AddEmbeddedResource("/src/templates/(?<path>.+\\.html)", System.Net.Mime.MediaTypeNames.Text.Html, GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.src.templates"));
        //            routes.AddEmbeddedResource("/languages/(?<path>.+\\.json)", "application/json", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.languages"));
        //            routes.AddEmbeddedResource("/images/(?<path>.+\\.png)", "image/png", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.images"));
        //            routes.AddEmbeddedResource("/images/(?<path>.+\\.gif)", System.Net.Mime.MediaTypeNames.Image.Gif, GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.images"));
        //            routes.AddEmbeddedResource("/libs/(?<path>.+\\.json)", "application/json", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
        //            routes.AddEmbeddedResource("/libs/(?<path>.+\\.js)", "application/javascript", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
        //            routes.AddEmbeddedResource("/libs/(?<path>.+\\.css)", "text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
        //            routes.AddEmbeddedResource("/libs/(?<path>.+\\.png)", "image/png", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.libs"));
        //            routes.AddEmbeddedResource("/themes/(?<path>.+\\.css)", "text/css", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.themes"));
        //            routes.AddEmbeddedResource("/themes/(?<path>.+\\.png)", "image/png", GetExecutingAssembly(), GetContentFolderNamespace("RichFilemanager.themes"));

        //            routes.Add("/config/filemanager.init.js", new EmbeddedResourceDispatcher("application/javascript", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.init.js")));
        //            routes.Add("/config/filemanager.config.json", new EmbeddedResourceDispatcher("application/json", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.config.json")));
        //            routes.Add("/config/filemanager.config.default.json", new EmbeddedResourceDispatcher("application/json", GetExecutingAssembly(), GetContentResourceName("RichFilemanager.config", "filemanager.config.default.json")));

        //            routes.AddCommand("/connectors/php/filemanager.php", context =>
        //           {
        //               //var time = context.Request.GetQuery("time");
        //               var mode = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("mode")?.Result?.FirstOrDefault() : context.Request.GetQuery("mode");
        //               var path = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("path")?.Result?.FirstOrDefault() : context.Request.GetQuery("path");
        //               var name = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("name")?.Result?.FirstOrDefault() : context.Request.GetQuery("name");
        //               var old = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("old")?.Result?.FirstOrDefault() : context.Request.GetQuery("old");
        //               var @new = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("new")?.Result?.FirstOrDefault() : context.Request.GetQuery("new");
        //               var source = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("source")?.Result?.FirstOrDefault() : context.Request.GetQuery("source");
        //               var target = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("target")?.Result?.FirstOrDefault() : context.Request.GetQuery("target");
        //               var content = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("content")?.Result?.FirstOrDefault() : context.Request.GetQuery("content");
        //               var thumbnail = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("thumbnail")?.Result?.FirstOrDefault() : context.Request.GetQuery("thumbnail");
        //               var @string = context.Request.Method == "POST" ? context.Request.GetFormValuesAsync("string")?.Result?.FirstOrDefault() : context.Request.GetQuery("string");
        //#if !NETFULL
        //               var files = context.Request.Method == "POST" ? context.Request.GetFilesAsync("files")?.Result : null;
        //#else
        //               //var files = context.Request.Method == "POST" ? context.Request.GetFilesAsync("files")?.Result : null;
        //#endif
        //               //using (var reader = new System.IO.StreamReader(context.Request.Body, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 4 * 1024, leaveOpen: true))
        //               //{
        //               //    var text = reader.ReadToEndAsync().Result;
        //               //}

        //               //string mode, string path, string name, List<IFormFile> files, string old, string @new, string source, string target, string content, bool thumbnail, string @string
        //               bool.TryParse(thumbnail, out var _thumbnail);
        //               var r = controller.Index(mode, path, name,
        //#if !NETFULL
        //           files,
        //#else
        //           /*files,*/
        //#endif
        //           old, @new, source, target, content, _thumbnail, @string);

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
        //               return true;
        //           });
        //        }
        //        private static System.Reflection.Assembly GetExecutingAssembly()
        //        {
        //            return typeof(ApplicationBuilderExtensions).GetTypeInfo().Assembly;
        //        }
        //        private static string GetContentResourceName(string contentFolder, string resourceName)
        //        {
        //            return $"{GetContentFolderNamespace(contentFolder)}.{resourceName}";
        //        }
        //        private static string GetContentFolderNamespace(string contentFolder)
        //        {
        //            var assemblyName = "FileManagerWebUI";//typeof(ApplicationBuilderExtensions).Namespace
        //            return $"{assemblyName}.Content.{contentFolder}";
        //        }
    }
}
