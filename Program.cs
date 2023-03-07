using Blazored.Toast;
using Fenrus;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

if (args?.Any() == true && args[0] == "--init-config")
{
    Console.WriteLine("Resetting initial configuration flags");
    new SystemSettingsService().Delete();
    if (Globals.IsDocker)
        Console.WriteLine("Restart the Docker container to take effect.");
    else
        Console.WriteLine("Restart the application to take effect.");
    return;
}

Console.WriteLine("Starting Fenrus...");
Logger.Initialize();
AppService.Initialize();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddMvc();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.MinifyJsFiles("js/**/*.js");
    pipeline.AddJavaScriptBundle("/js/_fenrus.js", "js/**/*.js");
    pipeline.CompileScssFiles(new () { MinifyCss = true, SourceComments = false});
    pipeline.AddScssBundle("/css/_fenrus.css", "css/**/*.scss");
});
builder.Services.AddBlazoredToast();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor(options =>
{
});

builder.Services.AddSignalR(hubOptions =>
{
    hubOptions.MaximumReceiveMessageSize = 10 * 1024 * 1024; // 10MB
});
builder.Services.AddTransient<Fenrus.Middleware.InitialConfigMiddleware>();

var dataDir = DirectoryHelper.GetDataDirectory();
if (Directory.Exists(dataDir) == false)
    Directory.CreateDirectory(dataDir);

bool oAuth = SystemSettingsService.InitConfigDone && SystemSettingsService.UsingOAuth;

if (oAuth)
{
    var system = new SystemSettingsService().Get();
    builder.Services.AddAuthentication(o =>
        {
            o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie()
        .AddOpenIdConnect(options =>
        {
            options.ClientId = system.OAuthStrategyClientId;
            options.ClientSecret = system.OAuthStrategySecret;
            options.Authority = system.OAuthStrategyIssuerBaseUrl;
            options.Events = new()
            {
                OnTokenValidated = async ctx =>
                {
                    //var userID = ctx.Principal.FindFirstValue("sub");

                    //var db = ctx.HttpContext.RequestServices.GetRequiredService<MyDb>();

                    //Do things I need to do with the user here.
                }
            };
        });
}
else
{
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            options.SlidingExpiration = true;
            options.AccessDeniedPath = "/Forbidden/";
        });
}

var app = builder.Build();

app.UseWhen(context =>
{
    Logger.DLog("Requesting: " + context.Request.Path);
    if (SystemSettingsService.InitConfigDone)
        return false;
    var path = context.Request.Path;
    if (path.StartsWithSegments("/settings"))
        return true;
    if (path.StartsWithSegments("/dashboard"))
        return true;
    if (path.StartsWithSegments("/fimage"))
        return false;
    if (path.Value == "/")
        return true;
    return false;
}, appBuilder => appBuilder.UseMiddleware<Fenrus.Middleware.InitialConfigMiddleware>());

app.UseWhen(context => SystemSettingsService.InitConfigDone,
    appBuild =>
    {
        appBuild.UseWebSockets();
    });

// app.UseWhen(context => Fenrus.Services.SystemSettingsService.InitConfigDone && Fenrus.Services.SystemSettingsService.UsingOAuth,
//     appBuild =>
//     {
//     });

if (oAuth)
{
    // app.MapGet("/login", (HttpContent ctx) =>
    // {
    // });
}

app.UseWebOptimizer();
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/apps") == false,
    appBuilder => appBuilder.UseStaticFiles(new StaticFileOptions()
    {
        HttpsCompression = Microsoft.AspNetCore.Http.Features.HttpsCompressionMode.Compress,
    
        OnPrepareResponse = (context) =>
        {
            var headers = context.Context.Response.GetTypedHeaders();
            headers.CacheControl = new Microsoft.Net.Http.Headers.CacheControlHeaderValue
            {
                Public = true,
                MaxAge = TimeSpan.FromDays(30)
            };
        }
    }));

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(x =>
{
    x.MapControllers();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub(options =>
{
});
app.MapFallbackToPage("/_Host");

// create an uptime service to monitor the uptime status for monitor apps/links
new UpTimeService();

Logger.ILog($"Fenrus v{Fenrus.Globals.Version} started");
app.Run();
Logger.ILog($"Fenrus v{Fenrus.Globals.Version} stopped");
