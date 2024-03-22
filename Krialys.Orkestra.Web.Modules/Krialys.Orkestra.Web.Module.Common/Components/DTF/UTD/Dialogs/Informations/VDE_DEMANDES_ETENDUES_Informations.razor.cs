using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD.Dialogs.Informations;
public partial class VDE_DEMANDES_ETENDUES_Informations
{
    #region Parameters
    /// <summary>
    /// Id of selected VDE_DEMANDES_ETENDUES.TD_DEMANDEID (mandatory)
    /// </summary>
    [Parameter] public int DemandeId { get; set; } //= 10; // TEST

    /// <summary>
    /// Is dialog visible
    /// </summary>
    [Parameter] public bool IsDisplayed { get; set; }

    [Parameter]
    public EventCallback<bool> IsDisplayedChanged { get; set; }

    private bool _isInfosDemandeDialogDisplayed;

    private VDE_DEMANDES_ETENDUES _infosDemande;

    private async Task CloseInfosDemandeDialog()
    {
        _isInfosDemandeDialogDisplayed = IsDisplayed = false;

        // Update parent with new value
        await IsDisplayedChanged.InvokeAsync(IsDisplayed);
    }

    #endregion

    #region Blazor life cycle

    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Get link table between TER_ETAT_RESSOURCE and TS_SCENARIO from DB.
        var data = await ProxyCore.GetEnumerableAsync<VDE_DEMANDES_ETENDUES>($"?$filter=TD_DEMANDEID eq {DemandeId}", useCache: false, convertToLocalDateTime: true);

        _infosDemande = data.FirstOrDefault();
        _isInfosDemandeDialogDisplayed = true;
    }

    #endregion
}
