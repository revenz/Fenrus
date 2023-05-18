using Fenrus.Models;
using LiteDB;

namespace Fenrus.Upgrades;

/// <summary>
/// Upgrades to version 0.9.2
/// </summary>
public class Upgrade_0_9_2 : UpgradeScript
{
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
            return;
        firstItem.CloudFeatures = CloudFeature.Apps | CloudFeature.Calendar | CloudFeature.Notes | CloudFeature.Files;
        collection.Update(firstItem);

        var collUsers = db.GetCollection<UserProfile>();
        foreach (var p in collUsers.FindAll())
        {
            p.CloudFeatures = CloudFeature.Calendar | CloudFeature.Notes | CloudFeature.Files;
            collUsers.Update(p);
        }
    }
}