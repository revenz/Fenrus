using System.Text;
using System.Text.Json.Nodes;
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
    public async Task<object> Import(string json)
    {
        await Task.Delay(1);
        try
        {
            OldConfig config = JsonSerializer.Deserialize<OldConfig>(json);
            Console.WriteLine("Docker Servers: " + config.Docker.Count);
            Console.WriteLine("Search Engines: " + config.SearchEngines.Count);
            Console.WriteLine("System Groups: " + config.SystemGroups.Count);

            ImportDockers(config.Docker);
            ImportSearchEngines(config.SearchEngines);
            ImportSystemGroups(config.SystemGroups);
        }
        catch (Exception ex)
        {
            
        }

        return Log.ToString();
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
            se.Uid = old.Uid;
            
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
            se.Uid = old.Uid;
            se.Items = new();
            foreach (var oldItem in old.Items)
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
                    se.Items.Add(app);
                }
                else if (oldItem._Type == "DashboardLink")
                {
                    var link = new Models.LinkItem();
                    link.Size = ParseSize(oldItem.Size);
                    link.Uid = oldItem.Uid != Guid.Empty ? oldItem.Uid : Guid.NewGuid();
                    if (string.IsNullOrEmpty(oldItem.IconBase64) == false)
                        link.Icon = ImageHelper.SaveImageFromBase64(oldItem.IconBase64);
                    else
                        link.Icon = oldItem.Icon;
                    se.Items.Add(link);
                }
                else if (oldItem._Type == "DashboardGroup")
                {
                    
                }
            }
            
            service.Add(se);
            Log.AppendLine($"System Group '{se.Name}' imported");
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
        foreach (var key in dict.Keys)
        {
            var value = dict[key];
            if (value is JsonElement je)
            {
                if(je.ValueKind == JsonValueKind.String)
                    value = je.GetString();
            }

            dict[key] = value;
        }

        return dict;
    }

    private class OldConfig
    {
        public List<Models.DockerServer> Docker { get; set; }

        public List<OldSearchEngine> SearchEngines { get; set; }

        public List<ImportedGroup> SystemGroups { get; set; }
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
        public string Width { get; set; }
        public int Height { get; set; }
        public string _Type { get; set; }
        public bool IsSystem { get; set; }

        public List<ImportedGroupItem> Items { get; set; }
    }

    public class ImportedGroupItem
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
}