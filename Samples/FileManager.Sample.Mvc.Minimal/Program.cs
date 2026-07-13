// MVC · Minimal 入门示例
// - 只读浏览 ContentRoot；/Browser/ + /Default/ 两种 UI
// - 无 Swagger / 可配置存储；完整演示见 Mvc.Full

using FileManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddFileManager(f => f.AddContentRootFileProviderReadOnly());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");

app.UseStaticFiles();
app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());
app.UseFileManagerUI("/Default", f => f.UseDefaultFileManagerUI());
app.UseRouting();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");

app.Run();
