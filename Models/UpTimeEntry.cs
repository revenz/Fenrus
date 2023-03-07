namespace Fenrus.Models;

/// <summary>
/// Model for an up time recording
/// </summary>
public class UpTimeEntry
{
    /// <summary>
    /// Gets or sets the URL for the uptime
    /// </summary>
    public string Url { get; set; }
    
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