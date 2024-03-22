using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Settings.References.Components;

public partial class TLE_LOGICIEL_EDITEURS_GridComponent
{
    #region Parameters
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    private OrkaGenericGridComponent<TLE_LOGICIEL_EDITEURS> Ref_TLE_LOGICIEL_EDITEURS;

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        ProxyCore.CacheClear();

        return instance.DataGrid.EndEditAsync();
    }
}
