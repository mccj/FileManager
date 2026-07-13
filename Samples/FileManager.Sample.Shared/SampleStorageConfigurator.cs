using Amazon;
using Amazon.S3;
using FileManager.FileStorage.Standard;
using FileManager.Storage.AwsS3;
using FileManager.Storage.Compress;
using FileManager.Storage.InMemory;
using FileManager.Storage.Nfs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Sample.Shared
{
  /// <summary>
  /// 根据 <see cref="SampleStorageOptions"/> 将选定的存储后端注册到 <see cref="FileServicesOptions"/>。
  /// </summary>
  public static class SampleStorageConfigurator
  {
    /// <summary>按配置节的 Provider 字段切换并注册存储实现。</summary>
    public static void Configure(FileServicesOptions options, IConfiguration configuration)
    {
      var storage = configuration.GetSection(SampleStorageOptions.SectionName).Get<SampleStorageOptions>() ?? new SampleStorageOptions();
      var provider = storage.Provider?.Trim() ?? "Physical";

      switch (provider.ToLowerInvariant())
      {
        case "physical":
          options.AddRootPhysicalFilePath(new PathString(storage.Physical.Path));
          break;
        case "contentroot":
          options.AddContentRootFileProviderReadOnly();
          break;
        case "webroot":
          options.AddWebRootFileProviderReadOnly();
          break;
        case "compress":
          options.AddCompressStore(sp =>
          {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var archivePath = Path.Combine(env.ContentRootPath, storage.Compress.ArchivePath);
            if (!File.Exists(archivePath))
              throw new FileNotFoundException($"压缩包示例文件不存在: {archivePath}", archivePath);
            return new CompressStore(File.OpenRead(archivePath));
          });
          break;
        case "ftp":
          options.AddFtpStore(storage.Ftp.Host, storage.Ftp.Port, storage.Ftp.User, storage.Ftp.Password);
          break;
        case "webdav":
          options.AddWebDavStore(new Uri(storage.WebDav.Url), storage.WebDav.User, storage.WebDav.Password);
          break;
        case "mongodb":
          options.AddMongoDBStore(storage.MongoDB.ConnectionString);
          break;
        case "inmemory":
          options.AddInMemoryStore(_ =>
          {
            var store = new InMemoryStore();
            SeedInMemoryStore(store);
            return store;
          });
          break;
        case "multidrive":
          options.AddCompositeFileStore(builder =>
          {
            builder.AddDrive("local", sp =>
            {
              var env = sp.GetRequiredService<IWebHostEnvironment>();
              var root = Path.Combine(env.ContentRootPath, storage.MultiDrive.PhysicalPath.TrimStart('/', '\\'));
              Directory.CreateDirectory(root);
              return new FileSystemStore(root);
            });
            builder.AddDrive("memory", _ =>
            {
              var store = new InMemoryStore();
              SeedInMemoryStore(store);
              return store;
            });
          });
          break;
        case "azureblob":
          options.AddAzureBlobStore(storage.AzureBlob.ConnectionString, storage.AzureBlob.ContainerName);
          break;
        case "awss3":
          options.AddAwsS3Store(sp =>
          {
            var aws = storage.AwsS3;
            var config = new AmazonS3Config();
            if (!string.IsNullOrWhiteSpace(aws.Region))
              config.RegionEndpoint = RegionEndpoint.GetBySystemName(aws.Region);

            IAmazonS3 client = string.IsNullOrWhiteSpace(aws.AccessKey)
              ? new AmazonS3Client(config)
              : new AmazonS3Client(aws.AccessKey, aws.SecretKey, config);

            return new AwsS3Store(client, aws.BucketName);
          });
          break;
        case "sftp":
          options.AddSftpStore(storage.Sftp.Host, storage.Sftp.Port, storage.Sftp.User, storage.Sftp.Password, storage.Sftp.Root);
          break;
        case "googlecloud":
          options.AddGoogleCloudStore(storage.GoogleCloud.BucketName);
          break;
        case "redis":
          options.AddRedisStore(storage.Redis.ConnectionString, storage.Redis.Database);
          break;
        case "sqlblob":
          options.AddSqlBlobStore(storage.SqlBlob.ConnectionString);
          break;
        case "minio":
          {
            var minio = storage.MinIO;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = minio.Endpoint;
              o.AccessKey = minio.AccessKey;
              o.SecretKey = minio.SecretKey;
              o.BucketName = minio.BucketName;
              o.ForcePathStyle = true;
              o.UseSsl = minio.UseSsl;
            });
          }
          break;
        case "dropbox":
          options.AddDropboxStore(storage.Dropbox.AccessToken);
          break;
        case "onedrive":
          options.AddOneDriveStore(storage.OneDrive.AccessToken);
          break;
        case "googledrive":
          options.AddGoogleDriveStore(storage.GoogleDrive.AccessToken);
          break;
        case "sharepoint":
          options.AddSharePointStore(storage.SharePoint.SiteUrl, storage.SharePoint.AccessToken);
          break;
        case "smb":
          options.AddSmbStore(storage.Smb.UncPath);
          break;
        case "nfs":
          options.AddNfsStore(sp =>
          {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var mountPath = storage.Nfs.MountPath;
            if (!Path.IsPathRooted(mountPath))
              mountPath = Path.Combine(env.ContentRootPath, mountPath.TrimStart('/', '\\'));
            Directory.CreateDirectory(mountPath);
            return new NfsStore(mountPath);
          });
          break;
        case "postgresqlblob":
          options.AddPostgreSqlBlobStore(storage.PostgreSqlBlob.ConnectionString);
          break;
        case "sqlserverblob":
          options.AddSqlServerBlobStore(storage.SqlServerBlob.ConnectionString);
          break;
        case "elasticsearch":
          options.AddElasticsearchStore(new Uri(storage.Elasticsearch.Endpoint), storage.Elasticsearch.IndexName);
          break;
        case "cassandra":
          options.AddCassandraStore(storage.Cassandra.ContactPoints, storage.Cassandra.Keyspace);
          break;
        case "box":
          options.AddBoxStore(storage.Box.AccessToken);
          break;
        case "backblazeb2":
          {
            var b2 = storage.BackblazeB2;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://s3.{b2.Region}.backblazeb2.com";
              o.AccessKey = b2.KeyId;
              o.SecretKey = b2.ApplicationKey;
              o.BucketName = b2.BucketName;
              o.AuthenticationRegion = b2.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "wasabi":
          {
            var wasabi = storage.Wasabi;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://s3.{wasabi.Region}.wasabisys.com";
              o.AccessKey = wasabi.AccessKey;
              o.SecretKey = wasabi.SecretKey;
              o.BucketName = wasabi.BucketName;
              o.AuthenticationRegion = wasabi.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "oracleobject":
          {
            var oci = storage.OracleObject;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://{oci.Namespace}.compat.objectstorage.{oci.Region}.oraclecloud.com";
              o.AccessKey = oci.AccessKey;
              o.SecretKey = oci.SecretKey;
              o.BucketName = oci.BucketName;
              o.AuthenticationRegion = oci.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "ibmcos":
          {
            var cos = storage.IbmCos;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://s3.{cos.Region}.cloud-object-storage.appdomain.cloud";
              o.AccessKey = cos.AccessKey;
              o.SecretKey = cos.SecretKey;
              o.BucketName = cos.BucketName;
              o.AuthenticationRegion = cos.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "ldap":
          {
            var ldap = storage.Ldap;
            options.AddLdapStore(ldap.Host, ldap.Port, ldap.BaseDn, ldap.BindDn, ldap.BindPassword, ldap.UseSsl);
          }
          break;
        case "digitaloceanspaces":
          {
            var dos = storage.DigitalOceanSpaces;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://{dos.Region}.digitaloceanspaces.com";
              o.AccessKey = dos.AccessKey;
              o.SecretKey = dos.SecretKey;
              o.BucketName = dos.BucketName;
              o.AuthenticationRegion = dos.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "cloudflarer2":
          {
            var r2 = storage.CloudflareR2;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://{r2.AccountId}.r2.cloudflarestorage.com";
              o.AccessKey = r2.AccessKey;
              o.SecretKey = r2.SecretKey;
              o.BucketName = r2.BucketName;
              o.AuthenticationRegion = "auto";
              o.ForcePathStyle = true;
            });
          }
          break;
        case "scaleway":
          {
            var scw = storage.Scaleway;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://s3.{scw.Region}.scw.cloud";
              o.AccessKey = scw.AccessKey;
              o.SecretKey = scw.SecretKey;
              o.BucketName = scw.BucketName;
              o.AuthenticationRegion = scw.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "hetzner":
          {
            var hz = storage.Hetzner;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = $"https://{hz.Region}.your-objectstorage.com";
              o.AccessKey = hz.AccessKey;
              o.SecretKey = hz.SecretKey;
              o.BucketName = hz.BucketName;
              o.AuthenticationRegion = hz.Region;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "openstackswift":
          {
            var swift = storage.OpenStackSwift;
            options.AddS3CompatibleStore(o =>
            {
              o.ServiceUrl = swift.Endpoint;
              o.AccessKey = swift.AccessKey;
              o.SecretKey = swift.SecretKey;
              o.BucketName = swift.BucketName;
              o.ForcePathStyle = true;
            });
          }
          break;
        case "mysqlblob":
          options.AddMySqlBlobStore(storage.MySqlBlob.ConnectionString);
          break;
        case "rabbitmq":
          options.AddRabbitMqStore(storage.RabbitMq.ConnectionString);
          break;
        case "kafka":
          options.AddKafkaStore(storage.Kafka.BootstrapServers, storage.Kafka.Topic);
          break;
        case "memcached":
          options.AddMemcachedStore(storage.Memcached.Host, storage.Memcached.Port);
          break;
        case "influxdb":
          options.AddInfluxDbStore(
              storage.InfluxDb.Url,
              storage.InfluxDb.Token,
              storage.InfluxDb.Org,
              storage.InfluxDb.Bucket);
          break;
        default:
          throw new InvalidOperationException(
              $"Unknown storage provider: {provider}. See Samples/ui-and-storage.md for valid values.");
      }

      if (!IsReadOnlyProvider(provider))
        options.AddFullAccessPermissionHandle();
    }

    private static void SeedInMemoryStore(InMemoryStore store)
    {
      store.TryCreateDirectoryAsync("documents").GetAwaiter().GetResult();
      store.CreateFileFromStreamAsync(
        "documents/welcome.txt",
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes("In-memory drive sample file.")),
        overwrite: true).GetAwaiter().GetResult();
    }

    /// <summary>判断存储提供程序是否为只读演示模式。</summary>
    public static bool IsReadOnlyProvider(string provider)
    {
      switch (provider?.Trim().ToLowerInvariant())
      {
        case "contentroot":
        case "webroot":
        case "compress":
        case "ldap":
          return true;
        default:
          return false;
      }
    }
  }
}
