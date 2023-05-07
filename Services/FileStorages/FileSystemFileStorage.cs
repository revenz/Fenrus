using Fenrus.Models;
using Microsoft.AspNetCore.StaticFiles;

namespace Fenrus.Services.FileStorages;

/// <summary>
/// File storage using the file system
/// </summary>
public class FileSystemFileStorage:IFileStorage
{
    private readonly FileExtensionContentTypeProvider MimeProvider;

    private static SemaphoreSlim _semaphoreSlim = new(1);

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
    /// Searches for files matching a search pattern
    /// </summary>
    /// <param name="path">the path to perform the search from</param>
    /// <param name="searchPattern">the searchPatten</param>
    /// <returns>a list of matching files</returns>
    public Task<List<UserFile>> SearchFiles(string path, string searchPattern)
    {
        
        if (searchPattern.StartsWith("*") == false && searchPattern.EndsWith("*") == false)
            searchPattern = "*" + searchPattern + "*";
        
        // Search for files using the given search pattern in the rootPath and all subdirectories
        var files = Directory.EnumerateFiles(GetFullPath(path), searchPattern, SearchOption.AllDirectories);
    
        // Create a UserFile object for each file found and add it to the result list
        var result = new List<UserFile>();
        string rootPath = GetRootPath();
        foreach (var file in files)
        {
            result.Add(GetUserFile(rootPath, new FileInfo(file)));
        }
    
        return Task.FromResult(result);
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

        string root = GetRootPath();
        string fulLName = GetFullPath(path);
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
        var dir = GetFullPath(path);
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
                Modified = sub.LastWriteTimeUtc,
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
            Modified = file.LastWriteTimeUtc,
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
        var file = new FileInfo(GetFullPath(fullPath));
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
        string directory = GetFullPath(path, ensuresExists: true);
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
        string path = GetFullPath(fullPath);
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
        string fullPath = GetFullPath(path);
        if (Directory.Exists(fullPath))
            throw new Exception("Folder already exists");
        
        Directory.CreateDirectory(fullPath);
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
        path = GetFullPath(path);
        dest = GetFullPath(dest);
        if (File.Exists(path))
        {
            if (File.Exists(dest))
                throw new Exception("File already exists");
            File.Move(path, dest);
        }
        else if (Directory.Exists(path))
        {
            if (Directory.Exists(dest))
                throw new Exception("Folder already exists");
            Directory.Move(path, dest);
        }
        else
            throw new Exception("File or file not found.");
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Moves files and folders to a new location
    /// </summary>
    /// <param name="destination">the location to move the items into</param>
    /// <param name="items">the items to move</param>
    /// <returns>an awaited task</returns>
    public async Task Move(string destination, string[] items)
    {
        destination = destination.Replace("\\", "/");
        if (destination.EndsWith("/") == false)
            destination += "/";
        for (int i=0;i<items.Length;i++)
        {
            string item = items[i].Replace("\\", "/");
            string name = item.Substring(item.LastIndexOf("/", StringComparison.Ordinal) + 1);
            if (item.EndsWith("/"))
            {
                // folder
                name = item[0..^1];
                name = name.Substring(name.LastIndexOf("/", StringComparison.Ordinal) + 1);
            }
            await this.Rename(item, destination + name);
        }
    }

    /// <summary>
    /// Gets the full path from the passed in relative path
    /// </summary>
    /// <param name="relative">the relative path</param>
    /// <param name="ensuresExists">whether or not the folder should be created if does not already exist</param>
    /// <returns>the full path</returns>
    /// <exception cref="ArgumentException">if the relative path is invalid</exception>
    private string GetFullPath(string relative, bool ensuresExists = false)
    {
        string rootPath = GetRootPath();
        if (string.IsNullOrEmpty(relative))
            return rootPath;
        string fullPath = Path.Combine(rootPath, relative.TrimStart('/'));
        if (fullPath.StartsWith(rootPath) == false)
            throw new ArgumentException("Invalid path provided");

        if (ensuresExists)
        {
            _semaphoreSlim.Wait();
            try
            {
                if (Directory.Exists(fullPath) == false)
                    Directory.CreateDirectory(fullPath);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        
        return fullPath;
    }
}