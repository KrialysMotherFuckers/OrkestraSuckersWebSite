using System.Text.Json;

namespace Krialys.Common.Extensions;

/// <summary>Generic static class extension based on <strong>Dictionary</strong></summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Decode "Value" from "payload"
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="payload"></param>
    /// <param name="elementAt"></param>
    /// <returns>Decoded dictionary as TValue</returns>
    public static IEnumerable<TValue> ConvertFrom<TValue>(IDictionary<string, object> payload, int elementAt)
        => JsonSerializer.Deserialize<IEnumerable<TValue>>(payload?.ToList()[elementAt].Value.ToString() ?? string.Empty);
}