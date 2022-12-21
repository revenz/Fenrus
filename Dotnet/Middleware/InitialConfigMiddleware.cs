using Fenrus.Services;

namespace Fenrus.Middleware;

/// <summary>
/// Middleware used to redirect to initial configuration if not configured
/// </summary>
public class InitialConfigMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (SystemSettingsService.InitConfigDone || context.Request.Path.Value.Contains("init-config"))
        {
            await next(context);
            return;
        }

        context.Response.Redirect("/init-config");
    }
}