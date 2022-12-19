using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for Apps
/// </summary>
public class AppService
{
    private static readonly Dictionary<string, FenrusApp> _Apps = new ();

    /// <summary>
    /// Initializes the apps in the system
    /// </summary>
    public static void Initialize()
    {
        LoadApps(DirectoryHelper.GetSmartAppsDirectory(), true);
        LoadApps(DirectoryHelper.GetBasicAppsDirectory(), false);
    }

    /// <summary>
    /// Load apps from a directory
    /// </summary>
    /// <param name="dir">the directory to load apps from</param>
    /// <param name="smartApps">if these apps are smart or not</param>
    private static void LoadApps(string dir, bool smartApps)
    {
        foreach (var file in new DirectoryInfo(dir).GetFiles("app.json", SearchOption.AllDirectories))
        {
            try
            {
                var json = File.ReadAllText(file.FullName);
                var app = JsonSerializer.Deserialize<FenrusApp>(json);
                if (string.IsNullOrWhiteSpace(app.Name))
                    continue;
                if (_Apps.ContainsKey(app.Name))
                    continue;
                app.IsSmart = smartApps;
                app.FullPath = file.Directory?.FullName ?? string.Empty;
                _Apps.Add(app.Name, app);
            }
            catch (Exception ex)
            {
                Logger.ELog($"Failed parsing app '{file.FullName}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Gets an app by its name
    /// For an unknown app, a basic app instance will be returned
    /// </summary>
    /// <param name="appName">the app name</param>
    /// <returns>a app instance</returns>
    public static FenrusApp GetByName(string appName)
    {
        if (_Apps.ContainsKey(appName))
            return _Apps[appName];
        // we dont like nulls
        return new FenrusApp()
        {
            Name = appName
        };
    }
}