namespace Fenrus.Pages;

/// <summary>
/// System Settings page
/// </summary>
public partial class SystemSettings:UserPage
{
    private string lblTitle, lblDescription,
        lblSmtpServer, lblSmtpPort, lblSmtpUser, lblSmtpPassword;

    private Models.SystemSettings Model;

    /// <summary>
    /// Called after the user has been fetched
    /// </summary>
    protected override async Task PostGotUser()
    {
        Model = new SystemSettingsService().Get();
        lblTitle = Translater.Instant("Pages.SystemSettings.Title");
        lblDescription = Translater.Instant("Pages.SystemSettings.Labels.PageDescription");
        lblSmtpServer = Translater.Instant("Pages.SystemSettings.Fields.SmtpServer");
        lblSmtpPort = Translater.Instant("Pages.SystemSettings.Fields.SmtpPort");
        lblSmtpUser = Translater.Instant("Pages.SystemSettings.Fields.SmtpUser");
        lblSmtpPassword = Translater.Instant("Pages.SystemSettings.Fields.SmtpPassword");
    }

}