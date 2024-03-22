using System.Globalization;
using System.Text.RegularExpressions;

namespace Krialys.Common.Extensions;

/// <summary>Generic static class extension based on <strong>string</strong></summary>
public static class StringExtensions
{
    /// <summary>
    /// Make first letter of a string upper case
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string FirstCharToUpper(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string ss = input.Trim();

        return string.IsNullOrEmpty(ss) ? ss : char.ToUpper(ss[0]) + ss[1..].ToLower();
    }

    public static string RemoveSpecialCharacters(this string str)
        => Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);

    public static string ToCamelCase(this string text)
        => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);

    public static string StringOrNull(this string input)
        => string.IsNullOrEmpty(input) ? null : input;

    public static string TrimSpacesWithinWords(this string sentence)
        => (string.IsNullOrEmpty(sentence) || string.IsNullOrWhiteSpace(sentence))
            ? sentence
            : Regex.Replace(sentence.Replace("\r\n", "").Trim(), @"\b[\s\t\v\r\n]+\b", " ");
}