using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.Common.Components.FilesAndBatchsManager;

public partial class FilesAndBatchsManagerComponent
{
    [Parameter] public TEM_ETAT_MASTERS Job { get; set; }
    [Parameter] public TE_ETATS JobVersion { get; set; }

    [Parameter] public int EtatId { get; set; } // 5

    [Parameter] public int ScenarioId { get; set; } // 13

    /// <summary>
    /// Set this flag to true if you want to show only parameters but disabling save buttons
    /// </summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// Event launched when files or batch associations changed.
    /// </summary>
    [Parameter] public EventCallback OnAssociationChange { get; set; }

    #region PARAMETERS
    private bool DisableSaveBatchButton { get; set; }
    private bool DisableSaveFichiersButton { get; set; }
    private bool IsSaving { get; set; }

    private const string IconCssRemove = "e-icons e-list-link-remove";
    private List<Fichiers> groupFichiersA, groupFichiersB, groupFichiersC;
    private SfListBox<int[], Fichiers> ListBoxFichiersA, ListBoxFichiersB, ListBoxFichiersC;

    private List<Batchs> groupBatchsA, groupBatchsB;
    private SfListBox<int[], Batchs> ListBoxBatchsA, ListBoxBatchsB;

    private readonly Dictionary<string, object> AttributesDisabled = new() { { "style", "background-color: darkgrey; font-weight: bold; color: red;" } };

    #endregion

    #region MOCK DATA

    private IEnumerable<TER_ETAT_RESSOURCES> GetFichiersScenarios = Enumerable.Empty<TER_ETAT_RESSOURCES>();

    private IEnumerable<Fichiers> GetFichiers { get; set; }

    private IEnumerable<TEB_ETAT_BATCHS> GetBatchsScenarios = Enumerable.Empty<TEB_ETAT_BATCHS>();

    private IEnumerable<Batchs> GetBatchs { get; set; } = Enumerable.Empty<Batchs>();

    #endregion

    #region FICHIERS

    // OnParametersSetAsync requis pour que les parametres soient appliqués lors de l'affichage et pas au chargement du composant qui se fait avec id=0
    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();

        // Raise this event to reveal inactive batchs
        DisableInactiveBatchs();
    }

    /// <summary>
    /// Startup
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (ScenarioId > 0) // avoid unnecessary query
        {
            GetFichiers = await GetFichiersAsync();
            GetBatchs = await GetBatchsScenarioAsync();

            InitializeFichiers();
            InitializeBatchs();

            // Disable save buttons based on ReadOnly parameter
            DisableSaveBatchButton = ReadOnly;
            DisableSaveFichiersButton = ReadOnly;
        }
    }

    private async Task<IList<Fichiers>> GetFichiersAsync()
    {
        IList<Fichiers> data = new List<Fichiers>();

        GetFichiersScenarios = await ProxyCore
            .GetEnumerableAsync<TER_ETAT_RESSOURCES>($"?$expand=TRS_RESSOURCE_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId})&$filter=TE_ETATID eq {EtatId}", useCache: false)
            ;

        foreach (var scenario in GetFichiersScenarios)
        {
            data.Add(new Fichiers
            {
                TER_ETAT_RESSOURCEID = scenario.TER_ETAT_RESSOURCEID,
                TER_NOM_FICHIER = scenario.TER_NOM_FICHIER,
                TRS_FICHIER_OBLIGATOIRE = scenario.TRS_RESSOURCE_SCENARIOS?.FirstOrDefault()?.TRS_FICHIER_OBLIGATOIRE
            });
        }

        return data;
    }

    private async Task<IList<Batchs>> GetBatchsScenarioAsync()
    {
        GetBatchsScenarios = await ProxyCore
            .GetEnumerableAsync<TEB_ETAT_BATCHS>($"?$expand=TBS_BATCH_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId})&$filter=TE_ETATID eq {EtatId}", useCache: false)
            ;

        IList<Batchs> data = new List<Batchs>();

        foreach (var batch in GetBatchsScenarios)
        {
            var id = batch.TBS_BATCH_SCENARIOS.Any() && batch.TBS_BATCH_SCENARIOS.FirstOrDefault()!.TS_SCENARIOID.Equals(ScenarioId);
            var order = batch.TBS_BATCH_SCENARIOS.FirstOrDefault()?.TBS_ORDRE_EXECUTION;

            data.Add(new Batchs
            {
                TEB_ETAT_BATCHID = batch.TEB_ETAT_BATCHID,
                TEB_CMD = batch.TEB_CMD,
                TRST_STATUTID = batch.TRST_STATUTID,
                ORDER = id ? order : null,
                IconCss = batch.TRST_STATUTID.Equals(StatusLiteral.Deactivated, StringComparison.OrdinalIgnoreCase) ? IconCssRemove : "e-icons e-list-link-checked"
            });
        }

        return data;
    }

    /// <summary>
    /// Init Fichiers
    /// </summary>
    private void InitializeFichiers()
    {
        groupFichiersA = new(); groupFichiersB = new(); groupFichiersC = new();

        foreach (var fichier in GetFichiers)
        {
            switch (fichier.TRS_FICHIER_OBLIGATOIRE)
            {
                case "":
                case null:
                    groupFichiersA.Add(fichier); // Source files (left side list)
                    break;

                case StatusLiteral.Yes:
                    groupFichiersB.Add(fichier); // Mandatory files (right side list)
                    break;

                case StatusLiteral.No:
                    groupFichiersC.Add(fichier); // Facultative files (right side list)
                    break;
            }
        }
    }

    private void FichiersCreated(object args) => FichiersDropped(null);

    private void FichiersDropped(DropEventArgs<Fichiers> args)
    {
        //DisableSaveFichiersButton = !ListBoxFichiersB.GetDataList().Any() && !ListBoxFichiersC.GetDataList().Any();
    }

    /// <summary>
    /// Save all Fichiers items: swap matrix, bulk update and finally notify
    /// </summary>
    private async Task OnSaveFichiers()
    {
        if (!IsSaving)
        {
            IsSaving = true;
            DisableSaveFichiersButton = true;

            // Prepare matrix
            IList<Fichiers> saveFichiers = new List<Fichiers>();
            {
                // Fichiers disponibles
                foreach (var el in ListBoxFichiersA.GetDataList())
                    saveFichiers.Add(new Fichiers { TER_ETAT_RESSOURCEID = el.TER_ETAT_RESSOURCEID, TER_NOM_FICHIER = el.TER_NOM_FICHIER, TRS_FICHIER_OBLIGATOIRE = null });

                // Fichiers obligatoires
                foreach (var el in ListBoxFichiersB.GetDataList())
                    saveFichiers.Add(new Fichiers { TER_ETAT_RESSOURCEID = el.TER_ETAT_RESSOURCEID, TER_NOM_FICHIER = el.TER_NOM_FICHIER, TRS_FICHIER_OBLIGATOIRE = StatusLiteral.Yes });

                // Fichiers facultatifs
                foreach (var el in ListBoxFichiersC.GetDataList())
                    saveFichiers.Add(new Fichiers { TER_ETAT_RESSOURCEID = el.TER_ETAT_RESSOURCEID, TER_NOM_FICHIER = el.TER_NOM_FICHIER, TRS_FICHIER_OBLIGATOIRE = StatusLiteral.No });

                // Update lists
                GetFichiers = saveFichiers;
            }

            int counter = 0;
            string message = string.Empty;

            foreach (var fichier in GetFichiers)
            {
                var resFicId = fichier.TER_ETAT_RESSOURCEID;
                var resource = GetFichiersScenarios.FirstOrDefault(f => f.TER_ETAT_RESSOURCEID == resFicId)?.TRS_RESSOURCE_SCENARIOS?.FirstOrDefault();

                // Has ressource?
                if (resFicId == (resource?.TER_ETAT_RESSOURCEID ?? 0))
                {
                    var same = resource!.TRS_FICHIER_OBLIGATOIRE == fichier.TRS_FICHIER_OBLIGATOIRE;
                    var remove = resource.TRS_FICHIER_OBLIGATOIRE != null && fichier.TRS_FICHIER_OBLIGATOIRE == null;
                    if (!same)
                    {
                        if (!remove)
                        {
                            // Update File resource
                            resource.TRS_FICHIER_OBLIGATOIRE = fichier.TRS_FICHIER_OBLIGATOIRE;
                            var updResource = await ProxyCore.UpdateAsync(new List<TRS_RESSOURCE_SCENARIOS> { resource });
                            if (updResource.Count > 0)
                                counter++;
                            // If the update failed.
                            if (updResource.Count.Equals(Litterals.NoDataRow))
                            {
                                await ProxyCore.SetLogException(new LogException(GetType(), resource, updResource.Message));

                                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                            }
                        }
                        else
                        {
                            // Delete File resource
                            var sql = $"DELETE FROM {nameof(TRS_RESSOURCE_SCENARIOS)} WHERE {nameof(TRS_RESSOURCE_SCENARIOS.TS_SCENARIOID)} = {ScenarioId} AND {nameof(TRS_RESSOURCE_SCENARIOS.TER_ETAT_RESSOURCEID)} = {fichier.TER_ETAT_RESSOURCEID} RETURNING *";
                            var deleted = (await ProxyCore.GetAllSqlRaw<TRS_RESSOURCE_SCENARIOS>(sql)).Any();
                            if (deleted)
                                counter++;

                            // Delete File resource
                            //var delResource = await ProxyCore.DeleteAsync<TRS_RESSOURCE_SCENARIOS>(new List<string> { fichier.TER_ETAT_RESSOURCEID.ToString() });
                            //var delResource = await ProxyCore.GetAllSqlRaw<TRS_RESSOURCE_SCENARIOS>(delete);

                            //if (delResource.Count > 0)
                            //    counter++;
                        }
                    }
                }
                // Has no File ressource?
                else if (resource == null && fichier.TRS_FICHIER_OBLIGATOIRE != null)
                {
                    var resourceScenario = new TRS_RESSOURCE_SCENARIOS
                    {
                        TER_ETAT_RESSOURCEID = fichier.TER_ETAT_RESSOURCEID,
                        TS_SCENARIOID = ScenarioId,
                        TRS_FICHIER_OBLIGATOIRE = fichier.TRS_FICHIER_OBLIGATOIRE,
                        TRS_COMMENTAIRE = fichier.TER_NOM_FICHIER
                    };
                    // Add resource
                    var addResource = await ProxyCore.InsertAsync(new List<TRS_RESSOURCE_SCENARIOS> { resourceScenario });
                    if (addResource.Count > 0)
                        counter++;
                    // If the insertion failed.
                    else
                    {
                        await ProxyCore.SetLogException(new LogException(GetType(), resourceScenario, addResource.Message));

                        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                    }
                }
            }

            // Need to reload the Files matrix?
            if (counter > 0)
            {
                GetFichiersScenarios = await ProxyCore
                    .GetEnumerableAsync<TER_ETAT_RESSOURCES>($"?$expand=TRS_RESSOURCE_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId})&$filter=TE_ETATID eq {EtatId}", useCache: false)
                    ;
            }

            // Notify
            switch (counter)
            {
                case 0:
                    await Toast.DisplayInfoAsync("Informations", $"{message}Matrice non modifiée");
                    break;

                case > 0:
                    // Invoke parent method.
                    await OnAssociationChange.InvokeAsync();
                    await Toast.DisplaySuccessAsync("Informations", $"{message}Matrice ressources/scénarios sauvegardée");
                    break;

                default:
                    await Toast.DisplayErrorAsync("Erreur", $"Erreur(s) : {message}");
                    break;
            }

            IsSaving = false;
            DisableSaveFichiersButton = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    #endregion

    #region BATCHS

    /// <summary>
    /// Init Batchs
    /// </summary>
    private void InitializeBatchs()
    {
        groupBatchsA = new(); groupBatchsB = new();

        foreach (var batch in GetBatchs.OrderBy(o => o.ORDER))
        {
            switch (batch.TRST_STATUTID)
            {
                case StatusLiteral.Available:
                    if (batch.ORDER is null)
                        groupBatchsA.Add(batch); // Source batchs (left side list)
                    else
                        groupBatchsB.Add(batch); // Destination batchs (right side list)
                    break;

                case StatusLiteral.Deactivated:
                    if (batch.ORDER is null)
                        groupBatchsA.Add(batch); // Source batchs (left side list)
                    else
                        groupBatchsB.Add(batch); // Destination batchs (right side list)
                    break;
            }
        }
    }

    private void DisableInactiveBatchs()
    {
        List<int> disabled = new();

        var batchs = ListBoxBatchsA.GetDataList();

        foreach (var batch in batchs)
        {
            if (batch.TRST_STATUTID == StatusLiteral.Deactivated)
                disabled.Add(batch.TEB_ETAT_BATCHID);
        }

        ListBoxBatchsA.EnableItems(disabled.ToArray(), false);

        //DisableSaveBatchButton = !ListBoxBatchsB.GetDataList().Any();
    }

    private void BatchsCreated(object args) => DisableInactiveBatchs();

    private void BatchsDropped(DropEventArgs<Batchs> args) => DisableInactiveBatchs();

    /// <summary>
    /// Save all Batchs items
    /// </summary>
    private async Task OnSaveBatchs()
    {
        // Alert
        if (ListBoxBatchsB.GetDataList().Any(b => b.TRST_STATUTID == StatusLiteral.Deactivated))
        {
            await Toast.DisplayWarningAsync("Unable to save batchs", "Drop inactive batches first!");
        }
        else
        {
            // If we don't have any Inactive Batch then we can save
            var saveBatchs = new List<Batchs>();
            int counter = 0;
            int order = 1;

            // batchs disponibles
            foreach (var el in ListBoxBatchsA.GetDataList())
                saveBatchs.Add(new Batchs { TEB_ETAT_BATCHID = el.TEB_ETAT_BATCHID, TEB_CMD = el.TEB_CMD, TRST_STATUTID = el.TRST_STATUTID, ORDER = null });

            // batchs ordonnés
            foreach (var el in ListBoxBatchsB.GetDataList())
                saveBatchs.Add(new Batchs { TEB_ETAT_BATCHID = el.TEB_ETAT_BATCHID, TEB_CMD = el.TEB_CMD, TRST_STATUTID = el.TRST_STATUTID, ORDER = order++ });

            //foreach (var el in saveBatchs) Console.WriteLine($"{el.TEB_ETAT_BATCHID}-{el.TEB_CMD}-{el.TRST_STATUTID ?? "null"}-{el.ORDER?.ToString() ?? "null"}");

            GetBatchs = saveBatchs;
            order = 1;

            foreach (var batch in GetBatchs)
            {
                var AddBatchScenario = new List<TBS_BATCH_SCENARIOS>(1);
                var UpdBatchScenario = new List<TBS_BATCH_SCENARIOS>(1);
                var resBatId = batch.TEB_ETAT_BATCHID;
                var resource = GetBatchsScenarios.FirstOrDefault(f => f.TEB_ETAT_BATCHID == resBatId)?.TBS_BATCH_SCENARIOS?.FirstOrDefault();

                // Has Batch ressource?
                if (resBatId == (resource?.TEB_ETAT_BATCHID ?? 0))
                {
                    var remove = batch.ORDER == null;

                    if (!remove)
                    {
                        // Update resource
                        resource!.TBS_ORDRE_EXECUTION = order;
                        UpdBatchScenario.Add(resource);
                        var updResource = await ProxyCore.UpdateAsync(UpdBatchScenario);
                        // If the update failed.
                        if (updResource.Count.Equals(Litterals.NoDataRow))
                        {
                            await ProxyCore.SetLogException(new LogException(GetType(), UpdBatchScenario, updResource.Message));
                            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                        }
                        else
                        {
                            await Toast.DisplayInfoAsync("UpdBatchScenario", updResource.Message);
                            order++;
                            counter++;
                        }
                    }
                    else
                    {
                        // Delete Batch resource
                        var sql = $"DELETE FROM {nameof(TBS_BATCH_SCENARIOS)} WHERE {nameof(TBS_BATCH_SCENARIOS.TS_SCENARIOID)} = {ScenarioId} AND {nameof(TBS_BATCH_SCENARIOS.TEB_ETAT_BATCHID)} = {batch.TEB_ETAT_BATCHID} RETURNING *";
                        var deleted = (await ProxyCore.GetAllSqlRaw<TBS_BATCH_SCENARIOS>(sql)).Any();
                        if (deleted)
                            counter++;

                        //// Delete Batch resource
                        //var delResource = await ProxyCore.DeleteAsync<TBS_BATCH_SCENARIOS>(new[] { batch.TEB_ETAT_BATCHID.ToString() });
                        //await Toast.DisplayInfoAsync("DelBatchScenario", delResource.Message);
                        //counter++;
                    }
                }
                // Has no Batch ressource?
                else if (resource == null && batch.ORDER != null)
                {
                    // Add Batch resource
                    AddBatchScenario.Add(new TBS_BATCH_SCENARIOS
                    {
                        TEB_ETAT_BATCHID = resBatId,
                        TS_SCENARIOID = ScenarioId,
                        TBS_ORDRE_EXECUTION = order
                    });

                    var addResource = await ProxyCore.InsertAsync(AddBatchScenario);
                    // If the insertion failed.
                    if (addResource.Count < 1)
                    {
                        await ProxyCore.SetLogException(new LogException(GetType(), AddBatchScenario, addResource.Message));

                        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                    }
                    else
                    {
                        await Toast.DisplayInfoAsync("AddBatchScenario", addResource.Message);
                        order++;
                        counter++;
                    }
                }
            }

            // Need to reload the Batchs matrix?
            if (counter > 0)
            {
                GetBatchsScenarios = await ProxyCore
                    .GetEnumerableAsync<TEB_ETAT_BATCHS>($"?$expand=TBS_BATCH_SCENARIOS($filter=TS_SCENARIOID eq {ScenarioId})&$filter=TE_ETATID eq {EtatId}", useCache: false)
                    ;
            }

            // Invoke parent method.
            await OnAssociationChange.InvokeAsync();
        }
    }

    #endregion

    #region STRUCTURES

    /// <summary>
    /// Fichiers structure
    /// </summary>
    protected class Fichiers
    {
        public int TER_ETAT_RESSOURCEID { get; set; }
        public string TER_NOM_FICHIER { get; set; }
        public string TRS_FICHIER_OBLIGATOIRE { get; set; }
    }

    /// <summary>
    /// Batchs structure
    /// </summary>
    protected class Batchs
    {
        public int TEB_ETAT_BATCHID { get; set; }
        public string TEB_CMD { get; set; }
        public string TRST_STATUTID { get; set; }
        public int? ORDER { get; set; }
        public string IconCss { get; set; } = "e-icons e-list-link-checked";
    }

    #endregion
}