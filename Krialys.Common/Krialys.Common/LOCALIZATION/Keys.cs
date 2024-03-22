using System.Text.Json;
using YamlDotNet.Serialization;

namespace Krialys.Common.Localization;

/// <summary>Language service Keys</summary>
public class Keys
{
    private JsonDocument _document;

    /// <summary>
    /// Initialize the language object for a specific culture
    /// </summary>
    /// <param name="languageContent">String content that has the YAML language</param>
    public Keys(string languageContent) => Initialize(languageContent);

    /// <summary>
    /// Initialize the language file from the selected culture
    /// </summary>
    /// <param name="languageContent">String content that has the YAML language</param>
    private void Initialize(string languageContent)
    {
        _document = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(new Deserializer().Deserialize<object>(languageContent)));
    }

    /// <summary>
    /// this
    /// </summary>
    /// <param name="key"></param>
    /// <returns>Translated value</returns>
    public string this[string key]
    {
        get
        {
            // Translated value.
            string value = string.Empty;

            var nestedKey = key.Split(':');

            if (nestedKey.Length > 1)
            {
                value = _document.RootElement.GetProperty(nestedKey[0]).EnumerateObject().FirstOrDefault(el => el.NameEquals(nestedKey[1])).Value.ToString();
            }

            if (string.IsNullOrEmpty(value))
            {
                value = $"[{key}]";
            }

            return value;
        }
    }
}