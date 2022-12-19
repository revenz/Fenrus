using Fenrus.Models;
using Microsoft.AspNetCore.Components;
#pragma warning disable CS8618

namespace Fenrus.Components;

/// <summary>
/// Search component used on dashboard
/// </summary>
public partial class SearchComponent
{
    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter]
    public UserSettings Settings { get; set; }
}