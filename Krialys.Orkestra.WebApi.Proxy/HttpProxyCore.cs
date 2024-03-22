using Krialys.Common.LZString;
using Krialys.Orkestra.Common;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Exceptions;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.Common.Models.Email;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.Common.Shared;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Collections;
using System.Dynamic;
using System.Net;
using System.Reflection;
using System.Text;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Logs;
using static Krialys.Orkestra.Common.Shared.Univers;
using static System.Text.Json.JsonSerializer;

// Application name (based on its Namespace) => https://github.com/reactiveui/refit
namespace Krialys.Orkestra.WebApi.Proxy;

[Microsoft.AspNetCore.Authorization.Authorize]
public class HttpProxyCore : IHttpProxyCore
{
    private readonly IHttpProxyDef _httpProxyDef;
    private readonly IMemoryCache _cache;
    private readonly ILogger<HttpProxyCore> _logger;

    public HttpProxyCore(IHttpProxyDef httpProxyDef, IMemoryCache cache, ILogger<HttpProxyCore> logger)
    {
        _httpProxyDef = httpProxyDef;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Application session id
    /// </summary>
    public string ApplicationClientSessionId
    {
        get
        {
            return _httpProxyDef.Client.DefaultRequestHeaders.TryGetValues(Litterals.ApplicationClientSessionId, out var sessionId)
                ? sessionId.FirstOrDefault()
                : string.Empty;
        }
    }

    /// <summary>
    /// Get data from any known Entity as ODataContract
    /// This function is the most advanced scenario since it allows to get Exception as well
    /// <br/>If no data, the function always return an empty object instance, not null (best practice)
    /// </summary>
    /// <typeparam name="TEntity">Entity name</typeparam>
    /// <param name="queryOptions">Odata query</param>
    /// <param name="useCache">Use cache by default</param>
    /// <param name="convertToLocalDateTime">Convert resulting Dates as users's LocalDates by default</param>
    /// <returns>ODataContract of typed TEntity</returns>
    public async ValueTask<IEnumerable<TEntity>> GetEnumerableAsync<TEntity>(string queryOptions = null, bool useCache = true, bool convertToLocalDateTime = false)
    {
        var dbmsName = GetDbmsName<TEntity>();
        var entityName = typeof(TEntity).Name;
        var key = $"{nameof(IHttpProxyCore)}-{typeof(TEntity).Namespace}.{dbmsName}.{entityName}_ReadAsync-{queryOptions}";

        useCache &= IsCacheEnabled;

        using var datas = useCache
            ? await _cache?.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                return await _httpProxyDef.GetPagedQueryableAsync(dbmsName, entityName, queryOptions, null);
            })!
            : await _httpProxyDef.GetPagedQueryableAsync(dbmsName, entityName, queryOptions, null);

        // Convert resulting Dates as users's LocalDates.
        return datas is { IsSuccessStatusCode: true, Content: not null }
            ? OdataEntities.GetEntities(Deserialize<IEnumerable<TEntity>>(SerializeToUtf8Bytes(datas.Content.Result)), convertToLocalDateTime)
            : Enumerable.Empty<TEntity>();
    }

    /// <summary>
    /// Get data from any known Entity as IDictionary
    /// <br/>If no data, the function always return an empty object instance, not null (best practice)
    /// </summary>
    /// <typeparam name="TEntity">Entity name</typeparam>
    /// <param name="entityPkName">PK values</param>
    /// <param name="queryOptions">Odata query</param>
    /// <param name="useCache">Use cache by default</param>
    /// <returns>IEnumerable IDictionary</returns>
    public async ValueTask<IEnumerable<IDictionary<string, object>>> GetDictionaryAsync<TEntity>(string entityPkName, string queryOptions = null, bool useCache = true)
    {
        var dbmsName = GetDbmsName<TEntity>();
        var key = $"{nameof(IHttpProxyCore)}-{typeof(TEntity).Namespace}.{entityPkName}_ReadAsync-{queryOptions}";

        useCache &= IsCacheEnabled;

        using var datas = useCache
                ? await _cache?.GetOrCreateAsync(key, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                    return await _httpProxyDef.GetPagedQueryableAsync(dbmsName, entityPkName, queryOptions, null);
                })!
                : await _httpProxyDef.GetPagedQueryableAsync(dbmsName, entityPkName, queryOptions, null);

        return datas is { IsSuccessStatusCode: true, Content: not null }
            ? Deserialize<IEnumerable<IDictionary<string, object>>>(SerializeToUtf8Bytes(datas.Content.Result))
            : Enumerable.Empty<IDictionary<string, object>>();
    }

    /// <summary>
    /// Execute a raw SQL (with cache enabled by default)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="rawSql"></param>
    /// <param name="useCache"></param>
    /// <returns>IEnumerable ExpandoObject</returns>
    public async ValueTask<IEnumerable<IDictionary<string, object>>> GetAllSqlRaw<TEntity>(string rawSql, bool useCache = true)
    {
        var sql = LZString.CompressToEncodedURIComponent(rawSql);
        var dbmsName = GetDbmsName<TEntity>();
        var entityName = typeof(TEntity).Name;
        var key = $"{nameof(IHttpProxyCore)}-{typeof(TEntity).Namespace}.{dbmsName}.{entityName}{sql}_GetAllSqlRaw";

        useCache &= IsCacheEnabled;

        using var datas = useCache
            ? await _cache?.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                return await _httpProxyDef.GetAllSqlRawAsync(dbmsName, entityName, SerializeToUtf8Bytes(sql));
            })!
            : await _httpProxyDef.GetAllSqlRawAsync(dbmsName, entityName, SerializeToUtf8Bytes(sql));

        return datas is { IsSuccessStatusCode: true, Content: not null }
            ? datas.Content
            : Enumerable.Empty<IDictionary<string, object>>();
    }

    /// <summary>
    /// Execute a raw SQL (with cache enabled by default)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="rawSql"></param>
    /// <param name="useCache"></param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns>IEnumerable TResult</returns>
    public async ValueTask<IEnumerable<TResult>> GetAllSqlRaw<TEntity, TResult>(string rawSql, bool useCache = true, bool convertToLocalDateTime = true)
    {
        useCache &= IsCacheEnabled;

        var data = await GetAllSqlRaw<TEntity>(rawSql, useCache);
        var value = data as IDictionary<string, object>[] ?? data.ToArray();

        if (!value.Any())
        {
            return Enumerable.Empty<TResult>();
        }

        var result = Deserialize<IEnumerable<TResult>>(SerializeToUtf8Bytes(value));
        var enumerable = result as TResult[] ?? result.ToArray();

        if (!enumerable.Any())
        {
            return Enumerable.Empty<TResult>();
        }

        result = convertToLocalDateTime
            ? OdataEntities.ConvertDatesToLocaleDateTime(enumerable)
            : enumerable;

        return result.Any()
            ? result
            : Enumerable.Empty<TResult>();
    }

    /// <summary>
    /// Performs files removal
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileNames"></param>
    /// <returns></returns>
    public async ValueTask<KeyValuePair<string, string>> DeleteFiles(string folderName, string[] fileNames) =>
        await _httpProxyDef.DeleteDirectoryFiles(folderName, fileNames, false);

    /// <summary>
    /// Performs directory content removal
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    public async ValueTask<KeyValuePair<string, string>> DeleteDirectory(string folderName) =>
        await _httpProxyDef.DeleteDirectoryFiles(folderName, null, true);

    /// <summary>
    /// Implement rules for checking environment directory based on EnvId
    /// Implement insert/update TEB_ETAT_BATCHS
    /// Implement environment zip
    /// Implement update TE_ETATS
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="checkOnly"></param>
    /// <returns>Report status</returns>
    public async ValueTask<ApiResult> GetEnvironmentToc(string envId, bool checkOnly) =>
        await _httpProxyDef.GetEnvironmentToc(envId, checkOnly);

    /// <summary>
    /// Get specific key value from ApiUnivers appsettings.{env}.json
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async ValueTask<object> GetApiUniversAppSettings(string key)
    {
        var dico = await _cache?.GetOrCreateAsync("GetApiUniversAppSettings", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

            IDictionary<string, object> dictionary = new Dictionary<string, object>();

            try
            {
                // Deserialize JSON and decompress data
                dictionary = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object>>(
                    LZString.DecompressFromUint8Array(await _httpProxyDef.GetApiUniversAppSettings())); // OB-376

                // Find keys with empty/null values and store them in a list
                var keysToRemove = dictionary
                    .Where(kvp => kvp.Value == null || string.IsNullOrEmpty(kvp.Value.ToString()))
                    .Select(kvp => kvp.Key)
                    .ToList();

                // Remove keys with empty/null values from the dictionary
                foreach (var k in keysToRemove)
                    dictionary.Remove(k);
            }
            catch { /* Not used yet */ }

            return dictionary;
        })!;

        return dico.TryGetValue(key, out object value)
            ? value
            : default;
    }

    /// <summary>
    /// Get Paged TEntity using Blazor DataManagerRequest object (Mimmic DataAdapter's ReadAsync), can be used from any Component
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dataManagerRequest"></param>
    /// <param name="queryOptions"></param>
    /// <returns></returns>
    public async ValueTask<DataResult> GetPagedQueryableAsync<TEntity>(DataManagerRequest dataManagerRequest, string queryOptions)
    {
        using var datas = await _httpProxyDef.GetPagedQueryableAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name, queryOptions, dataManagerRequest);

        if (datas is { IsSuccessStatusCode: true, Content: not null })
        {
            return datas.Content
                ?? new DataResult { Result = Enumerable.Empty<TEntity>() };
        }
        else
        {
            return new DataResult
            {
                Count = datas.StatusCode switch
                {
                    HttpStatusCode.NotFound => Litterals.NotFound,
                    HttpStatusCode.Unauthorized => Litterals.AuthUnAuthorized,
                    _ => -1 * (int)datas.StatusCode
                },
                Result = Enumerable.Empty<TEntity>()
            };
        }
    }

    /// <summary>
    /// Get one or more data columns from a given entity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="queryOptions">Odata</param>
    /// <param name="propertyNames">Fields</param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<ExpandoObject>> GetSelectExpandoFromAsync<TEntity>(string queryOptions, string[] propertyNames)
    {
        var datas = await _httpProxyDef.GetSelectExpandoFromAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name, queryOptions, propertyNames);
        var enumerable = datas.ToList();
        var error = ((IDictionary<string, object>)enumerable.FirstOrDefault())!.TryGetValue(Litterals.ApiCallException, out var _);

        return error
            ? null
            : enumerable;
    }

    private static string GetDbmsName<TEntity>()
        => typeof(TEntity).FullName?.Split('.')[3];

    private static string GetDbmsName(Type type)
        => type.FullName?.Split('.')[3];

    private static ApiResult GetApiResult(object result)
        => Deserialize<ApiResult>(SerializeToUtf8Bytes(result));

    /// <summary>
    /// [CRUD] UpdateAsync TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityList"></param>
    /// <param name="modeBulk"></param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    public async ValueTask<ApiResult> UpdateAsync<TEntity>(IEnumerable<TEntity> entityList, bool modeBulk = false, bool convertToLocalDateTime = false)
    {
        // Convert each DateTime/offset to UTC
        // BODY content
        var dmContent = SerializeToUtf8Bytes(OdataEntities.GetEntities(entityList, convertToLocalDateTime));

        // API call
        var apiResult = GetApiResult(await _httpProxyDef.UpdateAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name, dmContent, modeBulk ? 1 : 0));

        return apiResult;
    }

    /// <summary>
    /// [CRUD] UpdateAsync TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityList"></param>
    /// <param name="modeBulk"></param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    public async ValueTask<ApiResult> InsertAsync<TEntity>(IEnumerable<TEntity> entityList, bool modeBulk = false, bool convertToLocalDateTime = false)
    {
        // Convert each DateTime/offset to UTC ?
        // BODY content
        var dmContent = SerializeToUtf8Bytes(OdataEntities.GetEntities(entityList, convertToLocalDateTime));

        // API call
        var apiResult = GetApiResult(await _httpProxyDef.InsertAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name, dmContent, modeBulk ? 1 : 0));

        return apiResult;
    }

    /// <summary>
    /// [CRUD] PatchAsync TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityIds"></param>
    /// <param name="patchDocs"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    public async ValueTask<ApiResult> PatchAsync<TEntity>(IEnumerable<object> entityIds, IEnumerable<JsonPatchDocument<TEntity>> patchDocs) where TEntity : class
    {
        // BODY content (must use Newtonsoft as JSON.NET does not support PATCH contracts yet)
        var patchDocBytes = patchDocs.Select(patchDoc => new UTF8Encoding().GetBytes(JsonConvert.SerializeObject(patchDoc))).ToList();

        // API call
        var apiResult = GetApiResult(await _httpProxyDef.PatchAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name,
            new PatchDoc { PatchDocs = patchDocBytes, EntityIds = entityIds }));

        return apiResult;
    }

    /// <summary>
    /// [CRUD] DeleteAsync TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityIdList"></param>
    /// <param name="modeBulk"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    public async ValueTask<ApiResult> DeleteAsync<TEntity>(IEnumerable<string> entityIdList, bool modeBulk = false)
    {
        // BODY content
        var dmContent = SerializeToUtf8Bytes(entityIdList.Select(id => id).ToArray());

        // API call
        var apiResult = GetApiResult(await _httpProxyDef.DeleteAsync(GetDbmsName<TEntity>(), typeof(TEntity).Name, dmContent, modeBulk ? 1 : 0));

        return apiResult;
    }

    // Helper function to remove items from cache based on a given cache key
    private readonly object _locker = new();

    /// <summary>
    /// Delete cache entries related to entities types list
    /// Compatible with Net Core 6.x
    /// </summary>
    public int CacheRemoveEntities(params Type[] entities)
    {
        int count = 0;

        lock (_locker)
        {
            if (_cache is not null)
            {
                var coherentStateField = typeof(MemoryCache).GetField("_coherentState", BindingFlags.Instance | BindingFlags.NonPublic);
                if (coherentStateField != null)
                {
                    var entriesField = coherentStateField.FieldType.GetField("_entries", BindingFlags.Instance | BindingFlags.NonPublic);
                    var coherentState = coherentStateField.GetValue(_cache);

                    if (entriesField?.GetValue(coherentState) is not IEnumerable keys)
                    {
                        return count;
                    }

                    var dbmsNameCache = new Dictionary<Type, string>();

                    string DbmsName(Type type)
                    {
                        if (dbmsNameCache.TryGetValue(type, out string dbmsName))
                        {
                            return dbmsName;
                        }

                        dbmsName = GetDbmsName(type);
                        dbmsNameCache[type] = dbmsName;

                        return dbmsName;
                    }

                    var cacheKeys = entities.SelectMany(entity =>
                    {
                        var dbmsName = DbmsName(entity);
                        var entityName = entity.Name;

                        return new[]
                        {
                            $"{nameof(IHttpProxyCore)}-{entity.Namespace}.{dbmsName}.{entityName}_ReadAsync",
                            $"IWasmDataAdaptor-{entity.Namespace}.{entityName}_ReadAsync"
                        };
                    }).ToHashSet();

                    object[] enumerable = keys as object[] ?? keys.Cast<object>().ToArray();
                    foreach (var cacheKey in cacheKeys)
                    {
                        foreach (var key in enumerable)
                        {
                            if (key?.GetType().GetProperty("Key")?.GetValue(key) is not string entryKey ||
                                !entryKey.StartsWith(cacheKey)
                                || !_cache.TryGetValue(entryKey, out _))
                            {
                                continue;
                            }

                            _cache.Remove(entryKey);
                            count++;
                        }
                    }
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Clear the cache atomically in a swoop
    /// </summary>
    public void CacheClear()
    {
        if (_cache is MemoryCache memoryCache)
        {
            var status = !IsCacheEnabled;

            memoryCache.Clear();

            if (status)
                GetOrSetDisablingCacheStatus(status);
        }
    }

    /// <summary>
    /// Get cache status
    /// </summary>
    public bool IsCacheEnabled => !GetOrSetDisablingCacheStatus();

    /// <summary>
    /// Get or set cache mode
    /// </summary>
    /// <param name="disableCache">When null, then it returns a boolean value checking if cache has been enabled or not.
    /// <br />When true, then the cache is told to be disabled.
    /// <br />When false, then the cache is told to be enabled.
    /// </param>
    /// <returns>False if cache does not exist or if no 'cache key' has been found, else will return true</returns>
    public bool GetOrSetDisablingCacheStatus(bool? disableCache = null)
    {
        const string cacheKey = "$_cache_key_$";

        if (_cache is MemoryCache)
        {
            if (!disableCache.HasValue)
            {
                _ = _cache.TryGetValue(cacheKey, out bool? result);

                if (!result.HasValue)
                    return false;
                else
                    return result.Value;
            }
            else
            {
                _cache.Remove(cacheKey);

                return _cache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpiration = DateTimeOffset.UtcNow.AddDays(1);
                    if (disableCache.HasValue)
                        entry.SetValue(disableCache.Value);

                    return disableCache != null && disableCache.Value;
                });
            }
        }

        return false;
    }

    /// <summary>
    /// Performs send mail from generic template
    /// </summary>
    /// <param name="template">Email</param>
    /// <returns></returns>
    public async ValueTask<bool> SendGenericMail(EmailTemplate template) =>
        await _httpProxyDef.SendGenericMail(template);

    /// <summary>
    /// Performs send mail from automated rules
    /// </summary>
    /// <param name="demandeId"></param>
    /// <param name="typeCode"></param>
    /// <returns>Tuple</returns>
    public async ValueTask<bool> SendAutomatedMailForRequest(int demandeId, string typeCode) =>
        await _httpProxyDef.SendAutomatedMailForRequest(demandeId, typeCode);

    /// <summary>
    /// Get online CPU
    /// </summary>
    /// <returns></returns>
    public async ValueTask<IEnumerable<IDictionary<string, object>>> GetOnlineCPU()
    {
        try
        {
            return await _httpProxyDef.GetOnlineCPU();
        }
        catch
        {
            return Enumerable.Empty<IDictionary<string, object>>();
        }
    }

    /// <summary>
    /// Start or suspend CPU
    /// </summary>
    /// <param name="off"></param>
    /// <returns></returns>
    public async ValueTask MainEntryPoint(int off)
        => await _httpProxyDef.MainEntryPoint(off);

    #region TRACKEDENTITIES
    /// <summary>
    /// Get latest tracked entities
    /// </summary>
    /// <param name="entityType">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities (ordered by date descending)</returns>
    public async ValueTask<IEnumerable<TrackedEntity>> GetTrackedEntities(string entityType, DateTime lastChecked)
        => await _httpProxyDef.GetTrackedEntities(entityType, lastChecked);

    public async ValueTask<IEnumerable<TrackedEntity>> GetTrackedEntities<TEntity>(DateTime lastChecked)
        => await _httpProxyDef.GetTrackedEntities(typeof(TEntity).FullName, lastChecked);

    public async ValueTask<bool> AddTrackedEntity(string[] fullNames, string verb, string userId, string uuid)
    {
        var fullNames2 = fullNames.Aggregate((x, y) => x + "," + y);

        return await _httpProxyDef.AddTrackedEntity(fullNames2, verb, userId, uuid);
    }

    public async ValueTask<IEnumerable<WorkerNodeLog>> GetTrackedLogger(string cpuId, DateTime lastChecked)
        => await _httpProxyDef.GetTrackedLogger(cpuId, lastChecked);
    #endregion

    #region Catalog (ETQ)
    public async ValueTask<IEnumerable<EtqOutput>> EtqCalculate(IEnumerable<CalculateEtqInput> etqInput)
        => await _httpProxyDef.EtqCalculate(etqInput);

    public async ValueTask<IEnumerable<EtqOutput>> EtqApplyRules(IEnumerable<EtqRules> etqRules)
        => await _httpProxyDef.EtqApplyRules(etqRules);

    public async ValueTask<IEnumerable<EtqOutput>> EtqSuiviRessource(IEnumerable<EtqSuiviRessourceFileRaw> etqSuiviRessources)
        => await _httpProxyDef.EtqSuiviRessource(etqSuiviRessources);

    public async ValueTask<IEnumerable<EtqOutput>> EtqExtraInfoAddon(IEnumerable<EtqExtraInfoAddonFileRaw> etqExtraInfosAddon)
        => await _httpProxyDef.EtqExtraInfoAddon(etqExtraInfosAddon);

    public async ValueTask<ODataContract<EtiquetteDetails>> GetEtiquetteDetails(string queryOptions, int pageSize)
    {
        // Call API.
        var datas = await _httpProxyDef.GetEtiquetteDetails(queryOptions, pageSize);

        // Convert dates to local date time.
        datas.Result = OdataEntities.ConvertDatesToLocaleDateTime(datas.Result);

        return datas;
    }

    public async ValueTask<HttpResponseMessage> SetEtiquetteAuthorizations(int id, EtqAuthorizationArguments args)
        => await _httpProxyDef.SetEtiquetteAuthorizations(id, args);
    #endregion

    #region AUTHENTICATION
    public async ValueTask<string> CreateCipheredPassword(string clearPassword)
        => await _httpProxyDef.CreateCipheredPassword(clearPassword);

    public async ValueTask<bool> IsValidPassword(string cipherPassword, string clearPassword)
        => await _httpProxyDef.IsValidPassword(cipherPassword, clearPassword);
    #endregion

    #region LOGS
    public async ValueTask SetLogException(LogException logException)
    {
        try
        {
            await _httpProxyDef.SetLogException(new List<LogException> { logException });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SetLogException)}::{logException.Message}");
        }
    }

    public async ValueTask SetLogException(IEnumerable<LogException> logExceptions)
    {
        var enumerable = logExceptions as LogException[] ?? logExceptions.ToArray();
        var exceptions = logExceptions as LogException[] ?? enumerable.ToArray();

        try
        {
            await _httpProxyDef.SetLogException(exceptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(SetLogException)}::{enumerable.FirstOrDefault()?.Message}");
        }
    }
    #endregion

    #region AUTHORIZATION RULES (DTF)

    /// <summary>
    /// DTF : Liste des Etats eligibles avec role de producteur
    /// </summary>
    /// <param name="isAdminMode"></param>
    /// <param name="userId"></param>
    /// <param name="catId"></param>
    /// <returns></returns>
    ///
    public async ValueTask<IEnumerable<TEntity>> GetExecutableTeEtatsForDTFAsync<TEntity>(bool isAdminMode, string userId, int catId) where TEntity : class, new()
        => await _httpProxyDef.GetExecutableTeEtatsForDTF<TEntity>(isAdminMode, userId, catId);
    #endregion

    #region CALENDAR (DTF)
    /// <summary>
    /// Calcul des demandes théoriques via décodage du CRON par demandes éligibles
    /// </summary>
    /// <param name="dtUtcStart">Date de départ (UTC)</param>
    /// <param name="dtUtcEnd">Date de fin (UTC)</param>
    /// <param name="useCache">Mettre en cache la requête ou non</param>
    /// <returns>List of ModeleDemandeCalendar</returns>
    public async ValueTask<IEnumerable<ModeleDemandeCalendar>> GetTheoricalDemandesCalendar(DateTimeOffset dtUtcStart, DateTimeOffset dtUtcEnd, bool useCache)
    {
        var model = typeof(ModeleDemandeCalendar);
        var dbmsName = GetDbmsName(model);
        var entityName = model.Name;
        var cacheKeyName = $"{nameof(IHttpProxyCore)}-{model.Namespace}.{dbmsName}.{entityName}_ReadAsync-{dtUtcStart:s}-{dtUtcEnd:s}";

        useCache &= IsCacheEnabled;

        var result = useCache
            ? await _cache?.GetOrCreateAsync(cacheKeyName, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                return await _httpProxyDef.GetTheoricalDemandesCalendar(dtUtcStart, dtUtcEnd);
            })!
            : await _httpProxyDef.GetTheoricalDemandesCalendar(dtUtcStart, dtUtcEnd);

        return result ?? Enumerable.Empty<ModeleDemandeCalendar>();
    }

    #endregion

    #region ORDERS (DTS)
    /// <summary>
    /// Delete an order (TCMD_COMMANDES).
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    /// <returns>Http response message.</returns>
    public async ValueTask<HttpResponseMessage> DeleteOrder(int id)
        => await _httpProxyDef.DeleteOrder(id);

    /// <summary>
    /// Change phase of an order.
    /// </summary>
    /// <param name="id">Id of the order.</param>
    /// <param name="args">Change order phase arguments.</param>
    /// <returns>Http response message.</returns>
    public async ValueTask<HttpResponseMessage> ChangeOrderPhase(int id, ChangeOrderPhaseArguments args)
        => await _httpProxyDef.ChangeOrderPhase(id, args);

    /// <summary>
    /// Create a copy of an existing order.
    /// </summary>
    /// <param name="id">Id of the order.</param>
    /// <returns>Http response message.</returns>
    public async ValueTask<HttpResponseMessage> DuplicateOrder(int id)
        => await _httpProxyDef.DuplicateOrder(id);

    /// <summary>
    /// Read productions associated with an order.
    /// </summary>
    /// <param name="orderId">Id of the order.</param>
    /// <returns>Http response message.</returns>
    public async ValueTask<HttpResponseMessage> GetProductionsAssociatedWithAnOrderAsync(int orderId)
        => await _httpProxyDef.GetProductionsAssociatedWithAnOrderAsync(orderId);
    #endregion
        
    #region DATAPROCESSINGUNIT

    public async ValueTask<bool> VersionDuplicate(int dpuIdToDuplicate)
    {
        if (dpuIdToDuplicate < 1)
            return await ValueTask.FromResult(false);

        try
        {
            return await _httpProxyDef.VersionDuplicate(dpuIdToDuplicate);
        }
        catch
        {
            return await ValueTask.FromResult(false);
        }
    }
    #endregion

    #region LOGSUNIVERS
    /// <summary>
    /// Get ETL Logs for EtlLogException for Datagrid Level 1
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="useCache"></param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<EtlLogExceptionGridLevel1>> GetEtlLogExceptionGridLevel1(DateTime startDate, DateTime endDate, bool useCache = true)
    {
        var dbmsName = GetDbmsName<EtlLogExceptionGridLevel1>();
        var entityName = nameof(EtlLogExceptionGridLevel1);
        var key = $"{nameof(IHttpProxyCore)}-{typeof(EtlLogExceptionGridLevel1).Namespace}.{dbmsName}.{entityName}_ReadAsync-{startDate}_{endDate}";

        useCache &= IsCacheEnabled;

        using var datas = useCache
            ? await _cache?.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                return await _httpProxyDef.GetEtlLogExceptionGridLevel1(startDate, endDate);
            })!
            : await _httpProxyDef.GetEtlLogExceptionGridLevel1(startDate, endDate);

        if (datas is { IsSuccessStatusCode: true, Content: not null })
            return datas.Content;

        return Enumerable.Empty<EtlLogExceptionGridLevel1>();
    }

    public async ValueTask<IEnumerable<EtlLogExceptionGridLevel2>> GetEtlLogExceptionGridLevel2(DateTime startDate, DateTime endDate, int demandeId, string workFlow, bool useCache = true)
    {
        var dbmsName = GetDbmsName<EtlLogExceptionGridLevel2>();
        var entityName = nameof(EtlLogExceptionGridLevel2);
        var key = $"{nameof(IHttpProxyCore)}-{typeof(EtlLogExceptionGridLevel2).Namespace}.{dbmsName}.{entityName}_ReadAsync-{demandeId}_{workFlow}_{startDate}_{endDate}";

        useCache &= IsCacheEnabled;

        using var datas = useCache
            ? await _cache?.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Litterals.AbsoluteExpiration;

                return await _httpProxyDef.GetEtlLogExceptionGridLevel2(startDate, endDate, demandeId, workFlow);
            })!
            : await _httpProxyDef.GetEtlLogExceptionGridLevel2(startDate, endDate, demandeId, workFlow);

        if (datas is { IsSuccessStatusCode: true, Content: not null })
            return datas.Content;

        return Enumerable.Empty<EtlLogExceptionGridLevel2>();
    }
    #endregion
}