using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.ADM.Components.ApplicationsAdministration;

public partial class TRCLICL_CLIENTAPPLICATIONS_CLAIMS_SfGridComponent
{
    private OrkaGenericGridComponent<TRCLICL_CLIENTAPPLICATIONS_CLAIMS> Ref_Grid;

    #region Parameters
    /// <summary>
    /// Id of the selected client application.
    /// </summary>
    [Parameter] public int ClientApplicationId { get; set; }

    [Parameter] public Query GridQuery { get; set; }
    #endregion

    #region SfGrid parameters
    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] DisplayedFields = {
        nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS.TRCLICL_CLIENTAPPLICATION_CLAIMID),
        nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS.TRCLICL_STATUS),
        nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS.TRCL_CLAIMID),
        nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS.TRCLICL_CLAIM_VALUE),
        nameof(TRCLICL_CLIENTAPPLICATIONS_CLAIMS.TRCLICL_DESCRIPTION),
     };

    /// <summary>
    /// List of ClaimsCatalog entries.
    /// </summary>
    private IEnumerable<TRCCL_CATALOG_CLAIMS> ClaimsCatalog = Enumerable.Empty<TRCCL_CATALOG_CLAIMS>();
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        // Get claims catalog.
        ClaimsCatalog = await ProxyCore.GetEnumerableAsync<TRCCL_CATALOG_CLAIMS>();
    }
    #endregion

    #region SfGrid Events
    /// <summary>
    /// Event triggered when DataGrid actions are completed.
    /// </summary>
    private void OnActionComplete(ActionEventArgs<TRCLICL_CLIENTAPPLICATIONS_CLAIMS> args)
    {
        /* Disable prevent render.*/
        if (args.RequestType is Action.Add or Action.BeginEdit)
        {
            args.PreventRender = false;
        }
    }

    /// <summary>
    /// Event called when user save an edition.
    /// </summary>
    /// <param name="clientApplicationsClaims">Item to update.</param>
    private Task FooterSaveEditAsync(TRCLICL_CLIENTAPPLICATIONS_CLAIMS clientApplicationsClaims)
    {
        // Set the ID of selected object.
        clientApplicationsClaims.TRCLI_CLIENTAPPLICATIONID = ClientApplicationId;

        return Ref_Grid.DataGrid.EndEditAsync();
    }
    #endregion
}
