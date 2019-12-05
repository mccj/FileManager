using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// 
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
