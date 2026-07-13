using System;

namespace FileManager
{
    public static class WasabiStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Wasabi 对象存储。</summary>
        public static FileServicesOptions AddWasabiStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region = "us-east-1",
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://s3.{region}.wasabisys.com";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Wasabi 对象存储。</summary>
        public static FileServicesOptions AddWasabiStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
