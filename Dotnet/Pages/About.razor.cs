namespace Fenrus.Pages;

/// <summary>
/// About Page
/// </summary>
public partial class About: UserPage
{
    private Fenrus.Components.Dialogs.FileInputDialog FileInputDialog { get; set; }
    private string lblTitle, lblImportConfig, lblImporting, lblAuthor, lblVersion, lblWebsite, lblThirdParty;
    private bool Importing = false;

    protected override Task PostGotUser()
    {
        lblTitle = Translater.Instant("Pages.About.Title");
        lblImporting = Translater.Instant("Pages.About.Labels.Importing");
        lblImportConfig = Translater.Instant("Pages.About.Buttons.ImportConfig");
        lblAuthor = Translater.Instant("Labels.Author");
        lblVersion = Translater.Instant("Labels.Version");
        lblWebsite = Translater.Instant("Labels.Website");
        lblThirdParty = Translater.Instant("Pages.About.Labels.ThirdParty");
        return Task.CompletedTask;
    }

    async Task ImportConfig()
    {
        var result = await FileInputDialog.Show(
            Translater.Instant("Dialogs.ImportConfig.Title"),
            Translater.Instant("Dialogs.ImportConfig.Description"),
            Translater.Instant("Labels.Import"),
            ".json"
        );
        if (string.IsNullOrWhiteSpace(result))
            return;

        this.Importing = true;
        this.StateHasChanged();
            
        var importResult = await new ConfigImporter().Import(result);
        
        this.Importing = false;
        this.StateHasChanged();
    }
}