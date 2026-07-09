# UI 组件

FileManager 提供三种 UI 集成方式，均通过 `app.UseFileManagerUI(path, options)` 挂载到指定 URL 路径。

## 对比

| UI | 扩展方法 | NuGet 包 | 状态 | 特点 |
|----|----------|----------|------|------|
| FileExplorer | `UseFileExplorerUI()` | FileManager.UI.Explorer | **推荐** | 功能完整：上传、预览、缩略图、多语言 |
| Rich Filemanager | `UseRichFileManagerUI()` | FileManager.UI.RichFilemanager | 已弃用 | 基于 Rich Filemanager 开源项目 |
| 默认目录浏览 | `UseDefaultFileManagerUI()` | FileManager.UI | 稳定 | ASP.NET Core 内置 FileServer |

## FileExplorer UI（推荐）

基于嵌入式 FileExplorer 前端，通过 AspNetCoreDashboard 路由提供 REST 风格 API。

### 启用

```csharp
app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());
```

### 功能

| 功能 | 状态 | 说明 |
|------|------|------|
| 浏览文件夹 | ✅ | ListFolder / ExpandFolder |
| 创建文件夹 | ✅ | Create |
| 删除 | ✅ | Delete |
| 重命名 | ✅ | Rename |
| 移动 | ✅ | Move |
| 复制文件 | ✅ | Copy（目录复制未实现） |
| 上传 | ✅ | Fileuploader 分块上传 |
| 下载 | ✅ | Download |
| 图片缩略图 | ✅ | GetThumbnail（ImageSharp） |
| 图片预览 | ✅ | ImageViewer |
| 文档预览 | ⚠️ | DocumentViewer（演示数据） |
| ZIP 打包下载 | ❌ | DownloadAsZip 未实现 |
| 压缩/解压 | ❌ | AddToZip / ExtractAll 未实现 |

### 多语言

FileExplorer 内置中英文语言包：

- `Content/FileExplorer/Languages/Languages.FileUltimate-en.xml`
- `Content/FileExplorer/Languages/Languages.FileUltimate-cn.xml`
- `Languages.Viewer-en.json` / `Languages.Viewer-cn.json`

通过 `/localization` 和 `/localization.json` 端点动态加载。

### 依赖

- `AspNetCoreDashboard` — 嵌入式资源路由
- `SixLabors.ImageSharp` — 缩略图生成
- `Microsoft.Extensions.Caching.Memory` — 上传会话缓存

## Rich Filemanager UI（已弃用）

基于 [Rich Filemanager](https://github.com/servocoder/rich-filemanager) 开源项目的前端界面。

### 启用

```csharp
app.UseFileManagerUI("/RichFileManagerBrowser", f => f.UseRichFileManagerUI());
```

### 工作原理

- 前端资源嵌入在 `Content/RichFilemanager/` 目录
- API 通过 `/connectors/php/filemanager.php` 端点模拟 PHP 连接器协议
- 后端由 `FileManagerController` 处理各种 `mode` 参数（initiate、readfolder、upload 等）

> 该包在 README 中已标记删除线（弃用），新项目不建议使用。

## 默认文件服务 UI

使用 ASP.NET Core 内置的 `FileServer` 中间件，提供简单的目录列表和文件下载。

### 启用

```csharp
app.UseFileManagerUI("/Files", f => f.UseDefaultFileManagerUI());
```

### 实现原理

通过 `StoreFileProvider` 将 `IFileStore` 适配为 `IFileProvider`，再交给 `UseFileServer`：

```csharp
var option = new FileServerOptions
{
    FileProvider = new StoreFileProvider(fileStore),
    RequestPath = pathMatch,
    EnableDefaultFiles = true,
    EnableDirectoryBrowsing = true
};
app.UseFileServer(option);
```

适合只需要简单文件浏览、不需要富 UI 的场景。

## 授权过滤

Explorer 和 RichFilemanager 的路由均支持 `IDashboardAuthorizationFilter` 授权过滤器（通过内部 `UseMapDashboard` 传递），可用于限制访问权限。具体用法需参考 AspNetCoreDashboard 文档。

## 路径约定

FileExplorer UI 使用 `[drive]:path` 格式的路径参数，例如 `[default]:folder/subfolder`。后端通过正则解析：

```
\[(?<drive>.*)\]:(?<path>.*)
```

当前实现中 `drive` 参数未被实际使用，所有操作均针对 `IFileStore` 的相对路径。
