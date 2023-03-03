using System.Timers;
using Fenrus.Models;
using Timer = System.Timers.Timer;

namespace Fenrus.Services;

/// <summary>
/// Service for monitoring up-time
/// </summary>
public class UpTimeService
{
    private readonly HttpClient HttpClient;
    private Timer Timer;

    public UpTimeService()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (_, _, _, _) => true;
        HttpClient = new(handler);
        Timer = new();
        Timer.Elapsed += TimerOnElapsed;
        Timer.AutoReset = true;
        Timer.Enabled = true;
    } 

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        => Monitor();

    public void Monitor()
    {
        var userGroups = new UserSettingsService().GetAllGroups();
        var systemGroups = new GroupService().GetSystemGroups();
        var items = userGroups.Union(systemGroups).Where(x => x.Enabled)
            .SelectMany(x => x.Items)
            .Select(x =>
            {
                string url = null;
                if (x is AppItem app)
                    url = app.Url;
                else if(x is LinkItem link)
                    url = link.Url;
                return url;
            }).Where(x => string.IsNullOrEmpty(x) == false).Distinct().ToList();

        foreach (var url in items)
        {
            _ = MonitorUptime(url);
        }
    }

    private async Task MonitorUptime(string url)
    {
        var upTimeSite = new UpTimeSite(this.HttpClient, url);
        var reachable = await upTimeSite.IsReachable();
        UpTimeEntry entry = new();
        entry.Url = url;
        entry.Date = DateTime.Now;
        entry.Message = reachable.Message;
        entry.Status = reachable.Reachable;
        DbHelper.Insert(entry);
    }
}


/// <summary>
/// Site to monitor 
/// </summary>
public class UpTimeSite
{
    /// <summary>
    /// Gets the Url this site is monitoring
    /// </summary>
    public string Url { get; init; }

    private readonly HttpClient HttpClient; 
    
    public UpTimeSite(HttpClient httpClient, string url)
    {
        this.HttpClient = httpClient;
        this.Url = url;
    }
    
    public async Task<(bool Reachable, string Message)> IsReachable()
    {
        try
        {
            var result = await HttpClient.SendAsync(new (HttpMethod.Get, Url));
            if (result.IsSuccessStatusCode)
                return (true, string.Empty);
            return (false, result.ReasonPhrase ?? result.StatusCode.ToString());
        }
        catch(Exception ex) 
        {
            // if(ex.Message === 'ERR_INVALID_PROTOCOL')
            //     return true; // this means its reached, but moaning about http/https, so record it as up
            //return ('' + err).replace(', reason:', '\nreason:');
            
            return (false, ex.Message);
        }
    }

    public void RecordUpTime()
    {
        
    }
}