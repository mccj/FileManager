using System;

namespace FileManager
{
    public static class HetznerStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Hetzner 对象存储。</summary>
        public static FileServicesOptions AddHetznerStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region = "fsn1",
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://{region}.your-objectstorage.com";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Hetzner 对象存储。</summary>
        public static FileServicesOptions AddHetznerStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
