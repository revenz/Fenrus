using Fenrus.Models;

namespace Fenrus.Services;

/// <summary>
/// Service for users
/// </summary>
public class UserService
{
    /// <summary>
    /// Validates a user and returns the user if valid
    /// </summary>
    /// <param name="username">the username</param>
    /// <param name="password">the password</param>
    /// <returns>the user if validates, otherwise null</returns>
    public Models.User? Validate(string username, string password)
    {
        try
        {
            var user = DbHelper.GetByName<Models.User>(username);
            if (user == null)
                return null;
            bool valid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (valid == false)
                return null;
            return user;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Registers a user
    /// </summary>
    /// <param name="username">the username</param>
    /// <param name="password">the password</param>
    /// <param name="isAdmin">if this user is an admin or not</param>
    /// <returns>the newly registered user</returns>
    public Models.User Register(string username, string password, bool isAdmin = false)
    {
        var user = DbHelper.GetByName<Models.User>(username);
        if (user != null)
        {
            // already exists, just update them
            user.Password = BCrypt.Net.BCrypt.HashPassword(password);;
            user.IsAdmin = isAdmin;
            DbHelper.Update(user);
            return user;
        }

        user = new();
        user.Name = username;
        user.Password = BCrypt.Net.BCrypt.HashPassword(password);
        user.Uid = Guid.NewGuid();
        user.IsAdmin = isAdmin;
        DbHelper.Insert(user);
        return user;
    }
    
    

    /// <summary>
    /// Gets all users in the system
    /// </summary>
    /// <returns>all users in the system</returns>
    public List<Models.User> GetAll()
        => DbHelper.GetAll<Models.User>();

    /// <summary>
    /// Updates a user
    /// </summary>
    /// <param name="user">the user to update</param>
    public void Update(Models.User user)
        => DbHelper.Update(user);

    /// <summary>
    /// Adds a new user
    /// </summary>
    /// <param name="user">the user being added</param>
    public void Add(User user)
    {
        if (user.Uid == Guid.Empty)
            user.Uid = Guid.NewGuid();
        DbHelper.Insert(user);
    }

    /// <summary>
    /// Changes a users password
    /// </summary>
    /// <param name="uid">The UID of the user to change the password to</param>
    /// <param name="newPassword">the new password</param>
    /// <returns>true if the password was changed, otherwise false</returns>
    public bool ChangePassword(Guid uid, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false; // need a password!

        var user = DbHelper.GetByUid<User>(uid);
        if (user == null)
            return false; // user doesnt exist
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        DbHelper.Update(user);
        return true;
    }
    
    
    /// <summary>
    /// Changes a users password
    /// </summary>
    /// <param name="uid">The UID of the user to change the password to</param>
    /// <param name="oldPassword">the old password</param>
    /// <param name="newPassword">the new password</param>
    /// <returns>true if the password was changed, otherwise false</returns>
    public bool ChangePassword(Guid uid, string oldPassword, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false; // need a password!

        var user = DbHelper.GetByUid<User>(uid);
        if (user == null)
            return false; // user doesnt exist

        if (BCrypt.Net.BCrypt.Verify(oldPassword, user.Password) == false)
            return false;
        
        user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
        DbHelper.Update(user);
        return true;
    }

    /// <summary>
    /// Gets a user by its UID
    /// </summary>
    /// <param name="uid">the UID of the user</param>
    /// <returns>the user</returns>
    public User GetByUid(Guid uid)
        => DbHelper.GetByUid<User>(uid);

    /// <summary>
    /// Deletes a user
    /// </summary>
    /// <param name="uid">the users UID</param>
    public void Delete(Guid uid)
    {
        DbHelper.Delete<UserSettings>(uid);
        DbHelper.Delete<User>(uid);
    }

    /// <summary>
    /// Finds a user by its username or email address
    /// </summary>
    /// <param name="usernameOrEmail">the username or email address of the user</param>
    /// <returns>the user if found, otherwise null</returns>
    public User? FindUser(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;
        usernameOrEmail = usernameOrEmail.ToLowerInvariant().Trim();
        using var db = DbHelper.GetDb();
        
        var collection = db.GetCollection<User>(nameof(User));
        var user = collection.Query().Where(x => x.Email.ToLower() == usernameOrEmail ||
                                      x.Username.ToLower() == usernameOrEmail)
            .FirstOrDefault();
        return user;
    }
}