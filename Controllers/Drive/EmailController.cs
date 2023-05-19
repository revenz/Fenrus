using Fenrus.Models;
using Fenrus.Workers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

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
    
    private readonly IMemoryCache Cache;
    
    /// <summary>
    /// Constructs an instance of Email Controller
    /// </summary>
    /// <param name="cache">memory cache</param>
    public EmailController(IMemoryCache cache)
    {
        this.Cache = cache;
    }

    
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

        string cacheKey = "EmailGetAll_" + userUid.Value;
        bool done = false;

        var cachedList = Cache.TryGetValue(cacheKey, out object obj) && obj is List<EmailMessage> cast ?
            cast : new List<EmailMessage>();

        Task<List<EmailMessage>> cached = Task.Run(async () =>
        {
            await Task.Delay(250);
            return cachedList ?? new ();
        });
        
        Task<List<EmailMessage>> notCached = Task.Run(async () =>
        {
            var result = await MailWorker.Instance.GetLatest(userUid.Value);
            if (result.Count == cachedList.Count)
            {
                // check if they are the same
                var cachedUids = cachedList.Select(x => x.Uid);
                bool different = result.Any(x => cachedUids.Contains(x.Uid) == false);
                if (different == false)
                    return result;
            }
            Cache.Set(cacheKey, result);
            if (done)
                MailWorker.Instance.TriggerEmailReloadEvent(userUid.Value);
            return result;
        });
        
        var completedTask = await Task.WhenAny(cached, notCached);
        var emails = await completedTask;
        done = true;
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