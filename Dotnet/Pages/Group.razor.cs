using System.Runtime.InteropServices;
using Fenrus.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Pages;

/// <summary>
/// Group page
/// </summary>
public partial class Group: UserPage
{
    Models.Group Model { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the JS Runtime
    /// </summary>
    [Inject] public IJSRuntime jsRuntime { get; set; }

    
    /// <summary>
    /// Gets or sets the UID as a string
    /// </summary>
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
        await UpdatePreview(250);
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
        await foreach(var result in Popup.GroupItemEditorNew())
        {
            if (result.Success)
            {
                this.Model.Items.Add(result.Data);
                StateHasChanged();
                await UpdatePreview();
            }
        }
    }

    async Task UpdatePreview(int timeout = 0)
    {
        if (timeout > 0)
        {
            string js = $"setTimeout(function() {{ if(themeInstance) themeInstance.initPreview(); }}, {timeout});";
            await jsRuntime.InvokeVoidAsync("eval", js);
        }
        else
            await jsRuntime.InvokeVoidAsync("eval", "if(themeInstance){themeInstance.initPreview();}");
    }


    async Task Edit(GroupItem item)
    {
        var index = Model.Items.IndexOf(item);
        var result = await Popup.GroupItemEditor(item);
        if (result.Success == false)
            return;
        Model.Items[index] = result.Data;
    }
}