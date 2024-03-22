using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.Common.Components.ETQ;

public partial class GridEtiquettesNew
{
    private OrkaGenericGridComponent<TETQ_ETIQUETTES> Ref_Grid;

    private OrkaGenericGridComponent<VDE_DEMANDES_ETENDUES> Ref_GridView;

    #region Parameters

    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    private Query GridQuery;

    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] CustomDisplayedFields = {
       //nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID),
       nameof(TETQ_ETIQUETTES.TETQ_CODE),
       nameof(TETQ_ETIQUETTES.TETQ_LIB),
       nameof(TETQ_ETIQUETTES.TETQ_DESC),
       nameof(TETQ_ETIQUETTES.TETQ_DATE_CREATION), // level 0
       $"{nameof(TETQ_ETIQUETTES.TOBJE_OBJET_ETIQUETTE)}.{nameof(TETQ_ETIQUETTES.TOBJE_OBJET_ETIQUETTE.TDOM_DOMAINE)}.{nameof(TETQ_ETIQUETTES.TOBJE_OBJET_ETIQUETTE.TDOM_DOMAINE.TDOM_LIB)}", // level 2
       $"{nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETRE)}.{nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETRE.TPRCP_LIB)}" // level 1
    };

    #endregion

    #region EtiquettesSearch Component
    /// <summary>
    /// OData orderby parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataOrderBy { get; set; }

    /// <summary>
    /// OData filter parameter for TETQ_ETIQUETTES.
    /// </summary>
    private string _odataFilter { get; set; }

    /// <summary>
    /// Total number of etiquettes (depending on applied filters and searches).
    /// </summary>
    private int _etiquettesCount;

    /// <summary>
    /// Etiquette for building Arbo
    /// </summary>
    private TETQ_ETIQUETTES _etq;

    /// <summary>
    /// Event trigger when TETQ_ETIQUETTES read order changed.
    /// </summary>
    /// <param name="odataOrderBy">OData orderby parameter.</param>
    private void OnOrderByChangedAsync(string odataOrderBy)
    {
        //// Get new value for OData orderby parameter.
        //_odataOrderBy = odataOrderBy;

        //OnDataSourceLoad(_queryBase, _odataFilter, _odataOrderBy);

        //// Re-render component.
        //StateHasChanged();
    }

    /// <summary>
    /// Event trigger when TETQ_ETIQUETTES filters changed.
    /// </summary>
    /// <param name="odataFilter">OData filter parameter.</param>
    private void OnFilterChangedAsync(string odataFilter)
    {
        // Get new value for OData filter parameter.
        _odataFilter = $"&{odataFilter}";

        OnDataSourceLoad(_queryBase, _odataFilter, _odataOrderBy);

        // Re-render component.
        StateHasChanged();
    }
    #endregion

    private const string _queryBase = "?$expand=TPRCP_PRC_PERIMETRE,TOBJE_OBJET_ETIQUETTE($expand=TDOM_DOMAINE)";

    protected override Task OnInitializedAsync()
    {
        //UserId = Session.GetUserId();
        OnDataSourceLoad(_queryBase, _odataFilter, _odataOrderBy);

        return base.OnInitializedAsync();
    }

    ///// <summary>
    ///// Used as callback to refill datagrid datasource
    ///// </summary>
    ///// <returns></returns>
    private void OnDataSourceLoad(string queryBase, string queryFilter = "", string queryOrderBy = "")
    {
        var queryFull = $"{queryBase}{queryFilter}&{queryOrderBy}";

        GridQuery = new Query()
               .AddParams(Litterals.OdataQueryParameters, queryFull);
        //.Sort(nameof(TH_HABILITATIONS.TH_MAJ_DATE), "descending");
    }

    /// <summary>
    /// Get Datagrid rows count once data has been bound
    /// </summary>
    private void OnDataBound()
        => _etiquettesCount = Ref_Grid.DataGrid.TotalItemCount;

    /// <summary>
    /// Join VDE_DEMANDES_ETENDUES on TETQ_ETIQUETTES
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private Query GetEtqViewQuery(TETQ_ETIQUETTES value)
        => new Query().Where("TD_DEMANDEID", "equal", value.DEMANDEID);

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click argument.</param>
    private void CommandClicked(CommandClickEventArgs<TETQ_ETIQUETTES> args)
    {
        // Data of the row where command is launched.
        _etq = args.RowData;

        //Task task = null;

        // Launch command.
        HideDataGrid = args.CommandColumn.ID switch
        {
            "EtqShowArborescence" => true,//task = Toast.DisplayInfoAsync("Informations", $"Afficher arbo : {data.TETQ_LIB}");
            _ => false,
        };

        //return task ?? Task.CompletedTask; 
    }

    private bool HideDataGrid;

    /// <summary>
    /// Show/hide toolbox
    /// </summary>
    private bool isChecked = true;

    private void ShowDataGrid() => HideDataGrid = false;

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
}
