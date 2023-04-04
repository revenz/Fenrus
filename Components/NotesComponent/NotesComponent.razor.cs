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

    private List<Note> Notes { get; set; }

    private string lblTitle;

    protected override void OnInitialized()
    {
        this.lblTitle = Translator.Instant("Labels.Notes");
        Notes = new NotesService().GetAllByUser(Settings.UserUid).OrderBy(x => x.Order).ToList();
    }
}