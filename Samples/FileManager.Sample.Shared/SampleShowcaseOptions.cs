namespace FileManager.Sample.Shared;

/// <summary>示例应用展示行为（UI 挂载级别、可选演示登录）。</summary>
public class SampleShowcaseOptions
{
    public const string SectionName = "FileManager:Showcase";

    /// <summary>
    /// Lite = 核心 UI（Explorer、RichFilemanager、elFinder、Spa、Vue、Default）；
    /// Full = 挂载全部演示 UI。
    /// </summary>
    public string MountMode { get; set; } = "Full";

    /// <summary>
    /// 为 true 时，文件管理 UI 路径需要 Cookie 登录（演示账号见 DemoUsername / DemoPassword）。
    /// </summary>
    public bool RequireAuth { get; set; }

    /// <summary>演示登录用户名。</summary>
    public string DemoUsername { get; set; } = "demo";

    /// <summary>演示登录密码（仅用于本地示例）。</summary>
    public string DemoPassword { get; set; } = "demo";
}
