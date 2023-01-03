using Newtonsoft.Json;

namespace Fenrus.Models;

/// <summary>
/// An app item
/// </summary>
public class AppItem:GroupItem
{
    /// <summary>
    /// Gets the type of the group item
    /// </summary>
    public override string Type => "DashboardApp";

    /// <summary>
    /// Gets or sets the app name
    /// </summary>
    public string AppName { get; set; }

    /// <summary>
    /// Gets or sets the Url of the link
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets teh target of the link
    /// </summary>
    public string Target { get; set; }
    
    /// <summary>
    /// Gets or sets the Uid of the docker server
    /// </summary>
    public Guid? DockerUid { get; set; }

    /// <summary>
    /// Gets or sets the name of the docker container
    /// </summary>
    public string DockerContainer { get; set; }

    /// <summary>
    /// Gets or sets the command to run in docker, eg bash, ash or sh
    /// </summary>
    public string DockerCommand { get; set; }

    /// <summary>
    /// Gets or sets the ssh server to connect to
    /// </summary>
    public string SshServer { get; set; }
    
    /// <summary>
    /// Gets or sets the ssh user name to connect with
    /// </summary>
    [JsonProperty("SshUsername")]
    public string SshUserName { get; set; }
    
    /// <summary>
    /// Gets or sets the ssh password to connect with
    /// </summary>
    public string SshPassword { get; set; }
}