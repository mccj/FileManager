# 存储提供程序

所有存储后端均实现 `FileManager.FileStorage.IFileStore` 接口，通过 `FileServicesOptions` 上的扩展方法注册。

## 概览

| 包 | 类 | 读写 | 说明 |
|----|-----|------|------|
| FileManager.Storage.Standard | `FileSystemStore` | 读写 | 本地物理文件系统 |
| FileManager.Storage.Standard | `FileProviderStore` | 只读 | 基于 `IFileProvider` |
| FileManager.Storage.Standard | `PhysicalFileSystemStore` | 读写 | 物理路径（另一实现） |
| FileManager.Storage.Ftp | `FtpStore` | 读写 | FTP 服务器 |
| FileManager.Storage.WebDav | `WebDavStore` | 读写 | WebDAV 协议 |
| FileManager.Storage.Compress | `CompressStore` | 只读 | ZIP 等压缩包 |
| FileManager.Storage.MongoDB | `MongoDBStore` | 读写 | MongoDB GridFS |
| FileManager.Storage.InMemory | `InMemoryStore` | 读写 | 内存存储（测试/演示） |
| FileManager.Storage.Composite | `CompositeFileStore` | 读写 | 多 Drive 组合 |
| FileManager.Storage.AzureBlob | `AzureBlobStore` | 读写 | Azure Blob Storage |
| FileManager.Storage.AwsS3 | `AwsS3Store` | 读写 | AWS S3 |
| FileManager.Storage.Sftp | `SftpStore` | 读写 | SFTP（SSH.NET） |
| FileManager.Storage.GoogleCloud | `GoogleCloudStore` | 读写 | Google Cloud Storage |
| FileManager.Storage.Redis | `RedisStore` | 读写 | Redis 键值存储 |
| FileManager.Storage.SqlBlob | `SqlBlobStore` | 读写 | SQL BLOB（SQLite 等） |
| FileManager.Storage.MinIO | — | 读写 | MinIO（S3 兼容，封装 AwsS3Store） |
| FileManager.Storage.Dropbox | `DropboxStore` | 读写 | Dropbox API |
| FileManager.Storage.OneDrive | `OneDriveStore` | 读写 | Microsoft Graph OneDrive |
| FileManager.Storage.GoogleDrive | `GoogleDriveStore` | 读写 | Google Drive API |
| FileManager.Storage.SharePoint | `SharePointStore` | 读写 | SharePoint 文档库 |
| FileManager.Storage.Smb | `SmbStore` | 读写 | SMB/UNC 路径 |
| FileManager.Storage.Nfs | `NfsStore` | 读写 | NFS 挂载点 |
| FileManager.Storage.PostgreSqlBlob | `PostgreSqlBlobStore` | 读写 | PostgreSQL BLOB |
| FileManager.Storage.SqlServerBlob | `SqlServerBlobStore` | 读写 | SQL Server BLOB |
| FileManager.Storage.Elasticsearch | `ElasticsearchStore` | 读写 | Elasticsearch 索引 |
| FileManager.Storage.Cassandra | `CassandraStore` | 读写 | Apache Cassandra |

## FileManager.Storage.Standard

### FileSystemStore — 本地物理路径

直接映射到服务器文件系统目录，支持完整的 CRUD 操作。

```csharp
services.AddFileManager(f => f
    .AddPhysicalFilePath(@"D:\Data\Files")
    // 或相对于 ContentRoot
    .AddRootPhysicalFilePath("/App_Data/Files")
);
```

实现：`Src/FileManager.Storage.Standard/FileSystemStore/FileSystemStore.cs`

### FileProviderStore — IFileProvider（只读）

将 ASP.NET Core 的 `IFileProvider` 适配为 `IFileStore`，适用于只读场景。

```csharp
// WebRoot（wwwroot）只读
services.AddFileManager(f => f.AddWebRootFileProviderReadOnly());

// ContentRoot 只读
services.AddFileManager(f => f.AddContentRootFileProviderReadOnly());

// 自定义物理路径只读
services.AddFileManager(f => f.AddFileProviderPhysicalPathReadOnly(@"D:\ReadOnly"));

// 自定义 IFileProvider
services.AddFileManager(f => f
    .AddFileProviderReadOnly(sp => new PhysicalFileProvider("/path"))
);
```

扩展方法位于 `Src/FileManager.Storage.Standard/FileProviderStore/ApplicationBuilderExtensions.cs`。

> Azure Blob、AWS S3 等云存储后端见 [其他存储后端](#其他存储后端) 一节。

## FileManager.Storage.Ftp

通过 FTP 协议访问远程文件服务器，内部使用 FluentFTP 实现。

```csharp
services.AddFileManager(f => f
    .AddFtpStore("ftp://host.example.com/", 21, "user", "pass")
);

// 或使用工厂方法自定义
services.AddFileManager(f => f
    .AddFtpStore(sp => new FileStorage.Ftp.FtpStore(host, port, user, pass))
);
```

实现：`Src/FileManager.Storage.Ftp/FtpStore.FluentFTP/FtpStore.cs`

## FileManager.Storage.WebDav

通过 WebDAV 协议访问远程存储（如 Nextcloud、ownCloud）。

```csharp
services.AddFileManager(f => f
    .AddWebDavStore(
        new Uri("https://cloud.example.com/remote.php/webdav/"),
        "username",
        "password")
);
```

实现：`Src/FileManager.Storage.WebDav/WebDavStore.cs`

## FileManager.Storage.Compress

将 ZIP（及其他 SharpCompress 支持的格式）压缩包作为只读虚拟文件系统。

```csharp
services.AddFileManager(f => f
    .AddCompressStore(File.OpenRead("archive.zip"))
);

// 带密码的压缩包
services.AddFileManager(f => f
    .AddCompressStore(File.OpenRead("protected.zip"), password: "secret")
);
```

特点：

- 只读，不支持写入、删除、创建
- 支持遍历压缩包内目录结构
- 使用 SharpCompress 库解析

实现：`Src/FileManager.Storage.Compress/CompressStore.cs`

## FileManager.Storage.MongoDB

使用 MongoDB GridFS 存储文件，适合分布式或无共享文件系统的场景。

```csharp
// 连接字符串
services.AddFileManager(f => f
    .AddMongoDBStore("mongodb://localhost:27017/mydb")
);

// MongoUrl
services.AddFileManager(f => f
    .AddMongoDBStore(new MongoUrl("mongodb://localhost:27017"))
);

// 已有 MongoClient
services.AddFileManager(f => f
    .AddMongoDBStore(mongoClient)
);
```

实现：`Src/FileManager.Storage.MongoDB/MongoDBStoreEntry.cs`（及对应 Store 类）

## 其他存储后端

以下包通过相同的 `AddFileManager` 扩展注册，详见 [UI 与存储组合](../Samples/ui-and-storage.md)：

| 包 | 注册方法 | 说明 |
|----|----------|------|
| FileManager.Storage.InMemory | `AddInMemoryStore()` | 内存存储，适合单元测试 |
| FileManager.Storage.Composite | `AddCompositeFileStore(...)` | 多 Drive，路径格式 `drive/folder/file` |
| FileManager.Storage.AzureBlob | `AddAzureBlobStore(...)` | Azure Blob Storage |
| FileManager.Storage.AwsS3 | `AddAwsS3Store(...)` | AWS S3 |
| FileManager.Storage.Sftp | `AddSftpStore(host, port, user, password, root)` | SFTP |
| FileManager.Storage.GoogleCloud | `AddGoogleCloudStore(bucketName)` | Google Cloud Storage |
| FileManager.Storage.Redis | `AddRedisStore(connectionString, database)` | Redis |
| FileManager.Storage.SqlBlob | `AddSqlBlobStore(connectionString)` | SQL BLOB（默认 SQLite） |
| FileManager.Storage.MinIO | `AddMinIOStore(endpoint, accessKey, secretKey, bucket, useSsl)` | MinIO S3 兼容 |

## 自定义存储后端

实现 `IFileStore` 和 `IFileStoreEntry`，然后注册：

```csharp
public class MyCustomStore : IFileStore
{
    // 实现所有接口方法...
}

services.AddFileManager(f => f
    .AddFileStore(sp => new MyCustomStore())
);
```

`AddFileStore` 定义于 `Src/FileManager.Core/ApplicationBuilderExtensions.cs`。

## 选择建议

| 场景 | 推荐后端 |
|------|----------|
| 单机 Web 应用文件管理 | `AddRootPhysicalFilePath` |
| 只浏览 wwwroot 静态资源 | `AddWebRootFileProviderReadOnly` |
| 远程 FTP 服务器 | `AddFtpStore` |
| 云盘 / Nextcloud | `AddWebDavStore` |
| 浏览 ZIP 备份包 | `AddCompressStore` |
| 分布式文件存储 | `AddMongoDBStore` |
| 对象存储（Azure / AWS / GCS / MinIO） | `AddAzureBlobStore` / `AddAwsS3Store` / `AddGoogleCloudStore` / `AddMinIOStore` |
| S3 兼容自定义端点 | `AddS3CompatibleStore(options => …)`（MinIO/Wasabi/R2 等薄封装均委托此方法） |
| 远程 SFTP | `AddSftpStore` |
| 缓存/临时存储 | `AddRedisStore` / `AddInMemoryStore` |
| 关系型 BLOB | `AddSqlBlobStore` |

## 实验性后端（不推荐生产）

以下包实现 `IFileStore` 但语义偏离真实文件存储，**未包含在 *·Full 示例默认构建**中。使用时需单独引用对应 NuGet/项目：

| 包 | 说明 |
|----|------|
| `FileManager.Storage.Kafka` | 消息队列模拟文件系统 |
| `FileManager.Storage.RabbitMq` | 同上 |
| `FileManager.Storage.Memcached` | 易失缓存，非持久化 |
| `FileManager.Storage.InfluxDb` | 时序数据库适配 |
