using Blazored.LocalStorage;
using CronExpressionDescriptor;
using Microsoft.Extensions.Configuration;

namespace Krialys.Orkestra.Web.Module.Common.DI;

public interface ICronServices
{
    string GetDescription(string cron);
}

public class CronServices : ICronServices
{
    private readonly IConfiguration _config;
    private readonly ISyncLocalStorageService _localStorage;

    public CronServices(IConfiguration config, ISyncLocalStorageService localStorage)
    {
        _config = config;
        _localStorage = localStorage;
    }

    /// <summary>
    /// Translate CRON in human language.
    /// </summary>
    /// <param name="cron">CRON string to translate.</param>
    /// <returns>CRON description if CRON is correct, cron parameter otherwise.</returns>
    public string GetDescription(string cron)
    {
        /* String describing the CRON in human language. */
        string description = string.Empty;

        if (string.IsNullOrEmpty(cron))
        {
            return description;
        }

        var options = new Options
        {
            DayOfWeekStartIndexZero = true,
            Use24HourTimeFormat = true,
            Verbose = false,
            ThrowExceptionOnParseError = true,
            Locale = _localStorage.GetItemAsString("BlazorCulture") ?? _config["CultureInfo"]
        };

        try
        {
            /* Generate CRON description. */
            description = ExpressionDescriptor.GetDescription(cron, options);
        }
        catch (Exception)
        {
            /* If it fails, just return CRON value. */
            description = cron;
        }

        return description;
    }
}
