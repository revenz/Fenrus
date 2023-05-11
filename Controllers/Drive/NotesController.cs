using Fenrus.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fenrus.Controllers;

/// <summary>
/// Controller for notes
/// </summary>
[Authorize]
[Route("notes")]
public class NotesController : BaseController
{
    /// <summary>
    /// Gets if the user is admin
    /// </summary>
    /// <param name="userUid"></param>
    /// <returns></returns>
    private bool IsAdmin(Guid userUid)
    {
        var user = new UserService().GetByUid(userUid);
        if (user == null)
            return false;
        return user.IsAdmin;
    }

    /// <summary>
    /// Checks if the user has write access
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="note">the note to check</param>
    /// <returns>true if has write access, otherwise false</returns>
    private bool HasWriteAccess(Guid userUid, Note note)
    {
        if (note == null)
            return false;
        if (note.UserUid == userUid)
            return true;
        if (note.Shared == false)
            return false;
        return IsAdmin(userUid);
    }
    
    /// <summary>
    /// Gets all the notes for the user
    /// </summary>
    /// <returns>the notes</returns>
    [HttpGet]
    public IEnumerable<object> GetAll([FromQuery] NoteType type, [FromQuery(Name = "db")] Guid dashboardUid)
    {
        var uid = User.GetUserUid().Value;
        var notes = GetNotes(uid, type, dashboardUid);
        return notes.OrderBy(x => x.Order).Select(x => new
        {
            x.Uid,
            Name = EncryptionHelper.Decrypt(x.Name),
            Content = EncryptionHelper.Decrypt(x.Content),
            ReadOnly = IsAdmin(uid) == false && x.UserUid != uid
        });
    }

    /// <summary>
    /// Gets the list of notes the user is accessing
    /// </summary>
    /// <param name="userUid">the UID of the user</param>
    /// <param name="type">the type of notes the user is accessing</param>
    /// <param name="dashboardUid">the UID of the dashboard the user is using</param>
    /// <returns>the list of notes</returns>
    List<Note> GetNotes(Guid userUid, NoteType type, Guid dashboardUid)
    {
        if (type == NoteType.Shared)
            return new NotesService().GetShared();
        return new NotesService().GetAllByUser(userUid).Where(x =>
        {
            if (x.Shared) return false;
            if (type == NoteType.Dashboard) return x.DashboardUid == dashboardUid;
            return x.DashboardUid == Guid.Empty;
        }).ToList();
    }

    /// <summary>
    /// Saves a note
    /// </summary>
    /// <param name="note">the note to save</param>
    /// <returns>the saved note</returns>
    [HttpPost]
    public Note SaveNote([FromBody] Note note, [FromQuery] NoteType type, [FromQuery(Name = "db")] Guid dashboardUid)
    {
        var uid = User.GetUserUid().Value;
        var service = new NotesService();
        if (note.Uid != Guid.Empty)
        {
            var existing = service.GetByUid(note.Uid);
            if (existing != null)
            {
                if (HasWriteAccess(uid, existing) == false)
                    throw new UnauthorizedAccessException();
                
                if(existing.Name != note.Name)
                    existing.Name = EncryptionHelper.Encrypt(note.Name ?? string.Empty);
                if(existing.Content != note.Content)
                    existing.Content = EncryptionHelper.Encrypt(note.Content ?? string.Empty);
                service.Update(existing);
                return existing;
            }
        }

        note.UserUid = uid;
        
        if (note.Created < new DateTime(2020, 1, 1))
            note.Created = DateTime.UtcNow;
        
        note.Name = EncryptionHelper.Encrypt(note.Name ?? string.Empty);
        note.Content = EncryptionHelper.Encrypt(note.Content ?? string.Empty);
        note.Shared = type == NoteType.Shared;
        note.UserUid = uid;
        
        if (type == NoteType.Personal || type == NoteType.Shared)
            note.DashboardUid = Guid.Empty;
        else
            note.DashboardUid = dashboardUid;

        if (note.Uid == Guid.Empty)
        {
            var items = GetNotes(uid, type, dashboardUid);
            note.Order = items.Any() ? items.Max(x => x.Order + 1) : 0;
            service.Add(note);
        }
        else
            service.Update(note);
        return note;
    }

    
    /// <summary>
    /// Deletes a note
    /// </summary>
    /// <param name="uid">the UID of the note</param>
    [HttpDelete("{uid}")]
    public void Delete([FromRoute] Guid uid)
    {
        var userUid = User.GetUserUid().Value;
        var service = new NotesService();
        var note = service.GetByUid(uid);
        if (note == null || HasWriteAccess(userUid, note) == false)
            return;
        service.Delete(note.Uid);
    }
    
    /// <summary>
    /// Deletes a note
    /// </summary>
    /// <param name="uid">the UID of the note</param>
    [HttpPost("{uid}/move/{up}")]
    public bool Move([FromRoute] Guid uid, [FromRoute] bool up, [FromQuery] NoteType type, [FromQuery(Name = "db")] Guid dashboardUid)
    {
        var userUid = User.GetUserUid().Value;
        var service = new NotesService();
        var notes = GetNotes(userUid, type, dashboardUid);
        int index = notes.FindIndex(x => x.Uid == uid);
        if (index < 0)
            return false;  // unknown
        if (index == 0 && up)
            return false;
        if (index == notes.Count - 1 && up == false)
            return false;
        int dest = up ? index - 1 : index + 1;
        (notes[dest], notes[index]) = (notes[index], notes[dest]);
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].Order == i)
                continue;
            notes[i].Order = i;
            service.Update(notes[i]);
        }

        return true;
    }

    /// <summary>
    /// Note types
    /// </summary>
    public enum NoteType
    {
        /// <summary>
        /// Personal notes
        /// </summary>
        Personal,
        /// <summary>
        /// Dashboard notes
        /// </summary>
        Dashboard,
        /// <summary>
        /// Shared notes
        /// </summary>
        Shared
    }
}