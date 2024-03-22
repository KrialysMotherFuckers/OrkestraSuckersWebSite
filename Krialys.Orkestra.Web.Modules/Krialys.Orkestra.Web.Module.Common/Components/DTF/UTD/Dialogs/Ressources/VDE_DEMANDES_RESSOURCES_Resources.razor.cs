using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD.Dialogs.Ressources;
public partial class VDE_DEMANDES_RESSOURCES_Resources
{
    #region Parameters
    /// <summary>
    /// Id of selected VDE_DEMANDES_RESSOURCES.TD_DEMANDEID (mandatory)
    /// </summary>
    [Parameter] public int DemandeId { get; set; } = 13;         // TEST

    /// <summary>
    /// Id of selected VDE_DEMANDES_RESSOURCES.TD_DEMANDE_ORIGINEID (optional)
    /// </summary>
    [Parameter] public int? DemandeOrigineId { get; set; }// = 16; // TEST

    /// <summary>
    /// Is dialog visible
    /// </summary>
    [Parameter] public bool IsDisplayed { get; set; }

    [Parameter] public EventCallback<bool> IsDisplayedChanged { get; set; }

    private const string PathRessource = "ParallelU:PathRessource";

    private const string PathRessourceModele = "ParallelU:PathRessourceModele";

    private IEnumerable<VDE_DEMANDES_RESSOURCES> Resources { get; set; } = Enumerable.Empty<VDE_DEMANDES_RESSOURCES>();

    private bool _isResourcesDialogDisplayed;

    private async Task CloseResourceDialog()
    {
        _isResourcesDialogDisplayed = IsDisplayed = false;

        // Update parent with new value
        await IsDisplayedChanged.InvokeAsync(IsDisplayed);
    }

    private void DownloadRessourceModele(int etatId, int resourceId, string nomModele)
    {
        //string demande = DemandeId.ToString().PadLeft(6, '0');
        var etat = etatId.ToString().PadLeft(6, '0');

        // Path of the file to download.
        var filePath = $"{{{PathRessourceModele}}}";

        // Name of the file to download.
        var fileName = $"{etat}/{resourceId}";

        // Download file
        NavigationManager.NavigateTo($"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}&downloadFileName={nomModele ?? fileName}");
    }

    private void DownloadRessourceTransmise(int demandeId, string nomFichier)
    {
        //string demande = DemandeId.ToString().PadLeft(6, '0');
        var demande = demandeId.ToString().PadLeft(6, '0');

        // Path of the file to download.
        var filePath = $"{{{PathRessource}}}";

        // Name of the file to download.
        var fileName = $"{demande}/{nomFichier}";

        // Download file
        NavigationManager.NavigateTo($"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}" +
            $"/FILE/DownloadFile?fromPath={filePath}&fileName={fileName}&downloadFileName={nomFichier}");
    }

    #endregion

    #region Blazor life cycle

    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Read the resources of the original demand if it exists (case of a recurring demand)
        // otherwise read resources of the selected demand.
        string queryOptions = $"?$filter=TD_DEMANDEID eq {(DemandeOrigineId.HasValue ? DemandeOrigineId : DemandeId)}"
                            + $"&$orderby=TER_NOM_FICHIER, TRD_NOM_FICHIER_ORIGINAL";
        Resources = await ProxyCore.GetEnumerableAsync<VDE_DEMANDES_RESSOURCES>(queryOptions);

        _isResourcesDialogDisplayed = true;
    }

    #endregion
}
