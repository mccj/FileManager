# FileManager.Sample.Owin



> **仅用于遗留系统**：本示例面向无法迁移到 ASP.NET Core 的 .NET Framework 4.8 + OWIN 宿主。  

> **新项目请使用** `FileManager.Sample.*.Full`（ASP.NET Core）— 支持全部 25 种 UI 与 40+ 存储后端。



.NET Framework 4.8 + OWIN 控制台宿主示例。



## 运行



```bash

dotnet run --project Samples/FileManager.Sample.Owin/FileManager.Sample.Owin.csproj

```



切换存储（本宿主支持 Physical、Compress）：



```bash

$env:FileManager__Storage__Provider="Compress"

dotnet run --project Samples/FileManager.Sample.Owin

```



自定义监听地址：



```bash

dotnet run --project Samples/FileManager.Sample.Owin -- http://localhost:9090/

```



## 端点



| 路径 | 说明 |

|------|------|

| `/` | 导航首页（当前存储、FileExplorer 链接） |

| `/Explorer/` | FileExplorer 文件管理界面 |

| `/Browser/` | 重定向到 `/Explorer/` |



## 端口对照（Samples）



| 示例 | 默认地址 |

|------|----------|

| FileManager.Sample.Mvc.Full | http://localhost:5000 |

| FileManager.Sample.Blazor.Full | http://localhost:2131 |

| FileManager.Sample.RazorPages.Full | http://localhost:2116 |

| FileManager.Sample.Owin | http://localhost:8080 |



## 存储



- **Physical**（默认）：`App_Data/Files`（可放入测试文件）
- **Compress**：只读 ZIP（`App_Data/sample.zip`）

InMemory、MultiDrive、云存储与数据库后端请在 ASP.NET Core `*·Full` 示例中体验；配合 `Samples/docker-compose.yml` 启动本地依赖。



## 代码要点



```csharp
OwinApplicationBuilderExtensions.UseFileManager(
    app, "/Explorer", OwinSampleStorage.Configure, o => o.UseFileExplorerUI());
```

```



OWIN 仅支持 FileExplorer UI；`OwinApplicationBuilderExtensions` 的 UI 模块种类限于 `explorer`。

