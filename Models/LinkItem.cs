namespace Fenrus.Models;

/// <summary>
/// A link item
/// </summary>
public class LinkItem:GroupItem
{
    /// <summary>
    /// Gets the type of the group item
    /// </summary>
    public override string Type => "DashboardLink";

    /// <summary>
    /// Gets or sets the Url of the link
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets teh target of the link
    /// </summary>
    public string Target { get; set; }
}