# FileManager 项目文档

FileManager 是一套面向 ASP.NET Core 的**在线文件管理系统**，采用分层、可插拔架构：存储后端与 UI 前端解耦，通过统一的 `IFileStore` 抽象访问多种文件源。

## 文档目录

| 文档 | 说明 |
|------|------|
| [快速开始](getting-started.md) | 安装、配置与运行示例 |
| [架构设计](architecture.md) | 分层结构、核心接口与请求流程 |
| [存储提供程序](storage-providers.md) | 本地、FTP、WebDAV、压缩包、MongoDB 等后端 |
| [UI 组件](ui-components.md) | FileExplorer、Rich Filemanager、elFinder、SPA、Vue、AdminLTE、FilePicker |
| [API 参考](api-reference.md) | Web API 与 SDK 使用说明 |
| [开发指南](development.md) | 本地构建、测试项目与扩展开发 |
| [Samples 使用指南](../Samples/README.md) | 可运行示例的说明与命令 |
| [UI 与存储组合](../Samples/ui-and-storage.md) | 各 UI 与存储后端的注册方式 |

## 项目概览

```
FileManager.slnx
├── Src/                          # 核心库与 NuGet 包
│   ├── FileManager.Core          # IFileStore、权限、DI 扩展
│   ├── FileManager.Storage.*     # 各存储后端实现
│   ├── FileManager.UI            # UI 注册与默认文件服务
│   ├── FileManager.UI.Explorer   # FileExplorer 富 UI（推荐）
│   ├── FileManager.UI.RichFilemanager  # Rich Filemanager UI
│   ├── FileManager.UI.ElFinder       # elFinder UI
│   ├── FileManager.UI.Spa / .Vue / .AdminLTE / .FilePicker  # REST 驱动 UI
│   ├── FileManager.UI.Common         # 共享 REST API
│   ├── FileManager.UI.WebApi     # REST API 控制器
│   └── FileManager.UI.WebApi.Sdk # HTTP 客户端 SDK
├── Samples/                      # 可运行的示例项目（见 [Samples/README.md](../Samples/README.md)）
│   ├── FileManager.Sample.Shared           # 共享存储配置与 UI 挂载
│   ├── FileManager.Sample.*.Minimal        # 最小示例（只读 ContentRoot）
│   ├── FileManager.Sample.*.Full           # 完整示例（多 UI + 读写 + API + Swagger）
│   └── FileManager.Sample.Owin             # OWIN 宿主示例
└── Test/                         # 自动化测试项目
    ├── FileManager.Core.Test           # 核心库与 FileProvider 单元测试
    ├── FileManager.UI.WebApi.Sdk.Test  # Web API SDK 集成测试
    ├── FileManager.Samples.Test        # 示例项目 Smoke Test
    └── FileManager.Storage.*.Test      # 各存储后端单元测试
```

## 核心能力

- **多存储后端**：…共 **42 种** Provider
- **多种 UI**：…共 **25 种** UI
- **权限控制**：基于 `Permission` 枚举的细粒度文件夹/文件操作权限
- **NuGet 分发**：各模块独立打包，可按需引用
- **REST API**：通过 `FileManager.UI.WebApi` 提供文件上传、下载、删除等接口

## 技术栈

| 类别 | 技术 |
|------|------|
| 运行时 | .NET Standard 2.0/2.1、.NET Framework 4.8、ASP.NET Core 2.x/3.x |
| UI 路由 | [AspNetCoreDashboard](https://www.nuget.org/packages/AspNetCoreDashboard) |
| 图像处理 | SixLabors.ImageSharp |
| 压缩包 | SharpCompress |
| FTP | FluentFTP / CoreFtp |
| 数据库 | MongoDB.Driver（GridFS） |

## 相关链接

- [NuGet 包列表](../README.md#packages)
- [AppVeyor CI](https://ci.appveyor.com/project/mccj/filemanager)
- [MyGet 预发布源](https://www.myget.org/feed/mccj)
