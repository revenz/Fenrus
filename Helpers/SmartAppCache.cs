namespace Fenrus.Helpers;

/// <summary>
/// Cache used to store smart app updates
/// </summary>
public static class SmartAppCache
{
    private readonly static Dictionary<Guid, SmartAppCacheItem> Items = new();

    /// <summary>
    /// Adds a cached app result
    /// </summary>
    /// <param name="uid">the UID of the app</param>
    /// <param name="success">if the app was successful</param>
    /// <param name="log">the log of the app update</param>
    /// <param name="response">the response of the request</param>
    public static void AddData(Guid uid, bool success, string log, string response)
    {
        lock (Items)
        {
            if (Items.ContainsKey(uid) == false)
                Items.Add(uid, new SmartAppCacheItem()
                {
                    Uid = uid
                });
        }
        Items[uid].AddData(success, log, response);
    }

    /// <summary>
    /// Gets the data if available
    /// </summary>
    /// <param name="uid">the UID of the smart App</param>
    /// <returns>the data</returns>
    public static List<SmartAppCacheItemData> GetData(Guid uid)
    {
        if (Items.ContainsKey(uid))
            return Items[uid].Data.ToList();
        return new();
    }
}

/// <summary>
/// A cached item
/// </summary>
public class SmartAppCacheItem
{
    /// <summary>
    /// Gets the UID of the smart app 
    /// </summary>
    public Guid Uid { get; init; }

    readonly Queue<SmartAppCacheItemData> _Data = new ();
    
    /// <summary>
    /// Gets the data of the item
    /// </summary>
    public Queue<SmartAppCacheItemData> Data => _Data;

    /// <summary>
    /// Adds a new Item
    /// </summary>
    /// <param name="success"></param>
    /// <param name="log"></param>
    /// <param name="response">the response of the request</param>
    public void AddData(bool success, string log, string response)
    {
        Data.Enqueue(new SmartAppCacheItemData()
        {
            Date = DateTime.UtcNow,
            Log = log ?? string.Empty,
            Success = success,
            Response = response ?? string.Empty
        });
        while (Data.Count > 10)
            Data.Dequeue();
    }
}

/// <summary>
/// An actual smart app update result
/// </summary>
public class SmartAppCacheItemData
{
    /// <summary>
    /// Gets the date of the update
    /// </summary>
    public DateTime Date { get; init; }
    /// <summary>
    /// Gets the log of the update
    /// </summary>
    public string Log { get; init; }
    /// <summary>
    /// Gets if the update was successful
    /// </summary>
    public bool Success { get; init; }
    /// <summary>
    /// Gets response of the request
    /// </summary>
    public string Response { get; init; }
}