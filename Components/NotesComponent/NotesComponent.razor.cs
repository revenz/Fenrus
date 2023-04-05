using Fenrus.Models;
using Microsoft.AspNetCore.Components;
using Org.BouncyCastle.Asn1.TeleTrust;

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

    /// <summary>
    /// Gets or sets the user settings
    /// </summary>
    [Parameter]
    public UserSettings Settings { get; set; }

    private List<Note> Notes { get; set; } = new();

    private string lblTitle, lblPersonal, lblDashboard, lblShared;

    protected override void OnInitialized()
    {
        this.lblTitle = Translator.Instant("Labels.Notes");
        this.lblPersonal = Translator.Instant("Labels.Personal");
        this.lblDashboard = Translator.Instant("Labels.Dashboard");
        this.lblShared = Translator.Instant("Labels.Shared");
        // Notes = new NotesService().GetAllByUser(Settings.UserUid).OrderBy(x => x.Order).ToList();
    }
}