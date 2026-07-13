using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace FileManager.Sample.Shared;

/// <summary>为 *·Full 示例加载完整存储配置（<c>appsettings.storage.full.json</c>）。</summary>
public static class SampleFullHostExtensions
{
    /// <summary>追加 <c>appsettings.storage.full.json</c> 到配置源（可选、支持热重载）。</summary>
    /// <remarks>
    /// 加载顺序：appsettings.json → appsettings.{Environment}.json → appsettings.storage.full.json。
    /// 后加载的同名键会覆盖先前的值；launch profile 环境变量优先级更高。
    /// </remarks>
    public static WebApplicationBuilder AddSampleFullConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile(
            Path.Combine(builder.Environment.ContentRootPath, "appsettings.storage.full.json"),
            optional: true,
            reloadOnChange: true);
        return builder;
    }
}
