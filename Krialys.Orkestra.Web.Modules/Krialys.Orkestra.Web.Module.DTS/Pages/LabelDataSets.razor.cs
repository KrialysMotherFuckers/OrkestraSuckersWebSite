namespace Krialys.Orkestra.Web.Module.DTS.Pages;

public partial class LabelDataSets
{
    private bool _isLoading;

    private List<Krialys.Orkestra.Common.Shared.ETQ.EtiquetteDetails> _etiquetteDetailsList = new();
    private string _searchFilter;
    private string _searchSort;
    private string _searchSkip;

    private bool SortByNewest = false;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            await GetData();
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task GetData()
    {
        await ResetEtiquettesAsync();

        _etiquetteDetailsList = _etiquettes;

        //var res = await ProxyCore.GetEtiquetteDetails($"?{_searchSort}{_searchFilter}");
        //if (res != null)
        //{
        //    _etiquetteDetailsList = res.Result?.ToList();
        //}
    }

    #region EtiquettesSearch Component
    /// <summary>
    /// OData orderby parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataOrderBy { get; set; }

    /// <summary>
    /// OData filter parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataFilter { get; set; } = "&orderby=TETQ_CODE asc";

    /// <summary>
    /// Event trigger when TETQ_ETIQUETTES read order changed.
    /// </summary>
    /// <param name="odataOrderBy">OData orderby parameter.</param>
    private async Task OnOrderByChangedAsync(string odataOrderBy)
    {
        // Get new value for OData orderby parameter.
        _odataOrderBy = odataOrderBy;
        _searchSort = _odataOrderBy;

        //await GetData();

        // Reload displayed Etiquettes.
        await ResetEtiquettesAsync();
    }

    /// <summary>
    /// Event trigger when TETQ_ETIQUETTES filters changed.
    /// </summary>
    /// <param name="odataFilter">OData filter parameter.</param>
    private async Task OnFilterChangedAsync(string odataFilter)
    {
        // Get new value for OData filter parameter.
        _odataFilter = $"&{odataFilter}";
        _searchFilter = odataFilter;

        //await GetData();

        // Reload displayed Etiquettes.
        await ResetEtiquettesAsync();
    }
    #endregion

    #region Main: ETQ list
    /// <summary>
    /// Id of the HTML element used to trigger the loading of additional data.
    /// </summary>
    private readonly string _observerTargetId = "loadingCard";

    /// <summary>
    /// List of displayed TETQ_ETIQUETTES.
    /// </summary>
    private readonly List<Krialys.Orkestra.Common.Shared.ETQ.EtiquetteDetails> _etiquettes = new();

    /// <summary>
    /// Semaphore to avoid read-write collision on TETQ_ETIQUETTES list.
    /// </summary>
    private readonly SemaphoreSlim _semaphoreEtiquettes = new SemaphoreSlim(1);

    /// <summary>
    /// Number of TETQ_ETIQUETTES loaded by page.
    /// </summary>
    private const int _pageSize = Globals.PageSize;

    /// <summary>
    /// Number of TETQ_ETIQUETTES loaded.
    /// </summary>
    private int _etiquettesLoadedCount;

    /// <summary>
    /// Maximum number of TETQ_ETIQUETTES loaded.
    /// No other pages are loaded if this value is exceeded.
    /// </summary>
    private const int _etiquettesMax = 79;

    /// <summary>
    /// Total number of etiquettes (depending on applied filters and searches).
    /// </summary>
    private int _etiquettesCount;

    /// <summary>
    /// Fill the list of TETQ_ETIQUETTES when a new page is required.
    /// </summary>
    private Task FetchEtiquettesAsync()
        => FetchEtiquettesAsync(reset: false);

    /// <summary>
    /// Fill the list of TETQ_ETIQUETTES when a new page is required.
    /// </summary>
    /// <param name="reset">If true, the list of TETQ_ETIQUETTES is cleared.</param>
    private async Task FetchEtiquettesAsync(bool reset = false)
    {
        // Wait that TETQ_ETIQUETTES list is available for modification.
        await _semaphoreEtiquettes.WaitAsync();

        // OData orderby parameter must be initialized
        // And it is a data reset or there is still data to read
        if (_odataOrderBy is not null &&
            (reset || _etiquettes.Count == _etiquettesLoadedCount))
        {
            /***** Read from database *****/
            // Prepare Odata skip parameter.
            string _odataSkip = reset ? string.Empty : $"&$skip={_etiquettes.Count}";

            // Prepare Odata query:
            //  apply sorting and filtering,
            //  set page read start (with skip).
            string odataQuery = $"?{_odataFilter}{_odataSkip}";

            // Get one page of TETQ_ETIQUETTES
            var data = await ProxyCore.GetEtiquetteDetails(odataQuery);

            if (reset)
            {
                // Reset loaded count.
                _etiquettesLoadedCount = 0;

                // Empty list of TETQ_ETIQUETTES.
                _etiquettes.Clear();
            }

            /***** Update page with read data *****/
            // Update total count = EtiquetteDetails count + skip.
            _etiquettesCount = data.Count + _etiquettes.Count;

            // Add read data to data list.
            _etiquettes.AddRange(data.Result);

            // Update read counter.
            _etiquettesLoadedCount += _pageSize;
        }

        // Make TETQ_ETIQUETTES list available for modification.
        _semaphoreEtiquettes.Release();
    }

    /// <summary>
    /// Empty the list of TETQ_ETIQUETTES and re-render component.
    /// It will trigger a new read of TETQ_ETIQUETTES.
    /// </summary>
    private async Task ResetEtiquettesAsync()
    {
        await FetchEtiquettesAsync(reset: true);

        // Re-render component.
        StateHasChanged();
    }
    #endregion
}
