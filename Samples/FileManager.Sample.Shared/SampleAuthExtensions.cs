using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileManager.Sample.Shared;

/// <summary>可选 Cookie 演示登录（用于 Mvc.Full 等示例）。</summary>
public static class SampleAuthExtensions
{
    /// <summary>当 <see cref="SampleShowcaseOptions.RequireAuth"/> 为 true 时注册 Cookie 认证。</summary>
    public static IServiceCollection AddSampleDemoAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var showcase = configuration.GetSection(SampleShowcaseOptions.SectionName).Get<SampleShowcaseOptions>()
            ?? new SampleShowcaseOptions();
        if (showcase.RequireAuth)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options => options.LoginPath = "/Home/Login");
        }

        return services;
    }

    /// <summary>启用认证中间件，并拦截未登录的文件管理 UI 请求。</summary>
    public static IApplicationBuilder UseSampleDemoAuth(this IApplicationBuilder app, SampleShowcaseOptions showcase)
    {
        if (showcase.RequireAuth)
        {
            app.UseAuthentication();
            app.UseMiddleware<SampleUiAuthMiddleware>();
        }

        return app;
    }
}
