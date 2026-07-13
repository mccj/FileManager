# API 参考

## Web API（FileManager.UI.WebApi）

提供 REST 风格的文件操作接口，路由前缀为 `api/FileStorage/File/`。

### 控制器

`FileController`（`Src/FileManager.UI.WebApi/FileController.cs`）

### 端点

| 方法 | 路由 | 说明 |
|------|------|------|
| POST | `api/FileStorage/File/UploadAsync` | 上传文件 |
| POST | `api/FileStorage/File/GetFileAsync` | 获取单个文件内容 |
| POST | `api/FileStorage/File/GetListFilesAsync` | 按扩展名批量获取文件 |
| DELETE | `api/FileStorage/File/DeleteFileAsync` | 删除文件 |
| DELETE | `api/FileStorage/File/DeleteDirectoryAsync` | 删除目录 |

### 集成

在 ASP.NET Core 项目中引用 `FileManager.UI.WebApi` 包，确保已注册 `IFileStore`：

```csharp
services.AddFileManager(f => f.AddRootPhysicalFilePath("/App_Data/Files"));
services.AddMvc(); // 注册控制器
```

### 请求/响应示例

**上传文件**

```
POST /api/FileStorage/File/UploadAsync
Content-Type: application/json

{
  "Path": "folder/file.txt",
  "FileBytes": "<base64 或 byte[]>"
}
```

**获取文件**

```
POST /api/FileStorage/File/GetFileAsync?path=folder/file.txt
```

响应：文件内容的 `byte[]`。

**批量获取（按扩展名）**

```
POST /api/FileStorage/File/GetListFilesAsync?path=folder&extension=.txt
```

响应：`byte[][]` 数组。

## SDK（FileManager.UI.WebApi.Sdk）

自动生成的 HTTP 客户端，封装 Web API 调用。

### 使用

```csharp
using FileManagerSdk;

var client = new FileClient(new HttpClient
{
    BaseAddress = new Uri("https://localhost:5001/")
});

// 上传
await client.UploadAsync(new UploadFileInput
{
    Path = "test.txt",
    FileBytes = File.ReadAllBytes("local.txt")
});

// 下载
var bytes = await client.GetFileAsync("test.txt");

// 删除
await client.DeleteFileAsync("test.txt");
```

SDK 方法由 `FileClientGeneration.cs` 和 `FileClientModelsGeneration.cs` 生成。

测试项目：`Test/FileManager.UI.WebApi.Sdk.Test/`

## FileExplorer 内部 API

FileExplorer UI 使用的后端 API（非公开 REST，由前端直接调用）：

### 文件管理

| 端点 | 方法 | 功能 |
|------|------|------|
| `/Filemanager/ListFolder` | POST | 列出目录 |
| `/Filemanager/ExpandFolder` | POST | 展开目录树 |
| `/Filemanager/Create` | POST | 创建文件夹 |
| `/Filemanager/Delete` | POST | 删除 |
| `/Filemanager/Rename` | POST | 重命名 |
| `/Filemanager/Copy` | POST | 复制 |
| `/Filemanager/Move` | POST | 移动 |
| `/Filemanager/Download` | GET/POST | 下载 |
| `/Filemanager/GetImage` | GET/POST | 获取图片 |
| `/Filemanager/GetThumbnail` | GET/POST | 缩略图 |
| `/Filemanager/Preview` | GET/POST | 预览 |
| `/Filemanager/DownloadAsZip` | GET/POST | ZIP 下载（未实现） |
| `/Filemanager/AddToZip` | POST | 添加到 ZIP（未实现） |
| `/Filemanager/ExtractAll` | POST | 解压（未实现） |

### 文件上传

| 端点 | 方法 | 功能 |
|------|------|------|
| `/Fileuploader/Begin` | POST | 开始上传会话 |
| `/Fileuploader/SendStream` | POST | 发送文件数据 |
| `/Fileuploader/End` | POST | 完成上传 |
| `/Fileuploader/Skip` | POST | 跳过文件 |
| `/Fileuploader/Cancel` | POST | 取消上传 |
| `/Fileuploader/Fail` | POST | 上传失败 |
| `/Fileuploader/KeepSessionAlive` | POST | 保持会话 |

### 请求体模型（主要）

```csharp
// 列出目录
class ListFolderParameters {
    string StateId;
    string Path;       // 格式: [drive]:relative/path
    bool IsRefresh;
}

// 删除
class DeleteParameters {
    string Path;
    string[] ItemNames;  // 文件夹名以 \ 结尾
}

// 重命名
class RenameParameters {
    string Path;
    string ItemName;
    string ItemNewName;
}

// 复制/移动
class CopyParameters {
    string Path;
    string TargetPath;
    string[] ItemNames;
}
```

## 核心 DI 扩展方法

### FileServicesOptions（服务注册）

| 方法 | 包 | 说明 |
|------|-----|------|
| `AddFileStore(IFileStore)` | Core | 注册自定义存储 |
| `AddFileStore(Func<IServiceProvider, IFileStore>)` | Core | 工厂注册 |
| `AddPermissionHandle(...)` | Core | 注册权限处理 |
| `AddWebRootFileProviderReadOnly()` | Storage.Standard | wwwroot 只读 |
| `AddContentRootFileProviderReadOnly()` | Storage.Standard | ContentRoot 只读 |
| `AddFileProviderPhysicalPathReadOnly(path)` | Storage.Standard | 物理路径只读 |
| `AddPhysicalFilePath(path)` | Storage.Standard | 物理路径读写 |
| `AddRootPhysicalFilePath(path)` | Storage.Standard | 相对 ContentRoot 读写 |
| `AddFtpStore(...)` | Storage.Ftp | FTP |
| `AddWebDavStore(...)` | Storage.WebDav | WebDAV |
| `AddCompressStore(...)` | Storage.Compress | 压缩包 |
| `AddMongoDBStore(...)` | Storage.MongoDB | MongoDB |

### FileManagerOptions（中间件注册）

| 方法 | 包 | 说明 |
|------|-----|------|
| `UseFileExplorerUI()` | UI.Explorer | FileExplorer 界面 |
| `UseRichFileManagerUI()` | UI.RichFilemanager | Rich Filemanager |
| `UseDefaultFileManagerUI()` | UI | 默认目录浏览 |

### 顶层入口

| 方法 | 说明 |
|------|------|
| `services.AddFileManager(Action<FileServicesOptions>)` | 注册文件管理服务 |
| `app.UseFileManagerUI(PathString, Action<FileManagerOptions>)` | 挂载 UI 中间件 |
