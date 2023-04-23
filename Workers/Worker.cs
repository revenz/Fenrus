namespace Fenrus.Workers;

/// <summary>
/// A worker that will run at a set schedule
/// </summary>
public abstract class Worker
{
    /// <summary>
    /// Available schedule types
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// Runs every [Interval] seconds
        /// </summary>
        Second,
        /// <summary>
        /// Runs every [Interval] minutes
        /// </summary>
        Minute,
        /// <summary>
        /// Runs every [Interval] hour
        /// </summary>
        Hourly,
        /// <summary>
        /// Runs daily
        /// </summary>
        Daily,
        /// <summary>
        /// Special case, worker only runs once on startup
        /// </summary>
        Startup
    }

    /// <summary>
    /// Gets or sets the interval for the schedule
    /// </summary>
    protected int Interval { get; set; }
    /// <summary>
    /// Gets or sets the schedule of this worker
    /// </summary>
    protected ScheduleType Schedule { get; set; }

    static readonly List<Worker> Workers = new List<Worker>();

    private System.Timers.Timer timer;

    private bool Executing = false;

    /// <summary>
    /// Creates an instance of a worker
    /// </summary>
    /// <param name="schedule">the type of schedule this worker runs at</param>
    /// <param name="interval">the interval of this worker</param>
    protected Worker(ScheduleType schedule, int interval)
    {
        Initialize(schedule, interval);
    }

    /// <summary>
    /// Initialises the worker
    /// </summary>
    /// <param name="schedule">the schedule</param>
    /// <param name="interval">the interval</param>
    protected virtual void Initialize(ScheduleType schedule, int interval)
    {
        this.Schedule = schedule;
        this.Interval = interval;
    }

    /// <summary>
    /// Start the worker
    /// </summary>
    public virtual void Start()
    {
        Logger.ILog("Starting worker: " + this.GetType().Name);
        if (timer != null)
        {
            if (timer.Enabled)
                return; // already running
            timer.Start();
        }
        else
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += TimerElapsed;
            timer.SynchronizingObject = null;
            timer.Interval = ScheduleNext() * 1_000;
            timer.AutoReset = false;
            timer.Start();
        }
    }


    /// <summary>
    /// Stop the worker
    /// </summary>
    public virtual void Stop()
    {
        Logger.ILog("Stopping worker: " + this.GetType().Name);
        if (timer == null)
            return;
        timer.Stop();
        timer.Dispose();
        timer = null;
    }

    /// <summary>
    /// Timer elapsed
    /// </summary>
    /// <param name="sender">the sender</param>
    /// <param name="e">the event</param>
    private void TimerElapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            Trigger();
        }
        catch (Exception)
        {
        }
        finally
        {
            if (Schedule != ScheduleType.Startup)
            {
                timer.Interval = ScheduleNext() * 1_000;
                timer.AutoReset = false;
                timer.Start();
            }
        }
    }

    /// <summary>
    /// Trigger the worker
    /// </summary>
    public void Trigger()
    {
        try
        {
            if (Executing)
                return; // dont let run twice
            Logger.ILog("Triggering worker: " + this.GetType().Name);

            _ = Task.Run(() =>
            {
                Executing = true;
                try
                {
                    Execute();
                }
                catch (Exception ex)
                {
                    Logger.ELog(
                        $"Error in worker '{this.GetType().Name}': {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
                finally
                {
                    Executing = false;
                }
            });
        }
        catch (Exception ex)
        {
            Logger.WLog($"Error triggering worker '{this.GetType().Name}': " + ex.Message);
        }
    }

    /// <summary>
    /// Execute the worker
    /// </summary>
    protected virtual void Execute()
    {
    }

    /// <summary>
    /// Get seconds until next interval
    /// </summary>
    /// <returns>seconds until next interval</returns>
    private int ScheduleNext()
    {
        switch (this.Schedule)
        {
            case ScheduleType.Daily: return ScheduleDaily();
            case ScheduleType.Hourly: return ScheduleHourly();
            case ScheduleType.Minute: return ScheduleMinute();
            case ScheduleType.Startup: return 2; // 2 seconds
        }

        // seconds
        return Interval;
    }

    /// <summary>
    /// Gets how many how many seconds until specified hour
    /// </summary>
    /// <returns>how many seconds until specified hour</returns>
    private int ScheduleDaily()
    {
        DateTime now = DateTime.Now;
        DateTime next = DateTime.Today.AddHours(this.Interval);
        if (next < now)
            next = next.AddDays(1);
        return SecondsUntilNext(next);
    }

    /// <summary>
    /// Gets how many how many seconds until specified hour
    /// </summary>
    /// <returns>how many seconds until specified hour</returns>
    private int ScheduleHourly()
    {
        DateTime now = DateTime.Now.AddMinutes(3); // some padding
        DateTime next = DateTime.Today;
        while (next < now)
            next = next.AddHours(this.Interval);
        return SecondsUntilNext(next);
    }
    /// <summary>
    /// Gets how many how many seconds until specified minute
    /// </summary>
    /// <returns>how many seconds until specified minute</returns>
    private int ScheduleMinute()
    {
        DateTime now = DateTime.Now.AddSeconds(30); // some padding
        DateTime next = DateTime.Today;
        while (next < now)
            next = next.AddMinutes(this.Interval);
        return SecondsUntilNext(next);
    }
    private int SecondsUntilNext(DateTime next) => (int)Math.Ceiling((next - DateTime.Now).TotalSeconds);
}
