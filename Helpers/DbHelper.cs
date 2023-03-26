using Fenrus.Models;
using LiteDB;

namespace Fenrus.Helpers;

/// <summary>
/// Helper for the database
/// </summary>
public class DbHelper
{
    internal static readonly string DbFile = Path.Combine(DirectoryHelper.GetDataDirectory(), "Fenrus.db");

    // static DbHelper()
    // {
    //     var mapper = BsonMapper.Global;
    //
    //     mapper.Entity<Models.Dashboard>()
    //         .Ignore(x => x.Groups); // ignore this property (do not store)
    // }

    /// <summary>
    /// Gets the database 
    /// </summary>
    /// <returns>the database</returns>
    internal static LiteDatabase GetDb()
        => new LiteDatabase(DbFile);

    /// <summary>
    /// Gets the first or default of type
    /// </summary>
    /// <typeparam name="T">the type to get</typeparam>
    /// <returns>the first or default</returns>
    internal static T FirstOrDefault<T>()
    {
        using var db = GetDb();
        return db.GetCollection<T>(typeof(T).Name).Query().FirstOrDefault();
    }

    /// <summary>
    /// Gets an item by its UID
    /// </summary>
    /// <param name="uid">the UID of the item</param>
    /// <typeparam name="T">the type to get</typeparam>
    /// <returns>the item in the database</returns>
    internal static T GetByUid<T>(Guid uid) where T: IModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.EnsureIndex(x => x.Uid);
        var item = collection.Query().Where(x => x.Uid == uid)
            .FirstOrDefault();
        return item;
    }

    /// <summary>
    /// Gets an item by its name
    /// </summary>
    /// <param name="name">the name of the item</param>
    /// <typeparam name="T">the type to get</typeparam>
    /// <returns>the item in the database</returns>
    public static T GetByName<T>(string name) where T: IModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.EnsureIndex(x => x.Name);
        var item = collection.Query().Where(x => x.Name == name)
            .FirstOrDefault();
        return item;
    }

    /// <summary>
    /// Inserts an item in the database
    /// </summary>
    /// <param name="item">the item to insert</param>
    /// <typeparam name="T">the type being inserted</typeparam>
    internal static void Insert<T>(T item) where T : IModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        if(collection.Exists(x => x.Uid == item.Uid))
            collection.Update(item);
        else
        {
            if(item.Uid == Guid.Empty)
                item.Uid = Guid.NewGuid();
            collection.Insert(item.Uid, item);
        }
    }
    
    /// <summary>
    /// Inserts a basic item in the database
    /// </summary>
    /// <param name="item">the item to insert</param>
    /// <typeparam name="T">the type being inserted</typeparam>
    internal static void InsertBasic<T>(T item)
    {
        if (item == null)
            return;
        
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.Insert(item);
    }

    /// <summary>
    /// Updates an item in the database
    /// </summary>
    /// <param name="item">the item to update</param>
    /// <typeparam name="T">the type being updated</typeparam>
    internal static void Update<T>(T item)
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.Update(item);
    }

    /// <summary>
    /// Deletes an item from the database
    /// </summary>
    /// <param name="uid">the Uid of the item being deleted</param>
    /// <typeparam name="T">the type being deleted</typeparam>
    public static void Delete<T>(Guid uid) where T : IModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.Delete(uid);
    }

    /// <summary>
    /// Gets all items in the database
    /// </summary>
    /// <typeparam name="T">the type of items to get</typeparam>
    /// <returns>a list of all the items</returns>
    public static List<T> GetAll<T>()
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        return collection.Query().ToList();
    }

    /// <summary>
    /// Gets all items in the database for a user
    /// </summary>
    /// <typeparam name="T">the type of items to get</typeparam>
    /// <param name="uid">the users UID</param>
    /// <returns>a list of all the items</returns>
    public static List<T> GetAllForUser<T>(Guid uid) where T : IUserModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        return collection.Query().Where(x => x.UserUid == uid).ToList();
    }

    /// <summary>
    /// Deletes all items in the database for a user
    /// </summary>
    /// <typeparam name="T">the type of items to get</typeparam>
    /// <param name="uid">the users UID</param>
    public static void DeleteAllForUser<T>(Guid uid) where T : IUserModal
    {
        using var db = GetDb();
        var collection = db.GetCollection<T>(typeof(T).Name);
        collection.DeleteMany(x => x.UserUid == uid);
    }
}