using FileManager;
using Microsoft.Owin;
using Owin;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleOwinTest
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var filesRoot = OwinApplicationBuilderExtensions.ResolveContentPath("App_Data\\Files");
            Directory.CreateDirectory(filesRoot);

            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (path == "/" || path == string.Empty)
                {
                    context.Response.Redirect("/Browser/");
                    return;
                }

                await next();
            });

            app.UseFileManager("/Browser", filesRoot);
        }
    }
}
