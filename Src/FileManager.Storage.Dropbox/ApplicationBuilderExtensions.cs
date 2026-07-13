using FileManager.Storage.Dropbox;
using System;
using System.Net.Http;

namespace FileManager
{
    public static class DropboxStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Dropbox 存储。</summary>
        public static FileServicesOptions AddDropboxStore(this FileServicesOptions app, string accessToken)
        {
            return app.AddFileStore(_ => new DropboxStore(accessToken));
        }

        /// <summary>注册 Dropbox 存储。</summary>
        public static FileServicesOptions AddDropboxStore(this FileServicesOptions app, string accessToken, HttpClient httpClient)
        {
            return app.AddFileStore(_ => new DropboxStore(accessToken, httpClient));
        }

        /// <summary>注册 Dropbox 存储。</summary>
        public static FileServicesOptions AddDropboxStore(this FileServicesOptions app, Func<System.IServiceProvider, DropboxStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
