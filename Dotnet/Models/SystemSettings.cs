namespace Fenrus.Models;

/// <summary>
/// Global system settings
/// </summary>
public class SystemSettings
{
    /// <summary>
    /// Gets or sets the revision count
    /// </summary>
    public int Revision { get; set; }
    
    /// <summary>
    /// Gets or sets if user registrations are allowed
    /// </summary>
    public bool AllowRegister { get; set; }

    /// <summary>
    /// Gets or sets if guests are allowed
    /// </summary>
    public bool AllowGuest { get; set; }
    
}