using BlazorComponentBus;
using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Schedule;
using static Krialys.Orkestra.Common.Shared.Univers;

namespace Krialys.Orkestra.Web.Module.DTF.Components.Planifications;

/// <summary>
/// Expose realized Demandes and planified Demandes
/// </summary>
public partial class ProductionScheduledComponent
{
    #region Injected services
    [Inject] private ILogger<ProductionScheduledComponent> Logger { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Reference to the scheduler object.
    /// Used to call scheduler methods.
    /// </summary>
    private SfSchedule<AppointmentData> RefSchedule;

    private bool IsPrintWithOptions { get; set; }

    private bool IsPlanned { get; set; } = true;
    private bool IsEffective { get; set; } = true;
    private bool IsRecurrent { get; set; } = true;
    private bool IsOneShot { get; set; } = true;

    /// <summary>
    /// Filters applied on ETQ list.
    /// </summary>
    private SearchFilters _filters;

    /// <summary>
    /// Values of all applicable filters
    /// </summary>
    internal class SearchFilters
    {
        public SearchFilters()
        {
            SelectedUser = new HashSet<TRU_USERS>();
            SelectedCategory = new HashSet<TC_CATEGORIES>();
            SelectedUtd = new HashSet<TE_ETATS>();
            SelectedStatus = new HashSet<TRST_STATUTS>();
        }

        /// <summary>
        /// Selected Users
        /// </summary>
        public HashSet<TRU_USERS> SelectedUser { get; set; }

        /// <summary>
        /// Selected Categories
        /// </summary>
        public HashSet<TC_CATEGORIES> SelectedCategory { get; set; }

        /// <summary>
        /// Selected UTDs
        /// </summary>
        public HashSet<TE_ETATS> SelectedUtd { get; set; }

        /// <summary>
        /// Selected Statuses
        /// </summary>
        public HashSet<TRST_STATUTS> SelectedStatus { get; set; }
    }

    private SfChip _selectedUserSfChip;
    private SfChip _selectedCategorySfChip;
    private SfChip _selectedUtdSfChip;
    private SfChip _selectedStatusSfChip;

    /// <summary>
    /// Query applied on Users multi-select
    /// </summary>
    private readonly Query _userSelectQuery = new Query()
        .Where(nameof(TRU_USERS.TRU_STATUS), "equal", StatusLiteral.Available)
        .Sort(nameof(TRU_USERS.TRU_FULLNAME), "Ascending");

    /// <summary>
    /// Query applied on Categories multi-select
    /// </summary>
    private readonly Query _categorySelectQuery = new Query()
        .Where(nameof(TC_CATEGORIES.TRST_STATUTID), "equal", StatusLiteral.Available)
        .Sort(nameof(TC_CATEGORIES.TC_NOM), "Ascending");

    /// <summary>
    /// Query applied on UTD multi-select
    /// </summary>
    private readonly Query _utdSelectQuery = new Query()
        .Where(nameof(TE_ETATS.TRST_STATUTID), "equal", StatusLiteral.Available)
        .Sort(nameof(TE_ETATS.TE_NOM_ETAT), "Ascending");

    /// <summary>
    /// Query applied on Status multi-select
    /// </summary>
    private readonly Query _statusSelectQuery = new Query().AddParams(Litterals.OdataQueryParameters,
            $"?$filter={nameof(TRST_STATUTS.TRST_STATUTID)} eq '{StatusLiteral.RealizedRequest}'" +
            $" or {nameof(TRST_STATUTS.TRST_STATUTID)} eq '{StatusLiteral.CanceledRequest}'" +
            $" or {nameof(TRST_STATUTS.TRST_STATUTID)} eq '{StatusLiteral.InError}'" +
            $" or {nameof(TRST_STATUTS.TRST_STATUTID)} eq '{StatusLiteral.WaitingTriggerFileTimeout}'")
        .Sort(nameof(TRST_STATUTS.TRST_INFO), "Ascending");

    /// <summary>
    /// Initialize filters MemCache.
    /// </summary>
    private void InitializeCache()
    {
        if (MemCache is null)
        {
            return;
        }

        if (typeof(ProductionScheduledComponent).FullName is { } fullName)
        {
            _filters = MemCache.GetOrCreate(fullName, _ => new SearchFilters());
        }
    }

    /// <summary>
    /// Refresh planner
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private Task RefreshPlannerAsync(PopupEventArgs args)
        => BuildPlannerAsync();

    /// <summary>
    /// Action called when a new TEntity is selected.
    /// </summary>
    /// <param name="args">Select event arguments.</param>
    private Task FilterOnValueSelectAsync<TEntity>(SelectEventArgs<TEntity> args) where TEntity : class
    {
        switch (args)
        {
            case SelectEventArgs<TRU_USERS> arg:
                _filters.SelectedUser.Add(arg.ItemData);
                break;

            case SelectEventArgs<TC_CATEGORIES> arg:
                _filters.SelectedCategory.Add(arg.ItemData);
                break;

            case SelectEventArgs<TE_ETATS> arg:
                _filters.SelectedUtd.Add(arg.ItemData);
                break;

            case SelectEventArgs<TRST_STATUTS> arg:
                _filters.SelectedStatus.Add(arg.ItemData);
                break;

            default:
                throw new NotImplementedException(typeof(TEntity).FullName);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Action called when a TEntity is removed.
    /// </summary>
    /// <param name="args">Remove event arguments.</param>
    private Task FilterValueRemovedAsync<TEntity>(RemoveEventArgs<TEntity> args) where TEntity : class
    {
        switch (args)
        {
            case RemoveEventArgs<TRU_USERS> arg:
                {
                    _selectedUserSfChip.RemoveChips(new[] { arg.ItemData.TRU_FULLNAME });
                    _filters.SelectedUser.Remove(_filters.SelectedUser.First(d => d.TRU_USERID.Equals(arg.ItemData.TRU_USERID, StringComparison.Ordinal)));
                    break;
                }

            case RemoveEventArgs<TC_CATEGORIES> arg:
                {
                    _selectedCategorySfChip.RemoveChips(new[] { arg.ItemData.TC_NOM });
                    _filters.SelectedCategory.Remove(_filters.SelectedCategory.First(d => d.TC_CATEGORIEID.Equals(arg.ItemData.TC_CATEGORIEID)));
                    break;
                }

            case RemoveEventArgs<TE_ETATS> arg:
                {
                    _selectedUtdSfChip.RemoveChips(new[] { arg.ItemData.TE_NOM_ETAT });
                    _filters.SelectedUtd.Remove(_filters.SelectedUtd.First(d => d.TE_ETATID.Equals(arg.ItemData.TE_ETATID)));
                    break;
                }

            case RemoveEventArgs<TRST_STATUTS> arg:
                {
                    _selectedStatusSfChip.RemoveChips(new[] { arg.ItemData.TRST_INFO });
                    _filters.SelectedStatus.Remove(_filters.SelectedStatus.First(d => d.TRST_STATUTID.Equals(arg.ItemData.TRST_STATUTID, StringComparison.Ordinal)));
                    break;
                }

            default:
                throw new NotImplementedException(typeof(TEntity).FullName);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Action called when all TEntity Type are cleared.
    /// </summary>
    /// <param name="entityList">Mouse event arguments.</param>
    private Task FilterClearedAsync<TEntity>(HashSet<TEntity> entityList) where TEntity : class
    {
        entityList.Clear();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Demandes as calendar
    /// </summary>
    /// <param name="TYPE">0 means Appliable demandes, 1 means CRON demandes</param>
    /// <param name="TE_NOM_ETAT_VERSION"></param>
    /// <param name="TS_NOM_SCENARIO"></param>
    /// <param name="TD_DATE_EXECUTION_SOUHAITEE"></param>
    /// <param name="DEMANDEUR"></param>
    private record DEMANDES_CALENDAR(
        KIND TYPE,
        bool IS_RECURRENT,
        int TD_DEMANDEID,
        string TRU_DEMANDEUR, /* FULLNAME */
        string TS_NOM_SCENARIO,
        string TE_NOM_ETAT_VERSION,
        DateTime? TD_DATE_PRISE_EN_CHARGE,
        TimeSpan? TD_DUREE_PRODUCTION_REEL,  /* decode from seconds but adjust if < 20 min */
        string CODE_STATUT_DEMANDE,
        int TC_CATEGORIEID
    );

    /// <summary>
    /// Kind of Demande : past or future
    /// </summary>
    private enum KIND
    {
        EFFECTIVE = 1,
        THEORICAL = 2
    }

    /// <summary>
    /// Get all calendar Demandes
    /// </summary>
    private IList<DEMANDES_CALENDAR> _demandesCalendar = new List<DEMANDES_CALENDAR>
    {
        Capacity = 0
    };

    /// <summary>
    /// Get each detailed part of calendar Demandes
    /// </summary>
    private IEnumerable<VDE_DEMANDES_ETENDUES> _effectiveDemandes;
    private IEnumerable<ModeleDemandeCalendar> _theoricalDemandes;

    /// <summary>
    /// Default view selection
    /// </summary>
    private View CurrentView { get; set; } = View.Agenda;

    /// <summary>
    /// Current appointment's attributes
    /// </summary>
    private Dictionary<string, object> _appointmentAttributes = new Dictionary<string, object>();

    /// <summary>
    /// Resources table used for scheduler grouping.
    /// </summary>
    private readonly string[] _resources = { "Owners" };

    /// <summary>
    /// Slots names
    /// </summary>
    private IList<ResourceData> OwnersData { get; set; } = new List<ResourceData>();

    /// <summary>
    /// Appointments
    /// </summary>
    private IList<AppointmentData> _dataSource = new List<AppointmentData>
    {
        Capacity = 0
    };

    //private IEnumerable<AppointmentData> DataSourceBackup = null;
    #endregion

    #region References

    /// <summary>
    /// Appointment
    /// </summary>
    public class AppointmentData
    {
        /// <summary>
        /// Technical Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nom de l'UTD : TE_NOM_ETAT_VERSION
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Demande Id : TD_DEMANDEID
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Heure de début : TD_DATE_PRISE_EN_CHARGE
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Heure de fin : TD_DATE_PRISE_EN_CHARGE + AddSeconds(TD_DUREE_PRODUCTION_REEL) si inférieur à 20 min alors 20 min
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Heure de fin réelle : TD_DATE_PRISE_EN_CHARGE + AddSeconds(TD_DUREE_PRODUCTION_REEL)
        /// </summary>
        public DateTime EndTimeReal { get; set; }

        /// <summary>
        /// Nom du module : TS_NOM_SCENARIO
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Demandeur : TRU_DEMANDEUR
        /// </summary>
        public string Demandeur { get; set; }

        /// <summary>
        /// Status : CODE_STATUT_DEMANDE
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Status : TC_CATEGORIEID
        /// </summary>
        public int CategorieId { get; set; }

        /// <summary>
        /// Status : RECURRENT?
        /// </summary>
        public bool IsRecurrent { get; set; }

        public int OwnerId { get; set; }
    }

    /// <summary>
    /// Resources (SF required names)
    /// https://blazor.syncfusion.com/documentation/scheduler/resources#grouping-resources-by-date
    /// </summary>
    public class ResourceData
    {
        /// <summary>
        /// Resource Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Appointmant header slot name
        /// </summary>
        public string OwnerText { get; set; }

        /// <summary>
        /// Background slot cell appointment color
        /// </summary>
        public string OwnerColor { get; set; }
    }

    #endregion

    #region Core functions

    private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

    /// <summary>
    /// Build planner (startDate must start at 00:00:00 and endDate must end at 23:59:59)
    /// </summary>
    private async Task BuildPlannerAsync()
    {
        if (RefSchedule != null)
        {
            try
            {
                await _semaphoreSlim.WaitAsync();

                var viewStart = DateExtensions.ConvertToUTC(RefSchedule.GetCurrentViewDates().First());
                var viewEnd = DateExtensions.ConvertToUTC(RefSchedule.GetCurrentViewDates().Last().AddHours(23).AddMinutes(59).AddSeconds(59));

                await GetPlanificationsAsync(new DateTimeOffset(viewStart), new DateTimeOffset(viewEnd),
                    convertToLocalDateTime: true, useCache: false);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }

    /// <summary>
    /// Get both theorical and appliable planifications <br />
    /// From DAY_A YYYY-MM-DDT00:00:00 to DAY_B YYYY-MM-DDT23:59:59
    /// </summary>
    /// <param name="startDateTimeUtc"></param>
    /// <param name="endDateTimeUtc?">If endDateTime is given then take dates between startDateTime up to endDateTime, else endDateTime is computed from startDateTime</param>
    /// <param name="convertToLocalDateTime">When true, then convert dates from database to LocalDate</param>
    /// <param name="useCache?">When true, then use cache</param>
    /// <returns></returns>
    private async Task GetPlanificationsAsync(DateTimeOffset startDateTimeUtc, DateTimeOffset? endDateTimeUtc, bool convertToLocalDateTime, bool useCache)
    {
        // ** Cleanup first
        _demandesCalendar.Clear();

        // ** Decode date(s)
        var dates = endDateTimeUtc.HasValue && endDateTimeUtc.Value != default
            ? DateExtensions.FormatDateTimesOffset(startDateTimeUtc, endDateTimeUtc.Value)
            : DateExtensions.FormatDateTimesOffset(startDateTimeUtc);

        // ** Effective Demandes (past)
        if (IsEffective)
        {
            _effectiveDemandes = Enumerable.Empty<VDE_DEMANDES_ETENDUES>();

            _effectiveDemandes = await ProxyCore.GetEnumerableAsync<VDE_DEMANDES_ETENDUES>(
                $"?$filter={nameof(VDE_DEMANDES_ETENDUES.TD_DATE_PRISE_EN_CHARGE)} ge {dates.Start} " +
                $"and {nameof(VDE_DEMANDES_ETENDUES.TD_DATE_PRISE_EN_CHARGE)} le {dates.End}" +
                $"&$orderby={nameof(VDE_DEMANDES_ETENDUES.TD_DATE_PRISE_EN_CHARGE)}",
                useCache, false);

            var vdeDemandesEtendueses = _effectiveDemandes as VDE_DEMANDES_ETENDUES[] ?? _effectiveDemandes.ToArray();
            if (vdeDemandesEtendueses.Any())
            {
                // ** Convert to Local DateTime?
                if (convertToLocalDateTime)
                {
                    _effectiveDemandes = OdataEntities.ConvertDatesToLocaleDateTime(vdeDemandesEtendueses);
                }

                foreach (var el in vdeDemandesEtendueses.Where(e => e.TD_DATE_PRISE_EN_CHARGE is not null))
                {//el.CATEGORIE, el.CODE_STATUT_DEMANDE
                    _demandesCalendar.Add(new(
                        KIND.EFFECTIVE,
                        el.TPF_PLANIF_ORIGINEID.HasValue,
                        el.TD_DEMANDEID,
                        el.DEMANDEUR,
                        el.TS_NOM_SCENARIO,
                        el.TE_NOM_ETAT_VERSION,
                        el.TD_DATE_PRISE_EN_CHARGE, /* REEL */
                        el.TD_DUREE_PRODUCTION_REEL.HasValue ? TimeSpan.FromSeconds(el.TD_DUREE_PRODUCTION_REEL.Value) : TimeSpan.FromSeconds(0),
                        el.CODE_STATUT_DEMANDE,
                        el.TC_CATEGORIEID
                    ));
                }
            }
        }

        // ** Theorical Demandes (future (CRON))
        if (IsPlanned)
        {
            _theoricalDemandes = Enumerable.Empty<ModeleDemandeCalendar>();
            _theoricalDemandes = await ProxyCore.GetTheoricalDemandesCalendar(startDateTimeUtc,
                endDateTimeUtc.HasValue ? endDateTimeUtc.Value.AddDays(1).AddSeconds(-1) : startDateTimeUtc.AddDays(1).AddSeconds(-1),
                useCache);

            var modeleDemandeCalendars = _theoricalDemandes as ModeleDemandeCalendar[] ?? _theoricalDemandes.ToArray();
            if (modeleDemandeCalendars.Any())
            {
                foreach (var el in modeleDemandeCalendars.Where(e => e.TD_DATE_PRISE_EN_CHARGE is not null))
                {
                    _demandesCalendar.Add(new(
                        KIND.THEORICAL,
                        el.IS_RECURRENT, /* RECURRENT? */
                        el.TD_DEMANDEID,
                        el.TRU_DEMANDEUR,
                        el.TS_NOM_SCENARIO,
                        el.TE_NOM_ETAT_VERSION,
                        el.TD_DATE_PRISE_EN_CHARGE, /* THEORIQUE */
                        el.TD_DUREE_PRODUCTION_REEL.HasValue ? TimeSpan.FromSeconds(el.TD_DUREE_PRODUCTION_REEL.Value) : TimeSpan.FromSeconds(0),
                        el.CODE_STATUT_DEMANDE,
                        el.TC_CATEGORIEID
                    ));
                }
            }
        }

        // ** Is not Recurrent and/or not recurrent
        if (!IsRecurrent && !IsOneShot)
        {
            _dataSource = new List<AppointmentData>
            {
                Capacity = 0
            };
        }
        else if (IsRecurrent || IsOneShot)
        {
            // ** Prepare elements for planning, minimal duration time: 20min when < 20min else, take real timespan
            int id = 0;

            _dataSource.Clear();

            foreach (var el in _demandesCalendar)
            {
                if (el is { TD_DATE_PRISE_EN_CHARGE: not null, TD_DUREE_PRODUCTION_REEL: not null })
                {
                    _dataSource.Add(new AppointmentData
                    {
                        Id = ++id,
                        OwnerId = (int)el.TYPE,
                        Subject = el.TE_NOM_ETAT_VERSION,
                        Location = el.TD_DEMANDEID != 0 ? el.TD_DEMANDEID.ToString() : "",
                        Description = el.TS_NOM_SCENARIO,
                        StartTime = el.TD_DATE_PRISE_EN_CHARGE.Value,
                        EndTime = el.TD_DUREE_PRODUCTION_REEL is { TotalMinutes: > 20 }
                            ? el.TD_DATE_PRISE_EN_CHARGE.Value.AddSeconds(el.TD_DUREE_PRODUCTION_REEL.Value.TotalSeconds)
                            : el.TD_DATE_PRISE_EN_CHARGE.Value.AddMinutes(20),
                        EndTimeReal = el.TD_DATE_PRISE_EN_CHARGE.Value.AddSeconds(el.TD_DUREE_PRODUCTION_REEL.Value.TotalSeconds),
                        Demandeur = el.TRU_DEMANDEUR,
                        Status = el.CODE_STATUT_DEMANDE,
                        IsRecurrent = el.IS_RECURRENT,
                        CategorieId = el.TC_CATEGORIEID
                    });
                }
            }

            // ** Convert to Local DateTime?
            if (convertToLocalDateTime)
            {
                _dataSource = OdataEntities.ConvertDatesToLocaleDateTime(_dataSource);
            }

            var recurrentList = IsRecurrent switch
            {
                // Is recurrent only?
                true when !IsOneShot => _dataSource.Where(e => e.IsRecurrent),
                // Is one shot only?
                false when IsOneShot => _dataSource.Where(e => !e.IsRecurrent),
                _ => null
            };

            _dataSource = recurrentList is null ? _dataSource : _dataSource.Intersect(recurrentList).ToList();
        }

        // ** Selected Users or not
        if (_filters.SelectedUser.Any())
        {
            var filtered = new List<AppointmentData>
            {
                Capacity = 0
            };

            foreach (var el in _filters.SelectedUser.Select(e => e.TRU_FULLNAME))
            {
                var data = _dataSource.Where(e => e.Demandeur.Equals(el, StringComparison.Ordinal));
                if (data != null)
                {
                    filtered.AddRange(data);
                }
            }

            _dataSource = filtered;
        }

        // ** Selected Categories or not
        if (_filters.SelectedCategory.Any())
        {
            var filtered = new List<AppointmentData>
            {
                Capacity = 0
            };

            foreach (var data in from el in _filters.SelectedCategory.Select(e => e.TC_CATEGORIEID)
                                 let data = _dataSource.Where(e => e.CategorieId.Equals(el))
                                 select data)
            {
                filtered.AddRange(data);
            }

            _dataSource = filtered;
        }

        // ** Selected UTDs or not
        if (_filters.SelectedUtd.Any())
        {
            var filtered = new List<AppointmentData>
            {
                Capacity = 0
            };

            foreach (var data in from el in _filters.SelectedUtd.Select(e => e.TE_NOM_ETAT)
                                 let data = _dataSource.Where(e => e.Subject.StartsWith(el, StringComparison.Ordinal))
                                 where data != null
                                 select data)
            {
                filtered.AddRange(data);
            }

            _dataSource = filtered;
        }

        // ** Selected Statuses or not
        if (_filters.SelectedStatus.Any())
        {
            var filtered = new List<AppointmentData>
            {
                Capacity = 0
            };

            foreach (var data in from el in _filters.SelectedStatus.Select(e => e.TRST_STATUTID)
                                 let data = _dataSource.Where(e => !string.IsNullOrEmpty(e.Status) && e.Status.Equals(el, StringComparison.Ordinal)) //.ToList()
                                 where data != null
                                 select data)
            {
                filtered.AddRange(data);
            }

            _dataSource = filtered;
        }

        // Refresh UI
        //await InvokeAsync(StateHasChanged);
        if (RefSchedule != null)
        {
            await RefSchedule.RefreshEventsAsync();
        }
    }
    #endregion

    #region Blazor Lifecycle

    /// <summary>
    /// CTOR
    /// </summary>
    protected override void OnInitialized()
    {
        InitializeCache();

        OwnersData = new List<ResourceData>
        {
            new ResourceData{ OwnerText = Trad.Keys["DTF:Planned"], Id = (int)KIND.THEORICAL, OwnerColor = "#174e62" },
            new ResourceData{ OwnerText = Trad.Keys["DTF:Accomplished"], Id = (int)KIND.EFFECTIVE, OwnerColor = "#4dc3b9" },
        };

        Bus.Subscribe<IList<TrackedEntity>>(OnTrackedVDE_DEMANDES_ETENDUESAsync);
    }

    [ThreadStatic]
    private static int _prevId = 0;

    /// <summary>
    /// Callback for managing automatic smooth DataGrid refresh
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    //public Task OnTrackedEnties(IList<TrackedEntity> trackedEntities)
    private async Task OnTrackedVDE_DEMANDES_ETENDUESAsync(MessageArgs args, CancellationToken ct)
    {
        // Get the tracking
        var tracked = args
            .GetMessage<IList<TrackedEntity>>()
            .FirstOrDefault(e => e.FullName.Equals(typeof(VDE_DEMANDES_ETENDUES).FullName)
                 //&& !string.IsNullOrEmpty(e.UuidOrAny)
                 //&& !e.UuidOrAny.Equals(ProxyCore.ApplicationClientSessionId, StringComparison.Ordinal)
                 // We only need to intercept these messages coming from CRUD via CpuServices and/or coming form UI as well
                 && (e.UuidOrAny.Equals("ConfirmePriseEnChargeDemande", StringComparison.Ordinal)
                     || e.UuidOrAny.Equals("FinalizeDemande", StringComparison.Ordinal)
                     || e.UuidOrAny.Equals("CreateDemande", StringComparison.Ordinal)
                     || e.UuidOrAny.Equals("CancelDemande", StringComparison.Ordinal)
                     || e.UuidOrAny.Equals("PUCentralAnalysePrepareDemandesEligibles", StringComparison.Ordinal))
                 // OB-345: we need Insert + Update
                 && (e.Action.Equals("Insert", StringComparison.Ordinal) || e.Action.Equals("Update", StringComparison.Ordinal)));

        if (tracked != null)
        {
            if (_prevId != tracked.Id)
            {
                _prevId = tracked.Id;

                // Refresh planner
                await BuildPlannerAsync();

                //#if DEBUG
                // Trace to browser
                Logger.LogWarning($"[TRK-SCHED] Date: {tracked.Date:R}, Id: {tracked.Id}, Entity: {nameof(VDE_DEMANDES_ETENDUES)}, Action: {tracked.Action}, UserId: {tracked.UserId}, Uuid: {tracked.UuidOrAny}");
                //#endif
            }
        }
    }

    /// <summary>
    /// Destroy datagrid: Unsubscribe to dedicated events
    /// </summary>
    public void OnDestroyed()
        => Bus.UnSubscribe<IList<TrackedEntity>>(OnTrackedVDE_DEMANDES_ETENDUESAsync);

    /// <summary>
    /// Raised when calendar is created
    /// </summary>
    /// <returns></returns>
    private async Task OnCreated()
    {
        if (RefSchedule is not null)
        {
            await RefSchedule.ScrollToAsync(DateTime.Now.AddHours(-2).ToString("HH:00"));
            await BuildPlannerAsync();
        }
    }

    /// <summary>
    /// Update planner
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    private async Task OnActionCompleted(ActionEventArgs<AppointmentData> args)
    {
        if (RefSchedule is not null && args.ActionType is ActionType.ViewNavigate or ActionType.DateNavigate)
        {
            await RefSchedule.ScrollToAsync(DateTime.Now.AddHours(-2).ToString("HH:00"));
            await BuildPlannerAsync();
        }
    }

    /// <summary>
    /// Considering special 'error' cases when analyzing Status code, then apply orange code color to the appointment
    /// </summary>
    /// <param name="args"></param>
    private void OnEventRendered(EventRenderedArgs<AppointmentData> args)
    {
        switch (args.Data.Status)
        {
            case StatusLiteral.InError:
            case StatusLiteral.CanceledRequest:
            case StatusLiteral.WaitingTriggerFileTimeout:
                _appointmentAttributes.Clear();

                _appointmentAttributes.Add("style", (CurrentView is View.Agenda or View.MonthAgenda)
                    ? "border-left-color: orange"
                    : "background: orange");

                args.Attributes = _appointmentAttributes;
                break;
        }
    }

    /// <summary>
    /// Export to Excel
    /// </summary>
    /// <returns></returns>
    private Task OnExportToExcel()
        => RefSchedule is not null
            ? RefSchedule.ExportToExcelAsync()
            : Task.CompletedTask;
    #endregion
}
