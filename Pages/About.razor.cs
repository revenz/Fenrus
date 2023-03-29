namespace Fenrus.Pages;

/// <summary>
/// About Page
/// </summary>
public partial class About: UserPage
{
    private string lblTitle, lblAuthor, lblVersion, lblWebsite, lblThirdParty;

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant("Pages.About.Title");
        lblAuthor = Translator.Instant("Labels.Author");
        lblVersion = Translator.Instant("Labels.Version");
        lblWebsite = Translator.Instant("Labels.Website");
        lblThirdParty = Translator.Instant("Pages.About.Labels.ThirdParty");
        return Task.CompletedTask;
    }
}