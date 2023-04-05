using Microsoft.ClearScript.JavaScript;

namespace Fenrus.Services;

/// <summary>
/// Service for reading/writing a users media
/// </summary>
public class MediaService
{
    /// <summary>
    /// The prefix in the database for all media files
    /// </summary>
    public const string MEDIA_PREFIX = "db:/media/"; 
    
    /// <summary>
    /// Gets all the UID for media for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <returns>all the media UIDs for the user</returns>
    public List<string> GetAll(Guid userUid)
    {
        var db = DbHelper.GetDb();
        string userMatch = MEDIA_PREFIX + userUid + "/";
        var results = db.FileStorage
            .Find(x => x.Id.StartsWith(userMatch))
            .Select(x => x.Id[(userMatch.Length)..]).ToList();
        return results;
    }

    /// <summary>
    /// Gets a media by its id
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the id of the file</param>
    /// <returns>the media</returns>
    public (Stream Data, string Filename, string MimeType)? GetByUid(Guid userUid, Guid uid)
    {
        var fileUid = GetFileUid(userUid, uid);
        var db = DbHelper.GetDb();
        var file = db.FileStorage.FindById(fileUid);
        if(file == null)
            return null;
        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        return (ms, file.Filename, file.MimeType);
    }
    
    /// <summary>
    /// Adds a media item to the database for the user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="filename">the filename of the uploaded file</param>
    /// <param name="data">the binary data of the media being added</param>
    /// <returns>the id of the newly created media</returns>
    public Guid? Add(Guid userUid, string filename, byte[] data)
    {
        if (userUid == Guid.Empty || data?.Any() == false || string.IsNullOrWhiteSpace(filename))
            return null;

        if (filename.IndexOf("/") > 0)
            filename = filename.Substring(filename.LastIndexOf("/") + 1);
        if (filename.IndexOf(".") < 0)
            throw new Exception("No extension provided");
        
        var db = DbHelper.GetDb();
        var uid = Guid.NewGuid();
        var fileUid = GetFileUid(userUid, uid);
        db.FileStorage.Upload(fileUid, filename, new MemoryStream(data));
        return uid;
    }
    
    /// <summary>
    /// Gets the files UID in the db
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the UID of the file</param>
    /// <returns> the files UID in the db</returns>
    private string GetFileUid(Guid userUid, Guid uid)
        => MEDIA_PREFIX + userUid + "/" + uid;

    /// <summary>
    /// Adds a media item to the database for the user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="uid">the UID of the file</param>
    /// <returns>true if the media no longer exists afterwards</returns>
    public bool Delete(Guid userUid, Guid uid)
    {
        var fileUid = GetFileUid(userUid, uid);
        var db = DbHelper.GetDb();
        var file = db.FileStorage.FindById(fileUid);
        if (file == null)
            return true; // it no longer exists
        db.FileStorage.Delete(fileUid);
        return true;
    }

}