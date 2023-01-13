using System.Timers;
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
    
    public async Task<bool> IsReachable()
    {
        try
        {
            var result = await HttpClient.SendAsync(new (HttpMethod.Get, Url));
            return result.IsSuccessStatusCode;
        }
        catch(Exception) 
        {
            // if(ex.Message === 'ERR_INVALID_PROTOCOL')
            //     return true; // this means its reached, but moaning about http/https, so record it as up
            //return ('' + err).replace(', reason:', '\nreason:');
            return false;
        }
    }

    public void RecordUpTime()
    {
        
    }
}