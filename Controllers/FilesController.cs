using Fenrus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for files
/// </summary>
[Authorize]
[Route("files")]
public class FilesController : BaseController
{
    /// <summary>
    /// Gets all the files for the user
    /// </summary>
    /// <param name="path">the path to get</param>
    /// <returns>the files</returns>
    [HttpGet("path")]
    public IEnumerable<UserFile> GetAll([FromQuery]string? path = null)
    {
        var uid = User.GetUserUid().Value;
        return new UserFilesService().GetAll(uid, path ?? string.Empty);
    }
    
    /// <summary>
    /// Gets a media file for the user
    /// </summary>
    /// <param name="path">The full path of the file</param>
    /// <returns>the media file binary data if found</returns>
    [HttpGet("media")]
    [ResponseCache(Duration = 31 * 24 * 60 * 60)] // cache for 31 days
    public async Task<IActionResult> GetMedia([FromQuery] string path)
    {
        var userUid = User.GetUserUid().Value;
        var service = new UserFilesService();
        var file = service.GetFileData(userUid, path);
        if (file == null)
        {
            // might not be found if just added, wait and try again
            await Task.Delay(500);
            file = service.GetFileData(userUid, path);
            if(file == null)
                return new NotFoundResult();
        }

        if (file.Value.MimeType?.StartsWith("image") != true)
            return new NotFoundResult();

        string filename = file.Value.Filename;
        string extension = filename[(filename.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        // Or get binary data as Stream and copy to another Stream
        return File(file.Value.Data, file.Value.MimeType?.EmptyAsNull() ?? "image/" + extension);
    }
    /// <summary>
    /// Downloads a file for the user
    /// </summary>
    /// <param name="path">The full path of the file</param>
    /// <returns>the file download if found</returns>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        var userUid = User.GetUserUid().Value;
        var service = new UserFilesService();
        var file = service.GetFileData(userUid, path);
        if (file == null)
        {
            // might not be found if just added, wait and try again
            await Task.Delay(500);
            file = service.GetFileData(userUid, path);
            if(file == null)
                return new NotFoundResult();
        }

        string filename = file.Value.Filename;
        string extension = filename[(filename.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        // Or get binary data as Stream and copy to another Stream
        return File(file.Value.Data, file.Value.MimeType?.EmptyAsNull() ?? "image/" + extension, filename);
    }
    
    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="path">the full path of the file</param>
    [HttpPost("create-folder")]
    public void CreateFolder([FromQuery] string path)
    {
        var uid = User.GetUserUid().Value;
        new UserFilesService().CreateFolder(uid, path);
    }
    
    
    /// <summary>
    /// Uploads files
    /// </summary>
    /// <param name="file">the files being uploaded</param>
    /// <param name="path">the path to get</param>
    /// <returns>the UIDs of the newly uploaded files</returns>
    [HttpPost("path")]
    [DisableRequestSizeLimit]
    //[RequestFormLimits(BufferBodyLengthLimit = 10_737_418_240, MultipartBodyLengthLimit = 10_737_418_240)] // 10GiB limit
    [RequestFormLimits(BufferBodyLengthLimit = 14_737_418_240, MultipartBodyLengthLimit = 14_737_418_240)] // 10GiB limit
    public async Task<List<UserFile>> Upload([FromForm] List<IFormFile> file, [FromQuery] string? path = null)
    {
        List<UserFile> files = new();
        if (file?.Any() != true)
            return files;
        var uid = User.GetUserUid().Value;
        var service = new UserFilesService();
        foreach (var f in file)
        {
            var userFile = await service.Add(uid, path, f.FileName, f);
            if (userFile != null)
                files.Add(userFile);
        }

        return files;
    }

    /// <summary>
    /// Deletes files
    /// </summary>
    /// <param name="model">the files to delete</param>
    [HttpDelete]
    public void Delete([FromBody] DeleteModel model)
    {
        if (model?.Files?.Any() != true)
            return; // nothing to delete
        
        var uid = User.GetUserUid().Value;
        var service = new UserFilesService();
        foreach (var file in model.Files)
        {
            try
            {
                service.Delete(uid, file);
            }
            catch (Exception)
            {
                // litedb has some issues
                // https://github.com/mbdavid/LiteDB/issues/1940
                // a sleep seems to fix it, so try again
                Thread.Sleep(250);
                try
                {
                    // if this fails, just silently fail, the UI will refresh and show if it was removed or not
                    service.Delete(uid, file);
                }
                catch (Exception)
                {
                }
            }
                
        }
    }

    /// <summary>
    /// A delete model
    /// </summary>
    public class DeleteModel
    {
        /// <summary>
        /// Gets or sets files to delete
        /// </summary>
        public List<string> Files { get; init; }
    }
}