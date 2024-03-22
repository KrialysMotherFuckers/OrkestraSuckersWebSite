using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common;

/// <summary>
/// Default converters used for Deserialize
/// </summary>
public static class JsonOptions
{
    public static JsonSerializerOptions Options
    {
        get
        {
            return new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals |
                                 JsonNumberHandling.AllowReadingFromString,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                Converters = { new JsonStringEnumConverter() /*, new BoolConverter(), new NullableConverterFactory()*/ },
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
            };
        }
    }
}