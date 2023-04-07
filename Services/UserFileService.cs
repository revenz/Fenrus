using System.Text.RegularExpressions;
using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for user files
/// </summary>
public class UserFilesService
{
    /// <summary>
    /// The prefix in the database for all files files
    /// </summary>
    public const string FILES_PREFIX = "db:/user-file/"; 
    
    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="folder">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    public List<UserFile> GetAll(Guid userUid, string folder)
    {
        var db = DbHelper.GetDb();
        string userMatch = FILES_PREFIX + userUid + "/";
        string folderMatch = userMatch + (folder ?? string.Empty);
        if (folderMatch.EndsWith("/") == false)
            folderMatch += "/";
        var results = db.FileStorage
            .Find(x => x.Id.StartsWith(userMatch))
            .Where(x =>
            {
                if (x.Id.StartsWith(folderMatch) == false)
                    return false;
                var part = x.Id.Substring(folderMatch.Length);
                if (part == ".")
                    return false;
                if (part.IndexOf("/") < 0)
                    return true;
                // check if its a folder
                int slashCount = part.Count(f => f == '/');
                return slashCount == 1 && x.Filename == ".";
            })
            .Select(x =>
            {
                var uf = new UserFile();
                uf.FullPath = x.Id.Substring(userMatch.Length);
                uf.Created = x.UploadDate;
                if (uf.FullPath.EndsWith("/."))
                {
                    uf.Name = uf.FullPath.Substring(0, uf.FullPath.LastIndexOf("/"));
                    uf.Name = uf.Name.Substring(uf.Name.LastIndexOf("/") + 1);
                    uf.MimeType = "folder";
                }
                else
                {
                    uf.MimeType = x.MimeType;
                    uf.Name = uf.FullPath.Substring(uf.FullPath.LastIndexOf("/") + 1);
                    if (uf.Name.LastIndexOf(".") > 0)
                        uf.Extension = uf.Name.Substring(uf.Name.LastIndexOf(".") + 1);
                    uf.Size = x.Length;
                }

                return uf;
            })
            .OrderBy(x => x.MimeType == "folder" ? 1 :2)
            .ThenBy(x => x.Name).ToList();
        return results;
    }

    /// <summary>
    /// Gets the file data by its path
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    public (Stream Data, string Filename, string MimeType)? GetFileData(Guid userUid, string fullPath)
    {
        var fileUid = GetFileUid(userUid, fullPath);
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
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="data">the binary data of the file being added</param>
    /// <returns>the id of the newly created file</returns>
    public UserFile? Add(Guid userUid, string path, string filename, byte[] data)
    {
        if (userUid == Guid.Empty || data?.Any() == false || string.IsNullOrWhiteSpace(filename))
            return null;

        path ??= string.Empty;
        if (path.Length > 0 && path.EndsWith("/") == false)
            path += "/";

        var db = DbHelper.GetDb();
        var fileUid = GetFileUid(userUid, path + filename);
        string extension = filename.LastIndexOf(".") >= 0
            ? filename.Substring(filename.LastIndexOf(".") + 1)
            : string.Empty;

        string userPrefix = FILES_PREFIX + userUid + "/";
        var existingFiles = db.FileStorage.Find(x => x.Id.StartsWith(userPrefix)).Select(x => x.Id).ToList();
        if (existingFiles.Contains(fileUid))
        {
            string originalFileName = filename.LastIndexOf(".") > 0 ? filename.Substring(0, filename.LastIndexOf(".")) : filename;
            int count = 1;
            while (existingFiles.Contains(fileUid))
            {
                filename = originalFileName + "(" + (++count) + ")" + (extension == string.Empty ? string.Empty : "." + extension);
                fileUid = GetFileUid(userUid, path + filename);
            }
        }

        var result = db.FileStorage.Upload(fileUid, filename, new MemoryStream(data));
        return new UserFile()
        {
            Created = result.UploadDate,
            Extension = extension,
            Name = filename,
            FullPath = path + filename,
            MimeType = result.MimeType
        };
    }
    
    /// <summary>
    /// Gets the file UID in the db
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns> the file UID in the db</returns>
    private string GetFileUid(Guid userUid, string fullPath)
        => FILES_PREFIX + userUid + "/" + fullPath;

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    public bool Delete(Guid userUid, string fullPath)
    {
        var fileUid = GetFileUid(userUid, fullPath);
        var db = DbHelper.GetDb();
        var file = db.FileStorage.FindById(fileUid);
        if (file == null)
            return true; // it no longer exists

        if (file.Filename == ".")
        {
            // its a directory, we have to then delete all sub files too!
            var path = file.Id[..^1];
            db.BeginTrans();
            var subs = db.FileStorage.Find(x => x.Id != file.Id && x.Id.StartsWith(path)).ToList();
            foreach (var sub in subs)
            {
                db.FileStorage.Delete(sub.Id);
            }
            db.FileStorage.Delete(file.Id);
            db.Commit();
            return true;
        }
        
        db.FileStorage.Delete(fileUid);
        return true;
    }

    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the full path of the file</param>
    public void CreateFolder(Guid userUid, string path)
    {
        if (path.EndsWith("/"))
            path = path[..^1];
        
        var fileUid = GetFileUid(userUid, path +"/.");
        var db = DbHelper.GetDb();
        db.FileStorage.Upload(fileUid, ".", new MemoryStream(new byte[] { }));
    }
}