using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.ADM.Components;

public partial class UserAuthorizationComponent
{
    private OrkaGenericGridComponent<TRU_USERS> Ref_Grid;

    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] CustomDisplayedFields = {
       nameof(TRU_USERS.TRU_USERID),
       nameof(TRU_USERS.TRU_STATUS),
       nameof(TRU_USERS.TRLG_LNGID),
       nameof(TRU_USERS.TRTZ_TZID),
       nameof(TRU_USERS.TRU_LOGIN),
       nameof(TRU_USERS.TRU_NAME),
       nameof(TRU_USERS.TRU_FIRST_NAME),
       nameof(TRU_USERS.TRU_EMAIL),
       nameof(TRU_USERS.TRU_ALLOW_INTERNAL_AUTH)
    };

    /// <summary>
    /// Event called when user save an edition.
    /// </summary>
    /// <param name="user">Item to update.</param>
    private async Task FooterSaveEditAsync(TRU_USERS user)
    {
        // If password is undefined, create a new one based on login.
        if (user.TRU_PWD is null)
        {
            user.TRU_PWD = await ProxyCore.CreateCipheredPassword(user.TRU_LOGIN);
        }
        else if (!(user.TRU_PWD.Length.Equals(88) && user.TRU_PWD.EndsWith("==")))
        {
            user.TRU_PWD = await ProxyCore.CreateCipheredPassword(user.TRU_PWD);
        }

        // Call base save method.
        await Ref_Grid.DataGrid.EndEditAsync();
    }

    /// <summary>
    /// Model to use when using QueryCellInfo handler
    /// See: https://blazor.syncfusion.com/documentation/datagrid/cell
    /// </summary>
    /// <param name="args"></param>
    private void OnQueryCellInfoHandler<TEntity>(QueryCellInfoEventArgs<TEntity> args) where TEntity : class, new()
    {
        if (args.Data is TRU_USERS)
        {
            if (args.Column.Field.Equals(nameof(TRU_USERS.TRU_PWD)))
            {
                args.Cell.AddClass(new[] { "e-content-password" });
            }
        }
    }
}