using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.MSO.Components.Rapprochements;

public partial class RapprochementsLogsGridComponent
{
    #region Parameters
    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    [Parameter] public Query Query { get; set; }

    /// <summary>
    /// TRA_CODE from TRA_ATTENDUS parent grid.
    /// </summary>
    [Parameter] public string TRA_CODE { get; set; }
    #endregion

    #region Datagrid
    private OrkaGenericGridComponent<TTL_LOGS> _refGrid;
    #endregion
}
