using System.Text.RegularExpressions;
using Fenrus.Components;
using Fenrus.Components.Dialogs;
using Fenrus.Components.SideEditors;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Fenrus.Services.CalendarServices;
using Fenrus.Services.FileStorages;

namespace Fenrus.Pages;

/// <summary>
/// Profile page
/// </summary>
public partial class Profile: UserPage
{
    private string lblTitle, lblPageDescription, lblGeneral, lblChangePassword, lblCalendar, lblFileStorage, lblEmail, lblApps, 
        CalendarUrl, CalendarProvider, CalendarUsername, CalendarPassword, CalendarName, lblTest,
        FileStorageUrl, FileStorageProvider, FileStorageUsername, FileStoragePassword,
        EmailImapServer, EmailImapUsername, EmailImapPassword,
        EmailSmtpServer, EmailSmtpUsername, EmailSmtpPassword,
        ErrorGeneral, ErrorChangePassword, ErrorCalendar, ErrorFileStorage,
        lblCalendarPageDescription,lblFileStoragePageDescription, lblEmailPageDescription, lblAppsPageDescription,
        lblNewGroup;

    private bool FilesEnabled, EmailEnabled, CalendarEnabled, NotesEnabled, AppsEnabled;

    private int EmailImapPort, EmailSmtpPort;

    private EditorForm GeneralEditor, PasswordEditor, CalendarEditor, FileStorageEditor, EmailEditor, AppsEditor;

    private List<ListOption> CalendarProviders = new()
    {
        new() { Label = "NextCloud", Value = "NextCloud" }
    };
    
    private List<ListOption> FileStorageProviders = new()
    {
        new() { Label = "NextCloud", Value = "NextCloud" }
    };
    
    private User Model { get; set; }

    private string Username, Email, FullName, PasswordCurrent, PasswordNew, PasswordConfirm;

    private Models.SystemSettings _systemSettings;
    private bool AppsDirty = false;

    /// <summary>
    /// Gets if the cloud is enabled globally
    /// </summary>
    private bool CloudEnabled => _systemSettings.CloudFeatures != CloudFeature.None;
    /// <summary>
    /// Gets if files are enabled globally
    /// </summary>
    private bool CloudFilesGlobalEnabled =>  (_systemSettings.CloudFeatures & CloudFeature.Files) == CloudFeature.Files;
    /// <summary>
    /// Gets if emails is enabled globally
    /// </summary>
    private bool CloudCalendarGlobalEnabled =>  (_systemSettings.CloudFeatures & CloudFeature.Calendar) == CloudFeature.Calendar;
    /// <summary>
    /// Gets if emails is enabled globally
    /// </summary>
    private bool CloudEmailGlobalEnabled =>  (_systemSettings.CloudFeatures & CloudFeature.Email) == CloudFeature.Email;
    /// <summary>
    /// Gets if notes are enabled globally
    /// </summary>
    private bool CloudNotesGlobalEnabled =>  (_systemSettings.CloudFeatures & CloudFeature.Notes) == CloudFeature.Notes;
    /// <summary>
    /// Gets if apps are enabled globally
    /// </summary>
    private bool CloudAppsGlobalEnabled =>  (_systemSettings.CloudFeatures & CloudFeature.Apps) == CloudFeature.Apps;

    /// <summary>
    /// Initialized the component after the User is loaded
    /// </summary>
    /// <returns>an awaited task</returns>
    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Profile.Title");
        lblTest = Translator.Instant("Labels.Test");
        lblPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription");
        lblGeneral = Translator.Instant("Pages.Profile.Labels.General");
        lblCalendar = Translator.Instant("Pages.Profile.Labels.Calendar");
        lblFileStorage = Translator.Instant("Pages.Profile.Labels.FileStorage");
        lblEmail = Translator.Instant("Pages.Profile.Labels.Email");
        lblApps = Translator.Instant("Pages.Profile.Labels.Apps");
        lblChangePassword = Translator.Instant("Pages.Profile.Labels.ChangePassword");
        lblCalendarPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-Calendar");
        lblFileStoragePageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-FileStorage");
        lblEmailPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-Email");
        lblAppsPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-Apps");
        lblNewGroup = Translator.Instant("Pages.Profile.Labels.NewGroup");
        
        PasswordCurrent = string.Empty;
        PasswordNew = string.Empty;
        PasswordConfirm = string.Empty;

        _systemSettings = new SystemSettingsService().Get();

        this.Model = new UserService().GetByUid(this.UserUid);
        Username = this.Model.Username ?? string.Empty;
        Email = this.Model.Email ?? string.Empty;
        FullName = this.Model.FullName ?? string.Empty;
        var service = new UserService();
        var profile = service.GetProfileByUid(this.UserUid);
        CalendarUrl = string.Empty;
        CalendarUsername = string.Empty;
        CalendarPassword = string.Empty;
        CalendarName = string.Empty; 
        CalendarProvider = profile.CalendarProvider ?? string.Empty;
        if (string.IsNullOrEmpty(CalendarProvider) == false)
        {
            CalendarUrl = profile.CalendarUrl?.Value ?? string.Empty;
            CalendarName = profile.CalendarName?.Value ?? string.Empty;
            CalendarUsername = profile.CalendarUsername?.Value ?? string.Empty;
            CalendarPassword = string.IsNullOrEmpty(profile.CalendarPassword?.Value)
                ? string.Empty
                : Globals.DUMMY_PASSWORD;
        }

        FilesEnabled = (profile.CloudFeatures & CloudFeature.Files) == CloudFeature.Files && CloudFilesGlobalEnabled;
        EmailEnabled = (profile.CloudFeatures & CloudFeature.Email) == CloudFeature.Email && CloudEmailGlobalEnabled;
        CalendarEnabled = (profile.CloudFeatures & CloudFeature.Calendar) == CloudFeature.Calendar && CloudCalendarGlobalEnabled;
        NotesEnabled = (profile.CloudFeatures & CloudFeature.Notes) == CloudFeature.Notes && CloudNotesGlobalEnabled;
        AppsEnabled = (profile.CloudFeatures & CloudFeature.Apps) == CloudFeature.Apps && CloudAppsGlobalEnabled;

        FileStorageUrl = string.Empty;
        FileStorageUsername = string.Empty;
        FileStoragePassword = string.Empty;
        FileStorageProvider = profile.FileStorageProvider ?? string.Empty;
        if (string.IsNullOrEmpty(FileStorageProvider) == false)
        {
            FileStorageUrl = profile.FileStorageUrl?.Value ?? string.Empty;
            FileStorageUsername = profile.FileStorageUsername?.Value ?? string.Empty;
            FileStoragePassword = string.IsNullOrEmpty(profile.FileStoragePassword?.Value)
                ? string.Empty
                : Globals.DUMMY_PASSWORD;
        }

        EmailImapServer = profile.EmailImapServer ?? string.Empty;
        EmailImapUsername = string.Empty;
        EmailImapPassword = string.Empty;
        EmailImapPort = 993;
        if (string.IsNullOrEmpty(EmailImapServer) == false)
        {
            EmailImapUsername = profile.EmailImapUsername?.Value ?? string.Empty;
            EmailImapPassword = string.IsNullOrEmpty(profile.EmailImapPassword?.Value) ? string.Empty : Globals.DUMMY_PASSWORD;
            EmailImapPort = profile.EmailImapPort < 1 || profile.EmailImapPort > 65535 ? 993 : profile.EmailImapPort;
        }
        EmailSmtpServer = profile.EmailSmtpServer ?? string.Empty;
        EmailSmtpUsername = string.Empty;
        EmailSmtpPassword = string.Empty;
        EmailSmtpPort = 587;
        if (string.IsNullOrEmpty(EmailSmtpServer) == false)
        {
            EmailSmtpUsername = profile.EmailSmtpUsername?.Value ?? string.Empty;
            EmailSmtpPassword = string.IsNullOrEmpty(profile.EmailSmtpPassword?.Value) ? string.Empty : Globals.DUMMY_PASSWORD;
            EmailSmtpPort = profile.EmailSmtpPort < 1 || profile.EmailSmtpPort > 65535 ? 587 : profile.EmailSmtpPort;
        }

        // we want to clone this so we dont mess with the original data
        this.AppGroups = profile.AppGroups?.Select(grp =>
            new CloudAppGroup()
            {
                Uid = grp.Uid,
                Enabled = grp.Enabled,
                Name = grp.Name,
                Items = grp.Items?.Select(x => new CloudApp()
                {
                    Uid = x.Uid,
                    Name = x.Name,
                    Icon = x.Icon,
                    Address = x.Address,
                    Type = x.Type
                })?.ToList() ?? new()
            }
        ).ToList() ?? new();
        CleanAppGroupNames = AppGroups.Select(x => x.Name).ToList();
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Saves the general profile settings
    /// </summary>
    async Task Save()
    {
        ErrorGeneral = string.Empty;
        ErrorChangePassword = string.Empty;
        
        if (await GeneralEditor.Validate() == false)
            return;
                
        if (Regex.IsMatch(this.Email, @"^\S+@\S+\.\S+$") == false)
        {
            ErrorGeneral = Translator.Instant("Pages.Profile.Messages.InvalidEmail");
            return;
        }

        var service = new UserService();
        var other = service.GetByUsername(Username);
        if (other != null && other.Uid != this.UserUid)
        {
            ErrorGeneral = Translator.Instant("Pages.Profile.Messages.OtherUserExists");
            return;
        }

        Model.Email = Email;
        Model.Username = Username;
        Model.FullName = FullName;
        if (CloudNotesGlobalEnabled)
        {
            var profile = service.GetProfileByUid(UserUid);
            if (profile != null)
            {
                profile.CloudFeatures = NotesEnabled
                    ? profile.CloudFeatures | CloudFeature.Notes
                    : profile.CloudFeatures & ~CloudFeature.Notes;
                service.UpdateProfile(profile);
            }

        }
        service.Update(Model);
        GeneralEditor.MarkClean();
        ToastService.ShowSuccess("Labels.Saved");
    }

    /// <summary>
    /// Saves the calendar settings
    /// </summary>
    async Task CalendarSave()
    {
        var service = new UserService();
        var profile = service.GetProfileByUid(this.UserUid);
        profile.CalendarProvider = CalendarProvider;
        string url = CalendarUrl ?? string.Empty;
        if (string.IsNullOrEmpty(profile.CalendarProvider))
            url = string.Empty;
        
        profile.CalendarUrl = (EncryptedString)(url ?? string.Empty);
        if (string.IsNullOrEmpty(url))
        {
            profile.CalendarUsername = (EncryptedString)string.Empty;
            profile.CalendarPassword = (EncryptedString)string.Empty;
            profile.CalendarName = (EncryptedString)string.Empty;
        }
        else
        {
            profile.CalendarName = (EncryptedString)(CalendarName ?? string.Empty);
            profile.CalendarUsername = (EncryptedString)(CalendarUsername ?? string.Empty);
            if(CalendarPassword != Globals.DUMMY_PASSWORD) // check if it changed
                profile.CalendarPassword = (EncryptedString)(CalendarPassword ?? string.Empty);
        }
        profile.CloudFeatures = CalendarEnabled ? profile.CloudFeatures | CloudFeature.Calendar : profile.CloudFeatures & ~CloudFeature.Calendar;

        service.UpdateProfile(profile);
        CalendarEditor.MarkClean();
        ToastService.ShowSuccess("Calendar Saved");
    }
    
    /// <summary>
    /// Tests the calendar
    /// </summary>
    async Task CalendarTest()
    {
        if (string.IsNullOrEmpty(CalendarUrl) || string.IsNullOrEmpty(CalendarProvider))
            return;
        string password = CalendarPassword;
        if (password == Globals.DUMMY_PASSWORD)
        {
            var service = new UserService();
            var profile = service.GetProfileByUid(this.UserUid);
            password = profile.CalendarPassword?.Value ?? string.Empty;
        }
        var result = await CalDavCalendarService.TestCalDav(CalendarProvider,CalendarUrl, CalendarUsername, password, CalendarName);
        if (result.Success)
            ToastService.ShowSuccess("Successfully connected to CalDAV");
        else
            ToastService.ShowError(result.Error);
    }
    
    /// <summary>
    /// Saves the file storage settings
    /// </summary>
    async Task FileStorageSave()
    {
        var service = new UserService();
        var profile = service.GetProfileByUid(this.UserUid);
        profile.FileStorageProvider = FileStorageProvider;
        string url = FileStorageUrl ?? string.Empty;
        if (string.IsNullOrEmpty(profile.FileStorageProvider))
            url = string.Empty;
        
        profile.FileStorageUrl = (EncryptedString)(url ?? string.Empty);
        if (string.IsNullOrEmpty(url))
        {
            profile.FileStorageUsername = (EncryptedString)string.Empty;
            profile.FileStoragePassword = (EncryptedString)string.Empty;
        }
        else
        {
            profile.FileStorageUsername = (EncryptedString)(FileStorageUsername ?? string.Empty);
            if(FileStoragePassword != Globals.DUMMY_PASSWORD) // check if it changed
                profile.FileStoragePassword = (EncryptedString)(FileStoragePassword ?? string.Empty);
        }
        profile.CloudFeatures = FilesEnabled ? profile.CloudFeatures | CloudFeature.Files : profile.CloudFeatures & ~CloudFeature.Files;
        service.UpdateProfile(profile);
        FileStorageEditor.MarkClean();
        ToastService.ShowSuccess("File Storage Saved");
    }
    
    /// <summary>
    /// Tests the file storage settings
    /// </summary>
    async Task FileStorageTest()
    {
        if (string.IsNullOrEmpty(FileStorageUrl) || string.IsNullOrEmpty(FileStorageProvider))
            return;
        string password = FileStoragePassword;
        if (password == Globals.DUMMY_PASSWORD)
        {
            var service = new UserService();
            var profile = service.GetProfileByUid(this.UserUid);
            password = profile.FileStoragePassword?.Value ?? string.Empty;
        }
        var result = await WebDavFileStorage.Test(FileStorageProvider, FileStorageUrl, FileStorageUsername, password);
        if (result.Success)
            ToastService.ShowSuccess("Successfully connected to file storage server");
        else
            ToastService.ShowError(result.Error);
    }

    /// <summary>
    /// Saves the email settings
    /// </summary>
    async Task EmailSave()
    {
        var service = new UserService();
        var profile = service.GetProfileByUid(this.UserUid);
        profile.EmailImapServer = (EncryptedString)(EmailImapServer ?? string.Empty);
        if (string.IsNullOrEmpty(EmailImapServer))
        {
            profile.EmailImapUsername = (EncryptedString)string.Empty;
            profile.EmailImapPassword = (EncryptedString)string.Empty;
            profile.EmailImapPort = 0;
        }
        else
        {
            profile.EmailImapUsername = (EncryptedString)(EmailImapUsername ?? string.Empty);
            if(EmailImapPassword != Globals.DUMMY_PASSWORD) // check if it changed
                profile.EmailImapPassword = (EncryptedString)(EmailImapPassword ?? string.Empty);
            profile.EmailImapPort = EmailImapPort;
        }
        
        
        profile.EmailSmtpServer = (EncryptedString)(EmailSmtpServer ?? string.Empty);
        if (string.IsNullOrEmpty(EmailSmtpServer))
        {
            profile.EmailSmtpUsername = (EncryptedString)string.Empty;
            profile.EmailSmtpPassword = (EncryptedString)string.Empty;
            profile.EmailSmtpPort = 0;
        }
        else
        {
            profile.EmailSmtpUsername = (EncryptedString)(EmailImapUsername ?? string.Empty);
            if(EmailSmtpPassword != Globals.DUMMY_PASSWORD) // check if it changed
                profile.EmailSmtpPassword = (EncryptedString)(EmailSmtpPassword ?? string.Empty);
            profile.EmailSmtpPort = EmailSmtpPort;
        }
        
        profile.CloudFeatures = EmailEnabled ? profile.CloudFeatures | CloudFeature.Email : profile.CloudFeatures & ~CloudFeature.Email;
        service.UpdateProfile(profile);
        Workers.MailWorker.Instance.UserMailServer(profile);
        EmailEditor.MarkClean();
        ToastService.ShowSuccess("Email Saved");
    }

    /// <summary>
    /// Tests the email settings
    /// </summary>
    async Task EmailTest()
    {
        if (string.IsNullOrEmpty(EmailImapServer))
            return;
        string password = EmailImapPassword;
        if (password == Globals.DUMMY_PASSWORD)
        {
            var service = new UserService();
            var profile = service.GetProfileByUid(this.UserUid);
            password = profile.EmailImapPassword?.Value ?? string.Empty;
        }

        using var imapService = new ImapService(new UserProfile()
        {
            EmailImapServer = (EncryptedString)EmailImapServer, 
            EmailImapPort = EmailImapPort,
            EmailImapUsername = (EncryptedString)EmailImapUsername,
            EmailImapPassword = (EncryptedString)password   
        });
        var result = await imapService.Test();
        if (result.Success)
            ToastService.ShowSuccess("Successfully connected to file storage server");
        else
            ToastService.ShowError(result.Error);
    }

    /// <summary>
    /// Changes the users password
    /// </summary>
    async Task ChangePassword()
    {
        ErrorGeneral = string.Empty;
        ErrorChangePassword = string.Empty;
        
        if (await PasswordEditor.Validate() == false)
            return;

        if (this.PasswordNew.Length < 4)
        {
            ErrorChangePassword = Translator.Instant("Pages.Profile.Messages.PasswordValidationError");
            return;
        }

        if (PasswordNew != PasswordConfirm)
        {
            ErrorChangePassword = Translator.Instant("Pages.Profile.Messages.PasswordMismatch");
            return;
        }

        if (BCrypt.Net.BCrypt.Verify(PasswordCurrent, Model.Password) == false)
        {
            ErrorChangePassword = Translator.Instant("Pages.Profile.Messages.PasswordIncorrect");
            return;
            
        }

        var service = new UserService();
        service.ChangePassword(UserUid, PasswordNew);
        Model = service.GetByUid(UserUid);
        ToastService.ShowSuccess(Translator.Instant("Pages.Profile.Messages.PasswordChanged"));
        PasswordConfirm = string.Empty;
        PasswordCurrent = string.Empty;
        PasswordNew = string.Empty;
    }

    /// <summary>
    /// App Groups to show in the app drawer
    /// </summary>
    private List<CloudAppGroup> AppGroups = new();

    /// <summary>
    /// A clean list of app groups names, used to determine if the groups are dirty and should prevent user navigating away if not saved
    /// </summary>
    private List<string> CleanAppGroupNames = new();

    /// <summary>
    /// Saves the App settings
    /// </summary>
    private void AppsSave()
    {
        var service = new UserService();
        var profile = service.GetProfileByUid(this.UserUid);
        
        profile.CloudFeatures = AppsEnabled ? profile.CloudFeatures | CloudFeature.Apps : profile.CloudFeatures & ~CloudFeature.Apps;
        if (AppsEnabled)
        {
            foreach (var group in AppGroups)
            {
                if (group.Uid == Guid.Empty)
                    group.Uid = Guid.NewGuid();
                group.Enabled = true; // hardcoded for now
                group.Items ??= new();
                foreach (var item in group.Items)
                {
                    if (item.Icon?.StartsWith("data:") == true)
                    {
                        // base64 encoded file
                        item.Icon = ImageHelper.SaveImageFromBase64(item.Icon);
                    }

                    if (item.Uid == Guid.Empty)
                        item.Uid = Guid.NewGuid();
                }
            }

            profile.AppGroups = AppGroups ?? new ();
        }

        service.UpdateProfile(profile);
        AppsDirty = false;
        AppsEditor.MarkClean();
        CleanAppGroupNames = AppGroups.Select(x => x.Name).ToList();
        ToastService.ShowSuccess("Apps Saved");
    }

    /// <summary>
    /// Adds a new app group
    /// </summary>
    private async Task AppsAddGroup()
    {
        AppGroups.Add(new ()
        {
            Uid = Guid.NewGuid(),
            Name = "New Group",
            Enabled = true,
            Items = new ()
        });
        AppsDirty = true;
    }

    /// <summary>
    /// Deletes a app group
    /// </summary>
    /// <param name="group"></param>
    private async Task AppsDeleteGroup(CloudAppGroup group)
    {
        var result = await Confirm.Show("Delete Group", $"Are you sure you want to delete the group '{group.Name}'?");
        if (result)
        {
            AppGroups.Remove(group);
            AppsDirty = true;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Adds a new item to the app group
    /// </summary>
    /// <param name="group">the group to add the new item to</param>
    private async Task AppsAddItem(CloudAppGroup group)
    {
        var result = await Popup.OpenEditor<CloudAppEditor, CloudApp>(Translator, new CloudApp());
        if (result.Success == false)
            return;
        result.Data.Uid = Guid.NewGuid();
        group.Items.Add(result.Data);
        AppsDirty = true;
        StateHasChanged();
    }

    /// <summary>
    /// Edits an item
    /// </summary>
    /// <param name="item">the item being edited</param>
    private async Task AppsEditItem(CloudApp item)
    {
        var result = await Popup.OpenEditor<CloudAppEditor, CloudApp>(Translator, item);
        if (result.Success == false)
            return;
        item.Name = result.Data.Name;
        item.Icon = result.Data.Icon;
        item.Type = result.Data.Type;
        item.Address = result.Data.Address;
        AppsDirty = true;
        StateHasChanged();
    }

    /// <summary>
    /// Removes an item from the app group
    /// </summary>
    /// <param name="group">the group to remove the item from</param>
    /// <param name="item">the item to remove</param>
    private async Task AppsDeleteItem(CloudAppGroup group, CloudApp item)
    {
        var result = await Confirm.Show("Delete", $"Are you sure you want to delete this app '{item.Name}'?");
        if (result == false)
            return;
        group.Items.Remove(item);
        AppsDirty = true;
        StateHasChanged();
    }

    /// <summary>
    /// Gets or sets if this is dirty
    /// </summary>
    protected override bool IsDirty
    {
        get
        {
            if (AppsDirty)
                return true;
            if (AppsEditor?.IsDirty() == true || CalendarEditor?.IsDirty() == true || EmailEditor?.IsDirty() == true || GeneralEditor?.IsDirty() == true || FileStorageEditor?.IsDirty() == true)
                return true;
            bool differences = AppGroups?.Any(x => CleanAppGroupNames.Contains(x.Name) == false) == true;
            if (differences)
                return true;
            return base.IsDirty;
        }
        set => base.IsDirty = value;
    }
}