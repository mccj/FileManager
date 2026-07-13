# FileManager.Sample.Blazor.Full

Blazor Server 宿主 · **Showcase 级演示**：默认挂载全部 25 种 UI、40+ 存储后端、REST API、Swagger、丰富样本文件。

```bash
dotnet run --project Samples/FileManager.Sample.Blazor.Full/FileManager.Sample.Blazor.Full.csproj
dotnet run --project Samples/FileManager.Sample.Blazor.Full --launch-profile Compress
dotnet run --project Samples/FileManager.Sample.Blazor.Full --launch-profile MongoDB
```

| 端点 | 地址 |
|------|------|
| 首页（UI/存储导航） | http://localhost:2131 |
| FileExplorer（推荐） | http://localhost:2131/Explorer/ |
| Rich Filemanager | http://localhost:2131/RichFilemanager/ |
| 默认目录浏览 | http://localhost:2131/Default/ |
| Swagger | http://localhost:2131/swagger |
| 健康检查 | http://localhost:2131/health |

默认 `MountMode=Full`（全部 25 种 UI）。完整存储连接串见 `appsettings.storage.full.json`（由 `AddSampleFullConfiguration()` 加载）。

切换存储：修改 `FileManager:Storage:Provider` 或使用 launch profile（Physical、Compress、InMemory、MongoDB、AzureBlob、MinIO、Redis 等）。

本地依赖：`docker compose -f Samples/docker-compose.yml up -d`
