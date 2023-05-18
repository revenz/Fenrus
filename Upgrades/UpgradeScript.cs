namespace Fenrus.Upgrades;

/// <summary>
/// An upgrade script
/// </summary>
public abstract class UpgradeScript
{
    /// <summary>
    /// Executes the upgrade
    /// </summary>
    public void Execute()
    {
        try
        {
            Run();
        }
        catch (Exception ex)
        {
            Logger.WLog($"Failed running upgrade {Name}: " + ex.Message);
        }
    }

    /// <summary>
    /// Gets tne name of the upgrade script
    /// </summary>
    protected abstract string Name { get; }

    /// <summary>
    /// Runs the upgrade
    /// </summary>
    protected abstract void Run();
}