# 开发指南

## 开发环境

| 工具 | 版本 |
|------|------|
| Visual Studio | 2019+（解决方案格式 Version 16） |
| .NET SDK | 支持 netstandard2.0/2.1、net48 |
| Git | 版本控制 |

## 解决方案结构

```
FileManager.slnx
├── Src/                    # 核心库、存储后端、UI 模块
├── Samples/                # 示例项目（*·Full 默认 Lite 模式挂载 6 种核心 UI）
└── Test/                   # 单元测试
```

### 构建

```bash
cd FileManager
dotnet build FileManager.slnx
```

### 运行测试

```bash
dotnet test FileManager.slnx
```

## 示例项目说明

完整说明见 [Samples/README.md](../Samples/README.md)。

| 项目 | 框架 | 用途 |
|------|------|------|
| FileManager.Sample.*.Minimal | ASP.NET Core | 最小集成：只读 ContentRoot + FileExplorer |
| FileManager.Sample.*.Full | ASP.NET Core | 完整演示：多 UI、可切换存储、REST API、Swagger |
| FileManager.Sample.Owin | .NET Framework 4.8 + OWIN | 遗留 OWIN 宿主集成 |

### FileManager.Sample.*.Full 示例要点

- UI：默认 Lite 模式挂载六种核心 UI；`FileManager:Showcase:MountMode=Full` 或 **FullMount** profile 挂载全部
- 存储：通过 `FileManager:Storage:Provider` 配置切换
- Launch profile：`Physical`、`ContentRoot`、`WebRoot`、`Compress`、`InMemory`、`MultiDrive`、`FullMount`
- **Mvc.Full** 支持 `FileManager:Showcase:RequireAuth=true` 演示登录（`/Home/Login`，`demo`/`demo`）
- 详见 [ui-and-storage.md](../Samples/ui-and-storage.md)

### FileManager.Sample.*.Minimal 示例要点

- 存储：`AddContentRootFileProviderReadOnly()`（只读 ContentRoot）
- UI：`UseFileExplorerUI()` 挂载在 `/Browser`
- 适合验证最小 NuGet 引用与 UI 加载

## 测试项目说明

| 项目 | 框架 | 用途 |
|------|------|------|
| FileManager.Core.Test | xUnit | 核心库、权限与 FileProvider 单元测试 |
| FileManager.UI.WebApi.Sdk.Test | NUnit | Web API SDK 集成测试 |
| FileManager.Samples.Test | xUnit | 示例项目 Smoke Test |
| FileManager.Storage.Standard.Test | xUnit | Standard 存储后端单元测试 |

## 项目打包

各 `Src/` 下的项目独立配置 `<Version>` 并发布到 NuGet：

```bash
dotnet pack Src/FileManager.Core/FileManager.Core.csproj -c Release
dotnet pack Src/FileManager.UI.Explorer/FileManager.UI.Explorer.csproj -c Release
```

CI 通过 [AppVeyor](https://ci.appveyor.com/project/mccj/filemanager) 自动构建。

预发布包发布到 [MyGet](https://www.myget.org/feed/mccj)。

## 扩展开发

### 新增存储后端

1. 创建 `FileManager.Storage.MyBackend` 类库项目
2. 引用 `FileManager.Core`
3. 实现 `IFileStore` 和 `IFileStoreEntry`
4. 添加 `ApplicationBuilderExtensions` 静态类：

```csharp
namespace FileManager
{
    public static class ApplicationBuilderExtensions
    {
        public static FileServicesOptions AddMyBackendStore(
            this FileServicesOptions app,
            Func<IServiceProvider, MyBackendStore> factory)
        {
            return app.AddFileStore(factory);
        }
    }
}
```

5. 将项目加入解决方案并配置 NuGet 打包

### 新增 UI 组件

轻量 REST 界面（HTML/JS + `MapRestFileApi()`）优先使用 **`FileManager.UI.RestShell`**，通过主题区分外观，无需再复制独立包：

```csharp
// 推荐：统一 RestShell + 主题
app.UseFileManagerUI("/Vue", f => f.UseRestShellFileManagerUI("vue"));

// 兼容：旧扩展方法仍可用，内部路由到 RestShell
app.UseFileManagerUI("/Vue", f => f.UseVueFileManagerUI());
```

引用 `FileManager.UI.RestShell`（*·Full 示例已用此包替代 `FileManager.UI.Spa` / `.Vue` / `.React` 等 16 个重复包）。

功能完整或专用 UI（Explorer、RichFilemanager、elFinder、FilePicker、TinyMCE 等）仍使用独立项目。参考：`Src/FileManager.UI.RestShell/`、`Src/FileManager.UI.Explorer/ApplicationBuilderExtensions.cs`

### 自定义权限

```csharp
services.AddFileManager(f => f
    .AddPhysicalFilePath("/data")
    .AddPermissionHandle(async (sp, entry) =>
    {
        var httpContext = sp.GetService<IHttpContextAccessor>()?.HttpContext;
        var user = httpContext?.User;

        if (!user.Identity.IsAuthenticated)
            return Permission.ListSubFolders | Permission.ListFiles;

        if (entry.Path.StartsWith("public/"))
            return Permission.ListSubFolders | Permission.ListFiles
                 | Permission.Download | Permission.Preview;

        return Permission.ListSubFolders | Permission.ListFiles
             | Permission.Download | Permission.Upload | Permission.Delete
             | Permission.Rename | Permission.Create;
    })
);
```

## 已知限制

| 区域 | 限制 |
|------|------|
| FileExplorer | DocumentViewer PDF 转换仍返回原始文件流，非真实 PDF 转换 |
| CompressStore | 只读；首次访问会建立压缩包索引，大归档仍建议只用于浏览 |
| Azure Blob | 尚未实现（计划独立 `FileManager.Storage.AzureBlob` 包） |
| 认证 | 生产环境需自行配置 `AddAuthentication` / `[Authorize]`；默认权限为只读 |
| Web API | `GetListFilesAsync` 返回元数据列表（最多 100 项），不再加载全部文件字节 |

## 外部依赖

解决方案引用了同级目录的 `SixLabors.ImageSharp.Formats.PSD` 项目（PSD 格式支持），该引用在 `FileManager.UI.Explorer` 中已被注释，不影响正常构建。

## 贡献

1. Fork 仓库
2. 创建功能分支
3. 提交变更并确保 `dotnet build` 通过
4. 发起 Pull Request
