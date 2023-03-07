using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for database images
/// </summary>
[Authorize]
[Route("fimage")]
public class DbImageController:Controller
{
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
}