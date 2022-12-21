using Fenrus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fenrus.Shared;

/// <summary>
/// The Main Menu 
/// </summary>
public partial class MainMenu 
{
    [Inject] private NavigationManager Router { get; set; }
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    private List<MenuGroup> Menu = new();

    protected override async Task OnInitializedAsync()
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

        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var isAdmin = authState.User.FindFirst(x=> x.Type == "Role");
        if (isAdmin != null)
        {
            Menu.Add(new MenuGroup()
            {
                Name = "Administrator", 
                Items = new List<MenuItem>()
                {
                    new () { Name = "Guest Dashboard", Link = "/settings/guest-dashboard", Icon = "fa-solid fa-table-cells-large"},
                    new () { Name = "System Groups", Link = "/settings/groups?isSystem=true", Icon = "fa-solid fa-puzzle-piece"},
                    new () { Name = "System Search Engines", Link = "/settings/search-engines?isSystem=true", Icon = "fa-solid fa-magnifying-glass"},
                    new () { Name = "Users", Link = "/settings/users", Icon = "fa-solid fa-user-group"}
                }
            });
        }
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