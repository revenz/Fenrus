using Fenrus.Models;
using LiteDB;

namespace Fenrus.Upgrades;

/// <summary>
/// Upgrades to version 0.9.2
/// </summary>
public class Upgrade_0_9_2 : UpgradeScript
{
    protected override bool ShouldRun(Version previousVersion)
    {
        #if(DEBUG)
        return previousVersion < new Version(0, 9, 2);
        #else
        return previousVersion < new Version(0, 9, 2, 947);
        #endif
    }

    /// <summary>
    /// Gets tne name of the upgrade script
    /// </summary>
    protected override string Name => "0.9.2";

    /// <summary>
    /// Runs the upgrade
    /// </summary>
    protected override void Run()
    {
        var db = DbHelper.GetDb();
        var collection = db.GetCollection<SystemSettings>(nameof(SystemSettings));
        var firstItem = collection.FindOne(Query.All());
        if (firstItem == null)
        {
            Logger.ILog("System Settings not found, skipping upgrade");
            return;
        }

        firstItem.CloudFeatures = CloudFeature.Apps | CloudFeature.Calendar | CloudFeature.Notes | CloudFeature.Files;
        collection.Update(firstItem);
        Logger.ILog("Set System Cloud Features to: " + firstItem.CloudFeatures);

        var collUsers = db.GetCollection<UserProfile>();
        foreach (var p in collUsers.FindAll())
        {
            p.CloudFeatures = CloudFeature.Calendar | CloudFeature.Notes | CloudFeature.Files;
            collUsers.Update(p);
            Logger.ILog($"Set Cloud Features for user '{p.Uid}' to: " + p.CloudFeatures);
        }
    }
}