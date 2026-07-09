# 开发指南

## 开发环境

| 工具 | 版本 |
|------|------|
| Visual Studio | 2019+（解决方案格式 Version 16） |
| .NET SDK | 支持 netstandard2.0/2.1、net48 |
| Git | 版本控制 |

## 解决方案结构

```
FileManagerWebUI.sln
├── Src/                    # 11 个库项目
├── Samples/                # 3 个示例项目
└── Test/                   # 2 个测试项目
```

### 构建

```bash
cd FileManager
dotnet build FileManagerWebUI.sln
```

### 运行测试

```bash
dotnet test Test/FileManager.UI.Core.Test/FileManager.UI.Core.Test.csproj
```

## 示例项目说明

| 项目 | 框架 | 用途 |
|------|------|------|
| FileManagerTest | ASP.NET Core | 基础集成示例，FileExplorer UI |
| FileManagerCore | ASP.NET Core | 完整示例，含 Swagger 和 HealthChecks |
| ConsoleOwinTest | OWIN | 旧版 OWIN 宿主示例 |

## 测试项目说明

| 项目 | 框架 | 用途 |
|------|------|------|
| FileManagerSdkTest | NUnit | Web API SDK 调用测试 |
| FileManager.UI.Core.Test | xUnit | 核心库单元测试 |

### FileManagerCore 示例要点

- 存储：`AddRootPhysicalFilePath("/App_Data/Files")`
- UI：`UseFileExplorerUI()` 挂载在 `/Browser`
- API 文档：NSwag Swagger UI
- 健康检查：`/health`

### FileManagerTest 示例要点

- 存储：`AddContentRootFileProviderReadOnly()`（只读 ContentRoot）
- UI：`UseFileExplorerUI()` 挂载在 `/Browser`

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

1. 创建 `FileManager.UI.MyUI` 项目，引用 `FileManager.Core`
2. 将前端资源设为 `EmbeddedResource`
3. 使用 AspNetCoreDashboard 的 `RouteCollection` 注册路由
4. 添加 `UseMyUI(this FileManagerOptions app)` 扩展方法
5. 在路由处理器中通过 `applicationServices.GetService<IFileStore>()` 获取存储

参考实现：`Src/FileManager.UI.Explorer/ApplicationBuilderExtensions.cs`

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
| FileExplorer | 目录复制、ZIP 打包/解压未实现 |
| FileExplorer | DocumentViewer 使用硬编码演示数据 |
| CompressStore | 只读，每次操作需重置流位置 |
| Azure Blob | `BlobFileStore` 未完成 |
| RichFilemanager | 已弃用，`_fileManager` 提供者未完全接入 |
| Web API | `GetListFilesAsync` 使用 `.Result` 同步等待，不适合大文件 |

## 外部依赖

解决方案引用了同级目录的 `SixLabors.ImageSharp.Formats.PSD` 项目（PSD 格式支持），该引用在 `FileManager.UI.Explorer` 中已被注释，不影响正常构建。

## 贡献

1. Fork 仓库
2. 创建功能分支
3. 提交变更并确保 `dotnet build` 通过
4. 发起 Pull Request
