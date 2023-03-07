using System.Security.Cryptography;

namespace Fenrus.Helpers;

/// <summary>
/// Helper to generate passwords
/// </summary>
public class PasswordGenerator
{
    private static readonly char[] Punctuations = "!@#$%^&*()_-+[{]}:>|/?".ToCharArray();

    /// <summary>
    /// Generates a password
    /// </summary>
    /// <param name="length">the length of the password to generate</param>
    /// <param name="numberOfNonAlphanumericCharacters">the number of non alphanumeric character to use</param>
    /// <returns>the new password</returns>
    /// <exception cref="ArgumentException">if length is less than 1 or greater than 128</exception>
    public static string Generate(int length, int numberOfNonAlphanumericCharacters)
    {
        if (length < 1 || length > 128)
        {
            throw new ArgumentException("length");
        }

        if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
        {
            throw new ArgumentException("numberOfNonAlphanumericCharacters");
        }

        using (var rng = RandomNumberGenerator.Create())
        {
            var byteBuffer = new byte[length];

            rng.GetBytes(byteBuffer);

            var count = 0;
            var characterBuffer = new char[length];

            for (var iter = 0; iter < length; iter++)
            {
                var i = byteBuffer[iter] % 87;

                if (i < 10)
                {
                    characterBuffer[iter] = (char)('0' + i);
                }
                else if (i < 36)
                {
                    characterBuffer[iter] = (char)('A' + i - 10);
                }
                else if (i < 62)
                {
                    characterBuffer[iter] = (char)('a' + i - 36);
                }
                else
                {
                    characterBuffer[iter] = Punctuations[GetRandomInt(rng, Punctuations.Length)];
                    count++;
                }
            }

            if (count >= numberOfNonAlphanumericCharacters)
            {
                return new string(characterBuffer);
            }

            int j;

            for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
            {
                int k;
                do
                {
                    k = GetRandomInt(rng, length);
                } while (!char.IsLetterOrDigit(characterBuffer[k]));

                characterBuffer[k] = Punctuations[GetRandomInt(rng, Punctuations.Length)];
            }

            return new string(characterBuffer);
        }
    }

    /// <summary>
    /// Gets a random integer
    /// </summary>
    /// <param name="randomGenerator">the random number generator to use</param>
    /// <returns>a random int</returns>
    private static int GetRandomInt(RandomNumberGenerator randomGenerator)
    {
        var buffer = new byte[4];
        randomGenerator.GetBytes(buffer);

        return BitConverter.ToInt32(buffer);
    }

    /// <summary>
    /// Gets a random integer
    /// </summary>
    /// <param name="randomGenerator">the random number generator to use</param>
    /// <param name="maxInput">the maximum input of the random number</param>
    /// <returns>a random int</returns>
    private static int GetRandomInt(RandomNumberGenerator randomGenerator, int maxInput)
    {
        return Math.Abs(GetRandomInt(randomGenerator) % maxInput);
    }
}