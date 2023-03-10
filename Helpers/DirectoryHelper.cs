using System.Runtime.CompilerServices;

namespace Fenrus.Helpers;

/// <summary>
/// Helper for directory locations
/// </summary>
public class DirectoryHelper
{
    /// <summary>
    /// Gets the full path to the root Fenrus directory
    /// </summary>
    /// <returns>the full path to the root Fenrus directory</returns>
    public static string GetBaseDirectory()
    {
#if(DEBUG)
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        dir = dir.Substring(0, dir.IndexOf("bin") - 1);
        return dir;
#else
        return AppDomain.CurrentDomain.BaseDirectory;
#endif
    }
    
    /// <summary>
    /// Gets the data root directory
    /// </summary>
    /// <returns>the root data directory</returns>
    public static string GetDataDirectory()
        => Path.Combine(GetBaseDirectory(), "data");

    /// <summary>
    /// Gets the logs root directory
    /// </summary>
    /// <returns>the root logs directory</returns>
    public static string GetLogsDirectory()
        => Path.Combine(GetDataDirectory(), "logs");
    
    /// <summary>
    /// Gets the apps root directory
    /// </summary>
    /// <returns>the apps root directory</returns>
    public static string GetAppsDirectory()
        => Path.Combine(GetBaseDirectory(), "Apps");
    
    /// <summary>
    /// Gets the smart apps directory
    /// </summary>
    /// <returns>the smart apps directory</returns>
    public static string GetSmartAppsDirectory()
        => Path.Combine(GetAppsDirectory(), "Smart");
    
    /// <summary>
    /// Gets the basic apps directory
    /// </summary>
    /// <returns>the basic apps directory</returns>
    public static string GetBasicAppsDirectory()
        => Path.Combine(GetAppsDirectory(), "Basic");
    
    /// <summary>
    /// Gets the full path to wwwroot
    /// </summary>
    /// <returns>the full path to wwwroot</returns>
    public static string GetWwwRootDirectory()
        => Path.Combine(GetBaseDirectory(), "wwwroot");
    
    
    /// <summary>
    /// Gets the full path to wwwroot/themes
    /// </summary>
    /// <returns>the full path to wwwroot/themes</returns>
    public static string GetThemesDirectory()
        => Path.Combine(GetWwwRootDirectory(), "themes");
}