using AspNetCoreDashboard;
using AspNetCoreDashboard.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public static void UseRichFileManagerUI(this FileManagerOptions app)
        {
            app.Fun = new System.Action(() =>
            {
                app.ApplicationBuilder.UseFileManager(app.PathMatch);
            });
        }
    }
}
