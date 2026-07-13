using System;

namespace FileManager
{
    public static class OracleObjectStoreApplicationBuilderExtensions
    {
        /// <summary>注册 Oracle Cloud Object Storage。</summary>
        public static FileServicesOptions AddOracleObjectStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region,
            string namespaceName,
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://{namespaceName}.compat.objectstorage.{region}.oraclecloud.com";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 Oracle Cloud Object Storage。</summary>
        public static FileServicesOptions AddOracleObjectStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
