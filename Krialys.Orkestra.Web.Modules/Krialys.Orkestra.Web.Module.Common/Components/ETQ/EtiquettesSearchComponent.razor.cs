using BlazorComponentBus;
using Krialys.Common.Extensions;
using Krialys.Data.EF.Etq;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Caching.Memory;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.Common.Components.ETQ;

public partial class EtiquettesSearchComponent
{
    #region Parameters
    /// <summary>
    /// Total number of Etiquettes (depending on applied filters and searches).
    /// </summary>
    [Parameter]
    public int EtiquettesCount { get; set; }

    /// <summary>
    /// Cache key used to store filters and sorts.
    /// </summary>
    [Parameter]
    public string CacheKey { get; set; }

    /// <summary>
    /// If true, the sort section of the component is hidden.
    /// </summary>
    [Parameter]
    public bool IsSortHidden { get; set; }

    /// <summary>
    /// Event triggers when Etiquettes order changed.
    /// Return OData orderby parameter for TETQ_ETIQUETTES.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnOrderByChanged { get; set; }

    /// <summary>
    /// Event triggers when Etiquettes filter changed.
    /// Return OData filter parameter for TETQ_ETIQUETTES.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnFilterChanged { get; set; }

    /// <summary>
    /// Filter parameters (true for dts_etq, false for etiquettes_arbo)
    /// </summary>
    [Parameter]
    public bool OnlyAllowedPerimeters { get; set; }
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override Task OnInitializedAsync()
    {
        // Subscribe to TrackedEntity
        Bus.Subscribe<IList<TrackedEntity>>(OnTrackedChangesAsync);

        InitializeCache();

        return InitializeListsAsync();
    }

    private async Task InitializeListsAsync()
    {
        _perimetersSelectQuery = new Query().Take(Globals.PageSize).With(e =>
        {
            if (OnlyAllowedPerimeters) /* OB-214 */
                e.Where(nameof(TPRCP_PRC_PERIMETRES.TPRCP_ALLOW_DTS_ACCESS), "equals", true);

            e.Sort(nameof(TPRCP_PRC_PERIMETRES.TPRCP_LIB), "Ascending");
        });

        await UpdateEtqFilteringAsync();

        await UpdateEtqSortingAsync();
    }
    #endregion

    #region Cache
    /// <summary>
    /// Initialize filters cache.
    /// </summary>
    private void InitializeCache()
    {
        if (MemCache is not null)
        {
            _filters = MemCache.GetOrCreate(CacheKey, entry => new EtqFilters());
        }
    }

    /// <summary>
    /// Get tracked changes upoon TPRCP_PRC_PERIMETRES, TDOM_DOMAINES and TOBJE_OBJET_ETIQUETTES
    /// </summary>
    private async Task OnTrackedChangesAsync(MessageArgs args, CancellationToken ct)
    {
        var trackedEntities = args.GetMessage<IList<TrackedEntity>>()
            .Where(e => e.FullName.Equals(typeof(TOBJE_OBJET_ETIQUETTES).FullName)
                    || e.FullName.Equals(typeof(TPRCP_PRC_PERIMETRES).FullName)
                    || e.FullName.Equals(typeof(TDOM_DOMAINES).FullName))
            .ToList();

        var refreshUi = false;

        if (trackedEntities.Any(e => e.FullName.Equals(typeof(TOBJE_OBJET_ETIQUETTES).FullName, StringComparison.Ordinal)))
        {
            ProxyCore.CacheRemoveEntities(typeof(TOBJE_OBJET_ETIQUETTES));
            _filters.SelectedObjects.Clear();
            refreshUi = true;

        }
        else if (trackedEntities.Any(e => e.FullName.Equals(typeof(TPRCP_PRC_PERIMETRES).FullName, StringComparison.Ordinal)))
        {
            ProxyCore.CacheRemoveEntities(typeof(TPRCP_PRC_PERIMETRES));
            _filters.SelectedPerimeters.Clear();
            refreshUi = true;
        }
        else if (trackedEntities.Any(e => e.FullName.Equals(typeof(TDOM_DOMAINES).FullName, StringComparison.Ordinal)))
        {
            ProxyCore.CacheRemoveEntities(typeof(TDOM_DOMAINES));
            _filters.SelectedDomains.Clear();
            refreshUi = true;
        }

        if (refreshUi)
            MemCache.Remove(CacheKey);

        if (refreshUi)
        {
            await InitializeListsAsync();
            await UpdateEtqFilteringAsync();
        }
    }
    #endregion

    #region Filters on ETQ list
    /// <summary>
    /// Values of all applicable filters on ETQ list.
    /// </summary>
    private class EtqFilters
    {
        /// <summary>
        /// Selected sorting.
        /// </summary>
        public int SelectedSort;

        /// <summary>
        /// Value of searched string.
        /// </summary>
        public string SearchedValue { get; set; }

        /// <summary>
        /// Minimum value for the creation date.
        /// </summary>
        public DateTime? CreationDateMin { get; set; }

        /// <summary>
        /// Maximum value for the creation date.
        /// </summary>
        public DateTime? CreationDateMax { get; set; }

        /// <summary>
        /// Selected perimeters.
        /// </summary>
        public HashSet<TPRCP_PRC_PERIMETRES> SelectedPerimeters { get; set; } = new();

        /// <summary>
        /// Selected domains.
        /// </summary>
        public HashSet<TDOM_DOMAINES> SelectedDomains { get; set; } = new();

        /// <summary>
        /// Selected "labeled objects".
        /// </summary>
        public HashSet<TOBJE_OBJET_ETIQUETTES> SelectedObjects { get; set; } = new();
    }

    /// <summary>
    /// Filters applied on ETQ list.
    /// </summary>
    private EtqFilters _filters;

    /// <summary>
    /// Available sortings.
    /// </summary>
    private enum Sort
    {
        /// <summary>
        /// Latest created first.
        /// </summary>
        CreationDatesOrder,
        /// <summary>
        /// By alphabetical order.
        /// </summary>
        AlphabeticalOrder
    }

    /// <summary>
    /// OData orderby parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataOrderBy;

    /// <summary>
    /// Update OData orderby parameter for TETQ_ETIQUETTES
    /// and clear TETQ_ETIQUETTES list.
    /// </summary>
    private async Task UpdateEtqSortingAsync()
    {
        // Add OData orderby parameter.
        _odataOrderBy = (Sort)_filters.SelectedSort == Sort.CreationDatesOrder ?
            $"$orderby={nameof(TETQ_ETIQUETTES.TETQ_DATE_CREATION)} desc"
            : $"$orderby={nameof(TETQ_ETIQUETTES.TETQ_CODE)} asc";

        // Trigger event callback.
        await OnOrderByChanged.InvokeAsync(_odataOrderBy);
    }

    /// <summary>
    /// OData filter parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataFilter;

    /// <summary>
    /// Update OData filter parameter for TETQ_ETIQUETTES.
    /// </summary>
    private async Task UpdateEtqFilteringAsync()
    {
        // Table of all OData filters applied on TETQ_ETIQUETTES.
        IList<string> filtersTable = new List<string>();

        // Add search filters.
        if (!string.IsNullOrWhiteSpace(_filters.SearchedValue))
        {
            // Search is in lower case to avoid case sensitivity.
            var searchedValue = _filters.SearchedValue.ToLower();

            // Search on:
            // TETQ_ETIQUETTES code, label and description,
            // TPRCP_PRC_PERIMETRES label and code.
            filtersTable.Add(
                $"contains(tolower({nameof(TETQ_ETIQUETTES.TETQ_CODE)}),'{searchedValue}') " +
                $"or contains(tolower({nameof(TETQ_ETIQUETTES.TETQ_LIB)}),'{searchedValue}') " +
                $"or contains(tolower({nameof(TETQ_ETIQUETTES.TETQ_DESC)}),'{searchedValue}') " +
                $"or contains(tolower({nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETRE)}/{nameof(TPRCP_PRC_PERIMETRES.TPRCP_LIB)}),'{searchedValue}') " +
                $"or contains(tolower({nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETRE)}/{nameof(TPRCP_PRC_PERIMETRES.TPRCP_CODE)}),'{searchedValue}')");
        }

        // Add date filters, based on range picker.
        if (_filters.CreationDateMin.HasValue && _filters.CreationDateMax.HasValue)
        {
            // Get start date in UTC.
            var StartDate = _filters.CreationDateMin.Value.ToUniversalTime();
            // Get end date in UTC, add 23h59m59s to be at the end of the day.
            var EndDate = _filters.CreationDateMax.Value.Add(new TimeSpan(23, 59, 59)).ToUniversalTime();

            filtersTable.Add(
                $"{nameof(TETQ_ETIQUETTES.TETQ_DATE_CREATION)} ge {StartDate:yyyy-MM-ddTHH:mm:ssZ}" +
                $" and {nameof(TETQ_ETIQUETTES.TETQ_DATE_CREATION)} le {EndDate:yyyy-MM-ddTHH:mm:ssZ}");
        }

        // Add perimeter filters.
        if (_filters.SelectedPerimeters.Any())
        {
            filtersTable.Add(string.Join(" or ", _filters.SelectedPerimeters.Select(p =>
                $"{nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETREID)} eq {p.TPRCP_PRC_PERIMETREID}")));
        }

        // Add domain filters.
        if (_filters.SelectedDomains.Any())
        {
            filtersTable.Add(string.Join(" or ", _filters.SelectedDomains.Select(d =>
                    $"TOBJE_OBJET_ETIQUETTE/TDOM_DOMAINEID eq {d.TDOM_DOMAINEID}")));
        }

        // Add "labeled objects" filters.
        if (_filters.SelectedObjects.Any())
        {
            filtersTable.Add(string.Join(" or ", _filters.SelectedObjects.Select(o =>
                    $"TOBJE_OBJET_ETIQUETTE/TOBJE_OBJET_ETIQUETTEID eq {o.TOBJE_OBJET_ETIQUETTEID}")));
        }

        // Convert filters to oadata string.
        _odataFilter = filtersTable.Any() ? $"$filter=({string.Join(") and (", filtersTable)})" : string.Empty;

        // Trigger event callback.
        await OnFilterChanged.InvokeAsync(_odataFilter);
    }
    #endregion

    #region Sorting Tab
    /// <summary>
    /// Event triggers before selecting the Tab.
    /// </summary>
    /// <param name="args">Selecting event arguments.</param>
    private void OnTabSelecting(SelectingEventArgs args)
    {
        if (args.IsSwiped)
        {
            args.Cancel = true;
        }
        else
        {
            // Set Disabled value then fire event to SfTab
            Bus.Publish(new SfTabBusEvent { Disabled = true });
        }
    }

    /// <summary>
    /// Event triggers when a Tab is selected.
    /// </summary>
    /// <param name="args">Select event arguments.</param>
    private Task SortSelectedAsync(SelectEventArgs args)
        => UpdateEtqSortingAsync();
    #endregion

    #region Search TextBox
    /// <summary>
    /// Event triggers when the content of TextBox has changed (enter pressed or focus-out).
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private async Task SearchValueChangeAsync(ChangedEventArgs args)
        => await UpdateEtqFilteringAsync();
    #endregion

    #region Creation date Range Picker
    /// <summary>
    /// Triggers when the selected range is changed.
    /// </summary>
    /// <param name="args">Range picker event arguments.</param>
    private async Task RangePickerValueChangedAsync(RangePickerEventArgs<DateTime?> args)
        => await UpdateEtqFilteringAsync();

    /// <summary>
    /// This week preselect start date.
    /// </summary>
    private readonly DateTime _weekStart = DateTime.Now.GetFirstDayOfWeek();

    /// <summary>
    /// This week preselect end date.
    /// </summary>
    private readonly DateTime _weekEnd = DateTime.Now.GetLastDayOfWeek();

    /// <summary>
    /// This month preselect start date.
    /// </summary>
    private readonly DateTime _monthStart = DateTime.Now.GetFirstDayOfMonth();

    /// <summary>
    /// This month preselect end date.
    /// </summary>
    private readonly DateTime _monthEnd = DateTime.Now.GetLastDayOfMonth();

    /// <summary>
    /// Last month preselect start date.
    /// </summary>
    private readonly DateTime _lastMonthStart = DateTime.Now.GetFirstDayOfPreviousMonth();

    /// <summary>
    /// Last month preselect end date.
    /// </summary>
    private readonly DateTime _lastMonthEnd = DateTime.Now.GetLastDayOfPreviousMonth();
    #endregion

    #region Perimeter Multiselect (TPRCP_PRC_PERIMETRES)
    /// <summary>
    /// Reference to perimeter SfChip component.
    /// </summary>
    private SfChip _perimeterSfChipRef;

    /// <summary>
    /// Query applied on perimeter multiselect:
    ///  - First page size,
    ///  - Allowed perimeters (true for DTS and false for ETQArbo)
    ///  - Sort by perimeters labels in alphabetical order.  
    /// </summary>
    private Query _perimetersSelectQuery;

    /// <summary>
    /// Action called when a new perimeter is selected.
    /// </summary>
    /// <param name="args">Select event arguments.</param>
    private async Task PerimeterFilterOnValueSelectAsync(SelectEventArgs<TPRCP_PRC_PERIMETRES> args)
    {
        // Update selected perimeters.
        _filters.SelectedPerimeters.Add(args.ItemData);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when a perimeter is removed.
    /// </summary>
    /// <param name="args">Remove event arguments.</param>
    private async Task PerimeterFilterValueRemovedAsync(RemoveEventArgs<TPRCP_PRC_PERIMETRES> args)
    {
        // Remove chips.
        // When chips depend on a list, they are not automatically removed when the list is updated.
        _perimeterSfChipRef.RemoveChips(new[] { args.ItemData.TPRCP_LIB });

        // Update selected perimeters.
        var deletedPerimeter = _filters.SelectedPerimeters
            .FirstOrDefault(p => p.TPRCP_PRC_PERIMETREID.Equals(args.ItemData.TPRCP_PRC_PERIMETREID));
        _filters.SelectedPerimeters.Remove(deletedPerimeter);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when all perimeters are cleared.
    /// </summary>
    /// <param name="args">Mouse event arguments.</param>
    private async Task PerimeterFilterClearedAsync(MouseEventArgs args)
    {
        // Update selected perimeters.
        _filters.SelectedPerimeters.Clear();

        await UpdateEtqFilteringAsync();
    }
    #endregion

    #region Domain Multiselect (TDOM_DOMAINES)
    /// <summary>
    /// Reference to domain SfChip component.
    /// </summary>
    private SfChip _domainSfChipRef;

    /// <summary>
    /// Query applied on domain multiselect:
    ///  - First page size,
    ///  - Sort by domains labels in alphabetical order.
    /// </summary>
    private readonly Query _domainsSelectQuery = new Query().Take(Globals.PageSize)
        .Sort(nameof(TDOM_DOMAINES.TDOM_LIB), "Ascending");

    /// <summary>
    /// Action called when a new domain is selected.
    /// </summary>
    /// <param name="args">Select event arguments.</param>
    private async Task DomainFilterOnValueSelectAsync(SelectEventArgs<TDOM_DOMAINES> args)
    {
        // Update selected domains.
        _filters.SelectedDomains.Add(args.ItemData);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when a domain is removed.
    /// </summary>
    /// <param name="args">Remove event arguments.</param>
    private async Task DomainFilterValueRemovedAsync(RemoveEventArgs<TDOM_DOMAINES> args)
    {
        // Remove chips.
        // When chips depend on a list, they are not automatically removed when the list is updated.
        _domainSfChipRef.RemoveChips(new[] { args.ItemData.TDOM_LIB });

        // Update selected domains.
        var deletedDomain = _filters.SelectedDomains
            .FirstOrDefault(d => d.TDOM_DOMAINEID.Equals(args.ItemData.TDOM_DOMAINEID));
        _filters.SelectedDomains.Remove(deletedDomain);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when all domains are cleared.
    /// </summary>
    /// <param name="args">Mouse event arguments.</param>
    private async Task DomainFilterClearedAsync(MouseEventArgs args)
    {
        // Update selected domains.
        _filters.SelectedDomains.Clear();

        await UpdateEtqFilteringAsync();
    }
    #endregion

    #region Labeled Objects Multiselect (TOBJE_OBJET_ETIQUETTES)
    /// <summary>
    /// Reference to "labeled object" SfChip component.
    /// </summary>
    private SfChip _objectSfChipRef;

    /// <summary>
    /// Query applied on "labeled object" multiselect:
    ///  - First page size,
    ///  - Sort by "labeled objects" labels in alphabetical order.
    /// </summary>
    private readonly Query _objectsSelectQuery = new Query().Take(Globals.PageSize)
        .Sort(nameof(TOBJE_OBJET_ETIQUETTES.TOBJE_LIB), "Ascending");

    /// <summary>
    /// Action called when a new "labeled object" is selected.
    /// </summary>
    /// <param name="args">Select event arguments.</param>
    private async Task ObjectFilterOnValueSelectAsync(SelectEventArgs<TOBJE_OBJET_ETIQUETTES> args)
    {
        // Update selected "labeled object".
        _filters.SelectedObjects.Add(args.ItemData);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when a "labeled object" is removed.
    /// </summary>
    /// <param name="args">Remove event arguments.</param>
    private async Task ObjectFilterValueRemovedAsync(RemoveEventArgs<TOBJE_OBJET_ETIQUETTES> args)
    {
        // Remove chips.
        // When chips depend on a list, they are not automatically removed when the list is updated.
        _objectSfChipRef.RemoveChips(new[] { args.ItemData.TOBJE_LIB });

        // Update selected "labeled objects".
        var deletedObject = _filters.SelectedObjects
            .FirstOrDefault(o => o.TOBJE_OBJET_ETIQUETTEID.Equals(args.ItemData.TOBJE_OBJET_ETIQUETTEID));
        _filters.SelectedObjects.Remove(deletedObject);

        await UpdateEtqFilteringAsync();
    }

    /// <summary>
    /// Action called when all "labeled objects" are cleared.
    /// </summary>
    /// <param name="args">Mouse event arguments.</param>
    private async Task ObjectFilterClearedAsync(MouseEventArgs args)
    {
        // Update selected "labeled objects".
        _filters.SelectedObjects.Clear();

        await UpdateEtqFilteringAsync();
    }
    #endregion
}
