using System.Globalization;

namespace Krialys.Common.Localization;

/// <summary>Language service interface</summary>
public interface ILanguageContainerService
{
    /// <summary>
    /// Current culture associated with the selected language
    /// </summary>
    CultureInfo CurrentCulture { get; }

    /// <summary>
    /// Is current culture fr-FR?
    /// </summary>
    bool IsCultureFr { get; }

    /// <summary>
    /// Is current culture en-US?
    /// </summary>
    bool IsCultureEn { get; }

    /// <summary>
    /// Dictionary of the language keywords
    /// </summary>
    Keys Keys { get; }

    /// <summary>
    /// Set a new language explicitly
    /// </summary>
    /// <param name="culture">Culture associated with the language to be set</param>
    void SetLanguage(CultureInfo culture);
}