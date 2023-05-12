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

    /// <summary>
    /// Gets a message by its UID
    /// </summary>
    /// <param name="uid">the message UID</param>
    /// <returns>the message</returns>
    /// <exception cref="UnauthorizedAccessException">throw if the user does not have permission</exception>
    [HttpGet("{uid}")]
    [ResponseCache(Duration = 7 * 24 * 60 * 60)]
    public async Task<IActionResult> GetByUid([FromRoute] uint uid)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        var message = await Workers.MailWorker.Instance.GetByUid(userUid.Value, uid);
        return Ok(message);
    }

    /// <summary>
    /// Marks a message as read
    /// </summary>
    /// <param name="uid">the message UID</param>
    /// <returns>an awaited task</returns>
    /// <exception cref="UnauthorizedAccessException">throw if the user does not have permission</exception>
    [HttpPut("read/{uid}")]
    public async Task<IActionResult> MarkAsRead([FromRoute] uint uid)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        await Workers.MailWorker.Instance.MarkAsRead(userUid.Value, uid);
        return Ok();
    }

    /// <summary>
    /// Archives a message
    /// </summary>
    /// <param name="uid">the message UID</param>
    /// <returns>an awaited task</returns>
    /// <exception cref="UnauthorizedAccessException">throw if the user does not have permission</exception>
    [HttpPut("{uid}/archive")]
    public async Task<IActionResult> Archive([FromRoute] uint uid)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        await Workers.MailWorker.Instance.Archive(userUid.Value, uid);
        return Ok();
    }

    /// <summary>
    /// Deletes a message
    /// </summary>
    /// <param name="uid">the message UID</param>
    /// <returns>an awaited task</returns>
    /// <exception cref="UnauthorizedAccessException">throw if the user does not have permission</exception>
    [HttpDelete("{uid}")]
    public async Task<IActionResult> Delete([FromRoute] uint uid)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            throw new UnauthorizedAccessException();
        
        await Workers.MailWorker.Instance.Delete(userUid.Value, uid);
        return Ok();
    }
}