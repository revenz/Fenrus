using System.Text.Json.Serialization;

namespace Fenrus;

/// <summary>
/// Items size as they appear on a dashboard
/// </summary>
public enum ItemSize
{
    /// <summary>
    /// Small
    /// </summary>
    Small,
    /// <summary>
    /// Medium
    /// </summary>
    Medium,
    /// <summary>
    /// Large
    /// </summary>
    Large,
    /// <summary>
    /// Larger
    /// </summary>
    Larger,
    /// <summary>
    /// Extra Large
    /// </summary>
    XLarge,
    /// <summary>
    /// Extra Extra Large
    /// </summary>
    XXLarge
}

/// <summary>
/// An app property type, used to allow apps to request user input for custom properties
/// </summary>
public enum AppPropertyType
{
    /// <summary>
    /// String input
    /// </summary>
    String,
    /// <summary>
    /// Boolean input
    /// </summary>
    Bool,
    /// <summary>
    /// Password input
    /// </summary>
    Password,
    /// <summary>
    /// Select input
    /// </summary>
    Select
}