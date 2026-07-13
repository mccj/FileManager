using Azure.Storage.Blobs;
using FileManager.Storage.AzureBlob;
using System;

namespace FileManager
{
    public static class AzureBlobStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Azure Blob 存储。</summary>
        public static FileServicesOptions AddAzureBlobStore(this FileServicesOptions app, Func<System.IServiceProvider, AzureBlobStore> factory)
        {
            return app.AddFileStore(factory);
        }

        /// <summary>注册 Azure Blob 存储。</summary>
        public static FileServicesOptions AddAzureBlobStore(this FileServicesOptions app, string connectionString, string containerName)
        {
            return app.AddAzureBlobStore(_ => new AzureBlobStore(connectionString, containerName));
        }

        /// <summary>注册 Azure Blob 存储。</summary>
        public static FileServicesOptions AddAzureBlobStore(this FileServicesOptions app, BlobContainerClient containerClient)
        {
            return app.AddAzureBlobStore(_ => new AzureBlobStore(containerClient));
        }
    }
}
