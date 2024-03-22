using Krialys.Common.Enums;
using Krialys.Common.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Linq;

namespace Krialys.Common;

public static class DynamicDictionaryExtensions
{
    public static ParallelOptions StdParallelOptions()
    {
        var cancellationToken = new CancellationTokenSource();

        var options = new ParallelOptions();
#if DEBUG
        // 1 is sequential
        options.MaxDegreeOfParallelism = 1;
#else
        // -1 is unlimited
        options.MaxDegreeOfParallelism = -1;
#endif
        options.CancellationToken = cancellationToken.Token;

        return options;   // separate instance for each loop
    }

    public static bool SequenceEqual(this DynamicDictionary dict1, DynamicDictionary dict2)
    {
        // Check for null references
        if (ReferenceEquals(dict1, dict2))
            return true;

        if (dict1 == null || dict2 == null)
            return false;

        return JsonConvert.DeserializeObject<IDictionary<string, object>>(JsonConvert.SerializeObject(dict1))
            .SequenceEqual(JsonConvert.DeserializeObject<IDictionary<string, object>>(JsonConvert.SerializeObject(dict2)));
    }

    public static DynamicDictionary GetDictionary(this IEnumerable<DynamicDictionary> dict1, string propertyName, object lookupValue)
    {
        if (dict1 == null || lookupValue == null || string.IsNullOrEmpty(propertyName))
            return DynamicDictionary.CreateInstance();

        return dict1.FirstOrDefault(e => e.GetValue(propertyName).Equals(lookupValue));
    }

    public static IEnumerable<DynamicDictionary> ToDynamicDictionary(this JArray refTableData, IDictionary<string, ManagedTypes> refTableModel)
    {
        //foreach (var obj in from JObject obj in refTableData select obj)
        foreach (var jToken in refTableData)
        {
            var obj = (JObject)jToken;
            var dico = DynamicDictionary.CreateInstance();

            foreach (var property in obj.Properties())
            {
                if (property.Value.Type == JTokenType.Null)
                    dico.TryAddMember(property.Name, null);
                else
                {
                    dico.TryAddMember(property.Name, refTableModel[property.Name] switch
                    {
                        ManagedTypes.String => property.Value.Value<string>(),
                        ManagedTypes.Boolean => property.Value.Value<bool?>(),
                        ManagedTypes.Int32 or ManagedTypes.Int64 => property.Value.Value<long?>(),
                        ManagedTypes.Decimal => property.Value.Value<decimal?>(),
                        ManagedTypes.DateTime => property.Value.Value<DateTime?>(),
                        _ => null,
                    });
                }
            }

            yield return dico;
        }
    }

    /// <summary>
    /// Convert an IDictionary as a list of DynamicDictionary
    /// </summary>
    /// <param name="refTableData"></param>
    /// <param name="refTableModel"></param>
    /// <param name="isRefTableDataCompressed"></param>
    /// <returns></returns>
    public static IEnumerable<DynamicDictionary> ToDynamicDictionary(this byte[] refTableData, IDictionary<string, ManagedTypes> refTableModel, bool isRefTableDataCompressed = true)
    {
        if (refTableData == null || refTableData == Array.Empty<Byte>())
            return new List<DynamicDictionary>();

        var dicoData = System.Text.Json.JsonSerializer
            .Deserialize<ICollection<IDictionary<string, object>>>(isRefTableDataCompressed ? GZipExtensions.Decompress(refTableData) : refTableData);

        var jArray = new JArray();
        foreach (var (dictionary, jObject) in from dictionary in dicoData
                                              let jObject = new JObject()
                                              select (dictionary, jObject))
        {
            foreach (var kvp in dictionary)
                jObject.Add(new JProperty(kvp.Key, kvp.Value?.ToString()));

            jArray.Add(jObject);
        }

        return ToDynamicDictionary(jArray, refTableModel).ToList();
    }
}

public class DynamicDictionary : DynamicObject
{
    private readonly IDictionary<string, object> _dictionary = new Dictionary<string, object>(capacity: 0);

    public static DynamicDictionary CreateInstance()
        => new();

    public override bool TryGetMember(GetMemberBinder binder, out object result)
        => _dictionary.TryGetValue(binder.Name, out result);

    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        _dictionary[binder.Name] = value;

        return true;
    }

    public object GetValue(string propertyName)
        => _dictionary.TryGetValue(propertyName, out var result) ? result : null;

    public void TryAddMember(string propertyName, object value)
        => _dictionary[propertyName] = value;

    public IDictionary<string, object> GetDictionary()
        => _dictionary;

    public override IEnumerable<string> GetDynamicMemberNames()
        => _dictionary?.Keys ?? Enumerable.Empty<string>();
}
