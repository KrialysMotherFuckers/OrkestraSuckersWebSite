using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using System.Text.Json;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.ReferencesP2;

public partial class TETQ_ETIQUETTES_GridComponent
{
    #region Parameters
    /// <summary>
    /// Number of labels (depending on applied filters and searches).
    /// </summary>
    [Parameter] public int LabelsCount { get; set; }
    [Parameter] public EventCallback<int> LabelsCountChanged { get; set; }
    #endregion

    #region Blazor life cycle
    protected override void OnInitialized()
        // Initialize grid query.
        => SetQuery();
    #endregion

    #region Grid TETQ_ETIQUETTES
    /// <summary>
    /// Reference to the data grid component.
    /// </summary>
    private OrkaGenericGridComponent<TETQ_ETIQUETTES> _gridRef;
    #endregion

    #region Grid displayed fields
    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] _customLoadFields = {
        nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID), // OB-371
        nameof(TETQ_ETIQUETTES.TETQ_CODE),
        nameof(TETQ_ETIQUETTES.TOBJE_OBJET_ETIQUETTEID), // => TOBJE_OBJET_ETIQUETTES.TOBJE_CODE
        nameof(TETQ_ETIQUETTES.TETQ_LIB),
        nameof(TETQ_ETIQUETTES.TETQ_DESC),
        nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETREID),   // => TPRCP_PRC_PERIMETRES.TPRCP_CODE
        nameof(TETQ_ETIQUETTES.DEMANDEID),
        nameof(TETQ_ETIQUETTES.TETQ_VERSION_ETQ),
        nameof(TETQ_ETIQUETTES.TETQ_DATE_CREATION),
        //"TETQ_INCREMENT",
        //"TETQ_PATTERN",
        nameof(TETQ_ETIQUETTES.TETQ_PRM_VAL),
        nameof(TETQ_ETIQUETTES.etq_is_public_access)
    };

    /// <summary>
    /// Table of visible fields when editing.
    /// </summary>
    public string[] _customEditFields = {
      //  nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID), // OB-371
        nameof(TETQ_ETIQUETTES.TETQ_LIB),
        nameof(TETQ_ETIQUETTES.TETQ_DESC),
    };
    #endregion

    #region Grid Query
    /// <summary>
    /// Sf query used to filter and order grid.
    /// Sort with most recent.
    /// </summary>
    private Query _gridQuery;

    /// <summary>
    /// Update grid query to apply new filters.
    /// </summary>
    /// <param name="filters">Filters to apply.</param>
    private void SetQuery(EtqFilters filters = null)
    {
        _gridQuery = new Query()
            .Sort(nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID), FiltersLiterals.Descending);

        string serializedFilters = JsonSerializer.Serialize(filters);

        if (filters is not null)
            _gridQuery.AddParams(Litterals.FiltersEtq, serializedFilters);
    }
    #endregion

    #region Grid command buttons
    /// <summary>
    /// Selected label.
    /// </summary>
    private TETQ_ETIQUETTES _selectedLabel;

    /// <summary>
    /// Id of authorization management command.
    /// </summary>
    private const string AuthorizationCommandId = "AuthorizationManagement";

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click arguments.</param>
    private void CommandClicked(CommandClickEventArgs<TETQ_ETIQUETTES> args)
    {
        _selectedLabel = args.RowData;

        // Launch command.
        switch (args.CommandColumn.ID)
        {
            case AuthorizationCommandId:
                DisplayEtqAuthorizationDialog();
                break;
        }
    }
    #endregion

    #region Grid DataBound events
    /// <summary>
    /// Event triggered before data is bound to DataGrid.
    /// </summary>
    /// <param name="args">BeforeDataBound event arguments.</param>
    public Task OnDataBoundAsync(BeforeDataBoundArgs<TETQ_ETIQUETTES> args)
    {
        // In addition to the labels (TETQ_ETQ), the grid has column
        // containing the ids of the orders (TCMD_COMMANDES).

        // Since labels and orders are in two different databases,
        // they are read separately.

        // At this point, the labels have already been read and
        // now we will read the order ids.
        return ReadOrderIdsAsync(args.Result);
    }

    /// <summary>
    /// Event triggered when data source is populated in the DataGrid.
    /// </summary>
    private void DataBound()
    {
        _gridRef.DataBoundHander();

        UpdateLabelsCountAsync();
    }
    #endregion

    #region Column order ids
    /// <summary>
    /// Dictionary used to do a conversion between a label id (key)
    /// and its order ids (value).
    /// </summary>
    private Dictionary<int, string> _labelsToOrders = new();

    /// <summary>
    /// Read order ids for each of the labels displayed in the grid.
    /// </summary>
    /// <param name="labels">Labels displayed in the grid.</param>
    private async Task ReadOrderIdsAsync(List<TETQ_ETIQUETTES> labels)
    {
        // Orders and labels can be linked through
        // the request id (TD_DEMANDES.TD_DEMANDEID)
        // using table TCMD_DA_DEMANDES_ASSOCIEES.

        /**** Step 1: Get request ids *****/
        // Select distinct request ids that are not null from labels.
        IEnumerable<int> requestIds = labels.Where(l => l.DEMANDEID is not null)
                .Select(l => l.DEMANDEID.Value)
                .Distinct();

        // If there are no requests, there are no linked orders.
        if (!requestIds.Any())
            return;

        /***** Step 2: Read requests-orders link table *****/
        // Prepare list of request ids to read.
        string requestIdsList = $"'{String.Join("', '", requestIds)}'";

        // Prepare OData filter.
        string filter = $"{nameof(TCMD_DA_DEMANDES_ASSOCIEES.TD_DEMANDEID)} in ({requestIdsList})";
        // Read request-order link table.
        var _requestOrders = await ProxyCore.GetEnumerableAsync<TCMD_DA_DEMANDES_ASSOCIEES>(
            $"?$filter={filter}");

        /***** Step 3: Fill the labels/orders dictionary *****/
        // Reset dictionary because the values of TCMD_DA_DEMANDES_ASSOCIEES can change.
        _labelsToOrders = new();

        foreach (var label in labels)
        {
            // Get order ids of related labels,
            // using the request id to do the linking.
            IEnumerable<int> orderNumbers = _requestOrders
                .Where(ro => ro.TD_DEMANDEID.Equals(label.DEMANDEID))
                .Select(ro => ro.TCMD_COMMANDEID);

            // Add an item to the dictionary.
            _labelsToOrders.TryAdd(label.TETQ_ETIQUETTEID,
                String.Join(", ", orderNumbers));
        }
    }
    #endregion

    #region Update labels count
    /// <summary>
    /// Read number of labels in the grid.
    /// And update the related parameter.
    /// </summary>
    private Task UpdateLabelsCountAsync()
    {
        // Read cound from DataGrid Component.
        LabelsCount = _gridRef.DataGrid.TotalItemCount;

        // Update parent with new value.
        return LabelsCountChanged.InvokeAsync(LabelsCount);
    }
    #endregion

    #region Apply filters
    /// <summary>
    /// Apply filters to the grid and refresh data.
    /// </summary>
    /// <param name="filters">Filters to apply.</param>
    public Task ApplyFiltersAsync(EtqFilters filters)
    {
        // Update grid query.
        SetQuery(filters);

        // Refresh grid data.
        return _gridRef.Refresh();
    }
    #endregion

    #region ETQ authorization Dialog
    /// <summary>
    /// Is ETQ authorization dialog displayed?
    /// </summary>
    private bool _isEtqAuthorizationDialogDisplayed;

    /// <summary>
    /// Display dialog to manage ETQ authorizations.
    /// </summary>
    private void DisplayEtqAuthorizationDialog()
    {
        // Open dialog.
        _isEtqAuthorizationDialogDisplayed = true;
    }
    #endregion
}
