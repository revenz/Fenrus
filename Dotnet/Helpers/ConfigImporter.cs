using System.Text;
using Fenrus.Models;
using Fenrus.Services;

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
            link.Size = ParseSize(oldItem.Size);
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
        var settingsService = new UserSettingsService();
        var existing = service.GetAll().Select(x => x.Name.ToLowerInvariant()).ToArray();
        foreach (var old in users)
        {
            if (existing.Contains(old.Username.ToLowerInvariant()))
                continue;
            var se = new User();
            se.Name = old.Username;
            se.Uid = old.Uid != Guid.Empty ? old.Uid : Guid.NewGuid();
            se.IsAdmin = old.IsAdmin;
            se.Password = old.Password;
            
            service.Add(se);
            Log.AppendLine($"User '{se.Name}' imported");

            if (old.Config == null)
            {
                continue;
            }
            var settings = new UserSettings();
            settings.Uid = se.Uid;
            settings.Language = "en";

            settings.SearchEngines = old.Config.SearchEngines.Select(x =>
            {
                var se = new SearchEngine();
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
                return se;
            }).ToList();

            settings.Groups = old.Config.Groups.Select(x =>
            {
                var group = new Group();
                group.Enabled = x.Enabled;
                group.Name = x.Name;
                group.Uid = x.Uid != Guid.Empty ? x.Uid : Guid.NewGuid();
                group.Items = x.Items.Select(ParseGroupItem).Where(y => y != null).ToList();
                return group;
            }).ToList();

            settings.Dashboards = old.Config.Dashboards.Select(x =>
            {
                var db = new Dashboard();
                db.Name = x.Name;
                db.Enabled = x.Enabled;
                db.Uid = x.Uid != Guid.Empty ? x.Uid : Guid.NewGuid();
                db.AccentColor = x.AccentColor?.EmptyAsNull() ?? old.Config.AccentColor?.EmptyAsNull() ?? "#ff0090";
                db.Theme = old.Config.Theme?.EmptyAsNull() ?? "Default";
                db.Background = "default.js";
                db.BackgroundColor = "#009099";
                db.GroupUids = x.Groups.Select(x => x.Uid).ToList();
                db.LinkTarget = old.Config.LinkTarget?.EmptyAsNull() ?? "_self";
                db.ShowSearch = true;
                db.ShowGroupTitles = old.Config.ShowGroupTitles;
                db.ShowStatusIndicators = old.Config.ShowStatusIndicators;
                return db;
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
            return dict;
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