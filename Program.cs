using System.Net;
using Blazored.Toast;
using Fenrus;
using Fenrus.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.HttpOverrides;
using NUglify.Helpers;

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
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllersWithViews();
builder.Services.AddMvc();

//Gets the reverse proxy settings from the appsettings.json file
//to check if the app is running behind a reverse proxy
ReverseProxySettings reverseProxySettings = builder.Configuration.GetSection(nameof(ReverseProxySettings)).Get<ReverseProxySettings>();

if(reverseProxySettings.UseForwardedHeaders)
{
    ConfigureUsingForwardedHeaders(builder, reverseProxySettings);
}
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.MinifyJsFiles("js/**/*.js");
    var jsFiles = Directory.GetFiles("wwwroot/js", "*.js", SearchOption.AllDirectories)
        .OrderBy(f => f)
        .Select(x =>
        {
            int index = x.IndexOf("wwwroot/js");
            return x[(index + 7)..];
        })
        .ToArray();
    pipeline.AddJavaScriptBundle("/js/_fenrus.js", jsFiles);
    
    pipeline.CompileScssFiles(new () { MinifyCss = true, SourceComments = false});
    pipeline.AddScssBundle("/css/_fenrus.css", "css/**/*.scss");
});
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = long.MaxValue;
});
builder.Services.AddBlazoredToast();
if(Environment.GetEnvironmentVariable("DetailedErrors") == "1")
    builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });

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
                //Added option to debug request headers for reverse proxy
                //Sometimes it can be difficult to find out if X-Forwarded-X headers are set correctly
                if(reverseProxySettings.DebugPrintRequestHeaders)
                    Logger.DLog($"Request headers: {string.Join(Environment.NewLine, context.Request.Headers)}");
                return Task.FromResult(0);
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

if(reverseProxySettings.UseForwardedHeaders)
    app.UseForwardedHeaders();
bool debug = Environment.GetEnvironmentVariable("DEBUG") == "1";
app.UseWhen(context =>
{
    if(debug)
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
// move this into a worker...
new UpTimeService().Start();
var workers = new Fenrus.Workers.Worker[]
{
    new Fenrus.Workers.CalendarEventWorker(),
    Fenrus.Workers.MailWorker.Instance
};
workers.ForEach(x => x.Start());


Logger.ILog($"Fenrus v{Fenrus.Globals.Version} started");
app.Run();
Logger.ILog($"Fenrus v{Fenrus.Globals.Version} stopped");
workers.ForEach(x => x.Stop());

// Configure the app to use forwarded headers
//If the app is running behind a reverse proxy, the app needs to be configured to use the forwarded headers
//This means that X-Forwarded-For and X-Forwarded-Proto headers are used to determine if the request goes over https, 
//but is using ssl termination
void ConfigureUsingForwardedHeaders(WebApplicationBuilder webApplicationBuilder,
    ReverseProxySettings reverseProxySettings1)
{
    webApplicationBuilder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        foreach (var knownProxy in reverseProxySettings1.KnownProxies)
            options.KnownProxies.Add(IPAddress.Parse($"{knownProxy}"));
        if (reverseProxySettings1.KnownIpv4Network.Enabled)
        {
            if (string.IsNullOrWhiteSpace(reverseProxySettings1.KnownIpv4Network.IpAddress) ||
                reverseProxySettings1.KnownIpv4Network.PrefixLength == 0)
                throw new InvalidOperationException("Invalid IPv4 network configuration");
            options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(reverseProxySettings1.KnownIpv4Network.IpAddress),
                reverseProxySettings1.KnownIpv4Network.PrefixLength));
        }

        if (!reverseProxySettings1.KnownIpv6Network.Enabled) return;
        if(string.IsNullOrWhiteSpace(reverseProxySettings1.KnownIpv6Network.IpAddress) || reverseProxySettings1.KnownIpv6Network.PrefixLength == 0)
            throw new InvalidOperationException("Invalid IPv6 network configuration");
        options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse(reverseProxySettings1.KnownIpv6Network.IpAddress), reverseProxySettings1.KnownIpv6Network.PrefixLength));
    });
}