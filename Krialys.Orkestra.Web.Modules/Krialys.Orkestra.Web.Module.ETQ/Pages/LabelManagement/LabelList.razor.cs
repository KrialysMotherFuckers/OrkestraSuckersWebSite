using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.ETQ.Pages.LabelManagement;
public partial class LabelList
{
    [Inject] IJSInProcessRuntime JsProcessRuntime { get; set; }

    private OrkaGenericGridComponent<TETQ_ETIQUETTES> Ref_Grid;

    private OrkaGenericGridComponent<VDE_DEMANDES_ETENDUES> Ref_GridView;

    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    private Query GridQuery;

    /// <summary>
    /// Table of visible fields when editing.
    /// </summary>
    public string[] _customEditFields = {
      //  nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID), // OB-371
        nameof(TETQ_ETIQUETTES.TETQ_LIB),
        nameof(TETQ_ETIQUETTES.TETQ_DESC),
    };

    private TETQ_ETIQUETTES _selectedLabel;
    private bool _isEtqAuthorizationDialogDisplayed;

    private string _odataOrderBy { get; set; }

    private string _odataFilter { get; set; }

    private int _etiquettesCount;

    private void OnOrderByChangedAsync(string odataOrderBy)
    {
        //// Get new value for OData orderby parameter.
        //_odataOrderBy = odataOrderBy;

        //SetQuery(_queryBase, _odataFilter, _odataOrderBy);

        //// Re-render component.
        //StateHasChanged();
    }

    private const string _queryBase = "?$expand=TPRCP_PRC_PERIMETRE,TOBJE_OBJET_ETIQUETTE($expand=TDOM_DOMAINE)";

    private void OnDataBound() => _etiquettesCount = Ref_Grid.DataGrid.TotalItemCount;

    private void OnFilterChangedAsync(string odataFilter)
    {
        // Get new value for OData filter parameter.
        _odataFilter = $"&${odataFilter}";

        SetQuery(_queryBase, _odataFilter, _odataOrderBy);

        // Re-render component.
        StateHasChanged();
    }

    private void SetQuery(string queryBase, string queryFilter = "", string queryOrderBy = "")
    {
        var queryFull = $"{queryBase}{queryFilter}" + (string.IsNullOrEmpty(queryOrderBy) ? "" : $"&${queryOrderBy}");

        GridQuery = new Query()
               .AddParams(Litterals.OdataQueryParameters, queryFull)
               .Sort(nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID), FiltersLiterals.Descending);
    }

    private async Task OnCloseAsync()
    {
        // Reset cache for these entities
        var count = ProxyCore.CacheRemoveEntities(
            typeof(TETQ_ETIQUETTES),
            typeof(VDE_DEMANDES_ETENDUES)
        );

        var GetRefreshGridButtonId = $"RefreshGridButton_{Ref_Grid.DataGrid.ID}";

        await JsProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, GetRefreshGridButtonId, 250);
    }
}
