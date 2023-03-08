using Fenrus.Components.Dialogs;

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
        lblTitle = Translator.Instant("Pages.About.Title");
        lblImporting = Translator.Instant("Pages.About.Labels.Importing");
        lblImportConfig = Translator.Instant("Pages.About.Buttons.ImportConfig");
        lblAuthor = Translator.Instant("Labels.Author");
        lblVersion = Translator.Instant("Labels.Version");
        lblWebsite = Translator.Instant("Labels.Website");
        lblThirdParty = Translator.Instant("Pages.About.Labels.ThirdParty");
        return Task.CompletedTask;
    }

    async Task ImportConfig()
    {
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
}