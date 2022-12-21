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
    /// Gets the theme
    /// </summary>
    public Theme Theme { get; init; }
}