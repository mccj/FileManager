# UI 与存储组合参考



本文档列出 FileManager 支持的 **UI 前端** 与 **存储后端** 组合，以及对应的注册代码与 NuGet/项目引用。



## UI 前端



多种 UI 可同时挂载在不同路径，共享同一个 `IFileStore`：



| UI | 扩展方法 | 所需引用 | 说明 |

|----|----------|----------|------|

| **FileExplorer**（推荐） | `UseFileExplorerUI()` | `FileManager.UI`（含 Explorer） | 富管理界面：缩略图、预览、上传、ZIP |

| **Rich Filemanager** | `UseRichFileManagerUI()` | + `FileManager.UI.RichFilemanager` | 基于 Rich Filemanager 开源前端 |

| **elFinder** | `UseElFinderUI()` | + `FileManager.UI.ElFinder` | elFinder 2.x + .NET Connector |

| **SPA 文件管理** | `UseSpaFileManagerUI()` | + `FileManager.UI.RestShell` | RestShell 主题 `spa` |

| **默认目录浏览** | `UseDefaultFileManagerUI()` | `FileManager.UI` | ASP.NET Core `UseFileServer` 目录列表 |
| **Vue 文件管理** | `UseVueFileManagerUI()` | + `FileManager.UI.RestShell` | RestShell 主题 `vue` |
| **AdminLTE** | `UseAdminLTEFileManagerUI()` | + `FileManager.UI.AdminLTE` | AdminLTE 3 风格管理界面 |
| **文件选择器** | `UseFilePickerUI()` | + `FileManager.UI.FilePicker` | 嵌入式选择器，`postMessage` 回调 |
| **React** | `UseReactFileManagerUI()` | + `FileManager.UI.RestShell` | RestShell 主题 `react` |
| **Angular** | `UseAngularFileManagerUI()` | + `FileManager.UI.Angular` | Angular 风格 REST 界面 |
| **Blazor** | `UseBlazorFileManagerUI()` | + `FileManager.UI.Blazor` | Blazor 风格 REST 界面 |
| **Dropzone** | `UseDropzoneFileManagerUI()` | + `FileManager.UI.Dropzone` | 拖拽上传界面 |
| **Media Library** | `UseMediaLibraryFileManagerUI()` | + `FileManager.UI.MediaLibrary` | 媒体库浏览界面 |
| **Mobile** | `UseMobileFileManagerUI()` | + `FileManager.UI.Mobile` | 移动端响应式界面 |
| **Svelte** | `UseSvelteFileManagerUI()` | + `FileManager.UI.Svelte` | Svelte 风格 REST 界面 |
| **Next.js** | `UseNextJsFileManagerUI()` | + `FileManager.UI.NextJs` | Next.js 风格 REST 界面 |
| **TinyMCE** | `UseTinyMceFileManagerUI()` | + `FileManager.UI.TinyMce` | 集成 TinyMCE 编辑器 |
| **SDK Demo** | `UseSdkDemoFileManagerUI()` | + `FileManager.UI.SdkDemo` | REST API / SDK 演示 |
| **Themes** | `UseThemesFileManagerUI()` | + `FileManager.UI.Themes` | 明暗主题切换 |
| **Preact** | `UsePreactFileManagerUI()` | + `FileManager.UI.Preact` | Preact 风格 REST 界面 |
| **Solid.js** | `UseSolidJsFileManagerUI()` | + `FileManager.UI.SolidJs` | Solid.js 风格 REST 界面 |
| **CKEditor** | `UseCkEditorFileManagerUI()` | + `FileManager.UI.CkEditor` | 集成 CKEditor 5 |
| **Bootstrap 5** | `UseBootstrap5FileManagerUI()` | + `FileManager.UI.Bootstrap5` | Bootstrap 5 风格 |
| **PWA** | `UsePwaFileManagerUI()` | + `FileManager.UI.Pwa` | PWA 离线友好 |
| **i18n** | `UseI18nFileManagerUI()` | + `FileManager.UI.I18n` | 多语言切换 |
| **REST API** | MVC `FileController` | + `FileManager.UI.WebApi` | 供自定义前端或 SDK 调用 |



```csharp

app.UseFileManagerUI("/Explorer", f => f.UseFileExplorerUI());

app.UseFileManagerUI("/RichFilemanager", f => f.UseRichFileManagerUI());

app.UseFileManagerUI("/ElFinder", f => f.UseElFinderUI());

app.UseFileManagerUI("/Spa", f => f.UseSpaFileManagerUI());

app.UseFileManagerUI("/Default", f => f.UseDefaultFileManagerUI());

app.UseFileManagerUI("/Vue", f => f.UseVueFileManagerUI());

app.UseFileManagerUI("/AdminLTE", f => f.UseAdminLTEFileManagerUI());

app.UseFileManagerUI("/FilePicker", f => f.UseFilePickerUI());

```



## 存储后端



在 `services.AddFileManager(f => f....)` 中注册。通常 **同一时间仅一个** `IFileStore`；多盘请使用 **Composite**（见下）。



### Standard（本地 / IFileProvider）



| Provider | 方法 | 读写 | 包 |

|----------|------|------|-----|

| 物理路径（相对 ContentRoot） | `AddRootPhysicalFilePath("/App_Data/Files")` | 可读写 | `FileManager.Storage.Standard` |

| 物理路径（绝对） | `AddPhysicalFilePath(@"C:\data")` | 可读写 | 同上 |

| ContentRoot 只读 | `AddContentRootFileProviderReadOnly()` | 只读 | 同上 |

| WebRoot 只读 | `AddWebRootFileProviderReadOnly()` | 只读 | 同上 |



### InMemory（内存，可读写）



| 方法 | 包 |

|------|-----|

| `AddInMemoryStore()` | `FileManager.Storage.InMemory` |



适合单元测试、演示、临时会话存储。



### Composite（多 Drive）



| 方法 | 包 |

|------|-----|

| `AddCompositeFileStore(b => b.AddDrive("local", store)...)` | `FileManager.Storage.Composite` |



路径格式：`drive/folder/file.txt`。Explorer 中可使用 `[local]:folder/file.txt` 语法（由 `DrivePath` 解析）。



```csharp

services.AddFileManager(f => f.AddCompositeFileStore(b =>

{

    b.AddDrive("local", sp => new FileSystemStore(@"C:\data"));

    b.AddDrive("memory", _ => new InMemoryStore());

}));

```



### Azure Blob



| 方法 | 包 |

|------|-----|

| `AddAzureBlobStore(connectionString, containerName)` | `FileManager.Storage.AzureBlob` |



### AWS S3



| 方法 | 包 |

|------|-----|

| `AddAwsS3Store(client, bucketName)` | `FileManager.Storage.AwsS3` |



### Compress / FTP / WebDAV / MongoDB



| 后端 | 方法 | 包 |

|------|------|-----|

| ZIP 只读 | `AddCompressStore(...)` | `FileManager.Storage.Compress` |

| FTP | `AddFtpStore(...)` | `FileManager.Storage.Ftp` |

| WebDAV | `AddWebDavStore(...)` | `FileManager.Storage.WebDav` |

| MongoDB GridFS | `AddMongoDBStore(...)` | `FileManager.Storage.MongoDB` |
| SFTP | `AddSftpStore(...)` | `FileManager.Storage.Sftp` |
| Google Cloud Storage | `AddGoogleCloudStore(...)` | `FileManager.Storage.GoogleCloud` |
| Redis | `AddRedisStore(...)` | `FileManager.Storage.Redis` |
| SQL BLOB (SQLite 等) | `AddSqlBlobStore(...)` | `FileManager.Storage.SqlBlob` |
| MinIO (S3 兼容) | `AddMinIOStore(...)` 或 `AddS3CompatibleStore(...)` | `FileManager.Storage.MinIO` 或 `FileManager.Storage.AwsS3` |
| Dropbox | `AddDropboxStore(accessToken)` | `FileManager.Storage.Dropbox` |
| OneDrive | `AddOneDriveStore(accessToken)` | `FileManager.Storage.OneDrive` |
| Google Drive | `AddGoogleDriveStore(accessToken)` | `FileManager.Storage.GoogleDrive` |
| SharePoint | `AddSharePointStore(siteUrl, accessToken)` | `FileManager.Storage.SharePoint` |
| SMB/UNC | `AddSmbStore(uncPath)` | `FileManager.Storage.Smb` |
| NFS 挂载点 | `AddNfsStore(mountPath)` | `FileManager.Storage.Nfs` |
| PostgreSQL BLOB | `AddPostgreSqlBlobStore(...)` | `FileManager.Storage.PostgreSqlBlob` |
| SQL Server BLOB | `AddSqlServerBlobStore(...)` | `FileManager.Storage.SqlServerBlob` |
| Elasticsearch | `AddElasticsearchStore(endpoint, index)` | `FileManager.Storage.Elasticsearch` |
| Cassandra | `AddCassandraStore(contactPoints, keyspace)` | `FileManager.Storage.Cassandra` |
| Box | `AddBoxStore(accessToken)` | `FileManager.Storage.Box` |
| Backblaze B2 | `AddBackblazeB2Store(keyId, appKey, bucket, region)` | `FileManager.Storage.BackblazeB2` |
| Wasabi | `AddWasabiStore(accessKey, secretKey, bucket, region)` | `FileManager.Storage.Wasabi` |
| Oracle Object | `AddOracleObjectStore(...)` | `FileManager.Storage.OracleObject` |
| IBM COS | `AddIbmCosStore(...)` | `FileManager.Storage.IbmCos` |
| RabbitMQ | `AddRabbitMqStore(connectionString)` | `FileManager.Storage.RabbitMq` |
| Kafka | `AddKafkaStore(bootstrapServers, topic)` | `FileManager.Storage.Kafka` |
| LDAP | `AddLdapStore(host, port, baseDn, ...)` | `FileManager.Storage.Ldap` |
| DigitalOcean Spaces | `AddDigitalOceanSpacesStore(...)` | `FileManager.Storage.DigitalOceanSpaces` |
| Cloudflare R2 | `AddCloudflareR2Store(...)` | `FileManager.Storage.CloudflareR2` |
| Scaleway | `AddScalewayStore(...)` | `FileManager.Storage.Scaleway` |
| Hetzner | `AddHetznerStore(...)` | `FileManager.Storage.Hetzner` |
| OpenStack Swift | `AddOpenStackSwiftStore(...)` | `FileManager.Storage.OpenStackSwift` |
| MySQL BLOB | `AddMySqlBlobStore(connectionString)` | `FileManager.Storage.MySqlBlob` |
| InfluxDB | `AddInfluxDbStore(url, token, org, bucket)` | `FileManager.Storage.InfluxDb` |
| Memcached | `AddMemcachedStore(host, port)` | `FileManager.Storage.Memcached` |



## 可运行演示



[FileManager.Sample.Mvc.Full](FileManager.Sample.Mvc.Full/) 与 [FileManager.Sample.Blazor.Full](FileManager.Sample.Blazor.Full/) 默认 **Lite** 模式挂载 6 种核心 UI；设置 `FileManager:Showcase:MountMode=Full` 或使用 **FullMount** launch profile 可挂载全部 25 种。

> ***·Full 构建说明**：MinIO/Wasabi/R2 等 S3 兼容厂商统一通过 `FileManager.Storage.AwsS3` 的 `AddS3CompatibleStore()` 配置，无需单独引用各厂商包。实验性存储（Kafka/RabbitMq/Memcached/InfluxDb）需自行引用对应包。

> **安全提示**：REST 驱动 UI（Vue、Spa、React 等）现已通过 `DefaultPermissionHandle` 校验读写权限。Explorer、elFinder、RichFilemanager 写操作同样受权限守卫保护。*·Full 示例对可写存储默认启用 `AddFullAccessPermissionHandle()`，**仅供本地演示**，生产环境请配置身份认证与细粒度权限。

```bash
dotnet run --project Samples/FileManager.Sample.Mvc.Full
```

| 端点 | URL |
|------|-----|
| 首页 | http://localhost:5000 |
| FileExplorer | http://localhost:5000/Explorer/ |
| Rich Filemanager | http://localhost:5000/RichFilemanager/ |
| elFinder | http://localhost:5000/ElFinder/ |
| SPA | http://localhost:5000/Spa/ |
| 默认浏览 | http://localhost:5000/Default/ |
| Vue | http://localhost:5000/Vue/ |
| AdminLTE | http://localhost:5000/AdminLTE/ |
| 文件选择器 | http://localhost:5000/FilePicker/ |
| React | http://localhost:5000/React/ |
| Angular | http://localhost:5000/Angular/ |
| Blazor | http://localhost:5000/Blazor/ |
| Dropzone | http://localhost:5000/Dropzone/ |
| Media Library | http://localhost:5000/MediaLibrary/ |
| Mobile | http://localhost:5000/Mobile/ |
| Svelte | http://localhost:5000/Svelte/ |
| Next.js | http://localhost:5000/NextJs/ |
| TinyMCE | http://localhost:5000/TinyMce/ |
| SDK Demo | http://localhost:5000/SdkDemo/ |
| Themes | http://localhost:5000/Themes/ |
| Preact | http://localhost:5000/Preact/ |
| Solid.js | http://localhost:5000/SolidJs/ |
| CKEditor | http://localhost:5000/CkEditor/ |
| Bootstrap 5 | http://localhost:5000/Bootstrap5/ |
| PWA | http://localhost:5000/Pwa/ |
| i18n | http://localhost:5000/I18n/ |
| Swagger | http://localhost:5000/swagger |

### 本地依赖（可选）

```bash
docker compose -f Samples/docker-compose.yml up -d
```

包含 MongoDB、Azurite、MinIO、Redis。

### 环境变量切换存储



```powershell

# 内存存储

$env:FileManager__Storage__Provider = "InMemory"



# 多盘：本地 + 内存

$env:FileManager__Storage__Provider = "MultiDrive"



# Azure Blob（需 Azurite 或真实账户）

$env:FileManager__Storage__Provider = "AzureBlob"

$env:FileManager__Storage__AzureBlob__ConnectionString = "UseDevelopmentStorage=true"



dotnet run --project Samples/FileManager.Sample.Mvc.Full

```



有效 `Provider` 值：`Physical`, `ContentRoot`, `WebRoot`, `Compress`, `Ftp`, `WebDav`, `MongoDB`, `InMemory`, `MultiDrive`, `AzureBlob`, `AwsS3`, `Sftp`, `GoogleCloud`, `Redis`, `SqlBlob`, `MinIO`, `Dropbox`, `OneDrive`, `GoogleDrive`, `SharePoint`, `Smb`, `Nfs`, `PostgreSqlBlob`, `SqlServerBlob`, `Elasticsearch`, `Cassandra`, `Box`, `BackblazeB2`, `Wasabi`, `OracleObject`, `IbmCos`, `RabbitMq`, `Kafka`, `Ldap`, `DigitalOceanSpaces`, `CloudflareR2`, `Scaleway`, `Hetzner`, `OpenStackSwift`, `MySqlBlob`, `InfluxDb`, `Memcached`。



## 引用速查



| 场景 | 项目引用 |

|------|----------|

| FileExplorer + 本地存储 | `FileManager.UI` |

| Rich Filemanager | + `FileManager.UI.RichFilemanager` |

| elFinder | + `FileManager.UI.ElFinder` |

| SPA / Vue / React 等 REST UI | + `FileManager.UI.RestShell`（一个包覆盖 16 种主题） |
| AdminLTE / FilePicker | + `FileManager.UI.AdminLTE` / `.FilePicker` |
| REST API | + `FileManager.UI.WebApi` |
| InMemory / Composite / 云存储 / SFTP / Redis 等 | + 对应 `FileManager.Storage.*` |



只读存储下，UI 会通过 `IFileStoreCapabilities` 自动隐藏上传、删除等写操作。

