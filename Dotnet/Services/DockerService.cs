using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service used to manage docker servers
/// </summary>
public class DockerService
{
    /// <summary>
    /// Gets all the docker servers in the system
    /// </summary>
    /// <returns>all the docker servers in the system</returns>
    public List<DockerServer> GetAll()
        => DbHelper.GetAll<DockerServer>();

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
    /// Deletes a docker server from the system
    /// </summary>
    /// <param name="uid">the UID of the server to delete</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<DockerServer>(uid);
    
}