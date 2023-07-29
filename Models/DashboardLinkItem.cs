namespace Fenrus.Models;

/// <summary>
/// A dashboard link item
/// </summary>
public class DashboardLinkItem:GroupItem
{
    /// <summary>
    /// Gets the type of the group item
    /// </summary>
    public override string Type => "Dashboard";

    /// <summary>
    /// Gets or sets the UID of the dashboard
    /// </summary>
    public Guid DashboardUid { get; set; }
    
}