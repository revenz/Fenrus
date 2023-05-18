namespace Fenrus.Models;

/// <summary>
/// Model passed to the dashboard page
/// </summary>
public class DashboardPageModel
{
    /// <summary>
    /// Gets the current user settings
    /// </summary>
    public UserSettings Settings { get; init; }
    
    /// <summary>
    /// Gets the dashboard to render
    /// </summary>
    public Dashboard Dashboard { get; init; }
    
    /// <summary>
    /// Gets a list of dashboards for the user
    /// </summary>
    public List<Dashboard> Dashboards { get; init; }
    
    /// <summary>
    /// Gets a list of search engines for the user
    /// </summary>
    public List<SearchEngine> SearchEngines { get; init; }
    
    /// <summary>
    /// Gets the theme
    /// </summary>
    public Theme Theme { get; init; }
    
    /// <summary>
    /// Gets or sets the groups on this dashboard
    /// </summary>
    public List<Group> Groups { get; set; }
    
    /// <summary>
    /// Gets or sets the translator to use
    /// </summary>
    public Translator Translator { get; init; }
    
    /// <summary>
    /// Gets the up-time states of URLs
    /// </summary>
    public Dictionary<string, int> UpTimeStates { get; init; }
    
    /// <summary>
    /// Gets the the system settings
    /// </summary>
    public SystemSettings SystemSettings { get; init; }
    
    /// <summary>
    /// Gets the the users profile
    /// </summary>
    public UserProfile UserProfile { get; init; }
}