using System;

namespace FileManager
{
    public static class IbmCosStoreApplicationBuilderExtensions
    {
        /// <summary>注册 IBM Cloud Object Storage。</summary>
        public static FileServicesOptions AddIbmCosStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region = "us-south",
            string endpoint = null,
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = endpoint ?? $"https://s3.{region}.cloud-object-storage.appdomain.cloud";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 IBM Cloud Object Storage。</summary>
        public static FileServicesOptions AddIbmCosStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
