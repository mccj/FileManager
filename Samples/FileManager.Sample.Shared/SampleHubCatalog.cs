namespace FileManager.Sample.Shared;

/// <summary>首页 UI 导航卡片。</summary>
public sealed record SampleHubUiCard(
    string Title,
    string Description,
    string MethodName,
    string Path,
    bool Extended = false,
    bool Recommended = false);

/// <summary>首页 API / 运维链接卡片。</summary>
public sealed record SampleHubApiCard(
    string Title,
    string Description,
    string Path,
    string ButtonClass = "btn-outline-primary");

/// <summary>示例首页导航数据：UI 卡片、API 链接与存储提供程序列表。</summary>
public static class SampleHubCatalog
{
    public static IReadOnlyList<SampleHubUiCard> GetUiCards(bool includeExtended) =>
        UiCards.Where(card => includeExtended || !card.Extended).ToArray();

    public static IReadOnlyList<SampleHubApiCard> GetApiCards() => ApiCards;

    public static IReadOnlyList<string> GetStorageProviders() => StorageProviders;

    private static readonly SampleHubUiCard[] UiCards =
    [
        new("FileExplorer", "功能完整的富 UI：缩略图、预览、上传、ZIP 等。", "UseFileExplorerUI()", "/Explorer/", Recommended: true),
        new("Rich Filemanager", "基于 Rich Filemanager 开源前端。", "UseRichFileManagerUI()", "/RichFilemanager/"),
        new("elFinder", "elFinder 2.x 文件管理器 + .NET Connector。", "UseElFinderUI()", "/ElFinder/"),
        new("SPA 文件管理", "轻量 HTML/JS 单页，调用内置 /Spa/api/* 接口。", "UseSpaFileManagerUI()", "/Spa/"),
        new("Vue 文件管理", "Vue 单页界面，调用 /Vue/api/* REST 接口。", "UseVueFileManagerUI()", "/Vue/"),
        new("默认目录浏览", "ASP.NET Core 目录列表。", "UseDefaultFileManagerUI()", "/Default/"),
        new("AdminLTE", "AdminLTE 3 风格文件管理界面。", "UseAdminLTEFileManagerUI()", "/AdminLTE/", Extended: true),
        new("File Picker", "弹窗/iframe 文件选择器，通过 postMessage 回传结果。", "UseFilePickerUI()", "/FilePicker/", Extended: true),
        new("React", "React 风格文件管理界面。", "UseReactFileManagerUI()", "/React/", Extended: true),
        new("Angular", "Angular 风格文件管理界面。", "UseAngularFileManagerUI()", "/Angular/", Extended: true),
        new("Blazor UI", "Blazor 风格文件管理界面（REST Shell）。", "UseBlazorFileManagerUI()", "/Blazor/", Extended: true),
        new("Dropzone", "拖拽上传为主的文件管理界面。", "UseDropzoneFileManagerUI()", "/Dropzone/", Extended: true),
        new("Media Library", "媒体库风格界面，适合图片/视频浏览。", "UseMediaLibraryFileManagerUI()", "/MediaLibrary/", Extended: true),
        new("Mobile", "移动端响应式文件管理界面。", "UseMobileFileManagerUI()", "/Mobile/", Extended: true),
        new("Svelte", "Svelte 风格 REST 界面。", "UseSvelteFileManagerUI()", "/Svelte/", Extended: true),
        new("Next.js", "Next.js 风格 REST 界面。", "UseNextJsFileManagerUI()", "/NextJs/", Extended: true),
        new("TinyMCE", "集成 TinyMCE 编辑器的文件选择。", "UseTinyMceFileManagerUI()", "/TinyMce/", Extended: true),
        new("SDK Demo", "REST API / SDK 演示界面。", "UseSdkDemoFileManagerUI()", "/SdkDemo/", Extended: true),
        new("Themes", "明暗主题切换演示。", "UseThemesFileManagerUI()", "/Themes/", Extended: true),
        new("Preact", "Preact 风格 REST 界面。", "UsePreactFileManagerUI()", "/Preact/", Extended: true),
        new("Solid.js", "Solid.js 风格 REST 界面。", "UseSolidJsFileManagerUI()", "/SolidJs/", Extended: true),
        new("CKEditor", "集成 CKEditor 5 的文件选择。", "UseCkEditorFileManagerUI()", "/CkEditor/", Extended: true),
        new("Bootstrap 5", "Bootstrap 5 风格界面。", "UseBootstrap5FileManagerUI()", "/Bootstrap5/", Extended: true),
        new("PWA", "PWA 离线友好界面。", "UsePwaFileManagerUI()", "/Pwa/", Extended: true),
        new("i18n", "多语言切换演示。", "UseI18nFileManagerUI()", "/I18n/", Extended: true),
    ];

    private static readonly SampleHubApiCard[] ApiCards =
    [
        new("REST API", "通过 FileController 上传、下载与删除文件。", "/swagger"),
        new("健康检查", "ASP.NET Core 内置健康检查端点。", "/health", "btn-outline-secondary"),
    ];

    private static readonly string[] StorageProviders =
    [
        "Physical", "ContentRoot", "WebRoot", "Compress", "InMemory", "MultiDrive",
        "Ftp", "WebDav", "MongoDB", "AzureBlob", "AwsS3", "Sftp", "GoogleCloud",
        "Redis", "SqlBlob", "MinIO", "Dropbox", "OneDrive", "GoogleDrive", "SharePoint",
        "Smb", "Nfs", "PostgreSqlBlob", "SqlServerBlob", "Elasticsearch", "Cassandra",
        "Box", "BackblazeB2", "Wasabi", "OracleObject", "IbmCos", "Ldap",
        "DigitalOceanSpaces", "CloudflareR2", "Scaleway", "Hetzner", "OpenStackSwift", "MySqlBlob",
        "RabbitMq", "Kafka", "InfluxDb", "Memcached",
    ];
}
