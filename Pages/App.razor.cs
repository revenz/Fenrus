using System.Text.RegularExpressions;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fenrus.Pages;

/// <summary>
/// The main blazor App
/// </summary>
public partial class App
{
    /// <summary>
    /// Gets the popup component
    /// </summary>
    public Components.FenrusPopup Popup { get; private set; }
    
    public EventCallback AccentColorUpdated { get; set; }
    /// <summary>
    /// Gets or sets the navigation manager used for routing
    /// </summary>
    [Inject] private NavigationManager Router { get; set; }
    /// <summary>
    /// Gets or sets the authentication state provider, used to get the user
    /// </summary>
    [Inject] private AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    
    /// <summary>
    /// Gets if the current user is an admin
    /// </summary>
    public bool IsAdmin { get; private set; }
    
    /// <summary>
    /// Gets the accent color
    /// </summary>
    public string AccentColor { get; private set; }

    /// <summary>
    /// Gets the Translater used for this app
    /// </summary>
    public Translater Translater { get; private set; } = new Translater();
    
    private string AccentRgb;
    /// <summary>
    /// Updates the accent color
    /// </summary>
    /// <param name="accentColor">the accent color</param>
    public void UpdateAccentColor(string accentColor)
    {
        if (Regex.IsMatch(accentColor, "^#[a-fA-F0-9]{6}") == false)
            return;
        
        AccentColor = accentColor;
        AccentRgb = int.Parse(AccentColor[1..3], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[3..5], System.Globalization.NumberStyles.HexNumber) + "," +
                    int.Parse(AccentColor[5..7], System.Globalization.NumberStyles.HexNumber);
        StateHasChanged();
        _ = AccentColorUpdated.InvokeAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var uid = authState.GetUserUid();
        if (uid == null)
        {
            Router.NavigateTo("/login");
            return;
        }

        var user = new UserService().GetByUid(uid.Value);
        if (user == null)
        {
            Router.NavigateTo("/login", forceLoad: true);
            return;
        }

        IsAdmin = user.IsAdmin;
        
        Console.WriteLine("AppRazor IsAdmin: " + user.Name + " = " + user.IsAdmin);
        var settings = new UserSettingsService().Load(uid.Value);
        
        string language = settings.Language?.EmptyAsNull() ??
                          new SystemSettingsService().Get()?.Language?.EmptyAsNull() ?? "en";
        Translater = Translater.GetForLanguage(language);
    }
}