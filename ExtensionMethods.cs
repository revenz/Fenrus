using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Fenrus;

/// <summary>
/// Extensions Methods
/// </summary>
public static class ExtensionMethods
{
    /// <summary>
    /// If the string is empty or white space, then returns null, otherwise the strings value
    /// </summary>
    /// <param name="str">the string</param>
    /// <returns>null if empty</returns>
    public static string? EmptyAsNull(this string str)
         => string.IsNullOrWhiteSpace(str) ? null : str;


    /// <summary>
    /// Gets the users SID from their authentication state
    /// </summary>
    /// <param name="state">the users authentication state</param>
    /// <returns>the users SID</returns>
    public static Guid? GetUserUid(this AuthenticationState state)
        => state?.User?.GetUserUid();
    
    /// <summary>
    /// Gets the users SID from their claims principal
    /// </summary>
    /// <param name="user">the claims principal</param>
    /// <returns>the users SID</returns>
    public static Guid? GetUserUid(this ClaimsPrincipal user)
    {
        var sid = user?.Claims
            ?.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/sid")?.Value;
        if (sid == null)
            return null;
        if (Guid.TryParse(sid, out Guid uid))
            return uid;
        return null;
    }
}