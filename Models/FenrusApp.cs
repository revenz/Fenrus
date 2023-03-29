using System.Text.Json.Serialization;
using Fenrus.Models.UiModels;

namespace Fenrus.Models;

/// <summary>
/// Application
/// </summary>
public class FenrusApp
{
    /// <summary>
    /// Gets or sets the apps name
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the Icon for the app
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets any CSS for the application
    /// </summary>
    public string Css { get; set; }

    /// <summary>
    /// Gets or sets the apps refresh interval in seconds
    /// 0 for never refresh
    /// -1 for never fetch
    /// </summary>
    public int Interval { get; set; }

    /// <summary>
    /// Gets or sets if there is a test function
    /// </summary>
    public bool TestFunction { get; set; }
    
    /// <summary>
    /// Gets or sets the app info
    /// </summary>
    public FenrusAppInfo Info { get; set; }
    
    /// <summary>
    /// Gets or sets if this app uses a carousel
    /// </summary>
    public bool Carousel { get; set; }
    
    /// <summary>
    /// Gets or sets the default size for this app
    /// </summary>
    public ItemSize? DefaultSize { get; set; }
    
    /// <summary>
    /// Gets or sets the default URL for the application
    /// This is used to prefill the group item editor when selecting this app
    /// </summary>
    public string DefaultUrl { get; set; }

    /// <summary>
    /// Gets or sets if this is a smart app
    /// </summary>
    [JsonIgnore]
    public bool IsSmart { get; set; }

    /// <summary>
    /// Gets the full path where the app is located
    /// </summary>
    [JsonIgnore]
    public string FullPath { get; set; }

    private List<FenrusAppProperty> _Properties = new();

    /// <summary>
    /// Gets or sets the properties of the app
    /// </summary>
    public List<FenrusAppProperty> Properties
    {
        get => _Properties;
        set => _Properties = value ?? new(); // we dont like nulls
    }
}

/// <summary>
/// App info
/// </summary>
public class FenrusAppInfo
{
    /// <summary>
    /// Gets or sets the app authors
    /// </summary>
    public string[] Authors { get; set; }
    
    /// <summary>
    /// Gets or sets the app URL
    /// </summary>
    public string AppUrl { get; set; }
}

/// <summary>
/// App property
/// </summary>
public class FenrusAppProperty
{
    /// <summary>
    /// Gets or sets the human readable name for this property
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Gets or sets the ID to save this to 
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the type of property
    /// </summary>
    public AppPropertyType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the help to shown with this input property
    /// </summary>
    public string Help { get; set; }

    /// <summary>
    /// Gets or sets the default value
    /// </summary>
    [JsonPropertyName("default")]
    public object? DefaultValue { get; set; }
    
    /// <summary>
    /// Gets or sets list options for a Select input
    /// </summary>
    public List<ListOption> Options { get; set; }
}