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
        Timer.Interval = GetNextInterval();
        Timer.Enabled = true;
    }

    /// <summary>
    /// Gets the next interval when the monitor should run
    /// </summary>
    /// <returns>the number of milliseconds until the monitor should run next</returns>
    private int GetNextInterval()
    {
        var dt = DateTime.Now;
        var d = new TimeSpan(0, 10, 0);
        var modTicks = dt.Ticks % d.Ticks;
        var delta = modTicks != 0 ? d.Ticks - modTicks : 0;
        var target = new DateTime(dt.Ticks + delta, dt.Kind);
        var diff = target - dt;
        if (diff.TotalSeconds < 5)
        {
            target = target.AddMinutes(10);
            diff = target - dt;
        }
        return (int)diff.TotalMilliseconds;
    }

    /// <summary>
    /// Gets the up time data for a URL
    /// </summary>
    /// <param name="url">the URL </param>
    /// <returns>any data for the URL</returns>
    public List<UpTimeEntry> GetData(string url)
    {
        using var db = DbHelper.GetDb();
        var collection = db.GetCollection<UpTimeEntry>(nameof(UpTimeEntry));
        return collection.Query().Where(x => x.Name == url)
            .OrderByDescending(x => x.Date).Limit(100).ToList();
    }

    private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
    {
        try
        {
            Monitor();
        }
        catch (Exception)
        {

        }
        finally
        {
            Thread.Sleep(10_000);
            Timer.Interval = GetNextInterval();
            Timer.Start();
        }
    }

    public void Monitor()
    {
        var userGroups = new GroupService().GetAll() ?? new();
        var systemGroups = new GroupService().GetSystemGroups() ?? new();
        var items = userGroups.Union(systemGroups).Where(x => x.Enabled)
            .SelectMany(x => x.Items ?? new List<GroupItem>())
            .Where(x => x.Monitor)
            .Select(x =>
            {
                string? url = null;
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

    private bool AlreadyExists(string url, DateTime date)
    {
        using var db = DbHelper.GetDb();
        var collection = db.GetCollection<UpTimeEntry>(nameof(UpTimeEntry));
        return collection.Exists(x => x.Name == url && x.Date == date);
    }

    private async Task MonitorUptime(string url)
    {
        UpTimeEntry entry = new();
        entry.Name = url;
        entry.Date = RoundDown(DateTime.Now, new TimeSpan(0, 10, 0));
        if (AlreadyExists(url, entry.Date))
            return; // already monitored for this period

        var upTimeSite = new UpTimeSite(this.HttpClient, url);
        var reachable = await upTimeSite.IsReachable();
        entry.Uid = Guid.NewGuid();
        entry.Message = reachable.Message;
        entry.Status = reachable.Reachable;

        DbHelper.Insert(entry);
    }
    
    public static DateTime RoundDown(DateTime dt, TimeSpan d)
    {
        var delta = dt.Ticks % d.Ticks;
        return new DateTime(dt.Ticks - delta, dt.Kind);
    }

    /// <summary>
    /// Gets a list of all URLs and their up states
    /// </summary>
    /// <returns>a list of URLs and their up states</returns>
    public Dictionary<string, int> GetUpStates()
    {
        using var db = DbHelper.GetDb();
        var collection = db.GetCollection<UpTimeEntry>(nameof(UpTimeEntry));
        var all = collection.Query().ToList();
        return all
            .GroupBy(x => x.Name)
            .Select(x => x.MaxBy(x => x.Date))
            .ToDictionary(x => x.Name, x =>
            {
                if (x.Date < DateTime.Now.AddHours(-1))
                    return 2; // unknown, too old
                return x.Status ? 1 : 0;
            });
        
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