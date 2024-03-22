using Krialys.Orkestra.Common;
using Krialys.Orkestra.Common.Exceptions;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.Common.Models.Email;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.Common.Shared;
using Microsoft.AspNetCore.JsonPatch;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Dynamic;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Logs;
using static Krialys.Orkestra.Common.Shared.Univers;

// Application name (based on its Namespace) => https://github.com/reactiveui/refit
namespace Krialys.Orkestra.WebApi.Proxy;

/// <summary>
/// Define your interface, then use it via ProxyCore
/// Will automatically transmit authorization to the callee (claims...)
/// </summary>
public interface IHttpProxyCore
{
    /// <summary>
    /// Application session id
    /// </summary>
    string ApplicationClientSessionId { get; }

    /// <summary>
    /// [CRUD] [Read] TEntity as IEnumerable
    /// <br/>If no data, the function always return an empty object instance, not null (best practice).
    /// If you need more than 1000 items, then use $top=xxx
    /// </summary>
    /// <typeparam name="TEntity">Entity name</typeparam>
    /// <param name="queryOptions">Odata query</param>
    /// <param name="useCache">Use cache by default</param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns>IEnumerable of typed TEntity</returns>
    ValueTask<IEnumerable<TEntity>> GetEnumerableAsync<TEntity>(string queryOptions = null, bool useCache = true, bool convertToLocalDateTime = true);

    /// <summary>
    /// [CRUD] [Read] TEntity as IDictionary
    /// <br/>If no data, the function always return an empty object instance (best practice)
    /// If you need more than 1000 items, then use $top=xxx
    /// </summary>
    /// <typeparam name="TEntity">Entity name</typeparam>
    /// <param name="entityPkName">PK values</param>
    /// <param name="queryOptions">Odata query</param>
    /// <param name="useCache">Use cache by default</param>
    /// <returns>IDictionary</returns>
    ValueTask<IEnumerable<IDictionary<string, object>>> GetDictionaryAsync<TEntity>(string entityPkName, string queryOptions = null, bool useCache = true);

    /// <summary>
    /// [CRUD] [Read] TEntity as PagedQueryable
    /// Get Paged TEntity using Blazor DataManagerRequest object (Mimmic DataAdapter's ReadAsync), can be used from any Component
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="dataManagerRequest"></param>
    /// <param name="queryOptions"></param>
    /// <returns></returns>
    ValueTask<DataResult> GetPagedQueryableAsync<TEntity>(DataManagerRequest dataManagerRequest, string queryOptions);

    /// <summary>
    /// Get one or more data columns from a given entity
    /// <br />Never use $top with $apply=groupby
    /// </summary>
    /// <param name="queryOptions">Odata parameters</param>
    /// <param name="propertyNames">List of properties. If no property is set, then all properties will be retrieved</param>
    /// <returns></returns>
    ValueTask<IEnumerable<ExpandoObject>> GetSelectExpandoFromAsync<TEntity>(string queryOptions = null, string[] propertyNames = null);

    /// <summary>
    /// [CRUD] [Update] TEntity (does not use any UTC and/or Local DateTime conversion mechanism)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityList"></param>
    /// <param name="modeBulk"></param>
    /// <param name="convertToLocalDateTime">false by default</param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    ValueTask<ApiResult> UpdateAsync<TEntity>(IEnumerable<TEntity> entityList, bool modeBulk = false, bool convertToLocalDateTime = false);

    /// <summary>
    /// [CRUD] [Patch] TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityIds"></param>
    /// <param name="patchDocs"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    ValueTask<ApiResult> PatchAsync<TEntity>(IEnumerable<object> entityIds, IEnumerable<JsonPatchDocument<TEntity>> patchDocs) where TEntity : class;

    /// <summary>
    /// [CRUD] [Delete] TEntity
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityIdList"></param>
    /// <param name="modeBulk"></param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    ValueTask<ApiResult> DeleteAsync<TEntity>(IEnumerable<string> entityIdList, bool modeBulk = false);

    /// <summary>
    /// [CRUD] [Create] TEntity (does not use any UTC and/or Local DateTime conversion mechanism)
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="entityList"></param>
    /// <param name="modeBulk"></param>
    /// <param name="convertToLocalDateTime">false by default</param>
    /// <returns>-1 when error, else any value greater or equals to 0</returns>
    ValueTask<ApiResult> InsertAsync<TEntity>(IEnumerable<TEntity> entityList, bool modeBulk = false, bool convertToLocalDateTime = false);

    /// <summary>
    /// Execute a raw SQL
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="rawSql"></param>
    /// <param name="useCache"></param>
    /// <returns>ExpandoObject list</returns>
    ValueTask<IEnumerable<IDictionary<string, object>>> GetAllSqlRaw<TEntity>(string rawSql, bool useCache = true);

    /// <summary>
    /// Execute a raw SQL
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="rawSql"></param>
    /// <param name="useCache"></param>
    /// <param name="convertToLocalDateTime"></param>
    /// <returns>TOutput list</returns>
    ValueTask<IEnumerable<TResult>> GetAllSqlRaw<TEntity, TResult>(string rawSql, bool useCache = true, bool convertToLocalDateTime = true);

    /// <summary>
    /// Performs File removal
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileNames"></param>
    /// <returns></returns>
    ValueTask<KeyValuePair<string, string>> DeleteFiles(string folderName, string[] fileNames);

    /// <summary>
    /// Performs Directory content removal
    /// </summary>
    /// <param name="folderName"></param>
    /// <returns></returns>
    ValueTask<KeyValuePair<string, string>> DeleteDirectory(string folderName);

    /// <summary>
    /// Get specific key value from ApiUnivers appsettings.{env}.json
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    ValueTask<object> GetApiUniversAppSettings(string key);

    /// <summary>
    /// Implement rules for checking environment directory based on EnvId
    /// Implement insert/update TEB_ETAT_BATCHS
    /// Implement environment zip
    /// Implement update TE_ETATS
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="checkOnly"></param>
    /// <returns>Report status</returns>
    ValueTask<ApiResult> GetEnvironmentToc(string envId, bool checkOnly);

    /// <summary>
    /// Delete cache entries related to entities types list
    /// </summary>
    int CacheRemoveEntities(params Type[] entities);

    /// <summary>
    /// Empty cache
    /// </summary>
    void CacheClear();

    /// <summary>
    /// Get or set cache status mode
    /// </summary>
    /// <param name="disableCache">When null, then it returns a boolean value checking if cache has been enabled or not.
    /// <br />When true, then the cache is told to be disabled.
    /// <br />When false, then the cache is told to be enabled.
    /// </param>
    /// <returns>False if cache does not exist or if no 'cache key' has been found, else will return true</returns>
    bool GetOrSetDisablingCacheStatus(bool? disableCache = null);

    /// <summary>
    /// Get cache status
    /// </summary>
    bool IsCacheEnabled { get; }

    /// <summary>
    /// Performs send mail
    /// </summary>
    /// <param name="template">Email</param>
    /// <returns>Task</returns>
    ValueTask<bool> SendGenericMail(EmailTemplate template);

    /// <summary>
    /// Performs send mail from automated rules<br/>
    /// [!] Errors are logged to logUnivers.
    /// </summary>
    /// <param name="demandeId"></param>
    /// <param name="typeCode"></param>
    /// <returns>Boolean indicating success(true) or failure(false)</returns>
    ValueTask<bool> SendAutomatedMailForRequest(int demandeId, string typeCode);

    #region TRACKEDENTITIES

    /// <summary>
    /// Get online CPU
    /// </summary>
    /// <returns></returns>
    ValueTask<IEnumerable<IDictionary<string, object>>> GetOnlineCPU();

    /// <summary>
    /// Start or suspend CPU
    /// </summary>
    /// <param name="off"></param>
    /// <returns></returns>
    ValueTask MainEntryPoint(int off);

    /// <summary>
    /// Get latest tracked entities
    /// </summary>
    /// <param name="entityType">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable TrackedEntities (ordered by date descending)</returns>
    ValueTask<IEnumerable<TrackedEntity>> GetTrackedEntities(string entityType, DateTime lastChecked);

    ValueTask<IEnumerable<TrackedEntity>> GetTrackedEntities<TEntity>(DateTime lastChecked);

    ValueTask<bool> AddTrackedEntity(string[] fullNames, string verb, string userId, string uuid);

    ValueTask<IEnumerable<WorkerNodeLog>> GetTrackedLogger(string cpuId, DateTime lastChecked);

    #endregion

    #region CATALOG (ETQ)

    /// <summary>
    /// Calculate new Etiquette, but only if Simulation flag equals false
    /// </summary>
    /// <param name="etqInput"></param>
    /// <returns>EtqOutput object</returns>
    ValueTask<IEnumerable<EtqOutput>> EtqCalculate(IEnumerable<CalculateEtqInput> etqInput);

    /// <summary>
    /// Apply rules onto an Etiquette (not exposed in ETL catalog)
    /// </summary>
    /// <param name="etqRules"></param>
    /// <returns>Returns true if rules are correct</returns>
    ValueTask<IEnumerable<EtqOutput>> EtqApplyRules(IEnumerable<EtqRules> etqRules);

    /// <summary>
    /// Injection des données Suivi ressource d'une étiquette 
    /// 1 a N enregistrements pour une etiquette
    /// </summary>
    /// <param name="etqSuiviRessources"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<EtqOutput>> EtqSuiviRessource(IEnumerable<EtqSuiviRessourceFileRaw> etqSuiviRessources);

    /// <summary>
    /// Injection des données de libellés d'une étiquette
    /// 1 a N enregistrements pour une etiquette
    /// </summary>
    /// <param name="etqExtraInfoAddonContent"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<EtqOutput>> EtqExtraInfoAddon(IEnumerable<EtqExtraInfoAddonFileRaw> etqExtraInfoAddonContent);

    /// <summary>
    /// Get detailled etiquettes.
    /// </summary>
    /// <param name="queryOptions">Odata query applied on data.</param>
    /// <param name="pageSize">Number of items in page.</param>
    /// <returns>OData</returns>
    ValueTask<ODataContract<EtiquetteDetails>> GetEtiquetteDetails(string queryOptions = null, int pageSize = 20);

    /// <summary>
    /// Apply authorizations on a label.
    /// </summary>
    /// <param name="id">Id of the label.</param>
    /// <param name="args">Label authorization arguments.</param>
    /// <returns>Http response message.</returns>
    ValueTask<HttpResponseMessage> SetEtiquetteAuthorizations(int id, EtqAuthorizationArguments args);
    #endregion

    #region AUTHENTICATION
    ValueTask<string> CreateCipheredPassword(string clearPassword);

    ValueTask<bool> IsValidPassword(string cipherPassword, string clearPassword);
    #endregion

    #region LOGS
    /// <summary>
    /// Capture list of LogException then send it to LogUnivers
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    ValueTask SetLogException(IEnumerable<LogException> exception);

    /// <summary>
    /// Capture a LogException then send it to LogUnivers
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    ValueTask SetLogException(LogException exception);
    #endregion

    #region AUTHORIZATION RULES (DTF)

    /// <summary>
    /// DTF : Liste des Etats eligibles avec role de producteur
    /// </summary>
    /// <param name="isAdminMode"></param>
    /// <param name="userId"></param>
    /// <param name="catId"></param>
    /// <returns></returns>
    ValueTask<IEnumerable<TEntity>> GetExecutableTeEtatsForDTFAsync<TEntity>(bool isAdminMode, string userId, int catId) where TEntity : class, new();

    #endregion

    #region CALENDAR (DTF)

    /// <summary>
    /// Calcul des demandes théoriques via décodage du CRON par demande éligibles
    /// </summary>
    /// <param name="dtUtcStart">Date de départ (UTC)</param>
    /// <param name="dtUtcEnd">Date de fin (UTC)</param>
    /// <param name="useCache"></param>
    /// <returns>List of ModeleDemandeCalendar</returns>
    ValueTask<IEnumerable<ModeleDemandeCalendar>> GetTheoricalDemandesCalendar(DateTimeOffset dtUtcStart, DateTimeOffset dtUtcEnd, bool useCache);

    #endregion

    #region ORDERS (DTS)
    /// <summary>
    /// Delete an order (TCMD_COMMANDES).
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    /// <returns>Http response message.</returns>
    ValueTask<HttpResponseMessage> DeleteOrder(int id);

    /// <summary>
    /// Change phase of an order.
    /// </summary>
    /// <param name="id">Id of the order.</param>
    /// <param name="args">Change order phase arguments.</param>
    /// <returns>Http response message.</returns>
    ValueTask<HttpResponseMessage> ChangeOrderPhase(int id, ChangeOrderPhaseArguments args);

    /// <summary>
    /// Create a copy of an existing order.
    /// </summary>
    /// <param name="id">Id of the order.</param>
    /// <returns>Http response message.</returns>
    ValueTask<HttpResponseMessage> DuplicateOrder(int id);

    /// <summary>
    /// Read productions associated with an order.
    /// </summary>
    /// <param name="orderId">Id of the order.</param>
    /// <returns>Http response message.</returns>
    ValueTask<HttpResponseMessage> GetProductionsAssociatedWithAnOrderAsync(int orderId);
    #endregion
    
    #region DataProcessingUnit

    ValueTask<bool> VersionDuplicate(int dpuIdToDuplicate);

    #endregion

    #region LOGSUNIVERS
    // ETL Logs
    ValueTask<IEnumerable<EtlLogExceptionGridLevel1>> GetEtlLogExceptionGridLevel1(DateTime startDate, DateTime endDate, bool useCache = true);

    ValueTask<IEnumerable<EtlLogExceptionGridLevel2>> GetEtlLogExceptionGridLevel2(DateTime startDate, DateTime endDate, int demandeId, string workFlow, bool useCache = true);
    #endregion
}