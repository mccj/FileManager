using FileManager.Storage.GoogleCloud;
using Google.Cloud.Storage.V1;
using System;

namespace FileManager
{
    public static class GoogleCloudStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Google Cloud Storage。</summary>
        public static FileServicesOptions AddGoogleCloudStore(this FileServicesOptions app, string bucketName)
        {
            return app.AddFileStore(_ => new GoogleCloudStore(StorageClient.Create(), bucketName));
        }

        /// <summary>注册 Google Cloud Storage。</summary>
        public static FileServicesOptions AddGoogleCloudStore(this FileServicesOptions app, StorageClient client, string bucketName)
        {
            return app.AddFileStore(_ => new GoogleCloudStore(client, bucketName));
        }

        /// <summary>注册 Google Cloud Storage。</summary>
        public static FileServicesOptions AddGoogleCloudStore(this FileServicesOptions app, Func<System.IServiceProvider, GoogleCloudStore> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
