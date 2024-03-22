using Krialys.Orkestra.Common;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Exceptions;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Common.Models.Email;
using Krialys.Orkestra.Common.Models.WorkerNode;
using Krialys.Orkestra.Common.Shared;
using Refit;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;
using System.Dynamic;
using static Krialys.Orkestra.Common.Shared.ETQ;
using static Krialys.Orkestra.Common.Shared.Logs;
using static Krialys.Orkestra.Common.Shared.Univers;

// *** DECLARE APIS PROTOTYPES HERE *** => https://github.com/reactiveui/refit
// TODO : replace ApiResult by refit's ApiResponse 
namespace Krialys.Orkestra.WebApi.Proxy;

/// <summary>
/// Define your interface, then use it via ProxyCore
/// Will automatically transmit authorization to the callee (claims...)
/// </summary>
[Headers("Authorization: Bearer", "X-Sender: Orkestra v2")]
public interface IHttpProxyDef
{
    // This will automagically be populated by Refit if the property exists
    HttpClient Client { get; }

    #region FILES
    /// <summary>
    /// Performs File and/or Directory removal
    /// </summary>
    /// <param name="folderName"></param>
    /// <param name="fileNames"></param>
    /// <param name="deleteFolder"></param>
    /// <returns></returns>
    [Post($"/{Litterals.UniversRootPath}/File/DeleteDirectoryFiles")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<KeyValuePair<string, string>> DeleteDirectoryFiles([Query] string folderName, [Query] string[] fileNames, [Query] bool deleteFolder);

    /// <summary>
    /// Implement rules for checking environment directory based on EnvId
    /// Implement insert/update TEB_ETAT_BATCHS
    /// Implement environment zip
    /// Implement update TE_ETATS
    /// </summary>
    /// <param name="envId"></param>
    /// <param name="checkOnly"></param>
    /// <returns>Report status</returns>
    [Get($"/{Litterals.UniversRootPath}/File/GetEnvironmentToc")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<ApiResult> GetEnvironmentToc([Query] string envId, [Query] bool checkOnly);
    #endregion

    #region SETTINGS
    /// <summary>
    /// Get All keys/values from ApiUnivers appsettings.{env}.json
    /// </summary>
    /// <returns></returns>
    [Post($"/{Litterals.CommonRootPath}/GetApiUniversAppSettings")]
    Task<byte[]> GetApiUniversAppSettings();
    #endregion

    #region CRUD
    /// <summary>
    /// CRUD [CREATE] InsertAsync
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="content"></param>
    /// <param name="modeBulk"></param>
    /// <returns>ApiResult</returns>
    [Post($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/InsertAsync")]
    Task<ApiResult> InsertAsync(string dbmsName, string entityName, [Body] byte[] content, [Header("modeBulk")] int modeBulk);

    /// <summary>
    /// CRUD [READ] GetPagedQueryableAsync
    /// Get custom paged data list
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="queryOptions"></param>
    /// <param name="content"></param>
    /// <returns>ApiResult</returns>
    [Post($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/GetPagedQueryable/{{queryOptions}}")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<IApiResponse<DataResult>> GetPagedQueryableAsync(string dbmsName, string entityName, [Query] string queryOptions, [Body] DataManagerRequest content);

    /// <summary>
    /// Get one or more data columns from a given entity
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="queryOptions"></param>
    /// <param name="propertyNames"></param>
    /// <returns></returns>
    [Post($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/GetSelectExpandoFrom/{{queryOptions}}")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<IEnumerable<ExpandoObject>> GetSelectExpandoFromAsync(string dbmsName, string entityName, [Query] string queryOptions, [Body] string[] propertyNames);

    /// <summary>
    /// CRUD [READ] GetAllSqlRawAsync
    /// Get custom data list from raw SQL
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="sqlRawBytes"></param>
    /// <returns>ExpandoObject IEnumerable</returns>
    [Post($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/GetAllSqlRawAsync")]
    Task<IApiResponse<IEnumerable<IDictionary<string, object>>>> GetAllSqlRawAsync(string dbmsName, string entityName, [Body] byte[] sqlRawBytes);

    /// <summary>
    /// CRUD [UPDATE] UpdateAsync
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="content"></param>
    /// <param name="modeBulk"></param>
    /// <returns>ApiResult</returns>
    [Put($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/UpdateAsync")]
    Task<ApiResult> UpdateAsync(string dbmsName, string entityName, [Body] byte[] content, [Header("modeBulk")] int modeBulk);

    /// <summary>
    /// CRUD [DELETE] DeleteAsync
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="patchDocs"></param>
    /// <returns>ApiResult</returns>
    [Patch($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/PatchAsync")]
    Task<ApiResult> PatchAsync(string dbmsName, string entityName, [Body] PatchDoc patchDocs);

    /// <summary>
    /// CRUD [DELETE] DeleteAsync
    /// </summary>
    /// <param name="dbmsName"></param>
    /// <param name="entityName"></param>
    /// <param name="content"></param>
    /// <param name="modeBulk"></param>
    /// <returns>ApiResult</returns>
    [Delete($"/api/{{dbmsName}}/{Litterals.Version1}/{{entityName}}/DeleteAsync")]
    Task<ApiResult> DeleteAsync(string dbmsName, string entityName, [Body] byte[] content, [Header("modeBulk")] int modeBulk);

    #endregion

    #region SENDEMAIL
    /// <summary>
    /// Send a generic templated email using SMTP
    /// </summary>
    /// <returns></returns>
    [Post($"/{Litterals.UniversRootPath}/Email/SendGenericMail")]
    Task<bool> SendGenericMail(EmailTemplate template);

    /// <summary>
    /// Send an automated email using SMTP
    /// </summary>
    /// <returns>Tuple</returns>
    [Post($"/{Litterals.UniversRootPath}/Email/SendAutomatedMailForRequest")]
    Task<bool> SendAutomatedMailForRequest(int demandeId, string typeCode);
    #endregion

    #region Client Parallel U
    /// <summary>
    /// Get all online CPU
    /// </summary>
    /// <returns></returns>
    [Post($"/{Litterals.CommonRootPath}/GetOnlineCPU")]
    Task<IEnumerable<IDictionary<string, object>>> GetOnlineCPU();

    /// <summary>
    /// Start or suspend CPU
    /// </summary>
    /// <param name="off"></param>
    /// <returns></returns>
    [Get($"/{Litterals.UniversRootPath}/CPU/MainEntryPoint")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task MainEntryPoint([Query] int off);
    #endregion

    #region TRACKEDENTITIES
    /// <summary>
    /// Get latest tracked entities
    /// </summary>
    /// <param name="entityType">Name of an entity, else * if you want all entities, ordered by last execution time</param>
    /// <param name="lastChecked"></param>
    /// <returns>Enumerable CommonRootPath (ordered by date descending)</returns>
    [Post($"/{Litterals.CommonRootPath}/GetTrackedEntities")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IEnumerable<TrackedEntity>> GetTrackedEntities([Query] string entityType, [Query] DateTime lastChecked);

    [Post($"/{Litterals.CommonRootPath}/GetTrackedEntities")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IEnumerable<TrackedEntity>> GetTrackedEntities<TEntity>([Query] DateTime lastChecked);

    [Post($"/{Litterals.CommonRootPath}/AddTrackedEntity")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<bool> AddTrackedEntity([Query] string fullNames, [Query] string verb, [Query] string userId, [Query] string uuid);

    [Post($"/{Litterals.CommonRootPath}/GetTrackedLogger")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IEnumerable<WorkerNodeLog>> GetTrackedLogger([Query] string cpuId, [Query] DateTime lastChecked);
    #endregion

    #region CATALOG (ETQ)
    [Post($"/{Litterals.UniversRootPath}/CPU/EtqCalculate")]
    Task<IEnumerable<EtqOutput>> EtqCalculate([Body] IEnumerable<CalculateEtqInput> etqInput);

    [Post($"/{Litterals.UniversRootPath}/CPU/EtqApplyRules")]
    Task<IEnumerable<EtqOutput>> EtqApplyRules([Body] IEnumerable<EtqRules> etqRules);

    [Post($"/{Litterals.UniversRootPath}/CPU/EtqSuiviRessource")]
    Task<IEnumerable<EtqOutput>> EtqSuiviRessource([Body] IEnumerable<EtqSuiviRessourceFileRaw> etqSuiviRessources);

    [Post($"/{Litterals.UniversRootPath}/CPU/EtqExtraInfoAddon")]
    Task<IEnumerable<EtqOutput>> EtqExtraInfoAddon([Body] IEnumerable<EtqExtraInfoAddonFileRaw> etqExtraInfoAddonContent);

    [Post($"/{Litterals.EtqRootPath}/EtiquetteDetails/GetEtiquetteDetails/{{queryOptions}}")]
    [QueryUriFormat(UriFormat.Unescaped)]
    Task<ODataContract<EtiquetteDetails>> GetEtiquetteDetails([Body] string queryOptions, [Header("pageSize")] int pageSize);

    [Put($"/{Litterals.EtqRootPath}/Etiquettes/{{id}}/Authorizations")]
    Task<HttpResponseMessage> SetEtiquetteAuthorizations(int id, [Body] EtqAuthorizationArguments args);
    #endregion

    #region AUTHENTICATION
    [Put($"/{Litterals.CommonRootPath}/oauth/CreateCipheredPassword")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<string> CreateCipheredPassword([Query] string clearPassword);

    [Put($"/{Litterals.CommonRootPath}/oauth/IsValidPassword")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<bool> IsValidPassword([Query] string cipherPassword, [Query] string clearPassword);
    #endregion

    #region LOGS
    [Post($"/{Litterals.CommonRootPath}/SetLogException")]
    Task SetLogException([Body] IEnumerable<LogException> exception);
    #endregion

    #region AUTHORIZATION RULES (DTF)
    /// <summary>
    /// DTF : Liste des Etats eligibles avec role de producteur
    /// </summary>
    /// <param name="isAdminMode"></param>
    /// <param name="userId"></param>
    /// <param name="catId"></param>
    /// <returns>DataResult</returns>
    [Post($"/{Litterals.UniversRootPath}/CPU/GetExecutableTeEtatsForDTF")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IEnumerable<TEntity>> GetExecutableTeEtatsForDTF<TEntity>([Query] bool isAdminMode, [Query] string userId, [Query] int catId) where TEntity : class, new();
    #endregion

    #region CALENDAR (DTF)
    /// <summary>
    /// Calcul des demandes théoriques via décodage du CRON par demande éligibles<br/>
    /// Exemple: GetTheoricalDemandesCalendar(dtUtcStart: DateTimeOffset.UtcNow, dtUtcEnd: DateTimeOffset.UtcNow.AddMonth(1), maxNextOccurencesPerDemande: 10);
    /// </summary>
    /// <param name="dtUtcStart">Date de départ (UTC)</param>
    /// <param name="dtUtcEnd">Date de fin (UTC)</param>
    /// <returns>List of ModeleDemandeCalendar</returns>
    [Post($"/{Litterals.UniversRootPath}/CPU/GetTheoricalDemandesCalendar")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IEnumerable<ModeleDemandeCalendar>> GetTheoricalDemandesCalendar([Query] DateTimeOffset dtUtcStart, [Query] DateTimeOffset dtUtcEnd);
    #endregion

    #region ORDERS (DTS)
    /// <summary>
    /// Delete an order (TCMD_COMMANDES).
    /// </summary>
    /// <param name="id">Id of the order to delete.</param>
    /// <returns>Http response message.</returns>
    [Delete($"/{Litterals.UniversRootPath}/Orders/{{id}}")]
    Task<HttpResponseMessage> DeleteOrder(int id);

    [Put($"/{Litterals.UniversRootPath}/Orders/{{id}}")]
    Task<HttpResponseMessage> ChangeOrderPhase(int id, [Body] ChangeOrderPhaseArguments args);

    [Post($"/{Litterals.UniversRootPath}/Orders/{{id}}/Duplicate")]
    Task<HttpResponseMessage> DuplicateOrder(int id);

    [Get($"/{Litterals.UniversRootPath}/Orders/{{id}}/Productions")]
    Task<HttpResponseMessage> GetProductionsAssociatedWithAnOrderAsync(int id);
    #endregion

    #region JobDuplicate (DTM)
    [Post($"/{Litterals.UniversRootPath}/Version/Duplicate")]
    public Task<bool> VersionDuplicate(int dpuIdToDuplicate);
    #endregion

    #region LOGSUNIVERS
    /// <summary>
    /// Get ETL Logs for EtlLogException for Datagrid Level 1
    /// </summary>
    /// <param name="fromDays"></param>
    /// <returns></returns>
    [Get($"/{Litterals.LogsRootPath}/LogsProcessingUnit/GetEtlLogExceptionGridLevel1")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IApiResponse<IEnumerable<EtlLogExceptionGridLevel1>>> GetEtlLogExceptionGridLevel1(DateTime startDate, DateTime endDate);

    [Get($"/{Litterals.LogsRootPath}/LogsProcessingUnit/GetEtlLogExceptionGridLevel2")]
    [QueryUriFormat(UriFormat.UriEscaped)]
    Task<IApiResponse<IEnumerable<EtlLogExceptionGridLevel2>>> GetEtlLogExceptionGridLevel2(DateTime startDate, DateTime endDate, int demandeId, string workFlow);
    #endregion
}