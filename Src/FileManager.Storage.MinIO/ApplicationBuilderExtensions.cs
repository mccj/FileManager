using System;

namespace FileManager
{
    public static class MinIOStoreApplicationBuilderExtensions
    {
        /// <summary>注册 MinIO 对象存储。</summary>
        public static FileServicesOptions AddMinIOStore(
            this FileServicesOptions app,
            string endpoint,
            string accessKey,
            string secretKey,
            string bucketName,
            bool useSsl = false)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = endpoint;
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 MinIO 对象存储。</summary>
        public static FileServicesOptions AddMinIOStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
