using System.Text;

namespace Krialys.Common.Extensions;

public static class ListConvert
{
    /// <summary>
    /// Exemple:
    /// var datas = await ProxyCore.GetEnumerableAsync<TTL_LOGS>("?$top=5000");
    /// var csv = datas.ToCsvFromList(';');
    /// await JsInterop.DownloadFile("datas.csv", "text/csv", csv);
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="list"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static byte[] ToCsvFromList<TEntity>(this IEnumerable<TEntity> list, char separator = ';') where TEntity : class, new()
    {
        var type = typeof(TEntity).GetProperties();
        var sb = new StringBuilder
        {
            Capacity = 0,
            Length = 0
        };

        // Get Header
        sb.AppendLine(type.Aggregate(string.Empty, (current, prop) => current + $"{prop.Name}{separator}")[..^1]);

        // Get content
        foreach (var obj in list)
        {
            sb.AppendLine(type.Aggregate(string.Empty, (current, prop) => current + $"{prop.GetValue(obj, null)}{separator}")[..^1]);
        }

        // Converts to UTF-8 with BOM to keep the encodings (mandatory for Excel)
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, new UTF8Encoding(true));
        writer.Write(sb.ToString());
        writer.Flush();

        return ms.ToArray();
    }
}
