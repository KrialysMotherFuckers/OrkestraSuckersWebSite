using Newtonsoft.Json;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Krialys.Common.Extensions;

/// <summary>
/// Json extensions mostly used by GDB data transformation
/// </summary>
public static class JsonExtensions
{
    #region Internals
    public const string Id = "GUID";
    public const string CodeEtq = "CodeEtq_$";
    #endregion

    /// <summary>
    /// Get Json and convert to a dictionary that holds nested list of dictionaries
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static IDictionary<string, IEnumerable<IDictionary<string, object>>> AsNestedListOfDictionaries(this string json)
        => JsonConvert.DeserializeObject<IDictionary<string, IEnumerable<IDictionary<string, object>>>>(json);

    ///// <summary>
    ///// Get Dictionary and convert to an ExpandoObject list
    ///// </summary>
    ///// <param name="dico"></param>
    ///// <returns>An enumerable expando representing the data</returns>
    //public static IEnumerable<ExpandoObject> AsEnumerableExpando(this IDictionary<string, IEnumerable<IDictionary<string, object>>> dico, bool generateIds)
    //{
    //    if (dico != null && (dico?.Any() != true || dico.Values.Count == 0))
    //        yield break;

    //    foreach (var d in dico)
    //    {
    //        foreach (var items in d.Value)
    //        {
    //            IDictionary<string, object> expandoDict = new ExpandoObject();

    //            if (generateIds)
    //            {
    //                expandoDict.Add(Id, Guid.NewGuid().ToString("N"));
    //                expandoDict.Add(CodeEtq, d.Key);
    //            }

    //            foreach (var kvp in items)
    //                expandoDict.Add(kvp.Key, kvp.Value);

    //            yield return (ExpandoObject)expandoDict;
    //        }
    //    }
    //}

    /// <summary>
    /// Get all fields from a given Expando object list
    /// </summary>
    /// <param name="enumerableExpando"></param>
    /// <returns>An enumerable string representing keys</returns>
    public static IEnumerable<string> GetEnumerableFields(this IEnumerable<ExpandoObject> enumerableExpando)
    {
        return enumerableExpando.Any()
            ? (enumerableExpando!.FirstOrDefault()! as IDictionary<string, object>).Keys
            : Enumerable.Empty<string>();
    }

    /// <summary>
    /// Convert Datagrid's current view as a byte array (with an optional Label code)
    /// </summary>
    /// <param name="dataView"></param>
    /// <param name="newCodeEtiquette"></param>
    /// <returns></returns>
    public static byte[] ConvertToByteArray(this IEnumerable<object> dataView, byte flagNewOrUpdateOrDelete, object newCodeEtiquette = null)
    {
        IList<IDictionary<string, object>> dicoList = new List<IDictionary<string, object>>();

        foreach (var data in JsonConvert.DeserializeObject<IEnumerable<IDictionary<string, object>>>(JsonConvert.SerializeObject(dataView)))
        {
            IDictionary<string, object> dico = new Dictionary<string, object>();

            foreach (var kvp in data)
            {
                if (!(/*kvp.Key.Equals(Id, StringComparison.Ordinal) ||*/ kvp.Key.Equals(CodeEtq, StringComparison.Ordinal)))
                    dico.Add(kvp.Key, kvp.Value);
            }

            dicoList.Add(dico);
        }

        var jsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(newCodeEtiquette != null
            ? $"{{\"{newCodeEtiquette}\":{JsonConvert.SerializeObject(dicoList)}}}"
            : JsonConvert.SerializeObject(dicoList));

        var compress = GZipExtensions.Compress(jsonBytes);

        // Concatenate the flag with existing array
        return new byte[] { flagNewOrUpdateOrDelete }.Concat(compress).ToArray();
    }

    /// <summary>
    /// Check if a given string is a valid json object or a json array
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static bool IsValidJson(string jsonString)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(jsonString))
                return false;

            jsonString = jsonString.Trim();

            if (!jsonString.StartsWith('{') && !jsonString.EndsWith('}') || (jsonString.StartsWith("[") && jsonString.EndsWith("]")))
                return false;

            _ = JsonDocument.Parse(jsonString);

            return true;
        }
        catch { }

        return false;
    }
}