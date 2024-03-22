using Krialys.Orkestra.Common.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace Krialys.Orkestra.WebApi.Infrastructure.Common;

/// <summary>
/// Allow POST, PUT and PATCH json data directely instead of using JArray
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.Mvc.Formatters.InputFormatter" />
public class RawJsonBodyInputFormatter : InputFormatter
{
    public RawJsonBodyInputFormatter() => SupportedMediaTypes.Add(Litterals.ApplicationJson);

    protected override bool CanReadType(Type type) => type == typeof(string);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        using StreamReader reader = new(context.HttpContext.Request.Body);
        var content = await reader.ReadToEndAsync();

        return await InputFormatterResult.SuccessAsync(content);
    }
}

public static class JsonPatchInputFormatter
{
    public static NewtonsoftJsonPatchInputFormatter GetJsonPatchInputFormatter()
    {
        var builder = new ServiceCollection()
            .AddLogging()
            .AddMvc()
            .AddNewtonsoftJson()
            .Services.BuildServiceProvider();

        return builder
            .GetRequiredService<IOptions<MvcOptions>>()
            .Value
            .InputFormatters
            .OfType<NewtonsoftJsonPatchInputFormatter>()
            .First();
    }
}
