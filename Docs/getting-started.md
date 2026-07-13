# 快速开始

## 前置要求

- .NET 8 SDK（ASP.NET Core 示例）
- .NET Framework 4.8（OWIN 示例 `FileManager.Sample.Owin`）

## 通过 NuGet 安装

```bash
# 核心 UI（含默认文件服务）
dotnet add package FileManager.UI.Explorer

# 存储后端（按需选择）
dotnet add package FileManager.Storage.Standard
dotnet add package FileManager.Storage.Ftp
dotnet add package FileManager.Storage.WebDav
dotnet add package FileManager.Storage.Compress
dotnet add package FileManager.Storage.MongoDB
```

## 最小示例

### 1. 注册服务

在 `Startup.ConfigureServices` 中：

```csharp
using FileManager;

public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc(); // 或 AddControllersWithViews()

    services.AddFileManager(f => f
        .AddContentRootFileProviderReadOnly()  // 只读访问 ContentRoot
    );
}
```

### 2. 启用 UI

在 `Startup.Configure` 中：

```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    app.UseStaticFiles();

    app.UseFileManagerUI("/Browser", f => f
        .UseFileExplorerUI()
    );

    app.UseMvc();
}
```

### 3. 访问

启动应用后，浏览器打开 `https://localhost:<port>/Browser/` 即可使用文件管理界面。

## 存储后端配置示例

### 本地物理路径（可读写）

```csharp
services.AddFileManager(f => f
    .AddRootPhysicalFilePath("/App_Data/Files")
);
```

### 只读 WebRoot

```csharp
services.AddFileManager(f => f
    .AddWebRootFileProviderReadOnly()
);
```

### 自定义物理路径

```csharp
services.AddFileManager(f => f
    .AddPhysicalFilePath(@"D:\SharedFiles")
);
```

### FTP

```csharp
services.AddFileManager(f => f
    .AddFtpStore("ftp://10.0.0.1/", 21, "username", "password")
);
```

### WebDAV

```csharp
services.AddFileManager(f => f
    .AddWebDavStore(
        new Uri("https://example.com/remote.php/webdav/"),
        "username",
        "password")
);
```

### ZIP 压缩包（只读）

```csharp
services.AddFileManager(f => f
    .AddCompressStore(File.OpenRead(@"C:\archives\backup.zip"))
);
```

### MongoDB GridFS

```csharp
services.AddFileManager(f => f
    .AddMongoDBStore("mongodb://localhost:27017/")
);
```

## 权限控制

默认情况下，未配置 `AddPermissionHandle` 时仅授予**只读**权限（列表、下载、预览）。需要写操作请显式配置：

```csharp
services.AddFileManager(f => f
    .AddRootPhysicalFilePath("/App_Data/Files")
    .AddPermissionHandle(entry =>
    {
        if (entry.IsDirectory)
            return Task.FromResult(
                Permission.ListSubFolders | Permission.ListFiles | Permission.Download
                | Permission.Create | Permission.Upload | Permission.Delete | Permission.Rename);
        return Task.FromResult(Permission.Download | Permission.Preview | Permission.Delete | Permission.Rename);
    })
);
```

生产环境请同时配置身份认证（`AddAuthentication`）与授权策略。

REST 驱动 UI（`MapRestFileApi`）与 `FileController` 会通过 `FileStorePermissionGuard` 校验 `DefaultPermissionHandle`；未配置写权限时，上传/删除等操作将被拒绝。

演示/开发场景可使用内置扩展快速授予完整权限：

```csharp
services.AddFileManager(f => f
    .AddRootPhysicalFilePath("/App_Data/Files")
    .AddFullAccessPermissionHandle()
);
```

## UI 类型选择

| 方法 | 说明 | 包 |
|------|------|-----|
| `UseFileExplorerUI()` | 功能完整的文件管理器（推荐） | FileManager.UI.Explorer |
| `UseRichFileManagerUI()` | Rich Filemanager 界面 | FileManager.UI.RichFilemanager |
| `UseElFinderUI()` | elFinder 2.x 界面 | FileManager.UI.ElFinder |
| `UseSpaFileManagerUI()` | 轻量 SPA（RestShell 主题 `spa`） | FileManager.UI.RestShell |
| `UseVueFileManagerUI()` | Vue 风格（RestShell 主题 `vue`） | FileManager.UI.RestShell |
| `UseAdminLTEFileManagerUI()` | AdminLTE 3 风格 | FileManager.UI.AdminLTE |
| `UseFilePickerUI()` | 嵌入式文件选择器 | FileManager.UI.FilePicker |
| `UseDefaultFileManagerUI()` | ASP.NET Core 原生目录浏览 | FileManager.UI |

```csharp
// FileExplorer（推荐）
app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());

// Rich Filemanager
app.UseFileManagerUI("/RichFilemanager", f => f.UseRichFileManagerUI());

// elFinder / SPA / Vue / AdminLTE / FilePicker
app.UseFileManagerUI("/ElFinder", f => f.UseElFinderUI());
app.UseFileManagerUI("/Spa", f => f.UseSpaFileManagerUI());
app.UseFileManagerUI("/Vue", f => f.UseVueFileManagerUI());
app.UseFileManagerUI("/AdminLTE", f => f.UseAdminLTEFileManagerUI());
app.UseFileManagerUI("/FilePicker", f => f.UseFilePickerUI());

// 默认目录浏览
app.UseFileManagerUI("/Files", f => f.UseDefaultFileManagerUI());
```

## 从源码运行示例

详见 [Samples/README.md](../Samples/README.md)。

```bash
git clone <repo-url>
cd FileManager

# 完整示例（推荐）：读写存储 + 多 UI + FileExplorer + Swagger + REST API
dotnet run --project Samples/FileManager.Sample.Mvc.Full/FileManager.Sample.Mvc.Full.csproj

# 最小示例：只读浏览 ContentRoot
dotnet run --project Samples/FileManager.Sample.RazorPages.Minimal/FileManager.Sample.RazorPages.Minimal.csproj

# Blazor 多 UI + 可切换存储
dotnet run --project Samples/FileManager.Sample.Blazor.Full/FileManager.Sample.Blazor.Full.csproj

# OWIN 宿主（.NET Framework 4.8）
dotnet run --project Samples/FileManager.Sample.Owin/FileManager.Sample.Owin.csproj
```

| 示例 | 地址 | 说明 |
|------|------|------|
| FileManager.Sample.Mvc.Full | http://localhost:5000/Explorer/ | 多 UI、可读写 `App_Data/Files`、REST API、Swagger |
| FileManager.Sample.RazorPages.Minimal | http://localhost:2115/Browser/ | 只读浏览项目根目录 |
| FileManager.Sample.Blazor.Full | http://localhost:2131/Explorer/ | Lite 六种 UI（FullMount 全部）+ 可切换存储 |
| FileManager.Sample.Owin | http://localhost:8080/Browser/ | OWIN 控制台宿主 |

## 常见问题

### 上传与 IMemoryCache

`AddFileManager()` 会自动注册 `IMemoryCache`，一般无需手动调用 `AddMemoryCache()`。

### 路径格式

`IFileStore` 使用相对路径，以 `/` 分隔，例如 `folder/subfolder/file.txt`，不要以 `/` 开头。
