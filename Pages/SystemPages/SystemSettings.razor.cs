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

    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Model = new SystemSettingsService().Get();
        if (Model.SmtpPort < 1 || Model.SmtpPort > 65535)
            Model.SmtpPort = 25;
        
        lblTitle = Translater.Instant("Pages.SystemSettings.Title");
        lblDescription = Translater.Instant("Pages.SystemSettings.Labels.PageDescription");
        lblSmtp = Translater.Instant("Pages.SystemSettings.Label.Smtp");
        lblSmtpDescription = Translater.Instant("Pages.SystemSettings.Labels.SmtpDescription");
        lblSmtpServer = Translater.Instant("Pages.SystemSettings.Fields.SmtpServer");
        lblSmtpPort = Translater.Instant("Pages.SystemSettings.Fields.SmtpPort");
        lblSmtpUser = Translater.Instant("Pages.SystemSettings.Fields.SmtpUser");
        lblSmtpPassword = Translater.Instant("Pages.SystemSettings.Fields.SmtpPassword");
        lblSmtpSender = Translater.Instant("Pages.SystemSettings.Fields.SmtpSender");
        lblStmpSenderHelp = Translater.Instant("Pages.SystemSettings.Fields.SmtpSenderHelp");
        lblTest = Translater.Instant("Labels.Test");
    }

    /// <summary>
    /// Save the system settings
    /// </summary>
    private void Save()
    {
        new SystemSettingsService().SaveFromEditor(this.Model);
        ToastService.ShowSuccess(Translater.Instant("Labels.Saved"));
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
            Emailer.Send(new EmailOptions()
            {
                Server = Model.SmtpServer,
                Port = Model.SmtpPort,
                Username = Model.SmtpUser,
                Password = Model.SmtpPassword,
                Sender = Model.SmtpSender,
                Recipient = Model.SmtpSender,
                Subject = "Test email from Fenrus",
                PlainTextBody = "This is a test email from Fenrus.  Sent to your Sender address."
            });
            ToastService.ShowInfo(Translater.Instant($"Pages.{nameof(SystemSettings)}.Messages.TestEmailSent"));
        }
        catch (Exception ex)
        {
            ToastService.ShowError(ex.Message);
        }
    }

}