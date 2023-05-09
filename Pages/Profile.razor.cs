using System.Text.RegularExpressions;
using Fenrus.Components;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Fenrus.Services.CalendarServices;
using Fenrus.Services.FileStorages;
using Ical.Net;
using Jint.Runtime.Modules;

namespace Fenrus.Pages;

/// <summary>
/// Profile page
/// </summary>
public partial class Profile: UserPage
{
    private string lblTitle, lblPageDescription, lblGeneral, lblChangePassword, lblCalendar, lblFileStorage, 
        CalendarUrl, CalendarProvider, CalendarUsername, CalendarPassword, CalendarName, lblTest,
        FileStorageUrl, FileStorageProvider, FileStorageUsername, FileStoragePassword;
    private string ErrorGeneral, ErrorChangePassword, ErrorCalendar, ErrorFileStorage, lblCalendarPageDescription,lblFileStoragePageDescription;

    private EditorForm GeneralEditor, PasswordEditor, CalendarEditor, FileStorageEditor;

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

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Profile.Title");
        lblTest = Translator.Instant("Labels.Test");
        lblPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription");
        lblGeneral = Translator.Instant("Pages.Profile.Labels.General");
        lblCalendar = Translator.Instant("Pages.Profile.Labels.Calendar");
        lblFileStorage = Translator.Instant("Pages.Profile.Labels.FileStorage");
        lblChangePassword = Translator.Instant("Pages.Profile.Labels.ChangePassword");
        lblCalendarPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-Calendar");
        lblFileStoragePageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription-FileStorage");
        PasswordCurrent = string.Empty;
        PasswordNew = string.Empty;
        PasswordConfirm = string.Empty;

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

        return Task.CompletedTask;
    }

    async Task Save()
    {
        ErrorGeneral = string.Empty;
        ErrorChangePassword = string.Empty;
        
        if (await GeneralEditor.Validate() == false)
            return;

        if (this.Email == Model.Email && this.Username == Model.Username)
            return; // nothing to do

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
        service.Update(Model);
        ToastService.ShowSuccess("Labels.Saved");
    }

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
        service.UpdateProfile(profile);
        ToastService.ShowSuccess("Calendar Saved");
    }
    
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
        service.UpdateProfile(profile);
        ToastService.ShowSuccess("File Storage Saved");
    }
    
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
}