using Krialys.Common.Extensions;
using Krialys.Orkestra.Common;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using static System.Text.Json.JsonSerializer;

namespace Krialys.Orkestra.Web.Module.Common.DI;

/// <summary>
/// Implement custom adaptor by extending the Synfusion DataAdaptor class
/// </summary>
public interface IWasmDataAdaptor<TEntity> where TEntity : class, new() // <= mandatory to avoid exception at runtime when deployed ion IIS
{
    // Syncfusion DataAdaptor CRUD
    Task<object> BatchUpdateAsync(DataManager dataManager, object changedRecords, object addedRecords, object deletedRecords, string keyField, string key, int? dropIndex);

    Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null);

    Task<object> RemoveAsync(DataManager dataManager, object data, string keyField, string key);

    Task<object> UpdateAsync(DataManager dataManager, object data, string keyField, string key);
}

/// <summary>
/// Implement custom adaptor by extending the DataAdaptor class wired onto native HttpClient
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public class WasmDataAdaptor<TEntity> : DataAdaptor, IWasmDataAdaptor<TEntity> where TEntity : class, new()
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<WasmDataAdaptor<TEntity>> _uiLogger;
    private volatile DataResult _dataSource;
    // Specific Http client used by RefIt
    private readonly IHttpProxyCore _proxyCore;
    private bool _convertToLocalDateTime;
    private string _cacheKey;

    public WasmDataAdaptor(IHttpProxyCore proxyCore, IMemoryCache cache, ILogger<WasmDataAdaptor<TEntity>> logger, IConfiguration configuration)
    {
        // Reuse the app injected HttpClient (but don't dispose as far as this client belongs to its instance)
        _proxyCore = proxyCore;
        _cache = cache;
        _uiLogger = logger;
        _dataSource = null;

        // Use this flag because a Webapp is linked to a dedicated SqliteDB, we can consider ConvertToLocalDateTime flag global per WebApp
        _ = bool.TryParse(configuration["OdataSettings:ConvertToLocalDateTime"], out _convertToLocalDateTime);
    }

    /// <summary>
    /// Convert an object as DateTime or as DateTimeOffset
    /// </summary>
    /// <param name="targetValue"></param>
    /// <param name="convertToLocalDateTime">When true then convert Date to LocalTime, else convert Date to UTC (default)</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static DateTimeOffset ConvertDate(object targetValue, bool convertToLocalDateTime)
    {
        if (!DateTimeOffset.TryParse(targetValue?.ToString(), out var dateTime))
            return (DateTimeOffset)targetValue;

        // UTC => LocalTime when convertToLocalDateTime == true, else LocalTime => UTC
        dateTime = convertToLocalDateTime
            ? DateExtensions.ConvertToTimeZoneFromUtc(dateTime, null)
            : DateExtensions.ConvertToUTC(dateTime);

        //targetValue = dateTime;
        //var dataType = targetValue.GetType();
        //// Case DateTime or DateTime? value
        //if (typeof(DateTime) == dataType || typeof(DateTime?) == dataType)
        //{
        //    targetValue = dateTime.DateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        //}
        //// Case DateTimeOffset or DateTimeOffset? value
        //else if (typeof(DateTimeOffset) == dataType || typeof(DateTimeOffset?) == dataType)
        //{
        //    targetValue = dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
        //}

        var offset = ((DateTimeOffset)DateTime.SpecifyKind(dateTime.DateTime, DateTimeKind.Local)).Offset;
        var newDateTimeOffset = new DateTimeOffset(dateTime.DateTime).ToOffset(offset);

        //targetValue = newDateTimeOffset;//.ToString("yyyy-MM-ddTHH:mm:ss");

        return newDateTimeOffset;
    }

    // Performs Read (uses UTC and/or Local DateTime conversion mechanism related to _convertToLocalDateTime flag)
    public override async Task<object> ReadAsync(DataManagerRequest dataManagerRequest, string key = null)
    {
        // Add a way to pass extra parameters via Http Headers (example)
        //dataManagerRequest.Take = GetRequestHeaders("take") is null ? dataManagerRequest.Take : Convert.ToInt32(GetRequestHeaders("take"));

        // Verify data manager parameters.
        // Parameters can be passed from Grid to DataAdaptor using query parameter.
        // Example: Query gridQuery = new Query().AddParams(Litterals.ConvertToUTtc, true);
        // If the boolean "convertToUTtc" parameter is present

        var entity = $"{typeof(TEntity).Namespace}.{typeof(TEntity).Name}_ReadAsync";

        dataManagerRequest.LazyExpandAllGroup = true;
        dataManagerRequest.LazyLoad = true;
        string odataQueryParameters = null;

        if (dataManagerRequest.Params is not null)
        {
            // convertToUTtcValue
            if (dataManagerRequest.Params.TryGetValue(Litterals.ConvertToUTtc, out var convertToUTtcValue))
                _convertToLocalDateTime = !Convert.ToBoolean(convertToUTtcValue);

            odataQueryParameters = dataManagerRequest.Params.TryGetValue(Litterals.OdataQueryParameters, out object queryParameters)
                ? queryParameters?.ToString()
                : string.Empty;
        }

        // Convert Date to UTC for filtering 
        if (dataManagerRequest.Where?[0].predicates != null)
        {
            // Get filtered items
            var predicateList = dataManagerRequest.Where[0].predicates;

            // OB-226: get sub predicates when any (means several parameters)
            var subPredicates = predicateList.First().predicates;

            if (subPredicates is not null)
                predicateList = subPredicates;

            // Check for Date fields => when any then try to convert/adjust Dates
            for (int index = 0; index < predicateList.Count; index++)
            {
                var predicate = predicateList[index];

                switch (predicate.value)
                {
                    case null:
                        continue;

                    // OB-150: Value Kind (refresh case => converted to UTC at first)
                    case JsonElement { ValueKind: JsonValueKind.String } jsonElement
                        when jsonElement.TryGetDateTimeOffset(out var content):
                        {
                            // Breaking changes since version 20.3.0.57, but not documented by SF
                            predicate.value = ConvertDate(content, false); // !Convert.ToBoolean(convertToUTtcValue));
                            break;
                        }

                    default:
                        if (predicate.value is DateTime or DateTimeOffset)
                            predicate.value = ConvertDate(predicate.value, false); // !Convert.ToBoolean(convertToUTtcValue));
                        break;
                }
            }
        }

        _cacheKey = $"{nameof(IWasmDataAdaptor<TEntity>)}-{entity}.{GetSignature(dataManagerRequest, odataQueryParameters)}";
        //_logger.LogWarning($"[DGB] Key: {_cacheKey}, Count: {(_cache as MemoryCache)?.Count}");

        if (_proxyCore.IsCacheEnabled)
        {
            // Get or add all TEntity datas (cache used)
            _dataSource = await _cache.GetOrCreateAsync(_cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                var dataResult = await _proxyCore.GetPagedQueryableAsync<TEntity>(dataManagerRequest, odataQueryParameters);

                if (dataResult.Count > 0)
                {
                    // Convert Dates to user's LocalDates
                    dataResult.Result = _convertToLocalDateTime
                        ? OdataEntities.ConvertDatesToLocaleDateTime(OdataEntities.DeserializeEntity<TEntity>(dataResult.Result))
                        : OdataEntities.DeserializeEntity<TEntity>(dataResult.Result);
                }

                return dataResult;
            });
        }
        else
        {
            // Get all TEntity datas (cache not used)
            _dataSource = await _proxyCore.GetPagedQueryableAsync<TEntity>(dataManagerRequest, odataQueryParameters);

            if (_dataSource.Count > 0)
            {
                // Convert Dates to user's LocalDates
                _dataSource.Result = _convertToLocalDateTime
                    ? OdataEntities.ConvertDatesToLocaleDateTime(OdataEntities.DeserializeEntity<TEntity>(_dataSource.Result))
                    : OdataEntities.DeserializeEntity<TEntity>(_dataSource.Result);
            }
        }

        // When exception has been previously raised within GetPagedQueryableAsync, its count is less than 0 
        if (_dataSource.Count < 0)
        {
            if (_cache is not null)
            {
                _cache.Remove(_cacheKey);
                _cacheKey = null;
            }

            // Filter status code, since 401 is automatically managed, we can get rid off alerts for this status
            if (_dataSource.Count != Litterals.AuthUnAuthorized)
            {
                var statusCode = Math.Abs(_dataSource.Count);
                throw new InvalidDataException($"Error: {statusCode} ({((HttpStatusCode)statusCode).ToString()})<br />Table: {typeof(TEntity).Name}");
            }
        }

        if (dataManagerRequest.Group is null || _dataSource.Count < 1)
        {
            // var test = _dataSource.Result.Cast<TEntity>().ToList(); // For testing purpose only!
            return dataManagerRequest.RequiresCounts
                ? _dataSource
                : _dataSource.Result;
        }

        // Grouping
        var data = _dataSource.Result;
        Parallel.ForEach(dataManagerRequest.Group, group
            => data = DataUtil.Group<TEntity>(data, group, dataManagerRequest.Aggregates, 0, dataManagerRequest.GroupByFormatter));

        return dataManagerRequest.RequiresCounts
            ? new DataResult
            {
                Result = data, // .Cast<TEntity>().ToList() // For testing purpose only!
                Count = _dataSource.Count
            }
            : data;
    }

    /// <summary>
    /// Performs Insert (uses UTC and/or Local DateTime conversion mechanism related to _convertToLocalDateTime flag)
    /// </summary>
    /// <param name="dataManager"></param>
    /// <param name="data"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public override async Task<object> InsertAsync(DataManager dataManager, object data, string key)
    {
        IList<TEntity> datas = new List<TEntity> { (TEntity)data };
        var apiResult = await _proxyCore.InsertAsync(datas, convertToLocalDateTime: !_convertToLocalDateTime);

        if (apiResult.Count > 0)
        {
            // Yes we can remove this key because the datagrid needs to get fresh datas
            if (_cacheKey is null)
                return data;

            _proxyCore.CacheRemoveEntities(typeof(TEntity));
            _cacheKey = null;
        }
        else
        {
            _uiLogger?.LogError("Error: {0}", apiResult.Message);
            throw new InvalidDataException(apiResult.Message);
        }

        return data;
    }

    /// <summary>
    /// Performs Remove
    /// </summary>
    /// <param name="dataManager"></param>
    /// <param name="data"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public override async Task<object> RemoveAsync(DataManager dataManager, object data, string keyField, string key)
    {
        var apiResult = await _proxyCore.DeleteAsync<TEntity>(new[] { data?.ToString() });

        if (apiResult.Count > 0)
        {
            // Yes we can remove this key because the datagrid needs to get fresh datas
            if (_cacheKey is not null)
            {
                _proxyCore.CacheRemoveEntities(typeof(TEntity));
                _cacheKey = null;
            }
        }
        else
        {
            _uiLogger?.LogError("Error: {0}", apiResult.Message);
            throw new InvalidDataException(apiResult.Message);
        }

        return data;
    }

    /// <summary>
    /// Check if item exists and contains same datas in database
    /// </summary>
    /// <param name="item"></param>
    /// <param name="keyField"></param>
    /// <returns>If items exist and are equal as what we have in database then true, else false</returns>
    private async Task<bool> CompareWithExistingData(IList<TEntity> item, string keyField)
    {
        if (keyField == null)
            throw new ArgumentNullException(nameof(keyField));

        object keyValue = item.Select(el => typeof(TEntity).GetProperty(keyField)?.GetValue(el, null)).FirstOrDefault();

        if (keyValue == null)
            return false;

        var data = keyValue.GetType().Name switch
        {
            nameof(String) => await _proxyCore.GetEnumerableAsync<TEntity>($"?$filter={keyField} eq '{keyValue}'", false, _convertToLocalDateTime),
            _ => await _proxyCore.GetEnumerableAsync<TEntity>($"?$filter={keyField} eq {keyValue}", false, _convertToLocalDateTime)
        };

        var enumerable = data as TEntity[] ?? data.ToArray();
        var equality = enumerable.Any() && SerializeToUtf8Bytes(enumerable).SequenceEqual(SerializeToUtf8Bytes(item)); // Better comparer option

        return equality;
    }

    /// <summary>
    /// Performs Update (uses UTC and/or Local DateTime conversion mechanism related to _convertToLocalDateTime flag) 
    /// </summary>
    /// <param name="dataManager"></param>
    /// <param name="data"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public override async Task<object> UpdateAsync(DataManager dataManager, object data, string keyField, string key)
    {
        IList<TEntity> item = new List<TEntity> { (TEntity)data };

        // Update entity through odata API, but ONLY if datas are different in database
        if (await CompareWithExistingData(item, keyField))
        {
            return data;
        }

        // Then update entity
        var apiResult = await _proxyCore.UpdateAsync(item, convertToLocalDateTime: !_convertToLocalDateTime);

        if (apiResult.Count > 0)
        {
            // Yes we can remove this key because the datagrid needs to get fresh datas
            if (_cacheKey is null)
            {
                return data;
            }

            _proxyCore.CacheRemoveEntities(typeof(TEntity));
            _cacheKey = null;
        }
        else
        {
            _uiLogger?.LogError("Error: {0}", apiResult.Message);
            throw new InvalidDataException(apiResult.Message);
        }

        return data;
    }

    /// <summary>
    /// Get all PK values at once
    /// </summary>
    /// <param name="enumerables"></param>
    /// <param name="propertyName"></param>
    /// <returns>List of Ids</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static IEnumerable<string> GetKeyFieldValues(IEnumerable<TEntity> enumerables, string propertyName)
        => enumerables?.Select(item => typeof(TEntity).GetProperty(propertyName)?.GetValue(item, null)?.ToString());

    /// <summary>
    /// Performs BatchUpdate operation
    /// </summary>
    /// <param name="dataManager"></param>
    /// <param name="changedRecords"></param>
    /// <param name="addedRecords"></param>
    /// <param name="deletedRecords"></param>
    /// <param name="keyField"></param>
    /// <param name="key"></param>
    /// <param name="dropIndex"></param>
    /// <returns>default</returns>
    public override async Task<object> BatchUpdateAsync(DataManager dataManager, object changedRecords, object addedRecords,
        object deletedRecords, string keyField, string key, int? dropIndex)
    {
        // Updating
        if (changedRecords != null)
        {
            var data = JsonSerializer.Deserialize<IEnumerable<TEntity>>(JsonSerializer.SerializeToUtf8Bytes(changedRecords));
            var result = await _proxyCore.UpdateAsync(data, modeBulk: false);
        }

        // Adding
        if (addedRecords != null)
        {
            var data = JsonSerializer.Deserialize<IEnumerable<TEntity>>(JsonSerializer.SerializeToUtf8Bytes(addedRecords));
            var result = await _proxyCore.InsertAsync(data, modeBulk: false);
        }

        // Deleting
        if (deletedRecords != null)
        {
            var data = JsonSerializer.Deserialize<IEnumerable<TEntity>>(JsonSerializer.SerializeToUtf8Bytes(deletedRecords));
            var result = await _proxyCore.DeleteAsync<TEntity>(GetKeyFieldValues(data, keyField).ToList());
        }

        return changedRecords;
    }

    /// <summary>
    /// Convert DataManagerRequest to an encrypted sha256, 32 bytes signature.
    /// </summary>
    /// <param name="dm">Data manager request from DataGrid component.</param>
    /// <param name="odata">Odata request applied to DataGrid.</param>
    /// <returns>Signature: unique key representing the data manager request.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private static string GetSignature(DataManagerRequest dm, string odata)
    {
        // Serialize parameters to have a table of bytes.
        byte[] buffer = SerializeToUtf8Bytes(new { dm, V = string.IsNullOrEmpty(odata) ? "NO-DATA" : odata });

        // Generate SHA-256 hash.
        using var mySha256 = SHA256.Create();

        // Convert each byte to a string.
        var hash = mySha256
            .ComputeHash(buffer)
            .Select(h => h.ToString("X2")); // hex mode, shorter and better

        // Join hash in a single string.
        return string.Join(string.Empty, hash);
    }
}