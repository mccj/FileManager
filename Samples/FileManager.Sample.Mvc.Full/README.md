# FileManager.Sample.Mvc.Full

MVC 宿主 · 完整功能：默认挂载全部 25 种 UI、40+ 存储后端、FileExplorer、REST API、Swagger。

```bash
dotnet run --project Samples/FileManager.Sample.Mvc.Full/FileManager.Sample.Mvc.Full.csproj
dotnet run --project Samples/FileManager.Sample.Mvc.Full --launch-profile Compress
dotnet run --project Samples/FileManager.Sample.Mvc.Full --launch-profile AzureBlob
```

| 端点 | 地址 |
|------|------|
| 首页 | http://localhost:5000 |
| FileExplorer | http://localhost:5000/Explorer/ |
| Swagger | http://localhost:5000/swagger |
| 健康检查 | http://localhost:5000/health |

默认 `MountMode=Full`。完整存储连接串见 `appsettings.storage.full.json`。

## 可选认证

在 `appsettings.json` 中设置 `FileManager:Showcase:RequireAuth` 为 `true` 后，所有文件管理 UI 路径需先登录 `/Home/Login`（默认账号 `demo` / `demo`）。

## 本地依赖（可选）

```bash
docker compose -f Samples/docker-compose.yml up -d
```

包含 MongoDB、Azurite、MinIO、Redis、RabbitMQ、Kafka、Elasticsearch、Memcached、InfluxDB。
