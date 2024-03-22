using System.Data;
using System.Dynamic;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Krialys.Shared.Contracts;

public static class Extensions
{
    /// <summary>
    /// Convert ExpandoObject to IDictionary string, object
    /// </summary>
    /// <param name="expando"></param>
    /// <returns></returns>
    public static IDictionary<string, object> ToDictionary(this ExpandoObject expando)
        => expando is not null ? new Dictionary<string, object>(expando) : null;

    /// <summary>
    /// Convert ExpandoObject to Json
    /// </summary>
    /// <param name="expando"></param>
    /// <returns></returns>
    public static string ToJson(this ExpandoObject expando)
        => expando is not null ? JsonSerializer.Serialize(expando) : null;

    /// <summary>
    /// Convert to List
    /// </summary>
    /// <param name="expando"></param>
    /// <returns></returns>
    public static IList<ExpandoObject> ToList(this ExpandoObject expando)
        => expando is not null ? new List<ExpandoObject> { expando } : null;

    /// <summary>
    /// Create a datatable from a list of ExpandoObjects
    /// </summary>
    /// <param name="list">The list can be created from a dictionary with Dictionary.Values.ToList()</param>
    /// <param name="tableName">Name of the data table</param>
    /// <returns></returns>
    public static DataTable ToDataTable(this IList<ExpandoObject> list, string tableName)
    {
        if (list?.Count == 0)
        {
            return null;
        }

        //build columns
        var props = (IDictionary<string, object>)list?[0];
        var t = new DataTable(tableName);

        if (props != null)
        {
            foreach ((string key, object value) in props)
            {
                t.Columns.Add(new DataColumn(key, value.GetType()));
            }
        }

        //add rows
        if (list == null)
        {
            return t;
        }

        {
            foreach (var row in list)
            {
                var data = t.NewRow();
                foreach ((string key, object value) in row)
                {
                    data[key] = value;
                }

                t.Rows.Add(data);
            }
        }

        return t;
    }
}

public sealed class DynamicContracts : IDynamicContracts, IDisposable
{
    #region PARAMETERS

    private string Mapping { get; set; }
    private string Error { get; set; }
    private List<string> Fields { get; set; }
    private IList<string> Types { get; set; }
    private IList<string> FieldsNotFound { get; } = new List<string>();

    #endregion PARAMETERS

    /// <summary>
    /// Create a new instance
    /// </summary>
    /// <returns></returns>
    public static DynamicContracts CreateInstance() => new();

    /// <summary>
    /// Mapping
    /// </summary>
    /// <param name="mapping"></param>
    /// <returns></returns>
    public DynamicContracts Select(string mapping)
    {
        Mapping = mapping;

        return this;
    }

    /// <summary>
    /// Get last error
    /// </summary>
    public string LastError => Error;

    /// <summary>
    /// Get a contract by providing dynamically its interface contract
    /// </summary>
    /// <param name="json"></param>
    /// <param name="comparison"></param>
    /// <returns>ExpandoObject is success, else null</returns>
    public ExpandoObject From(string json, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
    {
        Error = null;

        try
        {
            Fields = Mapping.Split(';', StringSplitOptions.RemoveEmptyEntries).Where((_, index) => index % 2 == 0).ToList();
            Types = Mapping.Split(';', StringSplitOptions.RemoveEmptyEntries).Where((_, index) => index % 2 != 0).ToList();

            if (Fields.Count.Equals(Types.Count))
            {
                // Get Json as key/values
                var dicoJsonBody = JsonSerializer.Deserialize<IDictionary<string, object>>(json);
                if ((dicoJsonBody ?? throw new InvalidOperationException("Impossible to fill dictionary")).Any())
                {
                    // Build a new Json based on structure
                    var expando = new ExpandoObject();
                    using var ms = new MemoryStream();
                    using var writer = new Utf8JsonWriter(ms);
                    FieldsNotFound.Clear();

                    writer.WriteStartObject();
                    for (var i = 0; i < Types.Count; i++)
                    {
                        // Get value
                        var valueKind = dicoJsonBody.FirstOrDefault(x => x.Key.ToLowerInvariant().Equals(Fields[i].ToLowerInvariant()));

                        // Key was not found
                        if (valueKind.Key is null)
                        {
                            FieldsNotFound.Add(Fields[i]);
                        }
                        else
                        {
                            // Find, then insert key/field value
                            var index = Fields.FindIndex(x => x.ToLowerInvariant().Equals(valueKind.Key.ToLowerInvariant()));
                            if (dicoJsonBody.TryGetValue(valueKind.Key, out var value))
                            {
                                if (value is null)
                                {
                                    writer.WriteNull(valueKind.Key);
                                    expando.TryAdd(valueKind.Key, null);
                                }
                                else
                                {
                                    Utf8JsonWrite(writer, valueKind.Key, Types[index].ToLower(), Convert.ToString(valueKind.Value, CultureInfo.InvariantCulture), expando);
                                }
                            }
                        }
                    }
                    writer.WriteEndObject();
                    writer.Flush();

                    if (FieldsNotFound.Any())
                    {
                        // Error some fields are not matching
                        Error = $"Fields that do not match: {string.Join("|", FieldsNotFound)}";
                    }
                    else
                    {
                        // Compare Input vs Output
                        return string.Concat(Encoding.UTF8.GetString(ms.ToArray()).OrderBy(c => c))
                            .Equals(string.Concat(json.OrderBy(c => c)), comparison)
                            ? expando
                            : default;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return default;
    }

    /// <summary>
    /// To detect redundant calls
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Public implementation of Dispose pattern callable by consumers
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        //GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern
    /// https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose?redirectedfrom=MSDN#implement-the-dispose-pattern
    /// </summary>
    /// <param name="disposing"></param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            // Dispose managed state (managed objects).
            Error = null;
            Mapping = null;
        }

        _disposed = true;
    }

    /// <summary>
    /// Dynamically append Utf8Json to both:
    /// <br>+ Utf8JsonWriter</br>
    /// <br>+ IDictionary</br>
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="fieldName"></param>
    /// <param name="fieldType"></param>
    /// <param name="fieldValue"></param>
    /// <param name="dico"></param>
    private static void Utf8JsonWrite(Utf8JsonWriter writer, string fieldName, string fieldType, object fieldValue, IDictionary<string, object> dico)
    {
        switch (fieldType)
        {
            case "bool" or "boolean":
                var b = Convert.ToBoolean(fieldValue);
                if (dico.TryAdd(fieldName, b))
                    writer.WriteBoolean(fieldName, b);
                break;

            case "int" or "integer":
                var i = Convert.ToInt32(fieldValue);
                if (dico.TryAdd(fieldName, i))
                    writer.WriteNumber(fieldName, i);
                break;

            case "string":
                var str = Convert.ToString(fieldValue);
                if (dico.TryAdd(fieldName, str))
                    writer.WriteString(fieldName, str);
                break;

            case "double" or "float":
                var dbl = Convert.ToDouble(fieldValue, CultureInfo.InvariantCulture);
                if (dico.TryAdd(fieldName, dbl))
                    writer.WriteNumber(fieldName, dbl);
                break;

            case "decimal":
                var dec = Convert.ToDecimal(fieldValue, CultureInfo.InvariantCulture);
                if (dico.TryAdd(fieldName, dec))
                    writer.WriteNumber(fieldName, dec);
                break;

            case "date" or "datetime" or "datetimeoffset":
                var date = DateTimeOffset.Parse(Convert.ToString(fieldValue) ?? string.Empty, CultureInfo.InvariantCulture);
                if (dico.TryAdd(fieldName, date))
                    writer.WriteString(fieldName, date);
                break;

            default:
                throw new NotSupportedException($"Not yet supported type: {fieldType}, converter for {fieldValue} not implemented");
        }
    }
}