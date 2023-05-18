namespace Fenrus.Upgrades;

/// <summary>
/// An upgrade script
/// </summary>
public abstract class UpgradeScript
{
    /// <summary>
    /// Executes the upgrade
    /// </summary>
    /// <param name="previousVersion">the previous version</param>
    public void Execute(Version previousVersion)
    {
        if (ShouldRun(previousVersion) == false)
            return;
        try
        {
            Logger.ILog($"Running upgrade {Name}");
            Run();
        }
        catch (Exception ex)
        {
            Logger.WLog($"Failed running upgrade {Name}: " + ex.Message);
        }
    }

    /// <summary>
    /// Checks if an upgrade script should run
    /// </summary>
    /// <param name="previousVersion">the previous version</param>
    /// <returns>true if should run, otherwise false</returns>
    protected abstract bool ShouldRun(Version previousVersion);

    /// <summary>
    /// Gets tne name of the upgrade script
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    /// Runs the upgrade
    /// </summary>
    protected abstract void Run();
}