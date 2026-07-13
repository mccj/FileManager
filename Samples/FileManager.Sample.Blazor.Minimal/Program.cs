// Blazor Server · Minimal 入门示例
// - 无 appsettings 存储配置：固定只读浏览 ContentRoot
// - UI：/Browser/（FileExplorer）、/Default/（目录列表）
// - 不含 REST API / Swagger；需要多存储与多 UI 请使用 Blazor.Full

using FileManager;
using FileManager.Sample.Blazor.Minimal.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 只读挂载应用根目录，便于快速验证能否运行
builder.Services.AddFileManager(f => f.AddContentRootFileProviderReadOnly());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseStaticFiles();
app.UseAntiforgery();

app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());
app.UseFileManagerUI("/Default", f => f.UseDefaultFileManagerUI());

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
