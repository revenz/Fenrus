using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for database images
/// </summary>
[Authorize]
[Route("fimage")]
public class DbImageController:BaseController
{
    /// <summary>
    /// Gets a image from the database
    /// </summary>
    /// <param name="uid">the UID of the image</param>
    /// <returns>the binary data of the image</returns>
    [HttpGet("{uid}")]
    [ResponseCache(Duration = 31 * 24 * 60 * 60)] // cache for 31 days
    public IActionResult Get([FromRoute] Guid uid)
    {
        using var db = DbHelper.GetDb();
        var file = db.FileStorage.OpenRead("db:/image/" + uid);

        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Seek(0, SeekOrigin.Begin);
        string extension = file.FileInfo.Filename[(file.FileInfo.Filename.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        // Or get binary data as Stream and copy to another Stream
        return File(ms, "image/" + extension);
    }
    
    /// <summary>
    /// Gets a media file for the user
    /// </summary>
    /// <param name="uid">The UID of the file</param>
    /// <returns>the media file binary data if found</returns>
    [HttpGet("media/{uid}")]
    [ResponseCache(Duration = 31 * 24 * 60 * 60)] // cache for 31 days
    public async Task<IActionResult> GetMedia([FromRoute] Guid uid)
    {
        var userUid = User.GetUserUid().Value;
        var file = new MediaService().GetByUid(userUid, uid);
        if (file == null)
        {
            // might not be found if just added, wait and try again
            await Task.Delay(500);
            file = new MediaService().GetByUid(userUid, uid);
            if(file == null)
                return new NotFoundResult();
        }

        string filename = file.Value.Filename;
        string extension = filename[(filename.LastIndexOf(".", StringComparison.Ordinal) + 1)..];
        // Or get binary data as Stream and copy to another Stream
        return File(file.Value.Data, file.Value.MimeType?.EmptyAsNull() ?? "image/" + extension);
    }
}