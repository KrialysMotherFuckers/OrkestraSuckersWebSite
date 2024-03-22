using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd.Components;

public partial class TEB_ETAT_BATCHS_GridComponent
{
    #region Parameters
    /// <summary>
    /// Sf query applied to the grid.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    #region Grid
    /// <summary>
    /// Reference to the grid component.
    /// </summary>
    private OrkaGenericGridComponent<TEB_ETAT_BATCHS> Ref_TEB_ETAT_BATCHS;

    /// <summary>
    /// Table of hidden fields when editing.
    /// </summary>
    private readonly string[] _editingHiddenFields = {
        nameof(TEB_ETAT_BATCHS.TRST_STATUTID),
        nameof(TEB_ETAT_BATCHS.TEB_NOM_AFFICHAGE),
        nameof(TEB_ETAT_BATCHS.TEB_DATE_CREATION)
    };

    /// <summary>
    /// Get header for grid edit template.
    /// </summary>
    /// <param name="data">Edited item.</param>
    /// <returns>Edit header text.</returns>
    public string GetEditHeader(TEB_ETAT_BATCHS data) => $"{Trad.Keys["DTM:UTDCatalogBatchsGridEditHeader"]}{data.TEB_NOM_AFFICHAGE}";

    protected override Task OnInitializedAsync()
    {
        ProxyCore.CacheRemoveEntities(typeof(TEB_ETAT_BATCHS));

        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Event triggers when DataGrid actions starts.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    private async Task OnActionBeginAsync(ActionEventArgs<TEB_ETAT_BATCHS> args)
    {
        switch (args.RequestType)
        {
            case Action.BeginEdit:
                // Hide columns that can't be edited.
                await Ref_TEB_ETAT_BATCHS.DataGrid.HideColumnsAsync(_editingHiddenFields, hideBy: "Field");
                break;
        }
    }

    /// <summary>
    /// Event triggers when DataGrid actions are completed.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public async Task OnActionCompleteAsync(ActionEventArgs<TEB_ETAT_BATCHS> args)
    {
        switch (args.RequestType)
        {
            case Action.Save:
            case Action.Cancel:
                // Show again hidden columns.
                await Ref_TEB_ETAT_BATCHS.DataGrid.ShowColumnsAsync(_editingHiddenFields, showBy: "Field");
                break;
        }
    }
    #endregion
}
