using System;

namespace FileManager
{
    public static class DigitalOceanSpacesStoreApplicationBuilderExtensions
    {
        /// <summary>注册 DigitalOcean Spaces 存储。</summary>
        public static FileServicesOptions AddDigitalOceanSpacesStore(
            this FileServicesOptions app,
            string accessKey,
            string secretKey,
            string bucketName,
            string region = "nyc3",
            bool useSsl = true)
        {
            return app.AddS3CompatibleStore(o =>
            {
                o.ServiceUrl = $"https://{region}.digitaloceanspaces.com";
                o.AccessKey = accessKey;
                o.SecretKey = secretKey;
                o.BucketName = bucketName;
                o.AuthenticationRegion = region;
                o.ForcePathStyle = true;
                o.UseSsl = useSsl;
            });
        }

        /// <summary>注册 DigitalOcean Spaces 存储。</summary>
        public static FileServicesOptions AddDigitalOceanSpacesStore(this FileServicesOptions app, Func<System.IServiceProvider, Storage.AwsS3.AwsS3Store> factory)
        {
            return app.AddFileStore(sp => factory(sp));
        }
    }
}
