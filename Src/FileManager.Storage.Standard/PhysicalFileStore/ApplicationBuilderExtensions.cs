using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 物理路径存储注册扩展。
    /// </summary>
    public static partial class ApplicationBuilderExtensions
    {
        //public static FileServicesOptions AddWebDavStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.WebDav.WebDavStore> func)
        //{
        //    app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
        //    {
        //        return func(sp);
        //    });

        //    return app;
        //}
        //public static FileServicesOptions AddWebDavStore(this FileServicesOptions app, Uri url, string user, string pass)
        //{
        //    return AddWebDavStore(app, sp => new FileStorage.WebDav.WebDavStore(url,  user, pass));
        //}
    }
}
