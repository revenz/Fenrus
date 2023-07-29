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
            Model = new GroupService().GetByUid(Uid);
            if (Model == null || Model.UserUid != (IsSystem ? Guid.Empty : UserUid))
            {
                Router.NavigateTo(IsSystem ? "/settings/system/groups" : "/settings/groups");
                return;
            }
        }
        await UpdatePreview(250);
    }

    List<Models.Group> GetAll()
        => IsSystem ? new GroupService().GetSystemGroups() : new GroupService().GetAllForUser(UserUid);


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
            Model.UserUid = IsSystem ? Guid.Empty : UserUid;
        }
        else
        {
            var existing = new GroupService().GetByUid(Uid);
            existing.Name = Model.Name;
            existing.HideGroupTitle = Model.HideGroupTitle;
            existing.Items = Model.Items ?? new();
        }
            

        foreach (var item in Model.Items)
        {
            if (item.Uid == Guid.Empty)
                item.Uid = Guid.NewGuid();
            if (item.Icon?.StartsWith("data:") == true)
            {
                // base64 encoded file
                item.Icon = ImageHelper.SaveImageFromBase64(item.Icon);
            }
        }

        if (isNew)
            new GroupService().Add(Model);
        else
            new GroupService().Update(Model);
        
        this.Router.NavigateTo(IsSystem ? "/settings/system/groups" : "/settings/groups");

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
        await foreach(var result in Popup.GroupItemEditorNew(Translator, IsSystem, User))
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
        var result = await Popup.GroupItemEditor(Translator, item, IsSystem, User);
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
        this.StateHasChanged();
        await UpdatePreview(100);
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
            this.StateHasChanged();
            await Task.Delay(100);
            await UpdatePreview();
        }
        else
        {
            // copy to another group
            var service = new GroupService();
            var grp = service.GetByUid(dest);
            if (grp == null || (IsAdmin == false && grp.IsSystem) ||
                (grp.IsSystem == false && grp.UserUid != UserUid) )
            {
                ToastService.ShowError("Cannot copy to this group");
                return;
            }
            grp.Items.Add(cloned);
            service.Update(grp);
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
            // cant use the serialized properties as these will be deserialized as:
            // "mode": {
            //     "_type": "System.Text.Json.JsonElement, System.Text.Json",
            //     "ValueKind": "String"
            // } 
            newApp.Properties = app.Properties.ToDictionary(x => x.Key, x => x.Value);
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