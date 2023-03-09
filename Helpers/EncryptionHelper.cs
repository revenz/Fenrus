using System.Security.Cryptography;
using System.Text;

namespace Fenrus.Helpers;

/// <summary>
/// Helper for encryption
/// </summary>
public class EncryptionHelper
{
    private static byte[] IV =
    {
        0x13, 0x14, 0x15, 0x16,
        0x01, 0x02, 0x03, 0x04, 
        0x09, 0x10, 0x11, 0x12, 
        0x05, 0x06, 0x07, 0x08
    };

    private static byte[] Key;
    
    /// <summary>
    /// Tests if a string is likely encrypted
    /// </summary>
    /// <param name="input">the string to test</param>
    /// <returns>true if likely encrypted, otherwise false</returns>
    public static bool IsEncrypted(string input)
    {
        Span<byte> buffer = new Span<byte>(new byte[input.Length]);
        return Convert.TryFromBase64String(input, buffer , out int bytesParsed);
    }
    
    /// <summary>
    /// Encrypts a string 
    /// </summary>
    /// <param name="text">the text to encrypt</param>
    /// <returns>the data encrypted</returns>
    public static string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;

        using MemoryStream output = new();
        using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);

        cryptoStream.Write(Encoding.Unicode.GetBytes(text));
        cryptoStream.FlushFinalBlock();

        return Convert.ToBase64String(output.ToArray());
    }
    
    /// <summary>
    /// Decrypts a string
    /// </summary>
    /// <param name="text">the text to decrypt</param>
    /// <returns>the decrypted string</returns>
    public static string Decrypt(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        
        Span<byte> buffer = new Span<byte>(new byte[text.Length]);
        if (Convert.TryFromBase64String(text, buffer, out int bytesParsed) == false)
            return text;

        var data = buffer.Slice(0, bytesParsed).ToArray();
        
        //byte[] encrypted = Convert.FromBase64String(text);
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using MemoryStream input = new(data);
        using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using MemoryStream output = new();
        cryptoStream.CopyTo(output);
        return Encoding.Unicode.GetString(output.ToArray());
    }
    
    /// <summary>
    /// Gets a key from the key password
    /// </summary>
    /// <param name="password">the key password</param>
    /// <returns>the key bytes</returns>
    private static byte[] DeriveKeyFromPassword(string password)
    {
        var emptySalt = Array.Empty<byte>();
        var iterations = 1000;
        var desiredKeyLength = 16; // 16 bytes equal 128 bits.
        var hashMethod = HashAlgorithmName.SHA384;
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
            emptySalt,
            iterations,
            hashMethod,
            desiredKeyLength);
    }

    /// <summary>
    /// Initializes the encryption
    /// </summary>
    /// <param name="dataDir">the data directory</param>
    public static void Init(string dataDir)
    {

        string encryptionKeyFile = Path.Combine(dataDir, "encryptionkey.txt");
        if (File.Exists(encryptionKeyFile) == false)
        {
            Globals.EncryptionKey = Guid.NewGuid().ToString();
            File.WriteAllText(encryptionKeyFile, Globals.EncryptionKey);
        }
        else
        {
            Globals.EncryptionKey = File.ReadAllText(encryptionKeyFile);
        }

        Key = DeriveKeyFromPassword(Globals.EncryptionKey);

        string teststring = Guid.NewGuid() +
                            String.Join("", Enumerable.Range(65, 120).Select(x => ((char)x).ToString()).ToArray());
        string encrypted = Encrypt(teststring);
        string decrypted = Decrypt(encrypted);
        if (encrypted == teststring)
            throw new Exception("Encryption test failed");
        if (decrypted != teststring)
            throw new Exception("Decryption test failed");
    }
}