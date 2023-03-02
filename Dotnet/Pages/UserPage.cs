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
    protected string lblSave, lblCancel, lblHelp, lblDelete, lblEdit, lblMoveUp, lblMoveDown, lblCopy, lblName;
    
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

        lblCancel = Translater.Instant("Labels.Cancel");
        lblSave = Translater.Instant("Labels.Save");
        lblHelp = Translater.Instant("Labels.Help");
        lblDelete = Translater.Instant("Labels.Delete");
        lblEdit = Translater.Instant("Labels.Edit");
        lblMoveDown = Translater.Instant("Labels.MoveDown");
        lblMoveUp = Translater.Instant("Labels.MoveUp");
        lblCopy = Translater.Instant("Labels.Copy");
        lblName = Translater.Instant("Labels.Name");

        SystemSettings = new Services.SystemSettingsService().Get();

        var accent = Settings.Dashboards?.FirstOrDefault(x => x.Enabled)?.AccentColor ?? "#FF0090";
        App.UpdateAccentColor(accent);

        await PostGotUser();
    }

    /// <summary>
    /// Called after the user has been loaded
    /// </summary>
    protected virtual Task PostGotUser()
        => Task.CompletedTask;
}