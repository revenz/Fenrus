namespace Fenrus.Models;

/// <summary>
/// Search Engine
/// </summary>
public class SearchEngine
{
    /// <summary>
    /// Gets or sets the Uid
    /// </summary>
    public Guid Uid { get; set; }

    /// <summary>
    /// Gets or sets the name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the shortcut that can be used to switch to this search engine
    /// </summary>
    public string Shortcut { get; set; }

    /// <summary>
    /// Gets or sets the icon 
    /// </summary>
    public string Icon { get; set;  }
    
    /// <summary>
    /// Gets or sets the Url of the search engine
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Gets or sets if this search engine is enabled
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets if this is the default search engine
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Gets or sets if this is a system search engine
    /// </summary>
    public bool IsSystem { get; set; }
}