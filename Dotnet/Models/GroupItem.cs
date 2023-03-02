using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Fenrus.Models;

/// <summary>
/// A group item
/// </summary>
public abstract class GroupItem: IModal
{
    /// <summary>
    /// Gets the Type of the item
    /// </summary>
    [JsonPropertyName("_Type")]
    public abstract string Type { get; }

    /// <summary>
    /// Gets or sets the Uid of the item
    /// </summary>
    [LiteDB.BsonId]
    public Guid Uid { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// Gets or sets the Icon of the item
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Gets or sets the name of the item
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the item size
    /// </summary>
    public ItemSize Size { get; set; }
}