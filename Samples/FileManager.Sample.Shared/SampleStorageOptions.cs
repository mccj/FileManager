namespace FileManager.Sample.Shared

{

  /// <summary>
  /// 示例应用的存储配置根节点，绑定 <c>FileManager:Storage</c> 配置节。
  /// </summary>
  public class SampleStorageOptions

  {

    public const string SectionName = "FileManager:Storage";



    public string Provider { get; set; } = "Physical";



    public PhysicalStorageOptions Physical { get; set; } = new();



    public CompressStorageOptions Compress { get; set; } = new();



    public FtpStorageOptions Ftp { get; set; } = new();



    public WebDavStorageOptions WebDav { get; set; } = new();



    public MongoDbStorageOptions MongoDB { get; set; } = new();



    public InMemoryStorageOptions InMemory { get; set; } = new();



    public MultiDriveStorageOptions MultiDrive { get; set; } = new();



    public AzureBlobStorageOptions AzureBlob { get; set; } = new();



    public AwsS3StorageOptions AwsS3 { get; set; } = new();

    public SftpStorageOptions Sftp { get; set; } = new();

    public GoogleCloudStorageOptions GoogleCloud { get; set; } = new();

    public RedisStorageOptions Redis { get; set; } = new();

    public SqlBlobStorageOptions SqlBlob { get; set; } = new();

    public MinIOStorageOptions MinIO { get; set; } = new();

    public DropboxStorageOptions Dropbox { get; set; } = new();

    public OneDriveStorageOptions OneDrive { get; set; } = new();

    public GoogleDriveStorageOptions GoogleDrive { get; set; } = new();

    public SharePointStorageOptions SharePoint { get; set; } = new();

    public SmbStorageOptions Smb { get; set; } = new();

    public NfsStorageOptions Nfs { get; set; } = new();

    public PostgreSqlBlobStorageOptions PostgreSqlBlob { get; set; } = new();

    public SqlServerBlobStorageOptions SqlServerBlob { get; set; } = new();

    public ElasticsearchStorageOptions Elasticsearch { get; set; } = new();

    public CassandraStorageOptions Cassandra { get; set; } = new();

    public BoxStorageOptions Box { get; set; } = new();

    public BackblazeB2StorageOptions BackblazeB2 { get; set; } = new();

    public WasabiStorageOptions Wasabi { get; set; } = new();

    public OracleObjectStorageOptions OracleObject { get; set; } = new();

    public IbmCosStorageOptions IbmCos { get; set; } = new();

    public RabbitMqStorageOptions RabbitMq { get; set; } = new();

    public KafkaStorageOptions Kafka { get; set; } = new();

    public LdapStorageOptions Ldap { get; set; } = new();

    public DigitalOceanSpacesStorageOptions DigitalOceanSpaces { get; set; } = new();

    public CloudflareR2StorageOptions CloudflareR2 { get; set; } = new();

    public ScalewayStorageOptions Scaleway { get; set; } = new();

    public HetznerStorageOptions Hetzner { get; set; } = new();

    public OpenStackSwiftStorageOptions OpenStackSwift { get; set; } = new();

    public MySqlBlobStorageOptions MySqlBlob { get; set; } = new();

    public InfluxDbStorageOptions InfluxDb { get; set; } = new();

    public MemcachedStorageOptions Memcached { get; set; } = new();

  }



  public class PhysicalStorageOptions

  {

    public string Path { get; set; } = "/App_Data/Files";

  }



  public class CompressStorageOptions

  {

    public string ArchivePath { get; set; } = "App_Data/sample.zip";

  }



  public class FtpStorageOptions

  {

    public string Host { get; set; } = "ftp://127.0.0.1/";



    public int Port { get; set; } = 21;



    public string User { get; set; } = "anonymous";



    public string Password { get; set; } = "";

  }



  public class WebDavStorageOptions

  {

    public string Url { get; set; } = "http://127.0.0.1/webdav/";



    public string User { get; set; } = "";



    public string Password { get; set; } = "";

  }



  public class MongoDbStorageOptions

  {

    public string ConnectionString { get; set; } = "mongodb://127.0.0.1:27017/";

  }



  public class InMemoryStorageOptions

  {

  }



  public class MultiDriveStorageOptions

  {

    public string PhysicalPath { get; set; } = "App_Data/Files";

  }



  public class AzureBlobStorageOptions

  {

    public string ConnectionString { get; set; } = "UseDevelopmentStorage=true";



    public string ContainerName { get; set; } = "filemanager";

  }



  public class AwsS3StorageOptions

  {

    public string BucketName { get; set; } = "filemanager-demo";



    public string Region { get; set; } = "us-east-1";



    public string AccessKey { get; set; } = "";



    public string SecretKey { get; set; } = "";

  }

  public class SftpStorageOptions
  {
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 22;

    public string User { get; set; } = "";

    public string Password { get; set; } = "";

    public string Root { get; set; } = "/";
  }

  public class GoogleCloudStorageOptions
  {
    public string BucketName { get; set; } = "filemanager-demo";
  }

  public class RedisStorageOptions
  {
    public string ConnectionString { get; set; } = "localhost:6379";

    public int Database { get; set; } = 0;
  }

  public class SqlBlobStorageOptions
  {
    public string ConnectionString { get; set; } = "Data Source=filemanager.db";
  }

  public class MinIOStorageOptions
  {
    public string Endpoint { get; set; } = "http://127.0.0.1:9000";

    public string BucketName { get; set; } = "filemanager";

    public string AccessKey { get; set; } = "minioadmin";

    public string SecretKey { get; set; } = "minioadmin";

    public bool UseSsl { get; set; }
  }

  public class DropboxStorageOptions
  {
    public string AccessToken { get; set; } = "";
  }

  public class OneDriveStorageOptions
  {
    public string AccessToken { get; set; } = "";
  }

  public class GoogleDriveStorageOptions
  {
    public string AccessToken { get; set; } = "";
  }

  public class SharePointStorageOptions
  {
    public string SiteUrl { get; set; } = "https://contoso.sharepoint.com/sites/demo";

    public string AccessToken { get; set; } = "";
  }

  public class SmbStorageOptions
  {
    public string UncPath { get; set; } = @"\\server\share";
  }

  public class NfsStorageOptions
  {
    public string MountPath { get; set; } = "/mnt/nfs";
  }

  public class PostgreSqlBlobStorageOptions
  {
    public string ConnectionString { get; set; } = "Host=127.0.0.1;Database=filemanager;Username=postgres;Password=postgres";
  }

  public class SqlServerBlobStorageOptions
  {
    public string ConnectionString { get; set; } = "Server=127.0.0.1;Database=filemanager;User Id=sa;Password=Your_password123;TrustServerCertificate=True";
  }

  public class ElasticsearchStorageOptions
  {
    public string Endpoint { get; set; } = "http://127.0.0.1:9200";

    public string IndexName { get; set; } = "fm_nodes";
  }

  public class CassandraStorageOptions
  {
    public string ContactPoints { get; set; } = "127.0.0.1";

    public string Keyspace { get; set; } = "filemanager";
  }

  public class BoxStorageOptions
  {
    public string AccessToken { get; set; } = "";
  }

  public class BackblazeB2StorageOptions
  {
    public string KeyId { get; set; } = "";

    public string ApplicationKey { get; set; } = "";

    public string BucketName { get; set; } = "filemanager";

    public string Region { get; set; } = "us-west-004";
  }

  public class WasabiStorageOptions
  {
    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string BucketName { get; set; } = "filemanager";

    public string Region { get; set; } = "us-east-1";
  }

  public class OracleObjectStorageOptions
  {
    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string BucketName { get; set; } = "filemanager";

    public string Region { get; set; } = "us-ashburn-1";

    public string Namespace { get; set; } = "";
  }

  public class IbmCosStorageOptions
  {
    public string AccessKey { get; set; } = "";

    public string SecretKey { get; set; } = "";

    public string BucketName { get; set; } = "filemanager";

    public string Region { get; set; } = "us-south";
  }

  public class RabbitMqStorageOptions
  {
    public string ConnectionString { get; set; } = "amqp://guest:guest@localhost:5672/";
  }

  public class KafkaStorageOptions
  {
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string Topic { get; set; } = "fm-files";
  }

  public class LdapStorageOptions
  {
    public string Host { get; set; } = "127.0.0.1";

    public int Port { get; set; } = 389;

    public string BaseDn { get; set; } = "dc=example,dc=com";

    public string BindDn { get; set; } = "";

    public string BindPassword { get; set; } = "";

    public bool UseSsl { get; set; }
  }

  public class DigitalOceanSpacesStorageOptions
  {
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "filemanager";
    public string Region { get; set; } = "nyc3";
  }

  public class CloudflareR2StorageOptions
  {
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "filemanager";
    public string AccountId { get; set; } = "";
  }

  public class ScalewayStorageOptions
  {
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "filemanager";
    public string Region { get; set; } = "fr-par";
  }

  public class HetznerStorageOptions
  {
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "filemanager";
    public string Region { get; set; } = "fsn1";
  }

  public class OpenStackSwiftStorageOptions
  {
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public string BucketName { get; set; } = "filemanager";
    public string Endpoint { get; set; } = "https://swift.example.com";
  }

  public class MySqlBlobStorageOptions
  {
    public string ConnectionString { get; set; } = "Server=127.0.0.1;Database=filemanager;User=root;Password=password;";
  }

  public class InfluxDbStorageOptions
  {
    public string Url { get; set; } = "http://127.0.0.1:8086";
    public string Token { get; set; } = "";
    public string Org { get; set; } = "filemanager";
    public string Bucket { get; set; } = "fm-files";
  }

  public class MemcachedStorageOptions
  {
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 11211;
  }

}

