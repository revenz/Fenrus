using Microsoft.AspNetCore.Components;

namespace Fenrus.Shared;

/// <summary>
/// The Main Menu 
/// </summary>
public partial class MainMenu 
{
    /// <summary>
    /// Gets or sets if this user is an administrator
    /// </summary>
    public bool IsAdministrator { get; set; }

    [Inject] private NavigationManager Router { get; set; }

    private List<MenuGroup> Menu = new();

    protected override void OnInitialized()
    {
        Router.LocationChanged += (obj, e) => this.StateHasChanged();
        Menu.Add(new MenuGroup()
        {
            Name = "General", 
            Items = new List<MenuItem>()
            {
                new () { Name = "Home", Link = "/", Icon = "fa-solid fa-house"},
                new () { Name = "General", Link = "/settings", Icon = "fa-solid fa-sliders"},
            }
        });
        Menu.Add(new MenuGroup()
        {
            Name = "Dashboard", 
            Items = new List<MenuItem>()
            {
                new () { Name = "Dashboards", Link = "/settings/dashboards", Icon = "fa-solid fa-table-cells-large"},
                new () { Name = "Groups", Link = "/settings/groups", Icon = "fa-solid fa-puzzle-piece"},
                new () { Name = "Search Engines", Link = "/settings/search-engines", Icon = "fa-solid fa-magnifying-glass"}
            }
        });
    }

    private bool IsActive(MenuItem item)
    {
        string url = Router.Uri;
        url = url.Substring(Router.BaseUri.Length - 1);
        return item.Link == url;
    }
}

/// <summary>
/// Menu Group
/// </summary>
class MenuGroup
{
    /// <summary>
    /// Gets the name of of the menu group
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Gets the menu items
    /// </summary>
    public List<MenuItem> Items { get; init; } = new List<MenuItem>();
}

/// <summary>
/// A menu item
/// </summary>
public class MenuItem
{
    /// <summary>
    /// Gets the name
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// Gets the link
    /// </summary>
    public string Link { get; init; }
    /// <summary>
    /// Gets the icon
    /// </summary>
    public string Icon { get; init; }
}