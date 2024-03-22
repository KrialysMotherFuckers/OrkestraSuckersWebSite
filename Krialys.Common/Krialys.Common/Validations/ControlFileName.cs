using System.Text.RegularExpressions;

namespace Krialys.Common.Validations;

public static class ControlFileName
{
    /// <summary>
    /// Control if a file matches a mask.
    /// </summary>
    /// <param name="fileName">Name of the file to control.</param>
    /// <param name="mask">Applied mask.</param>
    /// <returns>True if file matches the mask, false otherwise.</returns>
    /// <remarks><br>"*" represents any sequence of characters (including no characters at all).</br>
    /// <br>"?" represents any single character.</br> </remarks>
    public static bool MatchesMask(string fileName, string mask)
    {
        // Mask is mandatory.
        if (string.IsNullOrEmpty(mask))
            return false;

        // Defined regex pattern :
        // "^" => Matches the starting position within the string.
        // Escape => Escape all regex metacharaters that can be into the file name like ".".
        // "*" and "?" => Accepted wildcards are unescaped.
        // "$" => Matches the ending position of the string.

        // Create Regex and control if the file name matches the Regex.
        return new Regex($"^{Regex.Escape(mask).Replace("\\*", ".*").Replace("\\?", ".")}$",
            RegexOptions.IgnoreCase).IsMatch(fileName);
    }
}
