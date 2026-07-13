using System;

namespace FileManager
{
    public static class BackblazeB2StoreApplicationBuilderExtensions
    {
        /// <summary>注册 Backblaze B2 存储。</summary>
        public static FileServicesOptions AddBackblazeB2Store(
            this FileServicesOptions app,
            string keyId,
            string applicationKey,
            string bucketName,
            string region = "us-west-004",
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://s3.{region}.backblazeb2.com";
                o.AccessKey = keyId;
                o.SecretKey = applicationKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Backblaze B2 存储。</summary>
        public static FileServicesOptions AddBackblazeB2Store(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
