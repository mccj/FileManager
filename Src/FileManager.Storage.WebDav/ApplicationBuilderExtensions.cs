using System;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager
{
    /// <summary>
    /// WebDAV 存储注册扩展。
    /// </summary>
    public static class ApplicationBuilderExtensions
    {
        /// <summary>注册 WebDAV 存储。</summary>
        public static FileServicesOptions AddWebDavStore(this FileServicesOptions app, System.Func<System.IServiceProvider, FileStorage.WebDav.WebDavStore> func)
        {
            app.Services.AddSingleton<FileStorage.IFileStore>((sp) =>
            {
                return func(sp);
            });

            return app;
        }
        /// <summary>注册 WebDAV 存储。</summary>
        public static FileServicesOptions AddWebDavStore(this FileServicesOptions app, Uri url, string user, string pass)
        {
            return AddWebDavStore(app, sp => new FileStorage.WebDav.WebDavStore(url,  user, pass));
        }
    }
}
