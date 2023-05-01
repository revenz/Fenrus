using Fenrus.Models;
using Fenrus.Services.CalendarServices;

namespace Fenrus.Workers;

/// <summary>
/// Worker for calendar events 
/// </summary>
public class CalendarEventWorker:Worker
{
    private Dictionary<Guid, DateTime> LastReminded = new();

    /// <summary>
    /// Gets the singleton of the calendar event worker
    /// </summary>
    public static CalendarEventWorker Instance { get; private set; }

    /// <summary>
    /// Constructs a calendar event worker
    /// </summary>
    public CalendarEventWorker() : base(ScheduleType.Second, 10)
    {
        Instance = this;
    }

    private static int ActiveClients;

    public static void RegisterClient()
    {
        ++ActiveClients;
        Instance.Schedule = ScheduleType.Minute;
        Instance.Interval = 5;
        Instance.Stop();
        Instance.Start();
        Instance.Trigger();
    }

    public static void UnregisterClient()
    {
        if (--ActiveClients < 0)
            ActiveClients = 0;
        
        Instance.Schedule = ScheduleType.Hourly;
        Instance.Interval = 1;
        Instance.Stop();
        Instance.Start();
    }

    protected override void Execute()
    {
        var service = new CalendarFeedService();
        var feeds = service.GetAll();
        foreach (var feed in feeds)
        {
            if (feed.Enabled == false)
                continue;
            if (feed.Type == CalendarFeedType.iCal)
            {
                if (string.IsNullOrEmpty(feed.Url) == false)
                {
                    foreach (var ev in new iCalService(feed.Url, feed.CacheMinutes).GetEvents(DateTime.UtcNow,
                                 DateTime.UtcNow.AddDays(7)))
                    {
                        NotifyEvent(feed.UserUid, ev);
                    }
                }
            }
        }

        var events = new CalendarService().GetAll();
        foreach (var ev in events)
        {
            NotifyEvent(ev.UserUid, CalendarEventModel.From(ev));
        }
    }

    private void NotifyEvent(Guid userUid, CalendarEventModel ev)
    {
        if (ev.StartDate > DateTime.UtcNow.AddDays(1))
            return;
        if (ev.StartDate < DateTime.UtcNow)
            return;
     
        string suffix = string.Empty;   
        if (ev.AllDay == false)
        {
            var minutes = DateTime.UtcNow.Subtract(ev.StartDate).TotalMinutes;
            if (minutes <= 5)
                suffix = "_5m";
            else if(minutes <= 10)
                suffix = "_10m";
            else if(minutes <= 15)
                suffix = "_15m";
            else if (minutes <= 60)
                suffix = "60m";
            else
                return;
        }
        else
        {
            if (ev.StartDate > DateTime.Now.AddDays(1))
                return;
            
            suffix = "_allday";
        }

        if (ev.AllDay)
            NotificationHelper.Send(userUid, NotificationType.Info, ev.Title, string.Empty, identifier: ev.Uid + suffix);
        else
            NotificationHelper.Send(userUid, NotificationType.Info, ev.Start, ev.Title, identifier: ev.Uid + suffix);
    }
}