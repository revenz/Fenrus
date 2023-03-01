using Blazored.Toast.Services;
using Fenrus.Components;
using Fenrus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fenrus.Pages;

/// <summary>
/// Page for an authorized user
/// </summary>
[Authorize]
public abstract class UserPage : ComponentBase
{
    /// <summary>
    /// Gets or sets the Authentication state provider
    /// </summary>
    [Inject] protected AuthenticationStateProvider AuthStateProvider { get; set; }
    
    /// <summary>
    /// Gets or sets the navigation manager (router)
    /// </summary>
    [Inject] protected NavigationManager Router { get; set; }

    
    /// <summary>
    /// Gets or sets the app instance
    /// </summary>
    [CascadingParameter] protected App App { get; set; }

    /// <summary>
    /// Gets the popup handler
    /// </summary>
    protected FenrusPopup Popup => App.Popup;

    /// <summary>
    /// Gets or sets the toast service
    /// </summary>
    [Inject] protected IToastService ToastService { get; set; }
    
    /// <summary>
    /// Gets the user settings
    /// </summary>
    protected UserSettings Settings { get; private set; }
    
    /// <summary>
    /// Gets the system settings
    /// </summary>
    protected SystemSettings SystemSettings { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var sid = authState?.User?.Claims?.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
        if (string.IsNullOrEmpty(sid) || Guid.TryParse(sid, out Guid uid) == false)
        {
            Router.NavigateTo("/login");
            return;
        }

        Settings = new Services.UserSettingsService().Load(uid);
        if (Settings.Uid != uid)
        {
            // guest dashboard, user doesn't exist
            Router.NavigateTo("/login");
            return;
        }

        SystemSettings = new Services.SystemSettingsService().Get();
        
        // App.UpdateAccentColor(Settings.AccentColor);

        await PostGotUser();
    }

    /// <summary>
    /// Called after the user has been loaded
    /// </summary>
    protected virtual Task PostGotUser()
        => Task.CompletedTask;
}