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
    /// Gets the translator to use for this page
    /// </summary>
    protected Translator Translator => App.Translator;
    
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
    /// Gets the user 
    /// </summary>
    protected User User { get; private set; }
    
    /// <summary>
    /// Gets the user settings
    /// </summary>
    protected UserSettings Settings { get; private set; }
    
    /// <summary>
    /// Gets the Users UID
    /// </summary>
    protected Guid UserUid { get; private set; }
    
    /// <summary>
    /// Gets the system settings
    /// </summary>
    protected Models.SystemSettings SystemSettings { get; private set; }

    /// <summary>
    /// Gets if the current user is an admin
    /// </summary>
    protected bool IsAdmin => User?.IsAdmin == true;

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

        this.UserUid = uid.Value;
        this.User = new UserService().GetByUid(uid.Value);
        if (this.User == null)
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

        lblCancel = Translator.Instant("Labels.Cancel");
        lblSave = Translator.Instant("Labels.Save");
        lblHelp = Translator.Instant("Labels.Help");
        lblDelete = Translator.Instant("Labels.Delete");
        lblEdit = Translator.Instant("Labels.Edit");
        lblMoveDown = Translator.Instant("Labels.MoveDown");
        lblMoveUp = Translator.Instant("Labels.MoveUp");
        lblCopy = Translator.Instant("Labels.Copy");
        lblName = Translator.Instant("Labels.Name");
        lblAdd = Translator.Instant("Labels.Add");
        lblEnabled = Translator.Instant("Labels.Enabled");
        lblActions = Translator.Instant("Labels.Actions");
        lblDefault = Translator.Instant("Labels.Default");
        
        SystemSettings = new Services.SystemSettingsService().Get();

        var accent = new DashboardService().GetAccentColorForUser(UserUid);
        App.UpdateAccentColor(accent);

        await PostGotUser();
    }

    /// <summary>
    /// Called after the user has been loaded
    /// </summary>
    protected virtual Task PostGotUser()
        => Task.CompletedTask;
}