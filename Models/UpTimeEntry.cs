namespace Fenrus.Models;

/// <summary>
/// Model for an up time recording
/// </summary>
public class UpTimeEntry : IModal
{
    /// <summary>
    /// Gets or sets the Uid
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; }

    /// <summary>
    /// Gets or sets the URL for the uptime
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the URL for the uptime
    /// </summary>
    [LiteDB.BsonIgnore]
    public string Url
    {
        get =>Name;
        set => Name = value;
    }
    
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