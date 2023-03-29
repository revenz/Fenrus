using Fenrus.Services;
using System.Web;
using Microsoft.AspNetCore.Authentication;


namespace Fenrus.Middleware;

/// <summary>
/// Middleware for openID connection
/// </summary>
public class OpenIDConnectMiddleware:IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (SystemSettingsService.UsingOAuth || context.Request.Path.Value.Contains("init-config"))
        {
            await next(context);
            return;
        }
        throw new NotImplementedException();
    }
}