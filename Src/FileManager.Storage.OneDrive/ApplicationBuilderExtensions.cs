using FileManager.Storage.OneDrive;
using System;
using System.Net.Http;

namespace FileManager
{
    public static class OneDriveStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Microsoft OneDrive 存储。</summary>
        public static FileServicesOptions AddOneDriveStore(this FileServicesOptions app, string accessToken)
        {
            return app.AddFileStore(_ => new OneDriveStore(accessToken));
        }

        /// <summary>注册 Microsoft OneDrive 存储。</summary>
        public static FileServicesOptions AddOneDriveStore(this FileServicesOptions app, string accessToken, HttpClient httpClient)
        {
            return app.AddFileStore(_ => new OneDriveStore(accessToken, httpClient));
        }

        /// <summary>注册 Microsoft OneDrive 存储。</summary>
        public static FileServicesOptions AddOneDriveStore(this FileServicesOptions app, Func<System.IServiceProvider, OneDriveStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
