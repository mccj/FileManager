// Razor Pages · Full 演示
// - 配置：appsettings.json + appsettings.storage.full.json（AddSampleFullConfiguration）
// - UI：25 种界面 + REST API + Swagger + /health（无演示登录，见 Mvc.Full）

using FileManager;
using FileManager.Controllers;
using FileManager.Sample.Shared;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddSampleFullConfiguration();

builder.Services.Configure<SampleStorageOptions>(builder.Configuration.GetSection(SampleStorageOptions.SectionName));
builder.Services.Configure<SampleShowcaseOptions>(builder.Configuration.GetSection(SampleShowcaseOptions.SectionName));

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddApplicationPart(typeof(FileController).Assembly);

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
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseOpenApi();
app.UseSwaggerUi();
app.UseHealthChecks("/health");

var showcase = app.Configuration.GetSection(SampleShowcaseOptions.SectionName).Get<SampleShowcaseOptions>() ?? new SampleShowcaseOptions();
SampleUiMountExtensions.MountConfiguredUis(app, showcase);

app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();

app.Run();
