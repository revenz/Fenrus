using System.Text.Json.Serialization;
using Fenrus.Models;
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
    public IEnumerable<CalendarEventModel> GetAll([FromQuery]DateTime start, [FromQuery]DateTime end)
    {
        var startUtc = start.ToUniversalTime();
        var endUtc = end.ToUniversalTime();
        var uid = User.GetUserUid().Value;
        var all = new CalendarService().GetAllForUser(uid);
        var results = all.Where(x => x.StartUtc >= startUtc && x.StartUtc < endUtc)
            .Select(x => CalendarEventModel.From(x)).ToList();
        // get the feed events
        var feeds = new CalendarFeedService().GetEvents(uid, start, end);
        return results.Union(feeds);
    }

    /// <summary>
    /// Saves a calendar event
    /// </summary>
    /// <param name="event">the event to save</param>
    /// <returns>the event as a calendar event model</returns>
    [HttpPost]
    public CalendarEventModel Save([FromBody] CalendarEvent @event)
    {
        var uid = User.GetUserUid().Value;
        @event.UserUid = uid;
        var service = new CalendarService();
        service.Save(@event);
        return CalendarEventModel.From(@event);
    }

    /// <summary>
    /// Deletes a calendar event
    /// </summary>
    /// <param name="uid">the UID of the event to delete</param>
    [HttpDelete("{uid}")]
    public IActionResult Delete([FromRoute] Guid uid)
    {
        var userUid = User.GetUserUid().Value;        
        var service = new CalendarService();
        var @event = service.GetByUid(uid);
        if (@event == null)
            return Ok(); // already deleted
        if (@event.UserUid != userUid)
            return Unauthorized();
        service.Delete(uid);
        return Ok();
    }
}

