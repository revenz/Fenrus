using System.Text.RegularExpressions;
using Fenrus.Components;
using Fenrus.Models;
using Jint.Runtime.Modules;

namespace Fenrus.Pages;

/// <summary>
/// Profile page
/// </summary>
public partial class Profile: UserPage
{
    private string lblTitle, lblPageDescription, lblGeneral, lblChangePassword;
    private string ErrorGeneral, ErrorChangePassword;

    private EditorForm GeneralEditor, PasswordEditor;
    
    private User Model { get; set; }

    private string Username, Email, PasswordCurrent, PasswordNew, PasswordConfirm;

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.Profile.Title");
        lblPageDescription = Translator.Instant("Pages.Profile.Labels.PageDescription");
        lblGeneral = Translator.Instant("Pages.Profile.Labels.General");
        lblChangePassword = Translator.Instant("Pages.Profile.Labels.ChangePassword");
        PasswordCurrent = string.Empty;
        PasswordNew = string.Empty;
        PasswordConfirm = string.Empty;

        this.Model = new UserService().GetByUid(this.UserUid);
        Username = this.Model.Username;
        Email = this.Model.Email;
        

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
        service.Update(Model);
        ToastService.ShowSuccess("Labels.Saved");
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