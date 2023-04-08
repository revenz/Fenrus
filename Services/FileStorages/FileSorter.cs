namespace Fenrus.Services.FileStorages;

/// <summary>
/// A static class for sorting a list of file names in a human-readable way.
/// </summary>
public static class FileSorter
{
    /// <summary>
    /// Sorts a list of file names in a human-readable way.
    /// </summary>
    /// <param name="fileNames">The list of file names to sort.</param>
    /// <returns>The sorted list of file names.</returns>
    public static List<string> SortFiles(List<string> fileNames)
    {
        // Sort the file names using a custom comparer
        fileNames.Sort(new FileNameComparer());
        
        // Return the sorted list of file names
        return fileNames;
    }

    /// <summary>
    /// A custom comparer for sorting file names in a human-readable way.
    /// </summary>
    private class FileNameComparer : IComparer<string>
    {
        /// <summary>
        /// Compares two file names and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// </summary>
        /// <param name="fileName1">The first file name to compare.</param>
        /// <param name="fileName2">The second file name to compare.</param>
        /// <returns>A negative integer, zero, or a positive integer, depending on the relative order of the file names.</returns>
        public int Compare(string fileName1, string fileName2)
        {
            // Get the file names without extensions
            string fileNameWithoutExtension1 = Path.GetFileNameWithoutExtension(fileName1);
            string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(fileName2);

            // Compare the file names without extensions
            int fileNameWithoutExtensionComparison = NaturalStringComparer.Compare(fileNameWithoutExtension1, fileNameWithoutExtension2);

            if (fileNameWithoutExtensionComparison != 0)
            {
                // If the file names without extensions are not equal, return the comparison result
                return fileNameWithoutExtensionComparison;
            }
            else
            {
                // If the file names without extensions are equal, compare the extensions
                string extension1 = Path.GetExtension(fileName1);
                string extension2 = Path.GetExtension(fileName2);

                // Compare the extensions
                int extensionComparison = string.Compare(extension1, extension2, true);

                if (extensionComparison != 0)
                {
                    // If the extensions are not equal, return the comparison result
                    return extensionComparison;
                }
                else
                {
                    // If the extensions are equal, compare the numbers in the file names
                    string numberString1 = new string(fileName1.SkipWhile(c => !char.IsDigit(c)).ToArray());
                    string numberString2 = new string(fileName2.SkipWhile(c => !char.IsDigit(c)).ToArray());

                    int number1;
                    int number2;

                    if (int.TryParse(numberString1, out number1) && int.TryParse(numberString2, out number2))
                    {
                        // If both file names have numbers, compare the numbers
                        return number1.CompareTo(number2);
                    }
                    else if (int.TryParse(numberString1, out number1))
                    {
                        // If only the first file name has a number, it comes first
                        return -1;
                    }
                    else if (int.TryParse(numberString2, out number2))
                    {
                        // If only the second file name has a number, it comes first
                        return 1;
                    }
                    else
                    {
                        // If neither file name has a number, they are equal
                        return 0;
                    }
                }
            }
        }
    }
}


/// <summary>
/// Provides a comparer for strings that sorts them in a natural order, taking into account any numeric sequences in the strings.
/// </summary>
public static class NaturalStringComparer
{
    /// <summary>
    /// Compares two strings using natural sorting order.
    /// </summary>
    /// <param name="a">The first string to compare.</param>
    /// <param name="b">The second string to compare.</param>
    /// <returns>An integer that indicates the relative position of the strings in the sort order.</returns>
    public static int Compare(string a, string b)
    {
        int i = 0; // Counter for the characters in string a
        int j = 0; // Counter for the characters in string b

        while (i < a.Length && j < b.Length)
        {
            if (char.IsDigit(a[i]) && char.IsDigit(b[j]))
            {
                int x = 0; // Integer representation of the number in string a
                int y = 0; // Integer representation of the number in string b

                // Parse the number in string a
                while (i < a.Length && char.IsDigit(a[i]))
                {
                    x = x * 10 + a[i] - '0';
                    i++;
                }

                // Parse the number in string b
                while (j < b.Length && char.IsDigit(b[j]))
                {
                    y = y * 10 + b[j] - '0';
                    j++;
                }

                if (x != y)
                {
                    return x.CompareTo(y);
                }
            }
            else if (a[i] != b[j])
            {
                return a[i].CompareTo(b[j]);
            }

            i++;
            j++;
        }

        return a.Length - b.Length;
    }
}
