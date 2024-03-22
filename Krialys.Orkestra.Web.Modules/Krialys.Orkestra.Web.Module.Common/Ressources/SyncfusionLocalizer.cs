using Krialys.Orkestra.Web.Module.Common.Ressources;
using Syncfusion.Blazor;
using System.Resources;

namespace Krialys.Orkestra.Web.Module.Common.Shared;

/// <summary>
/// Class for processing Syncfusion UI component's localization.
/// </summary>
public class SyncfusionLocalizer : ISyncfusionStringLocalizer
{
    /// <summary>
    /// Return the Localized value from the resource file.
    /// </summary>
    /// <param name="key">Key string to get the localized value.</param>
    /// <returns>Returns the localized string.</returns>
    public string GetText(string key)
    {
        return ResourceManager.GetString(key);
    }

    /// <summary>
    /// Define ressource manager to access the resource file and get the exact value for locale key.
    /// </summary>
    public ResourceManager ResourceManager => SfResources.ResourceManager;
}