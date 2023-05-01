using System.Text.Json.Serialization;
using Fenrus.Models;
using Fenrus.Services.CalendarServices;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for a users calendar
/// </summary>
[Authorize]
[Route("calendar")]
public class CalendarController : BaseController
{
    /// <summary>
    /// Gets all the calendar events for the user
    /// </summary>
    /// <param name="start">the start date for events to get</param>
    /// <param name="end">the end date for events to get</param>
    /// <returns>the events for the given period</returns>
    [HttpGet]
    public async Task<IEnumerable<CalendarEventModel>> GetAll([FromQuery]DateTime start, [FromQuery]DateTime end)
    {
        var uid = User.GetUserUid();
        if (uid == null)
            return new CalendarEventModel[] { };
        
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();
        var allTask = ICalendarService.GetService(uid.Value).GetAll();
        // get the feed events
        var feeds = new CalendarFeedService().GetEvents(uid.Value, start, end);
        var all = await allTask.WaitAsync(new TimeSpan(0, 0, 30));
        var results = all.Where(x => x.StartUtc >= startUtc && x.StartUtc < endUtc)
            .Select(x => CalendarEventModel.From(x)).ToList();
        return results.Union(feeds);
    }

    /// <summary>
    /// Saves a calendar event
    /// </summary>
    /// <param name="event">the event to save</param>
    /// <returns>the event as a calendar event model</returns>
    [HttpPost]
    public async Task<CalendarEventModel?> Save([FromBody] CalendarEvent @event)
    {
        var uid = User.GetUserUid();
        if (uid == null)
            return null;
            
        @event.UserUid = uid.Value;
        var service = ICalendarService.GetService(uid.Value);
        var result = await service.Save(@event);
        return CalendarEventModel.From(result);
    }

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    [HttpDelete("{uid}")]
    public async Task<IActionResult> Delete([FromRoute] string uid)
    {
        var userUid = User.GetUserUid();
        if (userUid == null)
            return Unauthorized();
        
        var service = ICalendarService.GetService(userUid.Value);
        var @event = service.GetByUid(uid);
        if (@event == null)
            return Ok(); // already deleted
        await service.Delete(uid);
        return Ok();
    }
}

