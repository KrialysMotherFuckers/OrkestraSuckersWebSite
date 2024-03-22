using Krialys.Common.Literals;
using System.Globalization;
using System.Reflection;

namespace Krialys.Common.Localization;

/// <summary>Language service class.<br />Has to be injected/registered as <strong>Singleton</strong></summary>
public class LanguageContainerInAssembly : ILanguageContainerService
{
    private readonly Assembly _resourcesAssembly;
    private readonly string _folderName;

    /// <summary>
    /// Create instance of the container that languages exists in a specific folder, initialized with the specific culture
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="folderName">Folder that contains the language files</param>
    /// <param name="assembly"></param>
    public LanguageContainerInAssembly(Assembly assembly, CultureInfo culture, string folderName)
    {
        _folderName = folderName;
        _resourcesAssembly = assembly;
        SetLanguage(culture, true);
    }

    /// <summary>
    /// Create instance of the container that languages exists in a specific folder, initialized with the default culture
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="folderName">Folder that contains the language files</param>
    public LanguageContainerInAssembly(Assembly assembly, string folderName)
    {
        _folderName = folderName;
        _resourcesAssembly = assembly;
        SetLanguage(CultureInfo.CurrentCulture, true);
    }

    /// <summary>
    /// Keys of the language values
    /// </summary>
    public Keys Keys { get; private set; }

    /// <summary>
    /// Current Culture related to the selected language
    /// </summary>
    public CultureInfo CurrentCulture { get; private set; }

    /// <summary>
    /// Is current culture fr-FR?
    /// </summary>
    public bool IsCultureFr => CurrentCulture.Name.Equals(CultureLiterals.FrenchFR);

    /// <summary>
    /// Is current culture en-US?
    /// </summary>
    public bool IsCultureEn => CurrentCulture.Name.Equals(CultureLiterals.EnglishUS);

    /// <summary>
    /// Set language manually based on a specific culture
    /// </summary>
    /// <param name="culture"></param>
    /// <param name="isDefault">To indicates if this is the initial function</param>
    /// <exception cref="FileNotFoundException">If there is no culture file exists in the resoruces folder</exception>
    private void SetLanguage(CultureInfo culture, bool isDefault)
    {
        CurrentCulture = culture;
        string[] languageFileNames = _resourcesAssembly.GetManifestResourceNames().Where(s => s.Contains(_folderName) && s.Contains(".yml") && s.Contains("-")).ToArray();

        // Get the keys from the file that has the current culture
        Keys = GetKeysFromCulture(languageFileNames.FirstOrDefault(n => n.Contains($"{culture.Name}.yml")));

        // Get the keys from a file that has the same language
        if (Keys is null)
        {
            var language = culture.Name.Split('-')[0];
            Keys = GetKeysFromCulture(Array.Find(languageFileNames, n => n.Contains(language)));
        }

        // Get the keys from the english resource
        if (Keys is null && culture.Name is not CultureLiterals.EnglishUS)
            Keys = GetKeysFromCulture(languageFileNames.FirstOrDefault(n => n.Contains($"{CultureLiterals.EnglishUS}.yml")));

        Keys ??= GetKeysFromCulture(languageFileNames.FirstOrDefault());

        if (Keys is null)
            throw new FileNotFoundException($"There is no language files existing in the Resource folder within '{_resourcesAssembly.GetName().Name}' assembly");
    }

    /// <summary>
    /// Set language manually based on a specific culture
    /// </summary>
    /// <param name="culture"></param>
    /// <exception cref="FileNotFoundException">If the required culture langage file is not exist</exception>
    public void SetLanguage(CultureInfo culture)
    {
        SetLanguage(culture, false);
    }

    private Keys GetKeysFromCulture(string fileName)
    {
        try
        {
            // Read the file
            using var fileStream = _resourcesAssembly.GetManifestResourceStream(fileName);
            using var streamReader = new StreamReader(fileStream ?? throw new InvalidOperationException());

            return new Keys(streamReader.ReadToEnd());
        }
        catch (Exception)
        {
            return null;
        }
    }
}