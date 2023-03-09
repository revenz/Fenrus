using Fenrus.Components.Dialogs;
using Fenrus.Models;
using Fenrus.Pages;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components;

/// <summary>
/// Page Group
/// </summary>
public partial class PageGroup: CommonPage<Models.Group>
{
    private CopyItemDialog CopyDialog { get; set; }

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

    private string lblPageTitle, lblAddItem, lblShowGroupTitle, lblShowGroupTitleHelp, lblNameHelp; 
    
    /// <summary>
    /// Called after the User has been retrieved
    /// </summary>
    protected override async Task PostGotUser()
    {
        lblPageTitle = Translator.Instant("Pages.Group.Title");
        lblAddItem = Translator.Instant("Pages.Group.Buttons.AddItem");
        lblShowGroupTitle = Translator.Instant("Pages.Group.Fields.ShowGroupTitle");
        lblShowGroupTitleHelp = Translator.Instant("Pages.Group.Fields.ShowGroupTitle-Help");
        lblNameHelp = Translator.Instant("Pages.Group.Fields.Name-Help");
        if (Uid == Guid.Empty)
        {
            // new item
            isNew = true;
            Model = new();
            Model.Name = "New Group";
            var usedNames = GetAll().Select(x => x.Name).ToArray();
            int count = 1;
            while (usedNames.Contains(Model.Name))
            {
                ++count;
                Model.Name = $"New Group ({count})";
            }
        }
        else
        {
            isNew = false;
            Model = GetById();
            if (Model == null)
            {
                Router.NavigateTo(IsSystem ? "/settings/system/groups" : "/settings/groups");
                return;
            }
        }
        await UpdatePreview(250);
    }

    List<Models.Group> GetAll()
    {
        if (IsSystem)
            return new Services.GroupService().GetSystemGroups();
        return Settings.Groups;
    }

    /// <summary>
    /// Gets the group by the Uid
    /// </summary>
    /// <returns>the group if found</returns>
    Models.Group? GetById()
    {
        if (IsSystem)
            return new Services.GroupService().GetByUid(Uid);
        return Settings.Groups.FirstOrDefault(x => x.Uid == Uid);
    }

    /// <summary>
    /// Saves the group and returns to the group page
    /// </summary>
    void Save()
    {
        if (isNew)
        {
            Model.Uid = Guid.NewGuid();
            Model.Enabled = true; // default to being enabled
            Model.IsSystem = IsSystem;
        }
        else if (IsSystem)
        {
            var existing = new GroupService().GetByUid(Uid);
            existing.Name = Model.Name;
            existing.HideGroupTitle = Model.HideGroupTitle;
            existing.Items = Model.Items ?? new();
        }
        else
        {
            var existing = Settings.Groups.First(x => x.Uid == Uid);
            existing.Name = Model.Name;
            existing.HideGroupTitle = Model.HideGroupTitle;
            existing.Items = Model.Items ?? new();
        }

        foreach (var item in Model.Items)
        {
            if (item.Uid == Guid.Empty)
                item.Uid = Guid.NewGuid();
        }

        if (IsSystem)
        {
            if (isNew)
                new GroupService().Add(Model);
            else
                new GroupService().Update(Model);
            
            this.Router.NavigateTo("/settings/system/groups");
        }
        else
        {
            if(isNew)
                Settings.Groups.Add(Model);
            Settings.Save();
            this.Router.NavigateTo("/settings/groups");
        }

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
        await foreach(var result in Popup.GroupItemEditorNew(Translator))
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
        var result = await Popup.GroupItemEditor(Translator, item);
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

    async Task Move(GroupItem item, bool up)
    {
        var source = Model.Items.IndexOf(item);
        var dest = up ? source - 1 : source + 1;
        if (dest < 0 || dest >= Model.Items.Count)
            return;
        Model.Items[source] = Model.Items[dest];
        Model.Items[dest] = item;
        await UpdatePreview();
    }

    async Task Copy(GroupItem item)
    {
        var result = await CopyDialog.Show();
        if (result == null)
            return; // was canceled

        var cloned = CloneItem(item);
        if (cloned == null)
            return;

        Guid dest = result.Value;

        if (dest == this.Uid || dest == Guid.Empty)
        {
            // copy to this group
            this.Model.Items.Add(cloned);
            await UpdatePreview();
        }
        else
        {
            // copy to another group
            if (IsAdmin)
            {
                var service = new GroupService();
                var sysGroup = service.GetByUid(dest);
                if (sysGroup != null)
                {
                    sysGroup.Items.Add(cloned);
                    service.Update(sysGroup);
                    ToastService.ShowSuccess(Translator.Instant("Pages.Group.Messages.ItemCopied", new
                    {
                        groupName = sysGroup.Name,
                        itemName = cloned.Name
                    }));
                    return;
                }
            }

            var grp = Settings.Groups.FirstOrDefault(x => x.Uid == dest);
            if (grp == null)
            {
                // shouldn't happen
                ToastService.ShowError("Failed to locate group");
                return;
            }

            grp.Items.Add(cloned);
            Settings.Save();
            ToastService.ShowSuccess(Translator.Instant("Pages.Group.Messages.ItemCopied", new
            {
                groupName = grp.Name,
                itemName = cloned.Name
            }));
        }
    }


    private GroupItem CloneItem(GroupItem item)
    {
        if (item is AppItem app)
        {
            var json = JsonSerializer.Serialize(app);
            var newApp = JsonSerializer.Deserialize<AppItem>(json);
            newApp.Uid = Guid.NewGuid();
            return newApp;
        }
        if (item is LinkItem link)
        {
            var json = JsonSerializer.Serialize(link);
            var newLink = JsonSerializer.Deserialize<LinkItem>(json);
            newLink.Uid = Guid.NewGuid();
            return newLink;
        }
        ToastService.ShowError("Item type not implemented yet");
        return null;
    }
}