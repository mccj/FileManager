using FileManager.Storage.GoogleDrive;
using System;
using System.Net.Http;

namespace FileManager
{
    public static class GoogleDriveStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Google Drive 存储。</summary>
        public static FileServicesOptions AddGoogleDriveStore(this FileServicesOptions app, string accessToken)
        {
            return app.AddFileStore(_ => new GoogleDriveStore(accessToken));
        }

        /// <summary>注册 Google Drive 存储。</summary>
        public static FileServicesOptions AddGoogleDriveStore(this FileServicesOptions app, string accessToken, HttpClient httpClient)
        {
            return app.AddFileStore(_ => new GoogleDriveStore(accessToken, httpClient));
        }

        /// <summary>注册 Google Drive 存储。</summary>
        public static FileServicesOptions AddGoogleDriveStore(this FileServicesOptions app, Func<System.IServiceProvider, GoogleDriveStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
