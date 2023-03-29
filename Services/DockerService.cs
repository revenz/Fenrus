using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service used to manage docker servers
/// </summary>
public class DockerService
{
    /// <summary>
    /// Gets a docker server by its UID
    /// </summary>
    /// <param name="uid">The UID of the docker server</param>
    /// <returns>the docker server</returns>
    public DockerServer GetByUid(Guid uid)
        => DbHelper.GetByUid<DockerServer>(uid);
    
    /// <summary>
    /// Gets all the docker servers in the system
    /// </summary>
    /// <returns>all the docker servers in the system</returns>
    public List<DockerServer> GetAll()
        => DbHelper.GetAll<DockerServer>();
    
    /// <summary>
    /// Gets all the docker servers for a user
    /// </summary>
    /// <param name="uid">The users UID</param>
    /// <returns>a list of docker servers</returns>
    public List<DockerServer> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<DockerServer>(uid);

    /// <summary>
    /// Saves a docker server to the database
    /// </summary>
    /// <param name="server">the server to save</param>
    public void Save(DockerServer server)
    {
        DockerServer? existing = null;
        if (server.Uid != Guid.Empty)
            existing = DbHelper.GetByUid<DockerServer>(server.Uid);

        if (existing != null)
        {
            DbHelper.Update(server);
        }
        else
        {
            if (server.Uid == Guid.Empty)
                server.Uid = Guid.NewGuid();
            DbHelper.Insert(server);
        }
    }

    /// <summary>
    /// Adds a docker to the database
    /// </summary>
    /// <param name="server">the docker server</param>
    public void Add(DockerServer server)
    {
        if(server.Uid == Guid.Empty)
            server.Uid = Guid.NewGuid();
        DbHelper.Insert(server);
    }

    /// <summary>
    /// Deletes a docker server from the system
    /// </summary>
    /// <param name="uid">the UID of the server to delete</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<DockerServer>(uid);
    
}