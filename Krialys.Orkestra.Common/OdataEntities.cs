using Krialys.Common.Extensions;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common;

/// <summary>
/// API result structure (partial allows to extend this class in the future)
/// <br/>DEPRECATED => please use IApiResponse interface instead
/// </summary>
//[Obsolete("TODO: replace with IApiResponse interface instead", false)]
public class ApiResult
{
    public ApiResult(string httpStatusCode, long count, string message)
    {
        HttpStatusCode = httpStatusCode;
        Count = count;
        Message = message;
    }

    public string HttpStatusCode { get; init; }
    public long Count { get; init; }
    public string Message { get; init; }
}

public class PatchDoc
{
    public IEnumerable<byte[]> PatchDocs { get; init; }

    public IEnumerable<object> EntityIds { get; init; }
}

/// <summary>
/// Internal class used for deserializing OData stream as TEntity.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class ODataContract<TEntity>
{
    [JsonPropertyName("result")]
    public IEnumerable<TEntity> Result { get; set; } = Enumerable.Empty<TEntity>();

    [JsonPropertyName("count")]
    public int Count { get; init; }

    [Newtonsoft.Json.JsonIgnore]
    public Exception Exception { get; set; }
}

/// <summary>
/// Odata entities tooling
/// </summary>
public static class OdataEntities
{
    /// <summary>
    /// Deserialize JSON object to IEnumerable of TEntity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <param name="serializedDatas">Json object.</param>
    /// <returns>IEnumerable of TEntity.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public static IList<TEntity> DeserializeEntity<TEntity>(IEnumerable serializedDatas)
        => serializedDatas.Cast<object>()
            .Select(serializedData => JsonSerializer.Deserialize<TEntity>(serializedData.ToString() ?? string.Empty /*, JsonOptions.Options*/)).ToList();

    /// <summary>
    /// Convert all dates fields from UTC to LocalDateTime.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <param name="datas">Datas with dates in UTC.</param>
    /// <returns>Datas with dates in LocalDateTime.</returns>
    public static IList<TEntity> ConvertDatesToLocaleDateTime<TEntity>(IEnumerable<TEntity> datas)
        => ConvertDates(datas, convertToLocalDateTime: true);

    /// <summary>
    /// Convert dates fields.
    /// </summary>
    /// <typeparam name="TEntity">Entity type.</typeparam>
    /// <param name="datas">Datas to convert.</param>
    /// <param name="convertToLocalDateTime">Conversion to "UTC" or "LocalDateTime".</param>
    /// <returns>Converted datas.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IList<TEntity> ConvertDates<TEntity>(IEnumerable<TEntity> datas, bool convertToLocalDateTime)
    {
        IList<TEntity> dates = new List<TEntity> { Capacity = 0 };

        foreach (var data in datas)
        {
            foreach (var prop in GetDatesProperties<TEntity>())
            {
                if (!DateTimeOffset.TryParse(prop.GetValue(data)?.ToString(), out var dateTime))
                    continue;

                // UTC => LocalTime when convertToLocalDateTime == true, else LocalTime => UTC
                dateTime = convertToLocalDateTime
                    ? DateExtensions.ConvertToTimeZoneFromUtc(dateTime, null)
                    : DateExtensions.ConvertToUTC(dateTime);

                // Case DateTime or DateTime? value
                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    prop.SetValue(data, dateTime.DateTime);
                // Case DateTimeOffset or DateTimeOffset? value
                else
                    prop.SetValue(data, dateTime);
            }

            dates.Add(data);
        }

        return dates;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<PropertyInfo> GetDatesProperties<T>()
        => typeof(T).GetProperties().Where(property => typeof(DateTime) == property.PropertyType || typeof(DateTimeOffset) == property.PropertyType
            || typeof(DateTime?) == property.PropertyType || typeof(DateTimeOffset?) == property.PropertyType);

    /// <summary>
    /// Get Entity X (UTC DateTime coming from server) => Entity X' (LocalDateTime to UI)
    /// Inspired from https://stackoverflow.com/questions/3392612/convert-datatable-to-ienumerablet
    /// </summary>
    /// <typeparam name="TEntity">Type of entity</typeparam>
    /// <param name="list"></param>
    /// <param name="convertToLocalDateTime">true if you want to convert all DateTime/Offset read from DB to LocalDateTime/Offset</param>
    /// <returns>IEnumerable TEntity with DateTime converted to Local or Return list if you don't want to do any conversion</returns>
    public static IEnumerable<TEntity> GetEntities<TEntity>(IEnumerable<TEntity> list, bool convertToLocalDateTime)
        => _getEntities(list, convertToLocalDateTime)
        ?.ToList(); // OB-341 (as far as it's a yield, all datas should be gathered using ToList)

    /// <summary>
    /// Get Entity X (UTC DateTime coming from server) => Entity X' (LocalDateTime to UI) or Get Entity X (LocalDateTime coming from UI) => Entity X' (UTC DateTime to server)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entities"></param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<TEntity> _getEntities<TEntity>(IEnumerable<TEntity> entities, bool convertToLocalDateTime)
    {
        if (entities == null)
            yield break;

        using var dataTable = CreateDataTableFrom(entities);
        if (dataTable == null)
            yield break;

        var entityType = typeof(TEntity);
        var entityProperties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

        IList<string> columnNames = dataTable.Columns.Cast<DataColumn>()
            .Select(c => c.ColumnName)
            .ToList();

        foreach (DataRow dataRow in dataTable.Rows)
        {
            var entity = Activator.CreateInstance<TEntity>();

            foreach (var entityProperty in entityProperties)
            {
                if (!columnNames.Contains(entityProperty.Name) || !entityProperty.CanWrite)
                    continue;

                if (dataRow[entityProperty.Name] == DBNull.Value)
                    entityProperty.SetValue(entity, null);
                else if (entityProperty.PropertyType == typeof(string))
                    entityProperty.SetValue(entity, dataRow[entityProperty.Name].ToString());
                else if (entityProperty.PropertyType == typeof(Guid))
                    entityProperty.SetValue(entity, Guid.Parse(dataRow[entityProperty.Name].ToString() ?? string.Empty));
                else if (entityProperty.PropertyType == typeof(DateTime) || entityProperty.PropertyType == typeof(DateTime?))
                {
                    if (!DateTime.TryParse(dataRow[entityProperty.Name].ToString(), out var dateTime) || dateTime.Equals(DateTime.MinValue))
                        continue;

                    dateTime = convertToLocalDateTime
                        ? DateExtensions.ConvertToTimeZoneFromUtc(dateTime)
                        : DateExtensions.ConvertToUTC(dateTime);

                    entityProperty.SetValue(entity, dateTime);
                }
                else if (entityProperty.PropertyType == typeof(DateTimeOffset) || entityProperty.PropertyType == typeof(DateTimeOffset?))
                {
                    if (!DateTimeOffset.TryParse(dataRow[entityProperty.Name].ToString(), out var dateTimeOffset) || dateTimeOffset.Equals(DateTimeOffset.MinValue))
                        continue;

                    dateTimeOffset = convertToLocalDateTime
                        ? DateExtensions.ConvertToTimeZoneFromUtc(dateTimeOffset.DateTime)
                        : DateExtensions.ConvertToUTC(dateTimeOffset.DateTime);

                    entityProperty.SetValue(entity, dateTimeOffset);
                }
                else
                    entityProperty.SetValue(entity, dataRow[entityProperty.Name]);
            }

            yield return entity;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    static DataTable CreateDataTableFrom<TEntity>(IEnumerable<TEntity> entities)
    {
        if (entities == null)
            return null;

        var dataTable = new DataTable();
        var entityProperties = typeof(TEntity).GetProperties();

        foreach (var entityProperty in entityProperties)
            dataTable.Columns.Add(entityProperty.Name, Nullable.GetUnderlyingType(entityProperty.PropertyType) ?? entityProperty.PropertyType);

        foreach (var entity in entities)
            dataTable.Rows.Add(entityProperties.Select(p => p.GetValue(entity)).ToArray());

        return dataTable;
    }
}