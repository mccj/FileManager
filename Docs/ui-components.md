# UI 组件

FileManager 提供多种 UI 集成方式，均通过 `app.UseFileManagerUI(path, options)` 挂载到指定 URL 路径。

## 对比

| UI | 扩展方法 | NuGet 包 | 状态 | 特点 |
|----|----------|----------|------|------|
| FileExplorer | `UseFileExplorerUI()` | FileManager.UI.Explorer | **推荐** | 功能完整：上传、预览、缩略图、多语言 |
| Rich Filemanager | `UseRichFileManagerUI()` | FileManager.UI.RichFilemanager | 稳定 | 基于 Rich Filemanager 开源项目 |
| elFinder | `UseElFinderUI()` | FileManager.UI.ElFinder | 稳定 | elFinder 2.x + .NET Connector |
| SPA 文件管理 | `UseSpaFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `spa`，共享 REST API |
| Vue 文件管理 | `UseVueFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `vue` |
| AdminLTE | `UseAdminLTEFileManagerUI()` | FileManager.UI.AdminLTE | 稳定 | AdminLTE 3 风格界面 |
| 文件选择器 | `UseFilePickerUI()` | FileManager.UI.FilePicker | 稳定 | 嵌入式选择器，`postMessage` 回调 |
| React | `UseReactFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `react` |
| Angular | `UseAngularFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `angular` |
| Blazor | `UseBlazorFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `blazor` |
| Dropzone | `UseDropzoneFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `dropzone`（拖拽区） |
| Media Library | `UseMediaLibraryFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `medialibrary` |
| Mobile | `UseMobileFileManagerUI()` | FileManager.UI.RestShell | 稳定 | RestShell 主题 `mobile` |
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
| 复制文件 | ✅ | Copy（目录复制已实现） |
| 上传 | ✅ | Fileuploader 分块上传 |
| 下载 | ✅ | Download |
| 图片缩略图 | ✅ | GetThumbnail（ImageSharp） |
| 图片预览 | ✅ | ImageViewer |
| 文档预览 | ✅ | DocumentViewer（基于 IFileStore 真实文件数据） |
| ZIP 打包下载 | ✅ | DownloadAsZip |
| 压缩/解压 | ✅ | AddToZip / ExtractAll |

### 多语言

FileExplorer 内置中英文语言包：

- `Content/FileExplorer/Languages/Languages.FileUltimate-en.xml`
- `Content/FileExplorer/Languages/Languages.FileUltimate-cn.xml`
- `Languages.Viewer-en.json` / `Languages.Viewer-cn.json`

通过 `/localization` 和 `/localization.json` 端点动态加载。

### 依赖

- `AspNetCoreDashboard` — 嵌入式资源路由
- `SixLabors.ImageSharp` — 缩略图生成
- `Microsoft.Extensions.Caching.Memory` — 上传会话缓存（`AddFileManager()` 已自动注册）

## Rich Filemanager UI

基于 [Rich Filemanager](https://github.com/servocoder/RichFilemanager) 开源项目的前端界面，通过 .NET connector 对接 `IFileStore`。

### 启用

```csharp
// 需引用 FileManager.UI.RichFilemanager
app.UseFileManagerUI("/RichFilemanager", f => f.UseRichFileManagerUI());
```

### 功能

| 功能 | 状态 | 说明 |
|------|------|------|
| 浏览文件夹 | ✅ | `readfolder` |
| 创建文件夹 | ✅ | `addfolder` |
| 删除 | ✅ | `delete` |
| 重命名 / 移动 | ✅ | `rename` / `move` |
| 复制文件 | ✅ | `copy` |
| 上传 | ✅ | `upload`（ASP.NET Core） |
| 下载 | ✅ | `download` |
| 图片预览 | ✅ | `getimage` |
| 在线编辑 | ✅ | `readfile` / `savefile` |
| 搜索 | ✅ | `seekfolder` |
| 权限控制 | ✅ | 接入 `DefaultPermissionHandle`，与 Explorer 一致 |
| 只读能力探测 | ✅ | 基于 `IFileStoreCapabilities` 自动设置 `readOnly` |

### 工作原理

- 前端资源嵌入在 `Content/RichFilemanager/`
- API 通过 `/connectors/php/filemanager.php` 模拟 PHP 连接器协议
- 后端由 `FileManagerController` / `FileManagerProvider` 处理 `mode` 参数
- 嵌入资源 LogicalName 与 AspNetCoreDashboard 的路径转换规则对齐（目录名中的 `-` → `_`）

### 依赖

- `AspNetCoreDashboard.Abstractions` — UI 模块与嵌入资源
- `FileManager.Core` — `IFileStore`
- 宿主需引用 `FileManager.UI.RichFilemanager`（运行时加载模块）

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

## elFinder UI

基于 [elFinder](https://github.com/Studio-42/elFinder) 2.x 前端，通过 .NET Connector 对接 `IFileStore`。

```csharp
app.UseFileManagerUI("/ElFinder", f => f.UseElFinderUI());
```

前端资源嵌入在 `Content/ElFinder/`，Connector 处理 `cmd` 参数（`open`、`mkdir`、`upload` 等）。

## SPA / Vue / AdminLTE / FilePicker

以下 UI 共享 `FileManager.UI.Common` 中的 REST API（`RestFileApi`），可独立挂载：

| UI | 扩展方法 | 说明 |
|----|----------|------|
| SPA | `UseSpaFileManagerUI()` | 纯 HTML/JS，无框架依赖 |
| Vue | `UseVueFileManagerUI()` | Vue 3 单页应用 |
| AdminLTE | `UseAdminLTEFileManagerUI()` | AdminLTE 3 布局 |
| FilePicker | `UseFilePickerUI()` | 弹窗式文件选择器，通过 `postMessage` 返回选中路径 |

```csharp
app.UseFileManagerUI("/Spa", f => f.UseSpaFileManagerUI());
app.UseFileManagerUI("/Vue", f => f.UseVueFileManagerUI());
app.UseFileManagerUI("/AdminLTE", f => f.UseAdminLTEFileManagerUI());
app.UseFileManagerUI("/FilePicker", f => f.UseFilePickerUI());
```

[FileManager.Sample.Mvc.Full](../Samples/FileManager.Sample.Mvc.Full/) 与 [FileManager.Sample.Blazor.Full](../Samples/FileManager.Sample.Blazor.Full/) 同时演示全部 UI。

## 授权过滤

Explorer 和 RichFilemanager 的路由均支持 `IDashboardAuthorizationFilter` 授权过滤器（通过内部 `UseMapDashboard` 传递），可用于限制访问权限。具体用法需参考 AspNetCoreDashboard 文档。

## 路径约定

FileExplorer UI 使用 `[drive]:path` 格式的路径参数，例如 `[default]:folder/subfolder`。后端通过正则解析：

```
\[(?<drive>.*)\]:(?<path>.*)
```

当前实现中 `drive` 参数未被实际使用，所有操作均针对 `IFileStore` 的相对路径。
