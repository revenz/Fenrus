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
        return results;
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
    /// Models for a calender event used by the UI control
    /// </summary>
    public class CalendarEventModel
    {
        /// <summary>
        /// Gets the UID of the event
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Uid { get; init; }
        /// <summary>
        /// Gets the name of the event
        /// </summary>
        [JsonPropertyName("title")]
        public string Title { get; init; }
        /// <summary>
        /// Gets the start date of the event
        /// </summary>
        [JsonPropertyName("start")]
        public string Start { get; init; }
        /// <summary>
        /// Gets the end date of the event
        /// </summary>
        [JsonPropertyName("end")]
        public string End { get; init; }

        /// <summary>
        /// Converts a CalendarEvent to a CalendarEventModel
        /// </summary>
        /// <param name="event">the CalendarEvent</param>
        /// <returns>the CalendarEventModel</returns>
        public static CalendarEventModel From(CalendarEvent @event)
            => new()
            {
                Uid = @event.Uid,
                Title = @event.Name, 
                Start = @event.StartUtc.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                End = @event.EndUtc.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };
    }
}

