using Amazon.S3;
using FileManager.Storage.AwsS3;
using System;

namespace FileManager
{
    public static class AwsS3StoreApplicationBuilderExtensions
    {
        /// <summary>注册 AWS S3 对象存储。</summary>
        public static FileServicesOptions AddAwsS3Store(this FileServicesOptions app, Func<System.IServiceProvider, AwsS3Store> factory)
        {
            return app.AddFileStore(factory);
        }

        /// <summary>注册 AWS S3 对象存储。</summary>
        public static FileServicesOptions AddAwsS3Store(this FileServicesOptions app, IAmazonS3 client, string bucketName)
        {
            return app.AddAwsS3Store(_ => new AwsS3Store(client, bucketName));
        }
    }
}
