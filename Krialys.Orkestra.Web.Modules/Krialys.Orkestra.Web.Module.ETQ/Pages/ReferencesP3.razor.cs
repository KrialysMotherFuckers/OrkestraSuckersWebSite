using Krialys.Data.EF.Etq;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.ETQ.Pages;

public partial class ReferencesP3
{
    [Inject] private ILogger<ReferencesP3> Logger { get; set; }

    // Parent instances
    private OrkaGenericGridComponent<TOBF_OBJ_FORMATS> Ref_TOBF_OBJ_FORMATS;
    private OrkaGenericGridComponent<TOBN_OBJ_NATURES> Ref_TOBN_OBJ_NATURES;
    private OrkaGenericGridComponent<TTDOM_TYPE_DOMAINES> Ref_TTDOM_TYPE_DOMAINES;
    private OrkaGenericGridComponent<TEQC_ETQ_CODIFS> Ref_TEQC_ETQ_CODIFS;
    private OrkaGenericGridComponent<TTE_TYPE_EVENEMENTS> Ref_TTE_TYPE_EVENEMENTS;
    private OrkaGenericGridComponent<TTR_TYPE_RESSOURCES> Ref_TTR_TYPE_RESSOURCES;
    private OrkaGenericGridComponent<TACT_ACTIONS> Ref_TACT_ACTIONS;

    #region OnRowSelected

    /// <summary>
    /// Custom event when a row is selected we can add some controls onto any cells
    /// </summary>
    /// <param name="args"></param>
    private void OnRowSelected<TEntity>(RowSelectEventArgs<TEntity> args) where TEntity : class, new()
    {
        // Without this, you may see noticable delay in selection with 75 rows in grid.
        args.PreventRender = true;

        // Here you can customize your code
        //var row = args.Data;

        //if(args.Data is TEQC_ETQ_CODIFS teqc)
        //{
        //    teqc.TEQC_SEPARATEUR ??= "z";
        //}

        args.PreventRender = false;
    }

    #endregion

    #region OnDataBound

    /// <summary>
    /// Custom event triggered before data is bound to datagrid
    /// Some business rules can be applied precisely onto all or onto several lines and cells using LINQ
    /// CSS rules can be applied to enable line edition for example or for colouring a cell...
    /// </summary>
    /// <param name="args"></param>
    public void BeforeDataBound<TEntity>(BeforeDataBoundArgs<TEntity> args) where TEntity : class, new()
    {
        // Without this, you may see noticable delay in selection with 75 rows in grid.
        args.PreventRender = true;

        // Here you can customize your code: apply rules onto each row/cell
        //foreach (var row in args.Result)
        //{
        //}

        args.PreventRender = false;
    }

    #endregion

    #region SaveAsync

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="Instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="Entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> Instance, object Entity) where TEntity : class, new()
    {
        // Update datas here if necessary
        if (Entity is TEQC_ETQ_CODIFS)
        {
        }

        await Instance.DataGrid.EndEditAsync();
    }

    #endregion

    private bool Disabled { get; set; }

    protected override void OnInitialized()
    {
        // Subscribe SfTabEvent
        Bus.Subscribe<SfTabBusEvent>(e =>
        {
            var result = e.GetMessage<SfTabBusEvent>().Disabled;
            if (Disabled != result)
            {
                Disabled = result;
                // Refresh UI
                StateHasChanged();
            }
        });
    }

    private void OnTabSelecting(SelectingEventArgs args)
    {
        // Disable Tab navigation on Tab selection.
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