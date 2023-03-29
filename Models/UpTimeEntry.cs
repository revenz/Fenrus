namespace Fenrus.Models;

/// <summary>
/// Model for a site uptime
/// </summary>
public class SiteUpTime
{
    /// <summary>
    /// Gets or sets the URL for the uptime
    /// </summary>
    [LiteDB.BsonId]
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the data for hte uptime entry
    /// </summary>
    public List<SiteUpTimeEntry> Data { get; set; }
}

/// <summary>
/// Model for an up time recording
/// </summary>
public class SiteUpTimeEntry
{
    
    /// <summary>
    /// Gets or sets the time when the up time was logged at
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Gets or sets if the site was up
    /// </summary>
    public bool Status { get; set; }

    /// <summary>
    /// Gets or sets any message for the up time
    /// </summary>
    public string Message { get; set; }

}