using Amazon.S3;
using FileManager.Storage.AwsS3;
using System;

namespace FileManager
{
    public sealed class S3CompatibleStoreOptions
    {
        public string ServiceUrl { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string BucketName { get; set; }
        public bool ForcePathStyle { get; set; } = true;
        public string AuthenticationRegion { get; set; }
        public bool UseSsl { get; set; } = true;
    }

    public static class S3CompatibleStoreApplicationBuilderExtensions
    {
        public static FileServicesOptions AddS3CompatibleStore(
            this FileServicesOptions app,
            Action<S3CompatibleStoreOptions> configure)
        {
            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            var options = new S3CompatibleStoreOptions();
            configure(options);
            return app.AddS3CompatibleStore(options);
        }

        public static FileServicesOptions AddS3CompatibleStore(
            this FileServicesOptions app,
            S3CompatibleStoreOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.ServiceUrl))
                throw new ArgumentException("ServiceUrl is required.", nameof(options));
            if (string.IsNullOrWhiteSpace(options.BucketName))
                throw new ArgumentException("BucketName is required.", nameof(options));

            return app.AddAwsS3Store(_ =>
            {
                var config = new AmazonS3Config
                {
                    ServiceURL = options.ServiceUrl,
                    ForcePathStyle = options.ForcePathStyle,
                    UseHttp = !options.UseSsl
                };
                if (!string.IsNullOrWhiteSpace(options.AuthenticationRegion))
                    config.AuthenticationRegion = options.AuthenticationRegion;

                var client = string.IsNullOrWhiteSpace(options.AccessKey)
                    ? new AmazonS3Client(config)
                    : new AmazonS3Client(options.AccessKey, options.SecretKey, config);
                return new AwsS3Store(client, options.BucketName);
            });
        }
    }
}
