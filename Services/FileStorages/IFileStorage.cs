using Fenrus.Models;

namespace Fenrus.Services.FileStorages;

/// <summary>
/// Interface for file storage
/// </summary>
public interface IFileStorage
{

    /// <summary>
    /// Gets information for a single file or folder
    /// </summary>
    /// <param name="path">the file of the file or folder</param>
    /// <returns>the info</returns>
    Task<UserFile?> GetFile(string path);
    
    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="path">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    Task<List<UserFile>> GetAll(string path);

    /// <summary>
    /// Gets the file data by its path
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    Task<FileData> GetFileData(string fullPath);


    /// <summary>
    /// Gets a thumbnail for an image
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file data</returns>
    Task<FileData> GetThumbnail(string fullPath);

    /// <summary>
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="formFile">the form file being added</param>
    /// <returns>the id of the newly created file</returns>
    Task<UserFile?> Add(string path, string filename, IFormFile formFile);

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    Task<bool> Delete(string fullPath);

    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="path">the full path of the file</param>
    Task CreateFolder(string path);

    /// <summary>
    /// Renames a file or folder
    /// </summary>
    /// <param name="path">the full path to the file or folder</param>
    /// <param name="dest">the new full path for the file or folder</param>
    /// <returns>an awaited task</returns>
    Task Rename(string path, string dest);


    /// <summary>
    /// Gets the file storage service to use for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <returns>the file storage service to use for the user</returns>
    public static IFileStorage GetService(Guid userUid)
    {
        var profile = DbHelper.GetByUid<UserProfile>(userUid);
        switch (profile.FileStorageProvider?.ToLowerInvariant() ?? string.Empty)
        {
            case "nextcloud":
                return new WebDavFileStorage(profile.FileStorageProvider!, profile.FileStorageUrl,
                     profile.FileStorageUsername, profile.FileStoragePassword);
            default:
                return new FileSystemFileStorage(userUid);
        }
    }
}

/// <summary>
/// File Data
/// </summary>
public class FileData
{
    /// <summary>
    /// Gets or sets the stream of the file data
    /// </summary>
    public Stream Data { get; set; } 
    /// <summary>
    /// Gets or sets the file name of the file
    /// </summary>
    public string Filename { get; set; }
    /// <summary>
    /// Gets or sets the MIME type of the file
    /// </summary>
    public string MimeType { get; set; }
}