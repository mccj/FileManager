# FileManager.Sample.RazorPages.Full

Razor Pages 宿主 · 完整功能：默认挂载全部 25 种 UI、40+ 存储后端、FileExplorer、REST API、Swagger。

```bash
dotnet run --project Samples/FileManager.Sample.RazorPages.Full/FileManager.Sample.RazorPages.Full.csproj
dotnet run --project Samples/FileManager.Sample.RazorPages.Full --launch-profile InMemory
dotnet run --project Samples/FileManager.Sample.RazorPages.Full --launch-profile MinIO
```

| 端点 | 地址 |
|------|------|
| 首页 | http://localhost:2116 |
| FileExplorer | http://localhost:2116/Explorer/ |
| Swagger | http://localhost:2116/swagger |
| 健康检查 | http://localhost:2116/health |

默认 `MountMode=Full`。完整存储连接串见 `appsettings.storage.full.json`。
