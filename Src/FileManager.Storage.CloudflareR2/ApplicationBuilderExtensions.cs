using System;

namespace FileManager
{
    public static class CloudflareR2StoreApplicationBuilderExtensions
    {
        /// <summary>注册 Cloudflare R2 存储。</summary>
        public static FileServicesOptions AddCloudflareR2Store(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string accountId,
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://{accountId}.r2.cloudflarestorage.com";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = "auto";
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Cloudflare R2 存储。</summary>
        public static FileServicesOptions AddCloudflareR2Store(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
