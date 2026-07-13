// Blazor Server · Full 演示
// - 配置：appsettings.json（日志） + appsettings.storage.full.json（40+ 存储、Showcase，见 AddSampleFullConfiguration）
// - UI：MountMode=Full 时挂载 25 种界面；推荐入口 /Explorer/
// - API：/swagger、/health；存储切换见 launch profile 或 FileManager__Storage__Provider 环境变量

using FileManager;
using FileManager.Controllers;
using FileManager.Sample.Blazor.Full.Components;
using FileManager.Sample.Shared;

var builder = WebApplication.CreateBuilder(args);

// 追加完整存储配置（后加载的键会覆盖 appsettings.json 中的同名项）
builder.AddSampleFullConfiguration();

builder.Services.Configure<SampleStorageOptions>(builder.Configuration.GetSection(SampleStorageOptions.SectionName));
builder.Services.Configure<SampleShowcaseOptions>(builder.Configuration.GetSection(SampleShowcaseOptions.SectionName));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(FileController).Assembly);

// 按 FileManager:Storage:Provider 注册对应 IFileStore
builder.Services.AddFileManager(options => SampleStorageConfigurator.Configure(options, builder.Configuration));

builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "FileManager API";
    document.Version = "v1";
});
builder.Services.AddHealthChecks();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseStaticFiles();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseHealthChecks("/health");
app.UseAntiforgery();

// 挂载 FileExplorer 等 UI；/Browser 自动重定向到 /Explorer
var showcase = app.Configuration.GetSection(SampleShowcaseOptions.SectionName).Get<SampleShowcaseOptions>() ?? new SampleShowcaseOptions();
SampleUiMountExtensions.MountConfiguredUis(app, showcase);

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
app.MapControllers();

app.Run();
