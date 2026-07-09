# 快速开始

## 前置要求

- .NET SDK（支持 .NET Standard 2.0+ 的目标框架）
- ASP.NET Core 2.1 或更高版本（示例项目使用 2.1 / 3.x）

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

```csharp
services.AddFileManager(f => f
    .AddRootPhysicalFilePath("/App_Data/Files")
    .AddPermissionHandle(entry =>
    {
        if (entry.IsDirectory)
            return Task.FromResult(
                Permission.ListSubFolders | Permission.ListFiles | Permission.Download);
        return Task.FromResult(Permission.Download | Permission.Preview);
    })
);
```

## UI 类型选择

| 方法 | 说明 | 包 |
|------|------|-----|
| `UseFileExplorerUI()` | 功能完整的文件管理器（推荐） | FileManager.UI.Explorer |
| `UseRichFileManagerUI()` | Rich Filemanager 界面（已弃用） | FileManager.UI.RichFilemanager |
| `UseDefaultFileManagerUI()` | ASP.NET Core 原生目录浏览 | FileManager.UI |

```csharp
// FileExplorer（推荐）
app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());

// 默认目录浏览
app.UseFileManagerUI("/Files", f => f.UseDefaultFileManagerUI());
```

## 从源码运行示例

```bash
# 克隆仓库
git clone <repo-url>
cd FileManager

# 运行 ASP.NET Core 3.x 示例（含 Swagger）
dotnet run --project Samples/FileManagerCore/FileManagerCore.csproj

# 或运行基础示例
dotnet run --project Samples/FileManagerTest/FileManagerTest.csproj
```

`FileManagerCore` 示例默认配置：

- 存储：`App_Data/Files` 物理路径
- UI：`/Browser` 路径下的 FileExplorer
- Swagger：`/swagger`（仅 FileManagerCore 项目）

## 常见问题

### 上传需要 IMemoryCache

FileExplorer 的分块上传依赖 `IMemoryCache`，请确保已注册：

```csharp
services.AddMemoryCache();
```

### 路径格式

`IFileStore` 使用相对路径，以 `/` 分隔，例如 `folder/subfolder/file.txt`，不要以 `/` 开头。

### RichFilemanager 已弃用

`FileManager.UI.RichFilemanager` 在 README 中标记为弃用，新项目请使用 `FileManager.UI.Explorer`。
