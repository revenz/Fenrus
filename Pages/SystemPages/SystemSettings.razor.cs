namespace Fenrus.Pages;

/// <summary>
/// System Settings page
/// </summary>
public partial class SystemSettings:UserPage
{
    private string lblTitle, lblDescription,
        lblSmtp, lblSmtpDescription, 
        lblSmtpServer, lblSmtpPort, lblSmtpUser, lblSmtpPassword,
        lblSmtpSender, lblStmpSenderHelp, lblTest;

    private Models.SystemSettings Model;
    private string SmtpPassword;
    

    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Model = new SystemSettingsService().Get();
        if (Model.SmtpPort < 1 || Model.SmtpPort > 65535)
            Model.SmtpPort = 25;
        SmtpPassword = string.IsNullOrEmpty(Model.SmtpPasswordEncrypted) ? string.Empty : Globals.DUMMY_PASSWORD; 
        
        lblTitle = Translator.Instant("Pages.SystemSettings.Title");
        lblDescription = Translator.Instant("Pages.SystemSettings.Labels.PageDescription");
        lblSmtp = Translator.Instant("Pages.SystemSettings.Label.Smtp");
        lblSmtpDescription = Translator.Instant("Pages.SystemSettings.Labels.SmtpDescription");
        lblSmtpServer = Translator.Instant("Pages.SystemSettings.Fields.SmtpServer");
        lblSmtpPort = Translator.Instant("Pages.SystemSettings.Fields.SmtpPort");
        lblSmtpUser = Translator.Instant("Pages.SystemSettings.Fields.SmtpUser");
        lblSmtpPassword = Translator.Instant("Pages.SystemSettings.Fields.SmtpPassword");
        lblSmtpSender = Translator.Instant("Pages.SystemSettings.Fields.SmtpSender");
        lblStmpSenderHelp = Translator.Instant("Pages.SystemSettings.Fields.SmtpSenderHelp");
        lblTest = Translator.Instant("Labels.Test");
    }

    /// <summary>
    /// Save the system settings
    /// </summary>
    private void Save()
    {
        if (SmtpPassword != Globals.DUMMY_PASSWORD)
            Model.SmtpPasswordEncrypted = EncryptionHelper.Encrypt(SmtpPassword);
        
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

}