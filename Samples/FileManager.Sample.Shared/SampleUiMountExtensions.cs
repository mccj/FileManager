using FileManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FileManager.Sample.Shared;

/// <summary>按 Showcase 配置挂载文件管理 UI，并提供旧路径 <c>/Browser</c> 重定向。</summary>
public static class SampleUiMountExtensions
{
    private static readonly string[] CoreUiPathPrefixes =
    {
        "/Explorer", "/RichFilemanager", "/ElFinder", "/Spa", "/Vue", "/Default"
    };

    private static readonly string[] ExtendedUiPathPrefixes =
    {
        "/AdminLTE", "/FilePicker", "/React", "/Angular", "/Blazor", "/Dropzone",
        "/MediaLibrary", "/Mobile", "/Svelte", "/NextJs", "/TinyMce", "/SdkDemo",
        "/Themes", "/Preact", "/SolidJs", "/CkEditor", "/Bootstrap5", "/Pwa", "/I18n"
    };

    /// <summary>判断路径是否属于需要认证的文件管理 UI。</summary>
    public static bool RequiresUiAuth(string path)
    {
        foreach (var prefix in CoreUiPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        foreach (var prefix in ExtendedUiPathPrefixes)
        {
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    /// <summary>挂载六种核心 UI（Explorer、RichFilemanager、elFinder、Spa、Vue、Default）。</summary>
    public static void MountCoreUis(IApplicationBuilder app)
    {
        app.UseFileManagerUI("/Explorer", f => f.UseFileExplorerUI());
        app.UseFileManagerUI("/RichFilemanager", f => f.UseRichFileManagerUI());
        app.UseFileManagerUI("/ElFinder", f => f.UseElFinderUI());
        app.UseFileManagerUI("/Spa", f => f.UseRestShellFileManagerUI("spa"));
        app.UseFileManagerUI("/Vue", f => f.UseRestShellFileManagerUI("vue"));
        app.UseFileManagerUI("/Default", f => f.UseDefaultFileManagerUI());
    }

    /// <summary>挂载扩展演示 UI（AdminLTE、React、PWA 等）。</summary>
    public static void MountExtendedUis(IApplicationBuilder app)
    {
        app.UseFileManagerUI("/AdminLTE", f => f.UseAdminLTEFileManagerUI());
        app.UseFileManagerUI("/FilePicker", f => f.UseFilePickerUI());
        app.UseFileManagerUI("/React", f => f.UseRestShellFileManagerUI("react"));
        app.UseFileManagerUI("/Angular", f => f.UseRestShellFileManagerUI("angular"));
        app.UseFileManagerUI("/Blazor", f => f.UseRestShellFileManagerUI("blazor"));
        app.UseFileManagerUI("/Dropzone", f => f.UseRestShellFileManagerUI("dropzone"));
        app.UseFileManagerUI("/MediaLibrary", f => f.UseRestShellFileManagerUI("medialibrary"));
        app.UseFileManagerUI("/Mobile", f => f.UseRestShellFileManagerUI("mobile"));
        app.UseFileManagerUI("/Svelte", f => f.UseRestShellFileManagerUI("svelte"));
        app.UseFileManagerUI("/NextJs", f => f.UseRestShellFileManagerUI("nextjs"));
        app.UseFileManagerUI("/TinyMce", f => f.UseTinyMceFileManagerUI());
        app.UseFileManagerUI("/SdkDemo", f => f.UseRestShellFileManagerUI("sdkdemo"));
        app.UseFileManagerUI("/Themes", f => f.UseRestShellFileManagerUI("themes"));
        app.UseFileManagerUI("/Preact", f => f.UseRestShellFileManagerUI("preact"));
        app.UseFileManagerUI("/SolidJs", f => f.UseRestShellFileManagerUI("solidjs"));
        app.UseFileManagerUI("/CkEditor", f => f.UseCkEditorFileManagerUI());
        app.UseFileManagerUI("/Bootstrap5", f => f.UseRestShellFileManagerUI("bootstrap5"));
        app.UseFileManagerUI("/Pwa", f => f.UsePwaFileManagerUI());
        app.UseFileManagerUI("/I18n", f => f.UseRestShellFileManagerUI("i18n"));
    }

    /// <summary>按 <see cref="SampleShowcaseOptions.MountMode"/> 挂载 UI，并处理 <c>/Browser</c> 兼容重定向。</summary>
    public static void MountConfiguredUis(IApplicationBuilder app, SampleShowcaseOptions? showcase)
    {
        app.Use(LegacyBrowserRedirect);
        MountCoreUis(app);
        if (string.Equals(showcase?.MountMode, "Full", StringComparison.OrdinalIgnoreCase))
            MountExtendedUis(app);
    }

    private static async Task LegacyBrowserRedirect(HttpContext context, RequestDelegate next)
    {
        var path = context.Request.Path.Value;
        if (path != null && path.StartsWith("/Browser", StringComparison.OrdinalIgnoreCase))
        {
            var target = "/Explorer" + path["/Browser".Length..];
            if (string.IsNullOrEmpty(target) || target == "/Explorer")
                target = "/Explorer/";
            context.Response.Redirect(target + context.Request.QueryString);
            return;
        }

        await next(context);
    }
}
