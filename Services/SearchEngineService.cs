using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Search engine service
/// </summary>
public class SearchEngineService
{
    /// <summary>
    /// Gets all search engines in the system
    /// </summary>
    /// <returns>the search engines</returns>
    public List<SearchEngine> GetAll() => DbHelper.GetAll<SearchEngine>();
    
    /// <summary>
    /// Gets all system search engines for a user
    /// </summary>
    /// <returns>all system search engines</returns>
    public List<SearchEngine> GetAllSystem()
        => DbHelper.GetAllForUser<SearchEngine>(Guid.Empty);
    
    /// <summary>
    /// Gets all search engines for a user
    /// </summary>
    /// <param name="uid">The UID of the user</param>
    /// <returns>all search engines</returns>
    public List<SearchEngine> GetAllForUser(Guid uid)
        => DbHelper.GetAllForUser<SearchEngine>(uid);

    /// <summary>
    /// Gets a search engine by its UID
    /// </summary>
    /// <param name="uid">the UID of search engine</param>
    /// <returns>the search engine</returns>
    public SearchEngine GetByUid(Guid uid) => DbHelper.GetByUid<SearchEngine>(uid);

    /// <summary>
    /// Adds a new engine to the database
    /// </summary>
    /// <param name="engine">the engine being added</param>
    public void Add(SearchEngine engine)
    {
        if (engine.Uid == Guid.Empty)
            engine.Uid = Guid.NewGuid();
        DbHelper.Insert(engine);
    }

    /// <summary>
    /// Updates a engine int the database
    /// </summary>
    /// <param name="engine">the engine being updated</param>
    public void Update(SearchEngine engine)
        => DbHelper.Update(engine);

    /// <summary>
    /// Deletes an engine from the database
    /// </summary>
    /// <param name="uid">the UID of the engine to delete</param>
    public void Delete(Guid uid)
        => DbHelper.Delete<SearchEngine>(uid);
}