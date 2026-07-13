# Samples 使用指南

本目录包含可运行的示例，按 **宿主模型 × 功能级别** 组织，并保留遗留 OWIN 示例。

## 如何选择

```
只想验证能否跑起来？              → 任选 *·Minimal（Razor Pages / MVC / Blazor）
要对比多种 UI / 切换存储后端？    → 任选 *·Full（推荐 Mvc.Full 或 Blazor.Full）
要接 REST API / 生产形态？        → 任选 *·Full
遗留 .NET Framework + OWIN？      → FileManager.Sample.Owin（新项目请勿使用）
```

## 宿主模型 × 级别矩阵

| 宿主 | Minimal（只读 ContentRoot） | Full（可配置存储 + 多 UI + API + Swagger） |
|------|----------------------------|---------------------------------------------|
| **Razor Pages** | [RazorPages.Minimal](FileManager.Sample.RazorPages.Minimal/) · 2115 | [RazorPages.Full](FileManager.Sample.RazorPages.Full/) · 2116 |
| **MVC** | [Mvc.Minimal](FileManager.Sample.Mvc.Minimal/) · 2120 | [Mvc.Full](FileManager.Sample.Mvc.Full/) · 5000 |
| **Blazor Server** | [Blazor.Minimal](FileManager.Sample.Blazor.Minimal/) · 2130 | [Blazor.Full](FileManager.Sample.Blazor.Full/) · 2131 |

| 其他示例 | 命令 | 默认端口 | 适用场景 |
|----------|------|----------|----------|
| [Owin](FileManager.Sample.Owin/) | `dotnet run --project Samples/FileManager.Sample.Owin` | 8080 | .NET Framework 4.8 OWIN 宿主 |

共享存储与 UI 挂载逻辑见 [FileManager.Sample.Shared](FileManager.Sample.Shared/)（纯类库：配置、数据与扩展方法；各 *·Full 项目自行实现首页 UI）。

**UI 与存储组合矩阵**见 [ui-and-storage.md](ui-and-storage.md)。

## 级别说明

### Minimal

- `AddContentRootFileProviderReadOnly()`：只读浏览应用 ContentRoot
- FileExplorer 挂载在 `/Browser/`
- Razor Pages / MVC 额外提供 `/Default/` 默认目录浏览

### Full

- 通过 `appsettings.storage.full.json` + launch profile 切换 **40+** 存储后端（`FileManager.Sample.Shared`）
- 默认 **Full** 模式挂载全部 **25** 种 UI
- 可读写 `App_Data/Files`（Physical 模式）
- FileExplorer（`/Explorer/`）+ `FileController` REST API + Swagger + `/health`
- **Mvc.Full** 支持可选演示登录（`FileManager:Showcase:RequireAuth=true`）

## 快速运行

```bash
# Razor Pages
dotnet run --project Samples/FileManager.Sample.RazorPages.Minimal
dotnet run --project Samples/FileManager.Sample.RazorPages.Full

# MVC
dotnet run --project Samples/FileManager.Sample.Mvc.Minimal
dotnet run --project Samples/FileManager.Sample.Mvc.Full

# Blazor Server
dotnet run --project Samples/FileManager.Sample.Blazor.Minimal
dotnet run --project Samples/FileManager.Sample.Blazor.Full
```

Full 级别切换存储示例：

```bash
dotnet run --project Samples/FileManager.Sample.Mvc.Full --launch-profile Compress
dotnet run --project Samples/FileManager.Sample.RazorPages.Full --launch-profile InMemory
dotnet run --project Samples/FileManager.Sample.Blazor.Full --launch-profile Compress
```

`appsettings.storage.full.json` 由 `AddSampleFullConfiguration()` 加载，包含全部存储后端的默认连接串。

## 本地依赖（可选）

云存储与数据库演示可使用 Docker 一键启动依赖服务：

```bash
docker compose -f Samples/docker-compose.yml up -d
```

## NuGet 引用（对外项目）

仓库内示例使用 `ProjectReference` 便于联调。在独立 ASP.NET Core 项目中，典型引用为：

| 场景 | NuGet 包 |
|------|----------|
| FileExplorer + 本地存储 | `FileManager.UI`、`FileManager.UI.Explorer`、`FileManager.Storage.Standard` |
| REST API | + `FileManager.UI.WebApi` |
| 云存储 / FTP 等 | + 对应 `FileManager.Storage.*` |

## FileManager.Sample.Owin — OWIN 宿主

> **仅用于遗留系统**。新项目请使用上述 ASP.NET Core 示例。

## REST API 注册

Full 级别示例需通过 `AddApplicationPart(typeof(FileController).Assembly)` 注册 `FileController` 所在程序集，参见各 `Program.cs`。

## 安全提示

Full 级别默认可写存储使用 `AddFullAccessPermissionHandle()`，**仅供本地演示**。生产环境请配置身份认证与细粒度权限。
