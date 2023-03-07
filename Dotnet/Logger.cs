namespace Fenrus;

/// <summary>
/// Logger for Fenrus
/// </summary>
public class Logger
{
    static NLog.Logger NLogger;
    
    /// <summary>
    /// Logs an information message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void ILog(string message)
        => Log(LogLevel.Information, message);
    
    /// <summary>
    /// Logs an debug message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void DLog(string message)
        => Log(LogLevel.Debug, message);
    
    /// <summary>
    /// Logs an warning message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void WLog(string message)
        => Log(LogLevel.Warning, message);
    
    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void ELog(string message)
        => Log(LogLevel.Error, message);


    /// <summary>
    /// Writes a log message
    /// </summary>
    /// <param name="level">the level to write</param>
    /// <param name="message">the message to write</param>
    private static void Log(LogLevel level, string message)
    {
        string lvl = level switch
        {
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERR",
            LogLevel.Trace => "TRCE",
            _ => "DBUG"
        };
        string msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + " [" + lvl + "] -> " + message;
        Console.WriteLine(msg);
    }
    
    /// <summary>
    /// Initializes the logger
    /// </summary>
    public static void Initialize()
    {
        // var config = new NLog.Config.LoggingConfiguration();
        //
        // // Targets where to log to: File and Console
        // var logfile = new NLog.Targets.FileTarget("logfile")
        // {
        //     FileName = Path.Combine("Logs", "Fenrus_${shortdate}.log"),
        //     MaxArchiveDays = 5, 
        //     Layout = "${longdate} [${uppercase:${level}}] => ${message}"
        // };
        // var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
        //     
        // // Rules for mapping loggers to targets            
        // config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logconsole);
        // config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logfile);
        //     
        // // Apply config           
        // NLog.LogManager.Configuration = config;
        //
        // NLogger = NLog.LogManager.LogFactory.GetLogger(string.Empty);
    }
}