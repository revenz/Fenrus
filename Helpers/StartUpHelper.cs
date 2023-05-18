using LiteDB;

namespace Fenrus.Helpers;

/// <summary>
/// Helper that runs at startup 
/// </summary>
public static class StartUpHelper
{
    /// <summary>
    /// Runs any startup actions
    /// </summary>
    public static void Run()
    {
        Logger.Initialize();
        InitializeDataDirectory();
        AppService.Initialize();
        InitializeDatabase();
    }

    /// <summary>
    /// Initializes the data directory and the encryption key
    /// </summary>
    private static void InitializeDataDirectory()
    {
        var dataDir = DirectoryHelper.GetDataDirectory();
        if (Directory.Exists(dataDir) == false)
            Directory.CreateDirectory(dataDir);

        EncryptionHelper.Init(dataDir);
    }

    /// <summary>
    /// Initializes the database
    /// </summary>
    private static void InitializeDatabase()
    {
        // future proofing, saving the database version here so we can run upgrade scripts if needed
        var db = DbHelper.GetDb();
        var coll = db.GetCollection<Fenrus>(nameof(Fenrus));
        var firstItem = coll.FindOne(Query.All());
        if (Version.TryParse(firstItem?.Version ?? string.Empty, out Version previous))
        {
            Upgrade(previous);
        }
        coll.DeleteAll();
        var version = new Fenrus { Version = Globals.Version };
        coll.Insert(version);
    }

    /// <summary>
    /// Basic fenrus information object
    /// </summary>
    private class Fenrus
    {
        /// <summary>
        /// Gets or sets the version of Fenrus
        /// </summary>
        public string Version { get; set; }
    }


    /// <summary>
    /// Runs upgrades from previous versions
    /// </summary>
    /// <param name="previousVersion">the previous version number</param>
    private static void Upgrade(Version previousVersion)
    {
        if (previousVersion < new Version(0, 9, 2))
            new Upgrades.Upgrade_0_9_2().Execute();
    }
}