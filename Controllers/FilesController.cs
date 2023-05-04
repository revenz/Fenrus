using Fenrus.Models;
using Fenrus.Services.FileStorages;
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
    [HttpGet]
    public async Task<IEnumerable<UserFile>> GetAll([FromQuery]string? path = null)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        return await IFileStorage.GetService(userUid.Value).GetAll(path ?? string.Empty);
    }
    
    /// <summary>
    /// Gets a media file for the user
    /// </summary>
    /// <param name="path">The full path of the file</param>
    /// <param name="thumbnail">true if requesting thumbnail, otherwise false</param>
    /// <returns>the media file binary data if found</returns>
    [HttpGet("media")]
    [ResponseCache(Duration = 31 * 24 * 60 * 60)] // cache for 31 days
    public async Task<IActionResult> GetMedia([FromQuery] string path, [FromQuery] bool thumbnail = false)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        var service = IFileStorage.GetService(userUid.Value);
        var file = await (thumbnail ? service.GetThumbnail(path) : service.GetFileData(path));
        if(file == null)
            return new NotFoundResult();

        if (file.MimeType?.StartsWith("image") != true)
            return new NotFoundResult();

        string filename = file.Filename;
        string extension = filename[(filename.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        // Or get binary data as Stream and copy to another Stream
        return File(file.Data, file.MimeType?.EmptyAsNull() ?? "image/" + extension);
    }
    /// <summary>
    /// Downloads a file for the user
    /// </summary>
    /// <param name="path">The full path of the file</param>
    /// <returns>the file download if found</returns>
    [HttpGet("download")]
    public async Task<IActionResult> Download([FromQuery] string path)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        var service = IFileStorage.GetService(userUid.Value);
        var file = await service.GetFileData(path);
        if(file == null)
            return new NotFoundResult();

        string filename = file.Filename;
        // Or get binary data as Stream and copy to another Stream
        return File(file.Data,   "application/octet-stream", filename);
    }
    
    /// <summary>
    /// Creates a folder
    /// </summary>
    /// <param name="path">the full path of the file</param>
    [HttpPost("create-folder")]
    public async Task<IActionResult> CreateFolder([FromQuery] string path)
    {
        var uid = User.GetUserUid();
        if (uid == null)
            throw new UnauthorizedAccessException();
        try
        {
            var service = IFileStorage.GetService(uid.Value);
            await service.CreateFolder(path);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    //[RequestFormLimits(BufferBodyLengthLimit = 10_737_418_240, MultipartBodyLengthLimit = 10_737_418_240)] // 10GiB limit
    [RequestFormLimits(BufferBodyLengthLimit = 14_737_418_240, MultipartBodyLengthLimit = 14_737_418_240)] // 10GiB limit
    public async Task<UserFile?> Upload([FromForm] IFormFile file, [FromQuery] string? path = null)
    {
        var uid = User.GetUserUid();
        if (uid == null)
            throw new UnauthorizedAccessException();
        
        var service = IFileStorage.GetService(uid.Value);
        try
        {
            var userFile = await service.Add(path ?? string.Empty, file.FileName, file);
            return userFile;
        }
        catch (Exception)
        {
            // can throw if aborted
            return null;
        }
    }
    /// <summary>
    /// Deletes files
    /// </summary>
    /// <param name="model">the files to delete</param>
    [HttpDelete]
    public async Task<IActionResult> Delete([FromBody] DeleteModel model)
    {
        if (model?.Files?.Any() != true)
            return Ok(); // nothing to delete
        
        var uid = User.GetUserUid();
        if (uid == null)
            throw new UnauthorizedAccessException();
        var service = IFileStorage.GetService(uid.Value);
        foreach (var file in model.Files)
        {
            try
            {
                await service.Delete(file);
            }
            catch (Exception ex)
            {
                // litedb has some issues
                // https://github.com/mbdavid/LiteDB/issues/1940
                // a sleep seems to fix it, so try again
                // Thread.Sleep(250);
                // try
                // {
                //     // if this fails, just silently fail, the UI will refresh and show if it was removed or not
                //     service.Delete(uid, file);
                // }
                // catch (Exception)
                // {
                // }
                return BadRequest(ex.Message);
            }
        }

        return Ok();
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