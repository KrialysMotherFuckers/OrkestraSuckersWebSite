using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD.Dialogs.Qualifs;
public partial class TDQ_DEMANDE_QUALIFS_Recueil
{
    #region Parameters
    /// <summary>
    /// Id of selected TDQ_DEMANDE_QUALIFS.TD_DEMANDEID (mandatory).
    /// </summary>
    [Parameter] public int DemandeId { get; set; }

    /// <summary>
    /// Is dialog visible.
    /// </summary>
    [Parameter] public bool IsDisplayed { get; set; }

    [Parameter] public EventCallback<bool> IsDisplayedChanged { get; set; }
    #endregion

    private async Task CloseQualifDialog()
    {
        IsDisplayed = false;

        // Update parent with new value
        await IsDisplayedChanged.InvokeAsync(IsDisplayed);
    }
}
