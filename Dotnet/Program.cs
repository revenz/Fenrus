Fenrus.Logger.Initialize();
Fenrus.Services.AppService.Initialize();

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

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var app = builder.Build();
app.UseWebSockets();
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
app.UseEndpoints(x =>
{
    x.MapControllers();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
