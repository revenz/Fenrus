using System.Text.RegularExpressions;
using Fenrus.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace Fenrus.Services.FileStorages;

/// <summary>
/// File storage using the file system
/// </summary>
public class FileSystemFileStorage:IFileStorage
{
    private readonly FileExtensionContentTypeProvider MimeProvider;
    
    /// <summary>
    /// Constructs a new instance of the file system file storage
    /// </summary>
    public FileSystemFileStorage()
    {
        MimeProvider = new FileExtensionContentTypeProvider();
    }
    
    /// <summary>
    /// Gets the root path for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <returns>the root path</returns>
    private string GetRootPath(Guid userUid)
        => Path.Combine(DirectoryHelper.GetDataDirectory(), "Drive", userUid.ToString());
    
    /// <summary>
    /// Gets the physical path on disk for a given user
    /// </summary>
    /// <param name="userUid">the user UID</param>
    /// <param name="path">the path</param>
    /// <returns>the physical path</returns>
    /// <exception cref="Exception">if the path contains any invalid characters</exception>
    private string GetDirectory(Guid userUid, string path)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        var parts = (path ?? string.Empty).Trim().Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
        path = string.Empty;
        foreach (var part in parts)
        {
            if (part == "..")
                continue;
            
            bool containsInvalidChars = part.IndexOfAny(invalidChars) != -1;
            if (containsInvalidChars)
                throw new Exception("Invalid path");
            
            path += part + Path.DirectorySeparatorChar;
        }

        if (path.EndsWith(Path.DirectorySeparatorChar))
            path = path[..^1];
        
        var root = GetRootPath(userUid);
        if (path == string.Empty)
            return root;
        return Path.Combine(root, path);
    }
    
    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    public List<UserFile> GetAll(Guid userUid, string path)
    {
        var dir = GetDirectory(userUid, path);
        if (Directory.Exists(dir) == false)
            return new ();
        List<UserFile> results = new();
        var dirInfo = new DirectoryInfo(dir);
        var root = GetRootPath(userUid);
        foreach (var sub in dirInfo.GetDirectories().OrderBy(x => x.Name))
        {
            results.Add(new ()
            {
                Created = sub.CreationTimeUtc,
                MimeType = "folder",
                Extension = string.Empty,
                FullPath = sub.FullName.Substring(root.Length + 1),
                Name = sub.Name,
                Size = 0 // we could calculate this, but lets not
            });
        }

        
        var sortedFiles = FileSorter.SortFiles(Directory.GetFiles(dir).ToList());
        foreach (var file in sortedFiles)
            results.Add(GetUserFile(root, new FileInfo(file)));

        return results;
    }

    private UserFile GetUserFile(string root, FileInfo file)
    {
        string mimeType;
        if (MimeProvider.TryGetContentType(file.Name, out mimeType) == false)
            mimeType = file.Extension;
        return new ()
        {
            Created = file.CreationTimeUtc,
            MimeType = mimeType,
            Extension = string.Empty,
            FullPath = file.FullName.Substring(root.Length + 1),
            Name = file.Name,
            Size = file.Length
        };
    }

    /// <summary>
    /// Gets the file data by its path
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    public (Stream Data, string Filename, string MimeType)? GetFileData(Guid userUid, string fullPath)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            return null;
        if (fullPath.Contains(".." + Path.DirectorySeparatorChar))
            return null;
        var file = new FileInfo(Path.Combine(GetRootPath(userUid), fullPath));
        if (file.Exists == false)
            return null;

        string mimeType;
        if (MimeProvider.TryGetContentType(file.Name, out mimeType) == false)
            mimeType = file.Extension;
        return (file.OpenRead(), file.Name, mimeType);
    }

    /// <summary>
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="formFile">the form file being added</param>
    /// <returns>the id of the newly created file</returns>
    public async Task<UserFile?> Add(Guid userUid, string path, string filename, IFormFile formFile)
    {
        string directory = GetDirectory(userUid, path);
        if (Directory.Exists(directory) == false)
            Directory.CreateDirectory(directory);
        if (filename.Contains(".." + Path.DirectorySeparatorChar))
            return null;

        string fullPath = Path.Combine(directory, filename);
        
        string fileName = Path.GetFileNameWithoutExtension(fullPath);
        string extension = Path.GetExtension(fullPath);
        int count = 1;
        while (File.Exists(fullPath))
            fullPath = Path.Combine(directory, $"{fileName} ({count++}){extension}");

        using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await formFile.CopyToAsync(stream);
        }

        return GetUserFile(GetRootPath(userUid), new FileInfo(fullPath));
    }

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    public bool Delete(Guid userUid, string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return true;
        if (fullPath.Contains(".." + Path.DirectorySeparatorChar))
            return false;
        string path = Path.Combine(GetRootPath(userUid), fullPath);
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }

    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="path">the full path of the file</param>
    public void CreateFolder(Guid userUid, string path)
    {
        if (string.IsNullOrEmpty(path))
            return;
        if (path.Contains(".." + Path.DirectorySeparatorChar))
            return;
        string fullPath = GetDirectory(userUid, path);
        try
        {
            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);
        }
        catch (Exception)
        {
        }
    }
}