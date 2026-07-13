// MVC · Full 演示
// - 配置：appsettings.json + appsettings.storage.full.json（AddSampleFullConfiguration）
// - 可选 Cookie 演示登录：FileManager:Showcase:RequireAuth=true → /Home/Login（demo/demo）
// - UI：25 种界面 + REST API（FileController）+ Swagger + /health

using FileManager;
using FileManager.Controllers;
using FileManager.Sample.Shared;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddSampleFullConfiguration();

builder.Services.Configure<SampleStorageOptions>(builder.Configuration.GetSection(SampleStorageOptions.SectionName));
builder.Services.Configure<SampleShowcaseOptions>(builder.Configuration.GetSection(SampleShowcaseOptions.SectionName));

builder.Services.AddControllersWithViews()
    .AddApplicationPart(typeof(FileController).Assembly);

builder.Services.AddSampleDemoAuth(builder.Configuration);

builder.Services.AddFileManager(options => SampleStorageConfigurator.Configure(options, builder.Configuration));

builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "FileManager API";
    document.Version = "v1";
});
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseHealthChecks("/health");

var showcase = app.Configuration.GetSection(SampleShowcaseOptions.SectionName).Get<SampleShowcaseOptions>() ?? new SampleShowcaseOptions();
app.UseSampleDemoAuth(showcase);
SampleUiMountExtensions.MountConfiguredUis(app, showcase);

app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapControllers();

app.Run();
