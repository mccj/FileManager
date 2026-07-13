// Razor Pages · Minimal 入门示例
// - 只读浏览 ContentRoot；/Browser/ + /Default/
// - 完整多存储、多 UI 见 RazorPages.Full

using FileManager;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddFileManager(f => f.AddContentRootFileProviderReadOnly());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Error");

app.UseStaticFiles();
app.UseFileManagerUI("/Browser", f => f.UseFileExplorerUI());
app.UseFileManagerUI("/Default", f => f.UseDefaultFileManagerUI());
app.UseRouting();
app.MapRazorPages();

app.Run();
