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
    /// the UID of the user
    /// </summary>
    private readonly Guid UserUid;
    
    /// <summary>
    /// Constructs a new instance of the file system file storage
    /// </summary>
    /// <param name="UserUid">the UID of the user</param>
    public FileSystemFileStorage(Guid UserUid)
    {
        this.UserUid = UserUid;
        MimeProvider = new FileExtensionContentTypeProvider();
    }
    
    /// <summary>
    /// Gets the root path for a user
    /// </summary>
    /// <returns>the root path</returns>
    private string GetRootPath()
        => Path.Combine(DirectoryHelper.GetDataDirectory(), "drive", UserUid.ToString());
    
    /// <summary>
    /// Gets the physical path on disk for a given user
    /// </summary>
    /// <param name="path">the path</param>
    /// <returns>the physical path</returns>
    /// <exception cref="Exception">if the path contains any invalid characters</exception>
    private string GetDirectory(string path)
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
        
        var root = GetRootPath();
        if (path == string.Empty)
            return root;
        return Path.Combine(root, path);
    }
    
    /// <summary>
    /// Gets information for a single file or folder
    /// </summary>
    /// <param name="path">the file of the file or folder</param>
    /// <returns>the info</returns>
    public Task<UserFile?> GetFile(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<UserFile?>(null);
        path = path.Replace("\\", "/");
        if (path.Contains("../"))
            return Task.FromResult<UserFile?>(null);

        string root = GetRootPath();
        string fulLName = Path.Combine(root, path);
        if (File.Exists(fulLName))
        {
            var fileInfo = new FileInfo(fulLName);
            if (fileInfo.FullName.StartsWith(root) == false)
                return Task.FromResult<UserFile?>(null);
            return Task.FromResult<UserFile?>(GetUserFile(root, fileInfo));
        }
        if (Directory.Exists(fulLName))
        {
            var dirInfo = new DirectoryInfo(fulLName);
            if (dirInfo.FullName.StartsWith(root) == false)
                return Task.FromResult<UserFile?>(null);
            return Task.FromResult<UserFile?>(
                new UserFile()
                {
                    Created = dirInfo.CreationTimeUtc,
                    MimeType = "folder",
                    Extension = string.Empty,
                    FullPath = dirInfo.FullName.Substring(root.Length + 1),
                    Name = dirInfo.Name,
                    Size = 0 // we could calculate this, but lets not
                });
        }

        return Task.FromResult<UserFile?>(null);
    }
    
    /// <summary>
    /// Gets all the UID for files for a user
    /// </summary>
    /// <param name="path">the users folder path to get</param>
    /// <returns>all the files UIDs for the user</returns>
    public Task<List<UserFile>> GetAll(string path)
    {
        var dir = GetDirectory(path);
        if (Directory.Exists(dir) == false)
            return Task.FromResult(new List<UserFile>());
        List<UserFile> results = new();
        var dirInfo = new DirectoryInfo(dir);
        var root = GetRootPath();
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

        return Task.FromResult(results);
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
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file</returns>
    public Task<FileData?> GetFileData(string fullPath)
    {
        FileData? result = null;
        if (string.IsNullOrWhiteSpace(fullPath))
            return Task.FromResult(result);
        if (fullPath.Contains(".." + Path.DirectorySeparatorChar))
            return Task.FromResult(result);
        var file = new FileInfo(Path.Combine(GetRootPath(), fullPath));
        if (file.Exists == false)
            return Task.FromResult(result);

        string mimeType;
        if (MimeProvider.TryGetContentType(file.Name, out mimeType) == false)
            mimeType = file.Extension;
        result = new()
        {
            Data = file.OpenRead(),
            Filename = file.Name,
            MimeType = mimeType
        };
        return Task.FromResult<FileData?>(result);
    }
    
    /// <summary>
    /// Gets a thumbnail for an image
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>the file data</returns>
    public Task<FileData?> GetThumbnail(string fullPath)
        => GetFileData(fullPath);

    /// <summary>
    /// Adds a file item to the database for the user
    /// </summary>
    /// <param name="path">the path to upload this file to</param>
    /// <param name="filename">the short filename of the file being added</param>
    /// <param name="formFile">the form file being added</param>
    /// <returns>the id of the newly created file</returns>
    public async Task<UserFile?> Add(string path, string filename, IFormFile formFile)
    {
        string directory = GetDirectory( path);
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

        return GetUserFile(GetRootPath(), new FileInfo(fullPath));
    }

    /// <summary>
    /// Deletes a file
    /// </summary>
    /// <param name="fullPath">the full path of the file</param>
    /// <returns>true if the files no longer exists afterwards</returns>
    public Task<bool> Delete(string fullPath)
    {
        if (string.IsNullOrEmpty(fullPath))
            return Task.FromResult(true);
        if (fullPath.Contains(".." + Path.DirectorySeparatorChar))
            return Task.FromResult(false);
        string path = Path.Combine(GetRootPath(), fullPath);
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            else if (Directory.Exists(path))
                Directory.Delete(path, true);
            return Task.FromResult(true);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }

    }

    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="path">the full path of the file</param>
    public Task CreateFolder(string path)
    {
        if (string.IsNullOrEmpty(path))
            return Task.CompletedTask;
        if (path.Contains(".." + Path.DirectorySeparatorChar))
            return Task.CompletedTask;
        string fullPath = GetDirectory(path);
        try
        {
            if (Directory.Exists(fullPath) == false)
                Directory.CreateDirectory(fullPath);
        }
        catch (Exception)
        {
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Renames a file or folder
    /// </summary>
    /// <param name="path">the full path to the file or folder</param>
    /// <param name="dest">the new full path for the file or folder</param>
    /// <returns>an awaited task</returns>
    public Task Rename(string path, string dest)
    {
        File.Move(path, Path.Combine(GetRootPath(), dest));
        return Task.CompletedTask;
    }
}