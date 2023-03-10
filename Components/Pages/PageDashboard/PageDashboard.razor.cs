using Fenrus.Components.Dialogs;
using Fenrus.Models;
using Fenrus.Models.UiModels;
using Fenrus.Pages;
using Fenrus.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Fenrus.Components;

/// <summary>
/// Page Dashboard
/// </summary>
public partial class PageDashboard : CommonPage<Models.Group>
{
    Models.Dashboard Model { get; set; } = new();

    /// <summary>
    /// Gets if the dashboard being edited is the guest dashboard
    /// </summary>
    private bool IsGuest => Uid == Globals.GuestDashbardUid;

    /// <summary>
    /// The groups in this dashboard
    /// </summary>
    private List<Models.Group> Groups = new();

    private FenrusTable<Models.Group> Table { get; set; }

    private GroupAddDialog GroupAddDialog { get; set; }

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
    
    /// <summary>
    /// Gets or sets the language for guest users
    /// </summary>
    private string Language { get; set; }

    private string lblTitle, lblNameHelp, lblShowSearch, lblShowGroupTitles, lblLinkTarget,
        lblOpenInThisTab,lblOpenInNewTab, lblOpenInSameTab, lblAccentColor, lblBackgroundColor,
        lblTheme, lblLanguage;

    private List<ListOption> Languages;

    private List<ListOption> Themes;

    protected override Task PostGotUser()
    {
        lblTitle = Translator.Instant(IsGuest
            ? "Pages.Dashboard.Title-Guest"
            : "Pages.Dashboard.Title");
        lblNameHelp = Translator.Instant("Pages.Dashboard.Fields.Name-Help");
        lblShowSearch = Translator.Instant("Pages.Dashboard.Fields.ShowSearch");
        lblShowGroupTitles = Translator.Instant("Pages.Dashboard.Fields.ShowGroupTitles");
        lblTheme = Translator.Instant("Pages.Dashboard.Fields.Theme");
        lblAccentColor = Translator.Instant("Pages.Dashboard.Fields.AccentColor");
        lblBackgroundColor = Translator.Instant("Pages.Dashboard.Fields.BackgroundColor");
        lblLinkTarget = Translator.Instant("Pages.Dashboard.Fields.LinkTarget");
        lblOpenInThisTab = Translator.Instant("Enums.LinkTarget.OpenInThisTab");
        lblOpenInNewTab = Translator.Instant("Enums.LinkTarget.OpenInNewTab");
        lblOpenInSameTab = Translator.Instant("Enums.LinkTarget.OpenInSameTab");
        lblLanguage = Translator.Instant("Labels.Language");

        Language = new SystemSettingsService().Get().Language?.EmptyAsNull() ?? "en";
        Languages = Translator.GetLanguages().Select(x => new ListOption(){ Value = x.Value, Label = x.Key}).ToList();

        Themes = new ThemeService().GetThemes().Select(x => new ListOption()
        {
            Label = x,
            Value = x
        }).ToList();
        
        if (Uid == Guid.Empty)
        {
            // new item
            isNew = true;
            Model = new();
            Model.Name = "New Dashboard";
            var usedNames = new DashboardService().GetAllForUser(UserUid).Select(x => x.Name).ToArray();
            int count = 1;
            while (usedNames.Contains(Model.Name))
            {
                ++count;
                Model.Name = $"New Dashboard ({count})";
            }

            Model.AccentColor = "#ff0090";;
            Model.Theme = "Default";
        }
        else if (IsGuest)
        {
            isNew = false;
            Model = new DashboardService().GetGuestDashboard();
        }
        else
        {
            isNew = false;
            Model = new DashboardService().GetByUid(Uid);
            if (Model == null)
            {
                Router.NavigateTo("/settings/dashboards");
                return Task.CompletedTask;
            }
        }
        var sysGroups = new GroupService().GetSystemGroups(enabledOnly: true);
        Groups = new GroupService().GetAllForUser(UserUid).Where(x => Model.GroupUids.Contains(x.Uid))
            .Union(sysGroups.Where(x => Model.GroupUids.Contains(x.Uid))).ToList();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all the groups that are available to this dashboard
    /// </summary>
    /// <param name="notInUse">When true, will filter out any group that is currently in this dashboard,
    /// otherwise all groups will be returned</param>
    /// <returns> all the groups that are available to this dashboard</returns>
    List<Models.Group> GetAllAvailableGroups(bool notInUse = false)
    {
        var userGroups = new GroupService().GetAllForUser(UserUid);
        var sysGroups = new GroupService().GetSystemGroups(enabledOnly: true);

        var result = userGroups.Union(sysGroups);
        if (notInUse)
        {
            var used = Groups.Select(x => x.Uid);
            result = result.Where(x => used.Contains(x.Uid) == false);
        }
        return result.ToList();
    }
    
    void Save()
    {
        var service = new DashboardService();
        if (IsGuest)
        {
            var ssService = new SystemSettingsService();
            var system = ssService.Get();
            system.AllowGuest = Model.Enabled;
            system.Language = Language?.EmptyAsNull() ?? "en";
            system.Save();
            
            var existing = service.GetGuestDashboard();
            existing.Enabled = Model.Enabled;
            existing.ShowSearch = Model.ShowSearch;
            existing.Theme = Model.Theme;
            existing.LinkTarget = Model.LinkTarget;
            existing.AccentColor = Model.AccentColor;
            existing.BackgroundColor = Model.BackgroundColor;
            existing.GroupUids = Groups?.Select(x => x.Uid)?.ToList() ?? new ();
            service.Update(existing);
            ToastService.ShowSuccess(Translator.Instant("Labels.Saved"));
            return;
        }
        
        if (isNew)
        {
            Model.Uid = Guid.NewGuid();
            Model.UserUid = UserUid;
            Model.GroupUids = Groups.Select(x => x.Uid).ToList();
            Model.Background = "default.js";
            Model.Enabled = true;
            Model.AccentColor = Globals.DefaultAccentColor;
            Model.BackgroundColor = Globals.DefaultBackgroundColor;
            Model.LinkTarget = "_self";
            service.Add(Model);
        } 
        else
        {
            var existing = service.GetByUid(Uid);
            if (existing == null)
                return; // shouldnt happen
            existing.Name = Model.Name;
            existing.GroupUids = Groups?.Select(x => x.Uid)?.ToList() ?? new ();
            service.Update(existing);
        }
        this.Router.NavigateTo("/settings/dashboards");
    }

    void Cancel()
    {
        this.Router.NavigateTo("/settings/dashboards");
    }

    async Task AddGroup()
    {
        var available = GetAllAvailableGroups(notInUse: true);
        if (available.Any() == false)
        {
            ToastService.ShowWarning(Translator.Instant("Pages.Dashboard.Messages.NoAvailableGroups"));
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
            if (Groups.Any(x => x.Uid == uid))
                continue;
            var group = available.FirstOrDefault(x => x.Uid == uid);
            if (group == null)
                continue;
            Groups.Add(group);
        }
        Table.SetData(Groups);
    }

    protected override bool DoDelete(Models.Group item)
    {
        if (Groups.Contains(item) == false)
            return false;
        Groups.Remove(item);
        Table.SetData(Groups);
        return true;
    }
}