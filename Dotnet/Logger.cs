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
        => NLogger.Info(message);
    
    /// <summary>
    /// Logs an debug message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void DLog(string message)
        => NLogger.Debug(message);
    
    /// <summary>
    /// Logs an warning message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void WLog(string message)
        => NLogger.Warn(message);
    
    /// <summary>
    /// Logs an error message
    /// </summary>
    /// <param name="message">the message to log</param>
    public static void ELog(string message)
        => NLogger.Error(message);
    
    /// <summary>
    /// Initializes the logger
    /// </summary>
    public static void Initialize()
    {
        var config = new NLog.Config.LoggingConfiguration();

        // Targets where to log to: File and Console
        var logfile = new NLog.Targets.FileTarget("logfile")
        {
            FileName = Path.Combine("Logs", "Fenrus_${shortdate}.log"),
            MaxArchiveDays = 5, 
            Layout = "${longdate} [${uppercase:${level}}] => ${message}"
        };
        var logconsole = new NLog.Targets.ConsoleTarget("logconsole");
            
        // Rules for mapping loggers to targets            
        config.AddRule(NLog.LogLevel.Info, NLog.LogLevel.Fatal, logconsole);
        config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, logfile);
            
        // Apply config           
        NLog.LogManager.Configuration = config;

        NLogger = NLog.LogManager.LogFactory.GetLogger(string.Empty);
    }
}