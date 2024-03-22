using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD.Dialogs.Qualifs;

public partial class TDQ_DEMANDE_QUALIFS_TableComponent
{
    #region Parameters
    /// <summary>
    /// Id of selected TD_DEMANDES.TD_DEMANDEID.
    /// </summary>
    [Parameter] public int DemandeId { get; set; }
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override Task OnInitializedAsync() => GetQualifs();
    #endregion

    #region TDQ_DEMANDE_QUALIFS Table
    /// <summary>
    /// List of TDQ_DEMANDE_QUALIFS.
    /// </summary>
    private IEnumerable<TDQ_DEMANDE_QUALIFS> Qualifs { get; set; } = Enumerable.Empty<TDQ_DEMANDE_QUALIFS>();

    /// <summary>
    /// Read TDQ_DEMANDE_QUALIFS from data base.
    /// </summary>
    private async Task GetQualifs()
    {
        // Read TDQ_DEMANDE_QUALIFS,
        // filter with selected TD_DEMANDEID,
        // Order by ascending TDQ_DEMANDE_QUALIFID.
        Qualifs = await ProxyCore.GetEnumerableAsync<TDQ_DEMANDE_QUALIFS>($"?$filter=TD_DEMANDEID eq {DemandeId}&$orderby=TDQ_DEMANDE_QUALIFID asc");
    }
    #endregion
}
