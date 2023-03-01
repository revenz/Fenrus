using Fenrus.Components;
using Fenrus.Models.UiModels;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Pages;

/// <summary>
/// Dashboard page
/// </summary>
public partial class Dashboard : CommonPage<Models.Group>
{
    [Inject] private NavigationManager Router { get; set; }
    Models.Dashboard Model { get; set; } = new();

    private Fenrus.Components.FenrusTable<Models.Group> Table { get; set; }

    private Fenrus.Components.Dialogs.GroupAddDialog GroupAddDialog { get; set; }

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
        if (Uid == Guid.Empty)
        {
            // new item
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

            Model.AccentColor = "#ff0090";;
            Model.Theme = "Default";
        }
        else
        {
            isNew = false;
            Model = Settings.Dashboards.First(x => x.Uid == Uid);
        }
    }
    void Save()
    {
        //Model.Groups = Table.Data ?? new();
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

    async Task AddGroup()
    {
        var inUse = Model.Groups.Select(x => x.Uid);
        var available = Settings.Groups.Where(x => inUse.Contains(x.Uid) == false).ToList();
        if (available.Any() == false)
        {
            ToastService.ShowWarning("No available groups to add");
            return;
        }
        var adding = await GroupAddDialog.Show(available.Select(x => new ListOption
        {
            Label = x.Name, 
            Value = x.Uid
        }).ToList());

        if (adding?.Any() != true)
            return;
        foreach (var opt in adding)
        {
            if (opt.Value is Guid uid == false)
                continue;
            if (Model.Groups.Any(x => x.Uid == uid))
                continue;
            var group = available.FirstOrDefault(x => x.Uid == uid);
            if (group == null)
                continue;
            group.Enabled = true;
            Model.Groups.Add(group);
        }
        Table.SetData(Model.Groups);
    }

    protected override bool DoDelete(Models.Group item)
    {
        if (Model.Groups.Contains(item) == false)
            return false;
        Model.Groups.Remove(item);
        Table.SetData(Model.Groups);
        return true;
    }
}