using Esprima.Ast;
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
    /// Saves a note
    /// </summary>
    /// <param name="note">the note to save</param>
    /// <returns>the saved note</returns>
    [HttpPost]
    public Note SaveNote([FromBody] Note note)
    {
        var uid = User.GetUserUid().Value;
        var service = new NotesService();
        if (note.Uid != Guid.Empty)
        {
            var existing = service.GetByUid(note.Uid);
            if (existing != null)
            {
                if (existing.UserUid != uid)
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

        if (note.Uid == Guid.Empty)
        {
            var items = service.GetAllByUser(uid);
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
        if (note == null || note.UserUid != userUid)
            return;
        service.Delete(note.Uid);
    }
    
    /// <summary>
    /// Deletes a note
    /// </summary>
    /// <param name="uid">the UID of the note</param>
    [HttpPost("{uid}/move/{up}")]
    public bool Move([FromRoute] Guid uid, [FromRoute] bool up)
    {
        var userUid = User.GetUserUid().Value;
        var service = new NotesService();
        var notes = service.GetAllByUser(userUid).OrderBy(x => x.Order).ToList();
        int index = notes.FindIndex(x => x.Uid == uid);
        if (index < 0)
            return false;  // unknown
        if (index == 0 && up)
            return false;
        if (index == notes.Count - 1 && up == false)
            return false;
        int dest = up ? index - 1 : index + 1;
        var other = notes[dest];
        notes[dest] = notes[index];
        notes[index] = other;
        for (int i = 0; i < notes.Count; i++)
        {
            if (notes[i].Order == i)
                continue;
            notes[i].Order = i;
            service.Update(notes[i]);
        }

        return true;
    }
}