using Fenrus.Models;
using Microsoft.AspNetCore.Components;

namespace Fenrus.Pages;

/// <summary>
/// Group page
/// </summary>
public partial class Group: UserPage
{
    Models.Group Model { get; set; } = new();

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
    /// Gets or sets the UID of the group
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
            Model.Name = "New Group";
            var usedNames = Settings.Groups.Select(x => x.Name).ToArray();
            int count = 1;
            while (usedNames.Contains(Model.Name))
            {
                ++count;
                Model.Name = $"New Group ({count})";
            }
            Model.AccentColor = Settings.AccentColor;
        }
        else
        {
            isNew = false;
            Model = Settings.Groups.First(x => x.Uid == Uid);
        }
    }

    void Save()
    {
        if (isNew)
        {
            Model.Uid = Guid.NewGuid();
            Settings.Groups.Add(Model);
        }
        else
        {
            var existing = Settings.Groups.First(x => x.Uid == Uid);
            existing.Name = Model.Name;
            existing.AccentColor = Model.AccentColor;
            existing.HideGroupTitle = Model.HideGroupTitle;
            existing.Items = Model.Items ?? new();
        }
        Settings.Save();
        this.Router.NavigateTo("/settings/groups");
    }

    void Cancel()
    {
        Router.NavigateTo("/settings/groups");
    }

    async Task AddItem()
    {
        var result = await Popup.GroupItemEditor(null);
    }
}