namespace Fenrus.Models;

/// <summary>
/// A terminal item
/// </summary>
public class TerminalItem:GroupItem
{
    /// <summary>
    /// Gets the type of the group item
    /// </summary>
    public override string Type => "DashboardTerminal";

    /// <summary>
    /// Gets or sets the type of the terminal
    /// </summary>
    public TerminalType TerminalType { get; set; }

    /// <summary>
    /// Gets or sets the SSH Server for this item
    /// </summary>
    public string SshServer { get; set; }
    /// <summary>
    /// Gets or sets the SSH username for this item
    /// </summary>
    public string SshUserName { get; set; }
    /// <summary>
    /// Gets or sets the SSH password for this item
    /// </summary>
    public string SshPassword { get; set; }
    /// <summary>
    /// Gets or sets the original SSH password
    /// </summary>
    public string SshPasswordOriginal { get; set; }
    /// <summary>
    /// Gets or sets the Docker UID for this item
    /// </summary>
    public Guid? DockerUid { get; set; }
    /// <summary>
    /// Gets or sets the Docker container for this item
    /// </summary>
    public string DockerContainer { get; set; }
    /// <summary>
    /// Gets or sets the Docker command for this item
    /// </summary>
    public string DockerCommand { get; set; }

}

/// <summary>
/// Types of terminals
/// </summary>
public enum TerminalType
{
    /// <summary>
    /// An ssh terminal
    /// </summary>
    Ssh = 0,
    /// <summary>
    /// A docker terminal
    /// </summary>
    Docker = 1
}