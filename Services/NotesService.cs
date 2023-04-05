using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// A service for getting users notes
/// </summary>
public class NotesService
{
    /// <summary>
    /// Gets all notes for a user
    /// </summary>
    /// <param name="uid">the UID of the user</param>
    /// <returns>all the notes for the user</returns>
    public List<Note> GetAllByUser(Guid uid)
        => DbHelper.GetAllForUser<Note>(uid);

    /// <summary>
    /// Gets a note by its UID
    /// </summary>
    /// <param name="uid">the UID of the object to get</param>
    /// <returns>the note</returns>
    public Note GetByUid(Guid uid)
        => DbHelper.GetByUid<Note>(uid);
    
    
    /// <summary>
    /// Adds a new note
    /// </summary>
    /// <param name="note">the new note being added</param>
    public void Add(Note note)
    {
        if(note.Uid == Guid.Empty)
            note.Uid = Guid.NewGuid();
        DbHelper.Insert(note);
    }

    /// <summary>
    /// Updates a note
    /// </summary>
    /// <param name="note">the note being updated</param>
    public void Update(Note note)
        => DbHelper.Update(note);

    /// <summary>
    /// Deletes a note
    /// </summary>
    /// <param name="uid">the UID of the note</param>
    public void Delete(Guid uid)
         => DbHelper.Delete<Note>(uid);
}