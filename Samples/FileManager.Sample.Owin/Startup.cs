using FileManager;
using Microsoft.Owin;
using Owin;
using System.Threading.Tasks;

namespace FileManager.Sample.Owin
{
    /// <summary>
    /// OWIN 管道配置：导航首页、/Browser 兼容重定向、FileExplorer 挂载。
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 首页导航与 /Browser → /Explorer 重定向（OWIN 仅支持 FileExplorer UI）
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value ?? string.Empty;
                if (path == "/" || path == string.Empty)
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    await context.Response.WriteAsync(OwinHubPage.Render(OwinSampleStorage.CurrentProvider));
                    return;
                }

                if (path.StartsWith("/Browser", System.StringComparison.OrdinalIgnoreCase))
                {
                    var suffix = path.Length > "/Browser".Length ? path.Substring("/Browser".Length) : "/";
                    if (suffix == string.Empty)
                        suffix = "/";
                    context.Response.Redirect("/Explorer" + suffix + context.Request.QueryString);
                    return;
                }

                await next();
            });

            OwinApplicationBuilderExtensions.UseFileManager(
                app,
                "/Explorer",
                OwinSampleStorage.Configure,
                o => o.UseFileExplorerUI());
        }
    }
}
