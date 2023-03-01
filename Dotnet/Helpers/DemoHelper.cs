using Fenrus.Models;

namespace Fenrus.Helpers;

/// <summary>
/// Helper for demoing Fenrus
/// </summary>
public class DemoHelper
{
    private static readonly Guid dockerUid = new Guid("cf4e0df5-f3a4-47c9-b69d-4e4327c88a99");
    
    /// <summary>
    /// Gets a demo dashboard
    /// </summary>
    /// <returns>a demo dashboard</returns>
    public static Dashboard GetDemoDashboard()
    {
        var db = new Dashboard();
        db.Name = "Demo Dashboard";
        db.Enabled = true;
        db.Theme = "Default";
        db.AccentColor = "#ff0090";
        db.Uid = new Guid("12db295d-6360-459a-8d24-383b55b75b47");
        db.Groups.Add(FenrusGroup());
        return db;
    }

    /// <summary>
    /// Gets a demo users settings
    /// </summary>
    /// <returns>a demo users settings</returns>
    public static UserSettings GetDemoUserSettings()
    {
        var settings = new UserSettings()
        {
            Uid = new Guid("40e5b3f9-be91-48aa-aa57-805f2de08869")
        };
        settings.Dashboards.Add(GetDemoDashboard());
        settings.Docker.Add(GetDemoDockerServer());
        settings.Groups = settings.Dashboards[0].Groups;

        settings.SearchEngines.Add(new()
        {
            Uid = new Guid("57e26485-0f3f-4ed7-9a2d-52f2accd4336"),
            Icon = "/search-engines/google.png",
            Name = "Google",
            Shortcut = "g",
            Enabled = true,
            IsDefault = true,
            Url = "https://google.co.nz/q=%s"
        });
        
        return settings;
    }
    
    /// <summary>
    /// Gets a demo docker server.
    /// Note: the demo docker server needs to be a real system and is resolving to 192.168.1.10
    /// </summary>
    /// <returns></returns>
    public static DockerServer GetDemoDockerServer()
    {
        var docker = new DockerServer();
        docker.Address = "192.168.1.10";
        docker.Name = "FileFlows";
        docker.Uid = dockerUid;
        return docker;
    }

    /// <summary>
    /// Basic Fenrus Group
    /// </summary>
    /// <returns>Fenrus Group</returns>
    public static Group FenrusGroup()
    {
        var group = new Group();
        group.Name = "Fenrus";
        group.Enabled = true;
        group.Uid = new Guid("aa163b32-b9d7-4a1f-8737-e170091723e5");
        group.Items.Add(new AppItem()
        {
            Uid = new Guid("82b52581-9005-4766-a27f-36dce7c8661e"),
            Enabled = true,
            Name = "GitHub - Fenrus",
            Url = "https://github.com/revenz/Fenrus",
            Size = ItemSize.Large,
            AppName = "GitHub",
            DockerContainer = "FileFlows",
            DockerUid = dockerUid
        });
        group.Items.Add(new AppItem()
        {
            Uid = new Guid("49049f51-3a83-4fbe-8160-f03a57c0527d"),
            Enabled = true,
            Name = "Reddit - Fenrus",
            Url = "https://reddit.com/r/Fenrus",
            Size = ItemSize.Medium,
            AppName = "Reddit",
            SshServer = "192.168.1.10"
        });
        group.Items.Add(new AppItem()
        {
            Uid = new Guid("4c571f69-6664-4965-912d-3100807c7b4d"),
            Enabled = true,
            Name = "Discord - Fenrus",
            Url = "https://discord.gg/xbYK8wFMeU",
            Size = ItemSize.Medium,
            AppName = "Discord"
        });
        group.Items.Add(new AppItem()
        {
            Uid = new Guid("6947269d-0480-4ddd-a47e-c1fd07e7197d"),
            Enabled = true,
            Name = "Patreon",
            Url = "https://www.patreon.com/revenz",
            Size = ItemSize.Medium,
            AppName = "Patreon"
        });
        return group;
    }
}