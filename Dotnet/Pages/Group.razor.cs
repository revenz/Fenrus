using Fenrus.Components.Dialogs;
using Fenrus.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Pages;

/// <summary>
/// Group page
/// </summary>
public partial class Group: UserPage
{
    /// <summary>
    /// Gets or sets if this is a new group or not
    /// </summary>
    private bool isNew { get; set; }
    
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
    
    /// <summary>
    /// Called after the User has been retrieved
    /// </summary>
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

    /// <summary>
    /// Saves the group and returns to the group page
    /// </summary>
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

        foreach (var item in Model.Items)
        {
            if (item.Uid == Guid.Empty)
                item.Uid = Guid.NewGuid();
        }
        Settings.Save();
        this.Router.NavigateTo("/settings/groups");
    }

    /// <summary>
    /// Cancels editing the group and returns to the groups page
    /// </summary>
    void Cancel()
    {
        Router.NavigateTo("/settings/groups");
    }

    /// <summary>
    /// Opens the new item dialog and adds any items to the group
    /// </summary>
    async Task AddItem()
    {
        await foreach(var result in Popup.GroupItemEditorNew())
        {
            if (result.Success)
            {
                var newItem = result.Data;
                newItem.Uid = Guid.NewGuid();
                this.Model.Items.Add(newItem);
                StateHasChanged();
                await UpdatePreview();
            }
        }
    }

    /// <summary>
    /// Updates the preview.
    /// Call this in case the theme needs to modify the rendering of the preview (eg default theme compacts the items into a bin)
    /// </summary>
    /// <param name="timeout">a wait timeout before calling the initPreview of the theme, used when first render in
    /// case the elements have not been added to the DOM yet</param>
    async Task UpdatePreview(int timeout = 0)
    {
        string js = "if(themeInstance){themeInstance.initPreview();}";
        try
        {
            if (timeout > 0)
                js = $"setTimeout(function() {{ if(themeInstance) themeInstance.initPreview(); }}, {timeout});";
            await jsRuntime.InvokeVoidAsync("eval", js);
        }
        catch (Exception)
        {
            //  may throw if prerendering, wait until after render
            try
            {
                await Task.Delay(100);
                await jsRuntime.InvokeVoidAsync("eval", js);
            }
            catch (Exception)
            {
            }
        }
    }


    /// <summary>
    /// Edits an item
    /// </summary>
    /// <param name="item">the item to edit</param>
    async Task Edit(GroupItem item)
    {
        var index = Model.Items.IndexOf(item);
        var result = await Popup.GroupItemEditor(item);
        if (result.Success == false)
            return;
        Model.Items[index] = result.Data;
        StateHasChanged();
        await UpdatePreview();
    }

    
    /// <summary>
    /// Deletes an item
    /// </summary>
    /// <param name="item">the item to delete</param>
    async Task Delete(GroupItem item)
    {
        if (await Confirm.Show("Delete Item", $"Are you sure you want to delete '{item.Name}'?") == false)
            return;
        Model.Items.Remove(item);
        StateHasChanged();
        await UpdatePreview();
    }
}