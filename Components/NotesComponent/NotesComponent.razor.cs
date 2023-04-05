using Microsoft.AspNetCore.Components;

namespace Fenrus.Components;

/// <summary>
/// Component that shows the users notes
/// </summary>
public partial class NotesComponent
{
    /// <summary>
    /// Gets or sets the Translator to use
    /// </summary>
    [Parameter] public Translator Translator { get; set; }

    private string lblTitle, lblPersonal, lblDashboard, lblShared, lblMedia;

    protected override void OnInitialized()
    {
        this.lblTitle = Translator.Instant("Labels.Notes");
        this.lblPersonal = Translator.Instant("Labels.Personal");
        this.lblDashboard = Translator.Instant("Labels.Dashboard");
        this.lblShared = Translator.Instant("Labels.Shared");
        this.lblMedia= Translator.Instant("Labels.Media");
    }
}