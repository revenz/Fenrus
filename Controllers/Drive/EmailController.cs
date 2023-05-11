using Fenrus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for emails
/// </summary>
[Authorize]
[Route("email")]
public class EmailController : BaseController
{
    // private ImapService? GetService()
    // {
    //     var userUid = User.GetUserUid();
    //     if (userUid == null)
    //         throw new UnauthorizedAccessException();
    //     
    //     var profile = DbHelper.GetByUid<UserProfile>(userUid.Value);
    //     if(profile == null)
    //         throw new UnauthorizedAccessException();
    //     
    //     if(string.IsNullOrWhiteSpace(profile.EmailServer))
    //         return null;
    //     return new ImapService(profile.EmailServer, profile.EmailPort, profile.EmailUsername, profile.EmailPassword);
    // }
    
    /// <summary>
    /// Gets the latest emails for a user
    /// </summary>
    /// <returns>the latest emails</returns>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        var emails = await Workers.MailWorker.Instance.GetLatest(userUid.Value);
        return Ok(emails);
    }

    [HttpGet("{messageId}")]
    [ResponseCache(Duration = 7 * 24 * 60 * 60)]
    public async Task<IActionResult> Open([FromRoute] uint messageId)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        var message = await Workers.MailWorker.Instance.GetByUid(userUid.Value, messageId);
        return Ok(message);
        
    }

}