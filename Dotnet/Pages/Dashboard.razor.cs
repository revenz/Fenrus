using Microsoft.AspNetCore.Components;

namespace Fenrus.Pages;

/// <summary>
/// Dashboard page
/// </summary>
public partial class Dashboard : CommonPage<Models.Group>
{
    [Inject] private NavigationManager Router { get; set; }
    Models.Dashboard Model { get; set; } = new();

    [Parameter]
    public string UidString
    {
        get => Uid.ToString();
        set
        {
            if (Guid.TryParse(value, out Guid guid))
                this.Uid = guid;
            else
            {
                // do a redirect
            }
        }
    }

    /// <summary>
    /// Gets or sets the UID of the dashboard
    /// </summary>
    public Guid Uid { get; set; }

    protected override async Task PostGotUser()
    {
        Console.WriteLine("Guid: " + Uid);
        this.Model = Settings.Dashboards.FirstOrDefault(x => x.Uid == Uid) ?? new ();
    }
    
    void Save()
    {
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/dashboards");
    }
}