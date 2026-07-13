using System;

namespace FileManager
{
    public static class OpenStackSwiftStoreApplicationBuilderExtensions
    {
        /// <summary>注册 OpenStack Swift 存储。</summary>
        public static FileServicesOptions AddOpenStackSwiftStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string endpoint,
            bool useSsl = true)
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

        /// <summary>注册 OpenStack Swift 存储。</summary>
        public static FileServicesOptions AddOpenStackSwiftStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
