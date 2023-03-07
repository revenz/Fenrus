using Blazored.Toast.Services;
using Fenrus.Components;
using Fenrus.Models;
using Fenrus.Services;
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
    protected string lblSave, lblCancel, lblHelp, lblDelete, lblEdit, lblMoveUp, 
        lblMoveDown, lblCopy, lblName, lblAdd, lblEnabled, lblActions, lblDefault;

    /// <summary>
    /// Gets the translater to use for this page
    /// </summary>
    protected Translater Translater => App.Translater;
    
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
    protected Models.SystemSettings SystemSettings { get; private set; }

    /// <summary>
    /// Signs out and redirect to the login
    /// </summary>
    protected void SignOut()
    {
        Router.NavigateTo("/logout", forceLoad: true);
    }

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var uid = authState.GetUserUid();
        if (uid == null)
        {
            SignOut();
            return;
        }

        if (Router.Uri.ToLowerInvariant().Contains("/system/") && App.IsAdmin != true)
        {
            SignOut();
            return;
        }

        Settings = new UserSettingsService().Load(uid.Value);
        if (Settings.Uid != uid.Value)
        {
            // guest dashboard, user doesn't exist
            SignOut();
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
        lblAdd = Translater.Instant("Labels.Add");
        lblEnabled = Translater.Instant("Labels.Enabled");
        lblActions = Translater.Instant("Labels.Actions");
        lblDefault  = Translater.Instant("Labels.Default");
        
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