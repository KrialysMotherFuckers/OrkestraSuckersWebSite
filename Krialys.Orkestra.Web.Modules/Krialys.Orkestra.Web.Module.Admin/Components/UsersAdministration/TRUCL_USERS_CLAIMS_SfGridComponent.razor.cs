using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.ADM.Components.UsersAdministration;

public partial class TRUCL_USERS_CLAIMS_SfGridComponent
{
    private OrkaGenericGridComponent<TRUCL_USERS_CLAIMS> _refGrid;

    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] CustomDisplayedFields = {
       nameof(TRUCL_USERS_CLAIMS.TRUCL_USER_CLAIMID),
       nameof(TRUCL_USERS_CLAIMS.TRUCL_STATUS),
       nameof(TRUCL_USERS_CLAIMS.TRCLI_CLIENTAPPLICATIONID),
       nameof(TRUCL_USERS_CLAIMS.TRCL_CLAIMID),
       nameof(TRUCL_USERS_CLAIMS.TRUCL_CLAIM_VALUE),
       nameof(TRUCL_USERS_CLAIMS.TRUCL_DESCRIPTION),
    };

    #region Parameters
    /// <summary>
    /// Id of the selected user.
    /// </summary>
    [Parameter] public string UserId { get; set; }

    [Parameter] public Query GridQuery { get; set; }
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Get claims catalog.
        ClaimsCatalog = await ProxyCore.GetEnumerableAsync<TRCCL_CATALOG_CLAIMS>();
    }
    #endregion

    #region SfGrid properties
    /// <summary>
    /// List of ClaimsCatalog entries.
    /// </summary>
    private IEnumerable<TRCCL_CATALOG_CLAIMS> ClaimsCatalog = Enumerable.Empty<TRCCL_CATALOG_CLAIMS>();
    #endregion

    #region SfGrid Events
    /// <summary>
    /// Event triggered when DataGrid actions are completed.
    /// </summary>
    private void OnActionComplete(ActionEventArgs<TRUCL_USERS_CLAIMS> args)
    {
        /* Disable prevent render.
         * https://blazor.syncfusion.com/documentation/datagrid/webassembly-performance/#avoid-unnecessary-component-renders*/
        if (args.RequestType is Action.Add or Action.BeginEdit)
        {
            args.PreventRender = false;
        }
    }

    /// <summary>
    /// Event called when user save an edition.
    /// </summary>
    /// <param name="usersClaims">Item to update.</param>
    private Task FooterSaveEditAsync(TRUCL_USERS_CLAIMS usersClaims)
    {
        // Set the ID of selected object.
        usersClaims.TRU_USERID = UserId;

        return _refGrid.DataGrid.EndEditAsync();
    }
    #endregion
}
