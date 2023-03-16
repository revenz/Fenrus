using Blazored.Toast;
using Fenrus;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

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
StartUpHelper.Run();

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

builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(DirectoryHelper.GetDataDirectory()))
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

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

            options.Scope.Add("email");
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.GetClaimsFromUserInfoEndpoint = true;
            options.DisableTelemetry = true;
            options.Events.OnRedirectToIdentityProvider = context =>
            {
                context.ProtocolMessage.Prompt = "login";
                return Task.CompletedTask;
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
_ = new UpTimeService();

Logger.ILog($"Fenrus v{Fenrus.Globals.Version} started");
app.Run();
Logger.ILog($"Fenrus v{Fenrus.Globals.Version} stopped");
