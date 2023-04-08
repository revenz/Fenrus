using Fenrus.Models;

namespace Fenrus.Services.FileStorages;

/// <summary>
/// Interface for file storage
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    List<UserFile> GetAll(Guid userUid, string path);

    /// <summary>
    /// Gets the file data by its path
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    (Stream Data, string Filename, string MimeType)? GetFileData(Guid userUid, string fullPath);


    /// <summary>
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="formFile">the form file being added</param>
    /// <returns>the id of the newly created file</returns>
    Task<UserFile?> Add(Guid userUid, string path, string filename, IFormFile formFile);


    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    bool Delete(Guid userUid, string fullPath);


    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the full path of the file</param>
    void CreateFolder(Guid userUid, string path);
}