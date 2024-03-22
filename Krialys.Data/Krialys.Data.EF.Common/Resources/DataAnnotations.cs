using Krialys.Common.Interfaces;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;

namespace Krialys.Data.EF.Resources;

/// <summary>
/// Call RESX  translation service based on RESX files
/// </summary>
public interface IDataAnnotations : IScopedService
{
    string Display<TEntity>(string field) where TEntity : class, new();
}

/// <summary>
/// Call RESX  translation service based on RESX files
/// </summary>
public sealed class DataAnnotations : IDataAnnotations
{
    private readonly IStringLocalizer _localizer;
    private const string Prefix = "Display_";

    public DataAnnotations(IStringLocalizerFactory factory)
        => _localizer = factory.Create(typeof(DataAnnotationsResources));

    /// <summary>
    /// Get localized attribute from an instanciated entity.
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>    
    private string DisplayByReflection<TEntity>(string field) where TEntity : class, new()
    {
        var localizedValue = $"[Missing]: {field}";

        var propertyInfo = typeof(TEntity)
            .GetProperties()
            .FirstOrDefault(prop => prop.Name.Equals(field, StringComparison.OrdinalIgnoreCase));

        var displayAttribute = propertyInfo?.GetCustomAttributes(typeof(DisplayAttribute), true)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();

        if (displayAttribute is not null)
        {
            var localized = _localizer[displayAttribute.Name ?? string.Empty];

            return localized.ResourceNotFound
                ? localizedValue
                : localized.Value;
        }

        return localizedValue;
    }

    /// <summary>
    /// Get localized attribute from a named entity.
    /// Example: DataAnnotations.Display(nameof(TACT_ACTIONS.TACT_CODE))
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="field"></param>
    /// <returns></returns>
    public string Display<TEntity>(string field) where TEntity : class, new()
    {
        var prefix = $"{Prefix}{typeof(TEntity).Name}_{field}";
        var data = _localizer[prefix];
        var eval = prefix.Equals(data.Name, StringComparison.Ordinal);

        return eval
            ? (data.Value.Equals(prefix, StringComparison.Ordinal) || data.Value.Equals(field)) ? $"[Missing]: {field}" : data.Value
            : DisplayByReflection<TEntity>(field);
    }
}