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
}