using System.Text;
using Fenrus.Models;

namespace Fenrus.Helpers;

public class ConfigImporter
{
    private StringBuilder Log = new();
    /// <summary>
    /// Imports configuration
    /// </summary>
    /// <param name="json">the raw JSON of the ocnfiguration to import</param>
    public async Task<string> Import(string json)
    {
        await Task.Delay(1);
        try
        {
            OldConfig config = JsonSerializer.Deserialize<OldConfig>(json);
            Console.WriteLine("Docker Servers: " + config.Docker.Count);
            Console.WriteLine("Search Engines: " + config.SearchEngines.Count);
            Console.WriteLine("System Groups: " + config.SystemGroups.Count);
            Console.WriteLine("Users: " + config.Users.Count);

            ImportDockers(config.Docker);
            ImportSearchEngines(config.SearchEngines);
            ImportSystemGroups(config.SystemGroups);
            ImportUsers(config.Users);
        }
        catch (Exception ex)
        {
            Log.AppendLine("Error parsing config: " + ex.Message);
            #if(DEBUG)
            Log.AppendLine(ex.StackTrace);
            #endif
        }

        var fullLog = Log.ToString();
        Console.WriteLine(fullLog);
        return fullLog;
    }

    private void ImportDockers(List<DockerServer> dockers)
    {
        if (dockers?.Any() != true)
            return;
        Log.AppendLine("Docker Servers: " + dockers.Count);
        var service = new DockerService();
        var existing = service.GetAll().Select(x => x.Name.ToLowerInvariant()).ToArray();
        foreach (var docker in dockers)
        {
            if (existing.Contains(docker.Name.ToLowerInvariant()))
                continue;
            service.Add(docker);
            Log.AppendLine($"Docker '{docker.Name}' imported");
        }
    }
    
    private void ImportSearchEngines(List<OldSearchEngine> searchEngines)
    {
        if (searchEngines?.Any() != true)
            return;
        Log.AppendLine("Search Engines: " + searchEngines.Count);
        var service = new SearchEngineService();
        var existing = service.GetAll().Select(x => x.Name.ToLowerInvariant()).ToArray();
        foreach (var old in searchEngines)
        {
            if (existing.Contains(old.Name.ToLowerInvariant()))
                continue;
            var se = new SearchEngine();
            se.Name = old.Name;
            se.Enabled = old.Enabled;
            se.Url = old.Url;
            se.Shortcut = old.Shortcut;
            se.IsDefault = old.IsDefault;
            se.IsSystem = true;
            se.Uid = old.Uid != Guid.Empty ? old.Uid : Guid.NewGuid();
            
            if (string.IsNullOrWhiteSpace(old.IconBase64) == false)
                se.Icon = ImageHelper.SaveImageFromBase64(old.IconBase64);
            
            service.Add(se);
            Log.AppendLine($"Search Engine '{se.Name}' imported");
        }
    }

    private void ImportSystemGroups(List<ImportedGroup> groups)
    {
        if (groups?.Any() != true)
            return;
        Log.AppendLine("System Groups: " + groups.Count);
        var service = new GroupService();
        var existing = service.GetSystemGroups(enabledOnly: false).Select(x => x.Name.ToLowerInvariant()).ToArray();
        foreach (var old in groups)
        {
            if (existing.Contains(old.Name.ToLowerInvariant()))
                continue;
            var se = new Models.Group();
            se.Name = old.Name;
            se.Enabled = old.Enabled;
            se.IsSystem = true;
            se.Uid = old.Uid != Guid.Empty ? old.Uid : Guid.NewGuid();
            se.Items = new();
            foreach (var oldItem in old.Items)
            {
                var groupItem = ParseGroupItem(oldItem);
                if (groupItem != null)
                    se.Items.Add(groupItem);
            }
            
            service.Add(se);
            Log.AppendLine($"System Group '{se.Name}' imported");
        }
    }

    private GroupItem ParseGroupItem(ImportedGroupItem oldItem)
    {
        if (oldItem._Type == "DashboardApp")
        {
            var app = new Models.AppItem();
            app.AppName = oldItem.AppName;
            app.Properties = ProcessDictionary(oldItem.Properties ?? new ());
            app.Url = oldItem.Url;
            app.Name = oldItem.Name;
            app.Uid = oldItem.Uid != Guid.Empty ? oldItem.Uid : Guid.NewGuid();
            app.AppName = oldItem.AppName;
            app.ApiUrl = oldItem.ApiUrl;
            app.DockerCommand = oldItem.DockerCommand;
            app.DockerContainer = oldItem.DockerContainer;
            app.DockerCommand = oldItem.DockerCommand;
            app.Monitor = true;
            if (string.IsNullOrEmpty(oldItem.IconBase64) == false)
                app.Icon = ImageHelper.SaveImageFromBase64(oldItem.IconBase64);
            else
                app.Icon = oldItem.Icon;
            app.Size = ParseSize(oldItem.Size);
            return app;
        }
        
        if (oldItem._Type == "DashboardLink")
        {
            var link = new Models.LinkItem();
            link.Name = oldItem.Name;
            link.Size = ParseSize(oldItem.Size);
            link.Url = oldItem.Url;
            link.Monitor = false;
            link.Uid = oldItem.Uid != Guid.Empty ? oldItem.Uid : Guid.NewGuid();
            if (string.IsNullOrEmpty(oldItem.IconBase64) == false)
                link.Icon = ImageHelper.SaveImageFromBase64(oldItem.IconBase64);
            else
                link.Icon = oldItem.Icon;
            return link;
        }
        if (oldItem._Type == "DashboardGroup")
        {
                    
        }

        return null;
    }
    
    
    private void ImportUsers(List<ImportedUser> users)
    {
        if (users?.Any() != true)
            return;
        Log.AppendLine("Users: " + users.Count);
        var service = new UserService();
        var searchEngineService = new SearchEngineService();
        var dashboardService = new DashboardService();
        var groupService = new GroupService();
        var settingsService = new UserSettingsService();
        var existingUers = service.GetAll().ToDictionary(x => x.Name.ToLowerInvariant(), x => x);
        var systemGroupUids = groupService.GetSystemGroups().Select(x => x.Uid).ToList();
        foreach (var old in users)
        {
            User user;
            string key = old.Username.ToLowerInvariant();
            var existingSearchEngines = new List<SearchEngine>();
            var existingGroups = new List<Group>();
            var existingDashboards = new List<Dashboard>();
            if (existingUers.ContainsKey(key))
            {
                user = existingUers[key];
                existingSearchEngines = new SearchEngineService().GetAllForUser(user.Uid);
                existingGroups = new GroupService().GetAllForUser(user.Uid);
                existingDashboards = new DashboardService().GetAllForUser(user.Uid);
            }
            else
            {
                user = new User();
                user.Name = old.Username;
                user.Uid = old.Uid != Guid.Empty ? old.Uid : Guid.NewGuid();
                user.IsAdmin = old.IsAdmin;
                user.Password = old.Password;

                service.Add(user);
                Log.AppendLine($"User '{user.Name}' imported");
            }

            if (old.Config == null)
            {
                continue;
            }
            var settings = new UserSettings();
            settings.Uid = user.Uid;
            settings.Language = "en";

            settings.SearchEngineUids = old.Config.SearchEngines.Select(x =>
            {
                var existing = existingSearchEngines.FirstOrDefault(y => x.Uid == y.Uid || x.Name == y.Name);
                if (existing != null)
                    return existing.Uid;
                
                var se = new SearchEngine();
                se.UserUid = user.Uid;
                if (string.IsNullOrEmpty(x.IconBase64) == false)
                    se.Icon = ImageHelper.SaveImageFromBase64(x.IconBase64);
                else
                    se.Icon = x.Icon;
                se.IsDefault = x.IsDefault;
                se.Shortcut = x.Shortcut;
                se.Enabled = x.Enabled;
                se.Url = x.Url;
                se.Name = x.Name;
                se.Uid = x.Uid != Guid.Empty ? x.Uid : Guid.NewGuid();
                searchEngineService.Add(se);
                return se.Uid;
            }).ToList();

            settings.GroupUids = old.Config.Groups.Select(x =>
            {
                // only search for UID match here because dashboards reference by UIDs and if they UIDs dont match
                // the item cannot be seen on the dashboard
                var existing = existingGroups.FirstOrDefault(y => x.Uid == y.Uid);
                if (existing != null)
                {
                    existing.Items = x.Items.Select(ParseGroupItem).Where(y => y != null).ToList();
                    groupService.Update(existing);
                    return existing.Uid;
                }

                var group = new Group();
                group.UserUid = user.Uid;
                group.Enabled = x.Enabled;
                group.Name = x.Name;
                group.Uid = x.Uid != Guid.Empty ? x.Uid : Guid.NewGuid();
                group.Items = x.Items.Select(ParseGroupItem).Where(y => y != null).ToList();
                groupService.Add(group);
                return group.Uid;
            }).ToList();

            var allowGroupUids = systemGroupUids.Union(settings.GroupUids).ToList();

            var defaultDashboard = old.Config.Dashboards.OrderBy(x => x.Enabled).ThenBy(x => x.Name.Contains("Default"))
                .ThenBy(x => x.Name).FirstOrDefault();
            
            settings.DashboardUids = old.Config.Dashboards.Select(x =>
            {
                var existing = existingDashboards.FirstOrDefault(y => x.Uid == y.Uid || x.Name == y.Name);
                var groupUids = x.Groups.Select(x => x.Uid).Where(x => allowGroupUids.Contains(x)).ToList();
                if (existing != null)
                {
                    existing.GroupUids = groupUids;
                    dashboardService.Update(existing);
                    return existing.Uid;
                }
                var db = new Dashboard();
                db.UserUid = user.Uid;
                db.Name = x.Name;
                db.Enabled = x.Enabled;
                db.IsDefault = x == defaultDashboard;
                db.Uid = x.Uid != Guid.Empty ? x.Uid : Guid.NewGuid();
                db.AccentColor = x.AccentColor?.EmptyAsNull() ?? old.Config.AccentColor?.EmptyAsNull() ?? Globals.DefaultAccentColor;
                db.Theme = old.Config.Theme?.EmptyAsNull() ?? "Default";
                db.Background = "default.js";
                db.BackgroundColor = Globals.DefaultBackgroundColor;
                db.GroupUids = groupUids;
                db.LinkTarget = old.Config.LinkTarget?.EmptyAsNull() ?? "_self";
                db.ShowSearch = true;
                db.ShowGroupTitles = old.Config.ShowGroupTitles;
                db.ShowStatusIndicators = old.Config.ShowStatusIndicators;
                dashboardService.Add(db);
                return db.Uid;
            }).ToList();
            
            settingsService.Add(settings);
        }
    }

    private ItemSize ParseSize(string size)
    {
        switch (size)
        {
            case "small":
                return ItemSize.Small;
            case "medium":
                return ItemSize.Medium;
            case "large":
                return ItemSize.Large;
            case "larger":
                return ItemSize.Larger;
            case "x-large":
                return ItemSize.XLarge;
            case "xx-large":
                return ItemSize.XXLarge;
            default:
                return ItemSize.Medium;
            
        }
    }

    private Dictionary<string, object> ProcessDictionary(Dictionary<string, object> dict)
    {
        if (dict?.Any() != true)
            return dict ?? new ();
        var toRemove = new List<string>();
        foreach (var key in dict.Keys)
        {
            var value = dict[key];
            if (value is JsonElement je)
            {
                if(je.ValueKind == JsonValueKind.String)
                    value = je.GetString();
                else if(je.ValueKind == JsonValueKind.Number)
                    value = je.GetInt32();
                else if (je.ValueKind == JsonValueKind.Null)
                    value = null;
                else if (je.ValueKind == JsonValueKind.False)
                    value = false;
                else if (je.ValueKind == JsonValueKind.True)
                    value = true;
                else if (je.ValueKind == JsonValueKind.Undefined)
                    value = null;
                else
                {
                    Log.AppendLine($"Invalid dictionary value '{key}'");
                    toRemove.Add(key);
                }
                
            }

            dict[key] = value;
        }

        foreach (var key in toRemove)
            dict.Remove(key);
        
        return dict;
    }

    private class OldConfig
    {
        public List<Models.DockerServer> Docker { get; set; }

        public List<OldSearchEngine> SearchEngines { get; set; }

        public List<ImportedGroup> SystemGroups { get; set; }
        
        public List<ImportedUser> Users { get; set; }
    }

    private class OldSearchEngine
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Shortcut { get; set; }
        public string Icon { get; set; }
        public bool Enabled { get; set; }
        public bool IsDefault { get; set; }
        public string IconBase64 { get; set; }
    }

    private class ImportedGroup
    {
        public Guid Uid { get; set; }
        public bool Enabled { get; set; }
        public string Name { get; set; }
        // public string Width { get; set; }
        // public string Height { get; set; }
        public string _Type { get; set; }
        public bool IsSystem { get; set; }

        public List<ImportedGroupItem> Items { get; set; }
    }

    private class ImportedGroupItem
    {
        public Guid Uid { get; set; }
        public string _Type { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string IconBase64 { get; set; }
        public string Size { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public string AppName { get; set; }
        public string ApiUrl { get; set; }
        public string DockerUid { get; set; }
        public string DockerContainer { get; set; }
        public string DockerCommand { get; set; }

    }

    private class ImportedUser
    {
        public Guid Uid { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public ImportedUserConfig Config { get; set; }
    }

    private class ImportedUserConfig
    {
        public string LinkTarget { get; set; }
        public string Theme { get; set; }
        public string AccentColor { get; set; }
        public bool ShowGroupTitles { get; set; }
        public bool CollapseMenu { get; set; }
        public string BackgroundImage { get; set; }
        public bool ShowStatusIndicators { get; set; }
        public Dictionary<string, object> ThemeSettings { get; set; }
        public List<OldSearchEngine> SearchEngines { get; set; }
        public List<ImportedDashboard> Dashboards { get; set; }
        public List<ImportedGroup> Groups { get; set; }
    }

    private class ImportedDashboard
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public List<ImportedDashboardGroup> Groups { get; set; }
        public bool Enabled { get; set; }
        public string AccentColor { get; set; }
        public string BackgroundImage { get; set; }
    }

    private class ImportedDashboardGroup
    {
        public Guid Uid { get; set; }
        public string Name { get; set; }
        public bool IsSystem { get; set; }
        public bool Enabled { get; set; }
    }
}