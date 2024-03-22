using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using static Krialys.Orkestra.Common.Shared.ETQ;

namespace Krialys.Orkestra.WebApi.Services.Data;

public interface IEtiquettesServices : IScopedService
{
    IEnumerable<EtqSearchOutput> EtqSearch(IEnumerable<EtqSearchInput> searchInput, int maxTop = Globals.MaxTop);

    IQueryable<TETQ_ETIQUETTES> FilterEtq(string serializedFilters);

    Task<IActionResult> SetEtqAuthorizationsAsync(int labelId, string userId, EtqAuthorizationArguments args);
}

/// <summary>
/// Services related to TETQ_ETIQUETTES table.
/// </summary>
public class EtiquettesServices : IEtiquettesServices
{
    #region Ctor
    /// <summary>
    /// Session with the ETQ database.
    /// </summary>
    private readonly Krialys.Data.EF.Etq.KrialysDbContext _dbContextEtq;

    /// <summary>
    /// Session with the Univers database.
    /// </summary>
    private readonly Krialys.Data.EF.Univers.KrialysDbContext _dbContextUnivers;

    /// <summary>
    /// Injected service which performs logging.
    /// </summary>
    private readonly Serilog.ILogger _logger;

    /// <summary>
    /// Injected service which triggers and tracks events shared with the client.
    /// </summary>
    private readonly ITrackedEntitiesServices _trackedEntitiesServices;

    private readonly ISqlRaw _sqlRaw;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtiquettesServices"/> class.
    /// </summary>
    /// <param name="dbContextEtq">Session with the Etq database.</param>
    /// <param name="dbContextUnivers">Session with the Univers database.</param>
    /// <param name="logger">Service which performs logging.</param>
    /// <param name="trackedEntitiesServices">Injected service which triggers and tracks events shared with the client.</param>
    /// <exception cref="ArgumentNullException">Dependency injection failed.</exception>
    public EtiquettesServices(Krialys.Data.EF.Etq.KrialysDbContext dbContextEtq,
        Krialys.Data.EF.Univers.KrialysDbContext dbContextUnivers,
        Serilog.ILogger logger, ITrackedEntitiesServices trackedEntitiesServices, ISqlRaw sqlRaw)
    {
        _dbContextEtq = dbContextEtq ?? throw new ArgumentNullException(nameof(dbContextEtq));
        _dbContextUnivers = dbContextUnivers ?? throw new ArgumentNullException(nameof(dbContextUnivers));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _trackedEntitiesServices = trackedEntitiesServices ?? throw new ArgumentNullException(nameof(trackedEntitiesServices));
        _sqlRaw = sqlRaw;
    }
    #endregion

    #region Authorizations
    /// <summary>
    /// Apply authorizations on a label (TETQ_ETIQUETTES).
    /// </summary>
    /// <param name="labelId">Id of the label (TETQ_ETIQUETTEID).</param>
    /// <param name="userId">Id of the user performing the operation.</param>
    /// <param name="args">Label authorization arguments.</param>
    /// <returns>Http status code.</returns>
    public async Task<IActionResult> SetEtqAuthorizationsAsync(int labelId, string userId, EtqAuthorizationArguments args)
    {
        // Check Id value.
        if (labelId <= 0)
            // Bad parameter.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);

        // Check arguments.
        if (args is null)
            // Bad parameters.
            return new StatusCodeResult(StatusCodes.Status400BadRequest);
        // Note: There is no check if authorized users exist and are active.

        // Get ressource.
        var label = _dbContextEtq.TETQ_ETIQUETTES
            .FirstOrDefault(etq => etq.TETQ_ETIQUETTEID.Equals(labelId));

        // Verify if ressource exists.
        if (label is null)
            // Code 410: Target ressource is no longer available.
            return new StatusCodeResult(StatusCodes.Status410Gone);

        try
        {
            // Update ressource.
            label.etq_is_public_access = args.IsAccessPublic;

            // If label is not public, its access is nominative.
            if (!args.IsAccessPublic && args.Authorize is not null
                && args.UsersIds is not null && args.UsersIds.Any())
            {
                // Read association table between labels and users.
                //  - Filter on selected label.
                //  - Filter on the users for which the authorizations are edited.
                //  => Get label-user associations that already exist and we will edit.
                var existingAuthorizations = _dbContextEtq.ETQ_TM_AET_Authorization
                    .Where(aet => aet.aet_etiquette_id.Equals(labelId))
                    .Where(aet => args.UsersIds.Contains(aet.aet_user_id))
                    .ToList();

                // Edit existing label-user associations with required rights.
                existingAuthorizations.ForEach(aet =>
                {
                    // Rights to apply: Authorized or rejected.
                    aet.aet_status_id = args.Authorize.Value ? StatusLiteral.Available : StatusLiteral.Deactivated;
                    // User editing the rights.
                    aet.aet_update_by = userId;
                    // Date on which the rights are edited.
                    aet.aet_update_date = DateExtensions.GetUtcNowSecond();
                });

                // If there are label-user associations that don't exist.
                // I.e. if edited associations are fewer than total associations.
                // And new authorizations are given.
                // => We will create missing associations.
                if (existingAuthorizations.Count() < args.UsersIds.Count() && args.Authorize.Value)
                {
                    // Get list of user Ids that already have an association with selected label.
                    var existingUserIds = existingAuthorizations.Select(aet => aet.aet_user_id);

                    // Get list of user Ids that don't have an association yet.
                    var missingUserIds = args.UsersIds.Except(existingUserIds);

                    // Add missing label-user associations.
                    foreach (var missingUserId in missingUserIds)
                    {
                        // Define a new label-user association.
                        ETQ_TM_AET_Authorization authorization = new()
                        {
                            // Associated label.
                            aet_etiquette_id = labelId,
                            // Associated user.
                            aet_user_id = missingUserId,
                            // User adding the rights.
                            aet_initializing_user_id = userId,
                            // Initialization date.
                            aet_initializing_date = DateExtensions.GetUtcNowSecond()
                        };

                        // Add association in table.
                        _dbContextEtq.ETQ_TM_AET_Authorization.Add(authorization);
                    }
                }
            }

            // Apply changes to the database.
            await _dbContextEtq.SaveChangesAsync();

            // Trigger an event that will be tracked by clients.
            await _trackedEntitiesServices.AddTrackedEntitiesAsync(
                null,
                new[]
                {
                    typeof(TETQ_ETIQUETTES).FullName,
                    typeof(ETQ_TM_AET_Authorization).FullName
                },
                Litterals.Update,
                userId,
                nameof(SetEtqAuthorizationsAsync));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"{nameof(EtiquettesServices)}.{nameof(SetEtqAuthorizationsAsync)}: {ex.Message}",
                ex.InnerException?.Message);

            // Code 500: Internal server error.
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new StatusCodeResult(StatusCodes.Status200OK);
    }
    #endregion

    #region Filters
    /// <summary>
    /// Get labels and apply filters.
    /// </summary>
    /// <param name="serializedFilters">Filters applied on labels.</param>
    /// <returns>Filtered labels.</returns>
    public IQueryable<TETQ_ETIQUETTES> FilterEtq(string serializedFilters)
    {
        var filters =
            // Create empty filters.
            string.IsNullOrEmpty(serializedFilters) ? new EtqFilters() :
            // Get filters applied on labels.
            JsonSerializer.Deserialize<EtqFilters>(serializedFilters);

        // Query against TD_DEMANDES.
        var demandes = _dbContextUnivers.Set<TD_DEMANDES>().AsNoTracking();

        // Is there a filter applied to "demandes".
        bool isDemandesFiltered = false;

        // Apply filter by order number.
        if (filters.OrderNumber is not null && filters.OrderNumber.Value > 0)
        {
            demandes = demandes
               .Where(d => d.TDC_DEMANDES_COMMANDES_TD_DEMANDE
                   .Any(tdc => tdc.TCMD_COMMANDEID.Equals(filters.OrderNumber))
               );

            isDemandesFiltered = true;
        }

        // Apply filter by module ids.
        if (filters.ModuleIds is not null && filters.ModuleIds.Any())
        {
            // demandes.TS_SCENARIOID must be in filters.ModuleIds list.
            demandes = demandes.Where(d => filters.ModuleIds.ToList().Any(id => id.Equals(d.TS_SCENARIOID.Value)));

            isDemandesFiltered = true;
        }

        // Query against TETQ_ETIQUETTES.
        var labels = _dbContextEtq.TETQ_ETIQUETTES.AsNoTracking();

        // Apply creation date filter.
        if (filters.CreationDateMin is not null)
        {
            labels = labels.Where(l => l.TETQ_DATE_CREATION >= filters.CreationDateMin);
        }
        if (filters.CreationDateMax is not null)
        {
            labels = labels.Where(l => l.TETQ_DATE_CREATION < filters.CreationDateMax);
        }

        // Apply search filter.
        if (!string.IsNullOrEmpty(filters.SearchedValue))
        {
            labels = labels.Where(l => l.TETQ_CODE.ToLower().Contains(filters.SearchedValue.ToLower()) ||
                l.TETQ_LIB.ToLower().Contains(filters.SearchedValue.ToLower()) ||
                l.TETQ_DESC.ToLower().Contains(filters.SearchedValue.ToLower()) ||
                l.TOBJE_OBJET_ETIQUETTE.TOBJE_CODE.ToLower().Contains(filters.SearchedValue.ToLower()));
        }

        // Apply filter by rule value ids.
        if (filters.RuleValueIds is not null && filters.RuleValueIds.Any())
        {
            labels = from etq in labels
                     let etqRules = _dbContextEtq.TETQR_ETQ_REGLES.AsNoTracking()
                          .Where(etqr => filters.RuleValueIds.ToList().Any(id => id.Equals(etqr.TRGLRV_REGLES_VALEURID)))
                          .ToList()
                     where (etqRules.Any(er => er.TETQ_ETIQUETTEID.Equals(etq.TETQ_ETIQUETTEID)))
                     select etq;
        }

        // Apply action filter.
        if (!string.IsNullOrWhiteSpace(filters.ActionLabel))
        {
            labels = labels.Where(l => l.TETQR_ETQ_REGLES != null &&
                l.TETQR_ETQ_REGLES.Any(er => er.TETQR_ETQ_REGLES_ACTION != null &&
                    er.TETQR_ETQ_REGLES_ACTION.Equals(filters.ActionLabel)));
        }

        // Join demandes and labels.
        if (isDemandesFiltered)
        {
            // Get list of "demandes" ids.
            var demandeIds = demandes.Select(d => d.TD_DEMANDEID).ToList();

            // Filter label based on selected "demandes" ids.
            labels = labels.Where(l => demandeIds.Any(id => id.Equals(l.DEMANDEID.Value)));
        }

        return labels;
    }
    #endregion

    #region Search
    /// <summary>
    /// Preamble: check some initial rules
    /// </summary>
    /// <returns>Empty list when all rules are respected, else a list of error message identified by its RequestId</returns>
    private IEnumerable<string> EtqSearchCheckParameters(EtqSearchInput searchInput)
    {
        IList<string> errors = new List<string>(capacity: 0);

        // Labels
        if (!string.IsNullOrEmpty(searchInput.LabelCode))
        {
            if (!string.IsNullOrEmpty(searchInput.LabelObjectCode))
            {
                errors.Add($"{nameof(searchInput.LabelObjectCode)} must be null");
            }

            if (!string.IsNullOrEmpty(searchInput.PerimeterValue))
                errors.Add($"{nameof(searchInput.PerimeterValue)} must be null");

            if (searchInput.Mode is not null)
                errors.Add($"{nameof(searchInput.Mode)} must be null");

            // Does LabelCode exists?
            if (!_dbContextEtq.TETQ_ETIQUETTES.Any(p => p.TETQ_CODE == searchInput.LabelCode))
                errors.Add($"{nameof(searchInput.LabelCode)}: {searchInput.LabelCode} does not exist");
        }
        else
        {
            // Does LabelObjectCode exists?
            if (!string.IsNullOrEmpty(searchInput.LabelObjectCode) && !_dbContextEtq.TOBJE_OBJET_ETIQUETTES.Any(p => p.TOBJE_CODE == searchInput.LabelObjectCode))
                errors.Add($"{nameof(searchInput.LabelObjectCode)}: {searchInput.LabelObjectCode} does not exist");
        }

        // Perimeters
        if (!string.IsNullOrEmpty(searchInput.PerimeterValue))
        {
            if (string.IsNullOrEmpty(searchInput.LabelObjectCode))
                errors.Add($"{nameof(searchInput.LabelObjectCode)} must have a value");

            // TODO : rien pigé aux specs sur cette partie : La valeur de périmètre indiquée n’existe pas pour l’objet recherché dans la liste des étiquettes
            //var data1 = _dbContextEtq.TPRCP_PRC_PERIMETRES.Any(p => p.TPRCP_PRC_PERIMETREID == searchInput.LabelObjectCode);
            //var data2 = _dbContextEtq.TETQ_ETIQUETTES.Any(p => p.TETQ_PRM_VAL == searchInput.PerimeterValue && p.do);
        }

        // Rules
        if (!(string.IsNullOrEmpty(searchInput.RuleCode) || string.IsNullOrEmpty(searchInput.RuleValue)))
        {
            if (string.IsNullOrEmpty(searchInput.LabelObjectCode))
                errors.Add($"{nameof(searchInput.LabelObjectCode)} must have a value");

            // Does RuleCode and RuleValue exist?
            if (!string.IsNullOrEmpty(searchInput.RuleCode) && !string.IsNullOrEmpty(searchInput.RuleValue))
            {
                if (!(from er in _dbContextEtq.TETQR_ETQ_REGLES
                      join rg in _dbContextEtq.TRGL_REGLES on er.TRGL_REGLEID equals rg.TRGL_REGLEID
                      join rv in _dbContextEtq.TRGLRV_REGLES_VALEURS on er.TRGLRV_REGLES_VALEURID equals rv.TRGLRV_REGLES_VALEURID
                      where rg.TRGL_CODE_REGLE == searchInput.RuleCode && rv.TRGLRV_VALEUR == searchInput.RuleValue
                      select new Rule(rg.TRGL_CODE_REGLE, rv.TRGLRV_VALEUR))
                                 .Any())
                    errors.Add($"{nameof(searchInput.RuleCode)}: {searchInput.RuleCode} and {nameof(searchInput.RuleValue)}: {searchInput.RuleValue} do not exist");
            }

            if (string.IsNullOrEmpty(searchInput.RuleCode))
                errors.Add($"{nameof(searchInput.RuleCode)} must have a value");

            if (string.IsNullOrEmpty(searchInput.RuleValue))
                errors.Add($"{nameof(searchInput.RuleValue)} must have a value");
        }

        // Catalogs
        if (searchInput.Catalog?.Any() is false)
            errors.Add($"{nameof(searchInput.Catalog)} must have a value");

        return errors;
    }

    /// <summary>
    /// Search by LabelCode
    /// </summary>
    /// <param name="searchInput"></param>
    /// <param name="maxTop"></param>
    /// <returns>Tuple (Errors, SearchOuput)</returns>
    public IEnumerable<EtqSearchOutput> EtqSearch(IEnumerable<EtqSearchInput> searchInput, int maxTop = Globals.MaxTop)
    {
        var outSearch = new List<EtqSearchOutput>();
        var labelCount = 0;

        // Get all expected labelCodes per RequestId
        foreach (var input in searchInput)
        {
            // RequestId
            var requestId = input.RequestId.StringOrNull();

            // Mode: Null, Last, All?
            var mode = input.Mode;

            // Message
            var message = string.Empty;

            // PerimeterValue
            var labelPerimeterValue = input.PerimeterValue.StringOrNull();

            // Check is there is any error
            var errList = EtqSearchCheckParameters(input).ToList();
            var errors = string.Join(", ", errList).StringOrNull();

            if (errors is not null)
            {
                outSearch.Add(new EtqSearchOutput(requestId, false, $"Incorrect search: {errors}", errList.Count, new[] {
                    new Orkestra.Common.Shared.ETQ.Label(input.LabelCode.StringOrNull(), null)
                }));
            }
            else
            {
                // Base list
                var listBase = new List<Base>(capacity: 0);
                {
                    // Search by LabelCode only (expected return: 1 item max)
                    if (!string.IsNullOrEmpty(input.LabelCode) && string.IsNullOrEmpty(input.LabelObjectCode))
                    {
                        var data = _dbContextEtq.TETQ_ETIQUETTES
                            .Where(p => p.TETQ_CODE == input.LabelCode)
                            .Select(e => new Base(e.TETQ_ETIQUETTEID, e.TETQ_PRM_VAL.StringOrNull(), e.TETQ_CODE.StringOrNull(),
                                    e.TETQ_LIB.StringOrNull(), e.TETQ_DESC.StringOrNull(), e.TETQ_DATE_CREATION));

                        if (data != null)
                            listBase.AddRange(data);
                    }

                    // Search by LabelObjectCode (w/o perimeter and/or rule) (expected return: 0 to N items)
                    if (string.IsNullOrEmpty(input.LabelCode) && !string.IsNullOrEmpty(input.LabelObjectCode))
                    {
                        var data = _dbContextEtq.TETQ_ETIQUETTES
                            .Join(_dbContextEtq.TOBJE_OBJET_ETIQUETTES, e => e.TOBJE_OBJET_ETIQUETTEID, p => p.TOBJE_OBJET_ETIQUETTEID,
                            (e, p) => new { e.TETQ_ETIQUETTEID, e.TETQ_PRM_VAL, e.TPRCP_PRC_PERIMETREID, e.TETQ_CODE, e.TETQ_LIB, e.TETQ_DESC, e.TETQ_DATE_CREATION, p.TOBJE_CODE })
                            .Where(p => p.TOBJE_CODE == input.LabelObjectCode)
                            .Select(e => new Base(e.TETQ_ETIQUETTEID, e.TETQ_PRM_VAL.StringOrNull(), e.TETQ_CODE.StringOrNull(),
                                    e.TETQ_LIB.StringOrNull(), e.TETQ_DESC.StringOrNull(), e.TETQ_DATE_CREATION));

                        if (data != null)
                            listBase.AddRange(data);
                    }
                }

                // Perimeter Value: apply potential restrictions
                if (labelPerimeterValue != null && listBase.Any())
                {
                    listBase = listBase
                        .Where(p => p.LabelPerimeterValue != null && p.LabelPerimeterValue.Equals(labelPerimeterValue, StringComparison.OrdinalIgnoreCase))
                        ?.ToList()
                        ?? new List<Base>();
                }

                if (listBase.Count == 0)
                    message = "Incorrect search: criteria for Base does not contain any data";

                // Get all expected labels
                var labels = new List<Orkestra.Common.Shared.ETQ.Label>(capacity: 0);

                // Search mode: order by last date
                if (listBase.Count > 1)
                {
                    listBase = listBase
                        .Where(e => e.LabelCreationDate is not null)
                        .OrderByDescending(e => e.LabelCreationDate).ToList();
                }

                for (int i = 0; i < listBase.Count; i++)
                {
                    // 1 base per labelCode
                    var @base = input.Catalog.Contains(CatalogType.Base)
                        ? listBase[i]
                        : null;

                    // 0 to N rules
                    var rule = (from er in _dbContextEtq.TETQR_ETQ_REGLES
                                join rg in _dbContextEtq.TRGL_REGLES on er.TRGL_REGLEID equals rg.TRGL_REGLEID
                                join rv in _dbContextEtq.TRGLRV_REGLES_VALEURS on er.TRGLRV_REGLES_VALEURID equals rv.TRGLRV_REGLES_VALEURID
                                where er.TETQ_ETIQUETTEID == listBase[i].LabelId
                                select new Rule(rg.TRGL_CODE_REGLE, rv.TRGLRV_VALEUR)
                                )?.ToList();

                    // Rules filtering: apply potential restrictions on ruleCode and ruleValue
                    if (rule.Any())
                    {
                        rule = rule.Where(r =>
                                (input.RuleCode.StringOrNull() is null || r.RuleCode.Equals(input.RuleCode, StringComparison.OrdinalIgnoreCase)) &&
                                (input.RuleValue.StringOrNull() is null || r.RuleValue.Equals(input.RuleValue, StringComparison.OrdinalIgnoreCase)))
                           .ToList();

                        // If we break here we don't grab the whole picture..., prefer geting what is possible instead
                        if (rule.Count == 0)
                        {
                            //message = "Incorrect search: criteria for Rules does not contain any data";
                            continue;
                        }
                    }

                    // 0 to N resources
                    var ress = input.Catalog.Contains(CatalogType.Ress)
                        ? _sqlRaw.GetAllSqlRaw<Ress>(_dbContextEtq, ArboEtiquettes.Ressources(listBase[i].LabelCode))
                        : (new List<Ress>(), 0);

                    // We stop searching when MaxTop has been reached
                    if (labelCount++ >= maxTop)
                        break;

                    // 1 to N labels per catalog
                    labels.Add(new Orkestra.Common.Shared.ETQ.Label(listBase[i].LabelCode, new Catalog(@base, rule, ress.Data)));
                }

                // Is there any label?
                if (labels.Any())
                    outSearch.Add(new EtqSearchOutput(requestId, true, null, errList.Count, labels));
                else
                    outSearch.Add(new EtqSearchOutput(requestId, false, string.IsNullOrEmpty(message)
                        ? "Incorrect search: criteria for Labels does not contain any data"
                        : message, errList.Count, null));

                // We stop searching when MaxTop has been reached
                if (labelCount >= maxTop)
                    break;
            }
        }

        return outSearch;
    }
    #endregion
}