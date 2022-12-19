using Microsoft.AspNetCore.Components;

namespace Fenrus.Pages;

/// <summary>
/// Dashboard page
/// </summary>
public partial class Dashboard : ComponentBase
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

    protected override void OnInitialized()
    {
        base.OnInitialized();
        Console.WriteLine("Guid: " + Uid);
        this.Model = DemoHelper.GetDemoUserSettings().Dashboards.FirstOrDefault(x => x.Uid == Uid) ?? new ();
    }
    
    

    private async Task Move(Models.Group item, bool up)
    {
        
    }

    private async Task Remove(Models.Group item)
    {
        
    }

    private async Task Add()
    {
        
    }
    
    void Save()
    {
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/dashboards");
    }
}