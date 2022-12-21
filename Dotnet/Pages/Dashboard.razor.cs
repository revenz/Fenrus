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
    private bool isNew = false;

    protected override async Task PostGotUser()
    {
        Console.WriteLine("Guid: " + Uid);
        
        if (Uid == Guid.Empty)
        {
            // new search engine
            isNew = true;
            Model = new();
            Model.Name = "New Dashboard";
            var usedNames = Settings.Dashboards.Select(x => x.Name).ToArray();
            int count = 1;
            while (usedNames.Contains(Model.Name))
            {
                ++count;
                Model.Name = $"New Dashboard ({count})";
            }
            Model.AccentColor = Settings.AccentColor;
            Model.Theme = Settings.Theme;
        }
        else
        {
            isNew = false;
            Model = Settings.Dashboards.First(x => x.Uid == Uid);
        }
    }
    void Save()
    {
        if (isNew)
        {
            Model.Uid = Guid.NewGuid();
            Settings.Dashboards.Add(Model);
        }
        else
        {
            var existing = Settings.Dashboards.First(x => x.Uid == Uid);
            existing.Name = Model.Name;
            existing.AccentColor = Model.AccentColor;
            existing.Theme = Model.Theme;
            existing.Groups = Model.Groups ?? new();
        }
        Settings.Save();
        this.Router.NavigateTo("/settings/dashboards");
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/dashboards");
    }
}