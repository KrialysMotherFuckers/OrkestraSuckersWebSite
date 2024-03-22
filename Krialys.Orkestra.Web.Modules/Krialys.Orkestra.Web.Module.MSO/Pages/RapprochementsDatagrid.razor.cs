using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;

namespace Krialys.Orkestra.Web.Module.MSO.Pages;

public partial class RapprochementsDatagrid
{
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    private bool AllowModify { get; set; } = true;

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Check if the user is authorized to modify the data.
        AllowModify = await Session.VerifyPolicy(PoliciesLiterals.RapprochementsEditor);
    }
    #endregion

    #region Datagrid
    private OrkaGenericGridComponent<TRA_ATTENDUS> Ref_Grid;
    #endregion
}
