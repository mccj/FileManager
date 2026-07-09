# FileManager 项目文档

FileManager（FileManagerWebUI）是一套面向 ASP.NET Core 的**在线文件管理系统**，采用分层、可插拔架构：存储后端与 UI 前端解耦，通过统一的 `IFileStore` 抽象访问多种文件源。

## 文档目录

| 文档 | 说明 |
|------|------|
| [快速开始](getting-started.md) | 安装、配置与运行示例 |
| [架构设计](architecture.md) | 分层结构、核心接口与请求流程 |
| [存储提供程序](storage-providers.md) | 本地、FTP、WebDAV、压缩包、MongoDB 等后端 |
| [UI 组件](ui-components.md) | FileExplorer、RichFilemanager、默认文件浏览 |
| [API 参考](api-reference.md) | Web API 与 SDK 使用说明 |
| [开发指南](development.md) | 本地构建、测试项目与扩展开发 |

## 项目概览

```
FileManagerWebUI.sln
├── Src/                          # 核心库与 NuGet 包
│   ├── FileManager.Core          # IFileStore、权限、DI 扩展
│   ├── FileManager.Storage.*     # 各存储后端实现
│   ├── FileManager.UI            # UI 注册与默认文件服务
│   ├── FileManager.UI.Explorer   # FileExplorer 富 UI（推荐）
│   ├── FileManager.UI.RichFilemanager  # Rich Filemanager UI（已弃用）
│   ├── FileManager.UI.WebApi     # REST API 控制器
│   └── FileManager.UI.WebApi.Sdk # HTTP 客户端 SDK
├── Samples/                      # 可运行的示例项目
│   ├── FileManagerTest           # ASP.NET Core 示例
│   ├── FileManagerCore           # 完整示例（含 Swagger）
│   └── ConsoleOwinTest           # OWIN 宿主示例
└── Test/                         # 自动化测试项目
    ├── FileManagerSdkTest        # SDK 调用测试
    └── FileManager.UI.Core.Test  # 单元测试
```

## 核心能力

- **多存储后端**：本地文件系统、ASP.NET Core `IFileProvider`、FTP、WebDAV、ZIP 压缩包、MongoDB GridFS
- **多种 UI**：内置 FileExplorer 管理界面、Rich Filemanager、或 ASP.NET Core 原生目录浏览
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
