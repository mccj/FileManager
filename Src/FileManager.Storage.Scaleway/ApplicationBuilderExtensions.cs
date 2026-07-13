using System;

namespace FileManager
{
    public static class ScalewayStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Scaleway 对象存储。</summary>
        public static FileServicesOptions AddScalewayStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region = "fr-par",
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://s3.{region}.scw.cloud";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Scaleway 对象存储。</summary>
        public static FileServicesOptions AddScalewayStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
