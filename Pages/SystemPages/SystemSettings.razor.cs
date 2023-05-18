using Fenrus.Components.Dialogs;
using Fenrus.Models;

namespace Fenrus.Pages;


/// <summary>
/// System Settings page
/// </summary>
public partial class SystemSettings:UserPage
{
    private string lblTitle, lblDescription,
        lblImportConfig, lblImporting, lblUpdateApps, lblUpdatingApps, 
        lblSmtp, lblSmtpDescription,
        lblCloud, lblCloudDescription,
        lblSmtpServer, lblSmtpPort, lblSmtpUser, lblSmtpPassword,
        lblSmtpSender, lblStmpSenderHelp, lblTest;

    private bool CloudEnabled, CloudNotes, CloudFiles, CloudCalendar, CloudEmail, CloudApps;

    private Models.SystemSettings Model;
    private string SmtpPassword;
    private bool Importing = false;
    private bool Updating;
    private Fenrus.Components.Dialogs.FileInputDialog FileInputDialog { get; set; }
    

    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Model = new SystemSettingsService().Get();
        if (Model.SmtpPort < 1 || Model.SmtpPort > 65535)
            Model.SmtpPort = 25;
        SmtpPassword = string.IsNullOrEmpty(Model.SmtpPasswordEncrypted) ? string.Empty : Globals.DUMMY_PASSWORD;
        CloudEnabled = (int)Model.CloudFeatures > 0;
        CloudNotes = (CloudFeature.Notes & Model.CloudFeatures) == CloudFeature.Notes;
        CloudFiles = (CloudFeature.Files & Model.CloudFeatures) == CloudFeature.Files;
        CloudCalendar = (CloudFeature.Calendar & Model.CloudFeatures) == CloudFeature.Calendar;
        CloudEmail = (CloudFeature.Email & Model.CloudFeatures) == CloudFeature.Email;
        CloudApps = (CloudFeature.Apps & Model.CloudFeatures) == CloudFeature.Apps;
        
        lblTitle = Translator.Instant("Pages.SystemSettings.Title");
        lblDescription = Translator.Instant("Pages.SystemSettings.Labels.PageDescription");
        lblSmtp = Translator.Instant("Pages.SystemSettings.Labels.Smtp");
        lblSmtpDescription = Translator.Instant("Pages.SystemSettings.Labels.SmtpDescription");
        lblCloud = Translator.Instant("Labels.Cloud");
        lblCloudDescription = Translator.Instant("Pages.SystemSettings.Labels.CloudDescription");
        lblSmtpServer = Translator.Instant("Pages.SystemSettings.Fields.SmtpServer");
        lblSmtpPort = Translator.Instant("Pages.SystemSettings.Fields.SmtpPort");
        lblSmtpUser = Translator.Instant("Pages.SystemSettings.Fields.SmtpUser");
        lblSmtpPassword = Translator.Instant("Pages.SystemSettings.Fields.SmtpPassword");
        lblSmtpSender = Translator.Instant("Pages.SystemSettings.Fields.SmtpSender");
        lblStmpSenderHelp = Translator.Instant("Pages.SystemSettings.Fields.SmtpSenderHelp");
        lblTest = Translator.Instant("Labels.Test");
        lblImporting = Translator.Instant("Pages.SystemSettings.Labels.Importing");
        lblImportConfig = Translator.Instant("Pages.SystemSettings.Buttons.ImportConfig");
        lblUpdateApps = Translator.Instant("Pages.SystemSettings.Labels.UpdateApps");
        lblUpdatingApps = Translator.Instant("Pages.SystemSettings.Labels.UpdatingApps");
    }

    /// <summary>
    /// Save the system settings
    /// </summary>
    private void Save()
    {
        if (SmtpPassword != Globals.DUMMY_PASSWORD)
            Model.SmtpPasswordEncrypted = EncryptionHelper.Encrypt(SmtpPassword);

        this.Model.CloudFeatures = CloudFeature.None;
        if (CloudEnabled)
        {
            if (CloudNotes) this.Model.CloudFeatures |= CloudFeature.Notes;
            if (CloudFiles) this.Model.CloudFeatures |= CloudFeature.Files;
            if (CloudCalendar) this.Model.CloudFeatures |= CloudFeature.Calendar;
            if (CloudEmail) this.Model.CloudFeatures |= CloudFeature.Email;
            if (CloudApps) this.Model.CloudFeatures |= CloudFeature.Apps;
        }
        
        new SystemSettingsService().SaveFromEditor(this.Model);
        ToastService.ShowSuccess(Translator.Instant("Labels.Saved"));
    }

    /// <summary>
    /// Gets if the SMTP server can be tested
    /// </summary>
    /// <returns>true if can be tested, otherwise false</returns>
    private bool CanTestSmtp()
    {
        if (string.IsNullOrWhiteSpace(Model.SmtpSender))
            return false;
        if (string.IsNullOrWhiteSpace(Model.SmtpServer))
            return false;
        return true;
    }

    /// <summary>
    /// Tests the SMTP sever
    /// </summary>
    private void TestSmtp()
    {
        try
        {
            string password = SmtpPassword == Globals.DUMMY_PASSWORD
                ? EncryptionHelper.Decrypt(Model.SmtpPasswordEncrypted)
                : SmtpPassword;
            
            Emailer.Send(new EmailOptions()
            {
                Server = Model.SmtpServer,
                Port = Model.SmtpPort,
                Username = Model.SmtpUser,
                Password = password,
                Sender = Model.SmtpSender,
                Recipient = Model.SmtpSender,
                Subject = "Test email from Fenrus",
                PlainTextBody = "This is a test email from Fenrus.  Sent to your Sender address."
            });
            ToastService.ShowInfo(Translator.Instant($"Pages.{nameof(SystemSettings)}.Messages.TestEmailSent"));
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

    async Task ImportConfig()
    {
        if (IsAdmin == false)
            return;
        
        var result = await FileInputDialog.Show(
            Translator.Instant("Dialogs.ImportConfig.Title"),
            Translator.Instant("Dialogs.ImportConfig.Description"),
            Translator.Instant("Labels.Import"),
            ".json"
        );
        if (string.IsNullOrWhiteSpace(result))
            return;

        this.Importing = true;
        this.StateHasChanged();

        var log = await new ConfigImporter().Import(result);

        this.Importing = false;
        this.StateHasChanged();

        await MessageBox.Show(
            title: Translator.Instant("Dialogs.ImportConfig.Result"),
            message: log
        );
    }

    async Task UpdateApps()
    {
        this.Updating = true;
        this.StateHasChanged();
        var result = await new AppUpdaterService().Update();
        this.Updating = false;
        this.StateHasChanged();
        if(string.IsNullOrEmpty(result.Error))
            ToastService.ShowSuccess(Translator.Instant("Pages.SystemSettings.Messages.AppsUpdated", new { total = result.TotalApps, newCount = (result.Updated - result.Original)}));
        else
            ToastService.ShowError(Translator.Instant("Pages.SystemSettings.Messages." + result.Error));
    }

}