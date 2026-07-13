using Microsoft.AspNetCore.Http;

namespace FileManager.Sample.Shared;

internal sealed class SampleUiAuthMiddleware
{
    private readonly RequestDelegate _next;

    public SampleUiAuthMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (SampleUiMountExtensions.RequiresUiAuth(path) && context.User?.Identity?.IsAuthenticated != true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized. Sign in at /Home/Login");
            return;
        }

        await _next(context);
    }
}
