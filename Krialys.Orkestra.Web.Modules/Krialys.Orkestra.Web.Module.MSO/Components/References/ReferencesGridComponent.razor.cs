using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.MSO.Components.References;

public partial class ReferencesGridComponent<TEntity> where TEntity : class, new()
{
    #region Parameters
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    #region Datagrid
    private OrkaGenericGridComponent<TEntity> _refGrid;
    #endregion
}
