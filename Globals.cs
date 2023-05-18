namespace Fenrus;

/// <summary>
/// Global variables
/// </summary>
public class Globals
{

    private const string BuildNumber = "0";
    /// <summary>
    /// Gets the version number of Fenrus
    /// </summary>
    public const string Version = "0.9.2." + BuildNumber;
    
    /// <summary>
    /// Gets or sets the encryption key used for data
    /// </summary>
    public static string EncryptionKey { get; set; }

    /// <summary>
    /// A dummy password to use in forms to show a password is set, but does not expose the actual password 
    /// </summary>
    public const string DUMMY_PASSWORD = "************";
    
    /// <summary>
    /// Gets the UID for the guest dashboard, this is a fixed UID that never changes
    /// </summary>
    public static readonly Guid GuestDashbardUid = new Guid("a8b0858a-aa8d-432a-9e32-e85adf1dfb67");
    
    /// <summary>
    /// Gets the magic string to indicate a list option is actually an option group and should be rendered as such
    /// An option group cannot be selected, it group all options below it
    /// </summary>
    public static string LIST_OPTION_GROUP = "###GROUP###";

    /// <summary>
    /// Gets if this app is running inside a docker container
    /// </summary>
    public static readonly bool IsDocker = Environment.GetEnvironmentVariable("Docker") == "1";

    /// <summary>
    /// Gets the default accent color
    /// </summary>
    public const string DefaultAccentColor = "#FF0090";
    
    /// <summary>
    /// Gets the default background color
    /// </summary>
    public const string DefaultBackgroundColor = "#111111";

    /// <summary>
    /// Gets the common HttpClient to use throughout the application
    /// </summary>
    public static readonly HttpClient Client;

    static Globals()
    {
        
        HttpClientHandler handler = new HttpClientHandler()
        {
            AllowAutoRedirect = true
        };
        Client = new(handler);
    }

}