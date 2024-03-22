using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using MudBlazor;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd;

public partial class UtdVersions
{
    #region Parameters
    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// Refresh parent datagrid
    /// </summary>
    [Parameter] public EventCallback OnGridRefresh { get; set; }

    /// <summary>
    /// Id of selected EtatMaster in parent grid.
    /// </summary>
    [Parameter] public int EtatMasterId { get; set; }

    [Parameter] public string NomEtatMaster { get; set; }

    [Parameter] public string StatutEtatMaster { get; set; }

    [Parameter] public TEM_ETAT_MASTERS Job { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion Parameters

    private string UserId { get; set; }   // id user courant

    private OrkaGenericGridComponent<TE_ETATS> Ref_TE_ETATS;

    private TE_ETATS SelectedData { get; set; } = new();

    private bool InfoDialogIsVisible { get; set; }
    private string _errorMsg = "";

    private bool SfDialogEnvViergeUploadVisible { get; set; } = false;

    private bool SfDialogPlanificationVisible { get; set; } = false;

    private bool SfCheckBoxGenereCubeIsChecked { get; set; } = false;

    private SfDialog _confirmActiveDialog;

    private bool ConfirmActiveDialogIsVisible { get; set; }

    private bool ConfirmActive { get; set; }

    #region Catalog
    /// <summary>
    /// Header of the catalog.
    /// </summary>
    private string CatalogHeader { get; set; }

    /// <summary>
    /// Initialize and open catalog.
    /// </summary>
    /// <param name="data">Catalog data.</param>
    private void OpenCatalog(TE_ETATS data)
    {
        // Update catalog parameters.
        CatalogHeader = $"{Trad.Keys["DTM:UTDCatalogHeader"]} {NomEtatMaster} {data.TE_VERSION}";
        _catalogEtatId = data.TE_ETATID;
        _catalogEtatStatusId = data.TRST_STATUTID;

        // Display catalog.
        _isCatalogDisplayed = true;
    }
    #endregion

    /// <summary>
    /// Give a unique id to each grid based on parent grid.
    /// </summary>
    private string GetSuffixId => EtatMasterId.ToString();

    protected override async Task OnInitializedAsync()
    {
        // Disable cache for this page
        ProxyCore.GetOrSetDisablingCacheStatus(true);

        var tEtatLogiciels = ProxyCore.GetEnumerableAsync<TEL_ETAT_LOGICIELS>().AsTask();
        var tLogiciels = ProxyCore.GetEnumerableAsync<TL_LOGICIELS>().AsTask();

        await Task.WhenAll(tEtatLogiciels, tLogiciels);

        _etatLogiciels = tEtatLogiciels.Result.ToList();
        _logiciels = tLogiciels.Result.ToList();

        UserId = Session.GetUserId();
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        if (entity is TE_ETATS teEtat)
        {
            // If no TL_LOGICIELS are selected, display tooltip and don't save.
            if (!SelectedLogiciels.Any())
            {
                await _logicielsTooltipObj.OpenAsync();

                return;
            }

            await _logicielsTooltipObj.CloseAsync();

            int EtatId = teEtat.TE_ETATID;

            teEtat.TE_GENERE_CUBE = SfCheckBoxGenereCubeIsChecked ? StatusLiteral.Yes : StatusLiteral.No;

            // verif saisie infos lié au cube
            if (SfCheckBoxGenereCubeIsChecked)
            {
                if (string.IsNullOrEmpty(teEtat.TE_NOM_DATABASE) || string.IsNullOrEmpty(teEtat.TE_NOM_SERVEUR_CUBE))
                {
                    _errorMsg = Trad.Keys["DTM:ErrMsgMissingDbOrServer"];
                    InfoDialogIsVisible = true;

                    return;
                }
            }
            else if (!string.IsNullOrEmpty(teEtat.TE_NOM_DATABASE) || !string.IsNullOrEmpty(teEtat.TE_NOM_SERVEUR_CUBE))
            {
                _errorMsg = Trad.Keys["DTM:ErrMsgEraseDbAndServer"];
                InfoDialogIsVisible = true;

                return;
            }

            // ajout nouvel enregistrement
            if (EtatId == 0)  // ==> le if permet de savoir si on est sur un nouvel enregistrement ou sur un enregistrement qu'on modifie 
            {
                // Initialize new item.
                teEtat.TE_NOM_ETAT = NomEtatMaster;
                teEtat.TEM_ETAT_MASTERID = EtatMasterId;
                teEtat.TRU_DECLARANTID = UserId;//   id user de la personne connectée 

                teEtat.TE_TYPE_SORTIE = "x";   // TODO temporaire

                teEtat.TE_DUREE_DERNIERE_PRODUCTION = 0;
                teEtat.TE_DUREE_PRODUCTION_ESTIMEE = 0;

                // Reset junction table with TL_LOGICIELS.
                teEtat.TEL_ETAT_LOGICIELS.Clear();

                // Add junction table with TL_LOGICIELS.
                // Browse through the selected TL_LOGICIELS.
                foreach (var logiciel in SelectedLogiciels)
                {
                    // Add this item to the list of elements to create.
                    teEtat.TEL_ETAT_LOGICIELS.Add(new()
                    {
                        TL_LOGICIELID = logiciel.TL_LOGICIELID
                    });
                }
            }

            int? VersionSuperieureExistante = await GetTE_ETATDerniereVersion(EtatMasterId, EtatId, teEtat.TE_INDICE_REVISION_L1, teEtat.TE_INDICE_REVISION_L2, teEtat.TE_INDICE_REVISION_L3);

            if (EtatId != 0)  // ==>On est sur un EDIT d'un enregistrement qu'on modifie 
            {
                switch (teEtat.TRST_STATUTID)
                {
                    case StatusLiteral.Deactivated:

                        var _requestList = await ProxyCore.GetEnumerableAsync<TD_DEMANDES>($"?$filter=TE_ETATID eq {EtatId} and TRST_STATUTID eq '{StatusLiteral.CreatedRequestAndWaitForExecution}'");
                        if (_requestList.Any())
                        {
                            var _planningList = new List<TPF_PLANIFS>();

                            _requestList?.ToList().ForEach(async x =>
                            {
                                x.TRST_STATUTID = OrderStatus.Canceled;

                                _planningList.AddRange(await ProxyCore.GetEnumerableAsync<TPF_PLANIFS>($"?$filter=TPF_DEMANDE_ORIGINEID eq {x.TD_DEMANDE_ORIGINEID} and TRST_STATUTID eq '{StatusLiteral.Available}'"));
                                if (_planningList.Any())
                                    _planningList?.ToList().ForEach(x => x.TRST_STATUTID = StatusLiteral.Canceled);
                            });

                            try
                            {
                                // One shot
                                await ProxyCore.UpdateAsync(_planningList, convertToLocalDateTime: false);

                                await ProxyCore.UpdateAsync(_requestList, convertToLocalDateTime: false);
                            }
                            catch (Exception ex)
                            {
                                await ProxyCore.SetLogException(new LogException(GetType(), _planningList, ex.Message));
                            }
                        }
                        break;

                    case StatusLiteral.Available:
                        // verification non existance version + recente

                        if (VersionSuperieureExistante.HasValue)
                        {
                            _errorMsg = Trad.Keys["DTM:ErrMsgVersion"];
                            InfoDialogIsVisible = true;

                            return;
                        }

                        // verif au moins un scenario actif
                        bool VerifScenarioActif = await GetTE_ETATScenarioStatutCompliance(EtatId);
                        if (!VerifScenarioActif)
                        {
                            // msg Aucun scenario actif associé a cette version
                            _errorMsg = Trad.Keys["DTM:ErrMsgNoActiveModule"];
                            InfoDialogIsVisible = true;

                            return;
                        }

                        //verif si autre version active
                        bool PresenceAutreVersionActive = await GetTE_ETATExisteActif(EtatMasterId, EtatId);
                        if (PresenceAutreVersionActive)
                        {
                            // necessite confirmation 
                            ConfirmActiveDialogIsVisible = true;
                            return;
                        }
                        break;

                    case StatusLiteral.Prototype:
                        // verification non existance version + recente
                        if (VersionSuperieureExistante.HasValue)
                        {
                            _errorMsg = Trad.Keys["DTM:ErrMsgVersion"];
                            InfoDialogIsVisible = true;
                            return;
                        }

                        // verif au moins un scenario actif
                        bool VerifScenarioActifBis = await GetTE_ETATScenarioStatutCompliance(EtatId);
                        if (!VerifScenarioActifBis)
                        {
                            // msg Aucun scenario actif associé a cette version
                            _errorMsg = Trad.Keys["DTM:ErrMsgNoActiveModule"];
                            InfoDialogIsVisible = true;
                            return;
                        }

                        break;

                    case StatusLiteral.Draft:
                        // nettoyage en base des demandes/planifs eventuels / batchs / ressources / prerequis et ressources contributrices
                        // suppression des fichiers de ressources des productions eventuelles
                        await CleanEtatPrototypeToBrouillon(EtatId);
                        break;
                }
            }

            teEtat.TE_DATE_REVISION = DateExtensions.GetLocaleNow();

            ProxyCore.CacheRemoveEntities(typeof(TE_ETATS), typeof(TEL_ETAT_LOGICIELS), typeof(TS_SCENARIOS));

            await Ref_TE_ETATS.DataGrid.EndEditAsync();

            await OnGridRefresh.InvokeAsync();
        }
        else
        {
            await Ref_TE_ETATS.DataGrid.CloseEditAsync();
        }
    }

    private void OnCommandClicked(CommandClickEventArgs<TE_ETATS> args)
    { //args.CommandColumn.ButtonOption.Content
        SelectedData = args.RowData; // pour utiliser dans les SFDialog and Co

        if (args.CommandColumn.ID == "CommandCatalogue")
        {
            OpenCatalog(args.RowData);
        }
        //else if (args.CommandColumn.ID == "CommandEnvViergeUpload")
        //{
        //    SfDialogEnvViergeUploadVisible = true;

        //}
        else if (args.CommandColumn.ID == "CommandEnvViergeDownload")
        {
            SfDialogEnvViergeUploadVisible = true;

            string etatId = 'E' + args.RowData.TE_ETATID.ToString().PadLeft(6, '0');
            NavigationManager.NavigateTo($"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/FILE/DownloadEnvironmentZip?envId={etatId}");

        }
        else if (args.CommandColumn.ID == "CommandPlanification")
        {
            SfDialogPlanificationVisible = true;
        }
        else if (args.CommandColumn.ID == "DuplicateVersionCommand")
        {
            DataProcessingUnitDuplicate(args.RowData);
        }
    }

    /// <summary>
    /// Uids of columns to show when entering edit
    /// (and hide when exiting edit).
    /// </summary>
    private readonly string[] EditableColumnsUids = {
        "GenereCubeColumn"
    };

    /// <summary>
    /// Fields of columns to show when entering edit
    /// (and hide when exiting edit).
    /// </summary>
    private readonly string[] _editableColumnsHeaderText = new string[1];

    /// <summary>
    /// Uids of columns to hide when entering edit
    /// (and show when exiting edit).
    /// </summary>
    private readonly string[] _notEditableColumnsUids = {
        "GestionEnvViergeColumn",
        nameof(TE_ETATS.TE_DATE_REVISION),
        nameof(TE_ETATS.TE_DATE_DERNIERE_PRODUCTION)
    };

    /// <summary>
    /// Fields of columns to hide when entering edit
    /// (and show when exiting edit).
    /// </summary>
    private readonly string[] _notEditableColumnsHeaderText = new string[3];

    /// <summary>
    /// Convert list of columns Uid to a list of columns HeaderText.
    /// Because DataGrid.ShowColumnsAsync & DataGrid.HideColumnsAsync need HeaderText 
    /// and HeaderText depends on selected culture.
    /// </summary>
    private async Task ConvertUidToHeaderText()
    {
        for (int i = 0; i < EditableColumnsUids.Length; i++)
        {
            var col = await Ref_TE_ETATS.DataGrid.GetColumnByUidAsync(EditableColumnsUids[i]);
            if (col is not null)
            {
                _editableColumnsHeaderText[i] = col.HeaderText;
            }
        }

        for (int i = 0; i < _notEditableColumnsUids.Length; i++)
        {
            var col = await Ref_TE_ETATS.DataGrid.GetColumnByUidAsync(_notEditableColumnsUids[i]);
            if (col is not null)
            {
                _notEditableColumnsHeaderText[i] = col.HeaderText;
            }
        }
    }

    private async Task OnActionBeginAsync(ActionEventArgs<TE_ETATS> args)
    {
        if (Action.BeginEdit.Equals(args.RequestType) ||
            Action.Add.Equals(args.RequestType))
        {
            // If the list of fields to show/hide are not initialized yet.
            if (_editableColumnsHeaderText[0] is null)
                await ConvertUidToHeaderText();

            // Hide columns that can't be edited.
            await Ref_TE_ETATS.DataGrid.HideColumnsAsync(_notEditableColumnsHeaderText);

            // Show columns that are editable only.
            await Ref_TE_ETATS.DataGrid.ShowColumnsAsync(_editableColumnsHeaderText);

        }

        if (Action.BeginEdit.Equals(args.RequestType))
        {
            SfCheckBoxGenereCubeIsChecked = StatusLiteral.Yes.Equals(args.Data.TE_GENERE_CUBE);
            args.Data.TRST_STATUTID_OLD = args.Data.TRST_STATUTID;

        }
        else if (Action.Add.Equals(args.RequestType))
        {
            //on prépositionne la valeur minimale que l utilisateur pourra changer
            args.Data.TE_NOM_ETAT = NomEtatMaster;
            args.Data.TRST_STATUTID = StatusLiteral.Draft;
            args.Data.TE_INDICE_REVISION_L1 = 1;
            args.Data.TE_INDICE_REVISION_L2 = 0;
            args.Data.TE_INDICE_REVISION_L3 = 0;

            args.Data.TE_GENERE_CUBE = StatusLiteral.No;
            SfCheckBoxGenereCubeIsChecked = false;
        }
    }

    /// <summary>
    /// Event triggers when DataGrid actions are completed.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    private async Task OnActionCompleteAsync(ActionEventArgs<TE_ETATS> args)
    {
        if (Action.Save.Equals(args.RequestType))
        {
            // Update selected TL_LOGICIELS.
            await UpdateLogicielsAsync(args.Data);
        }

        if (Action.Save.Equals(args.RequestType) ||
            Action.Cancel.Equals(args.RequestType))
        {
            // Hide columns that are editable only.
            await Ref_TE_ETATS.DataGrid.HideColumnsAsync(_editableColumnsHeaderText);
            // Show columns that were hidden.
            await Ref_TE_ETATS.DataGrid.ShowColumnsAsync(_notEditableColumnsHeaderText);
        }
    }

    private string GetHeader(TE_ETATS value)
    {
        return value.TE_ETATID == 0
            ? string.Format(Trad.Keys["DTM:CreateVersion"], value.TE_NOM_ETAT)
            : string.Format(Trad.Keys["DTM:EditVersion"], value.TE_NOM_ETAT);
    }

    #region OnRowSelected

    /// <summary>
    /// Custom event when a row is selected we can add some controls onto any cells
    /// </summary>
    /// <param name="args"></param>
    private void OnRowSelected(RowSelectEventArgs<TE_ETATS> args) => SelectedData = args.Data;

    /// <summary>
    /// </summary>
    /// <param name="args"></param>
    private void OnRowDataBound(RowDataBoundEventArgs<TE_ETATS> args)
    {
        if (args.Data.TE_ENV_VIERGE_UPLOADED == StatusLiteral.No)
            args.Row.AddClass(new[] { "e-removecommanddownload" });

        if (StatutEtatMaster != StatusLiteral.Available || (StatutEtatMaster == StatusLiteral.Available && !new[] { StatusLiteral.Available, StatusLiteral.Deactivated }.Contains(args.Data.TRST_STATUTID)))
            args.Row.AddClass(new[] { "e-removecommandduplicate" });

        SfCheckBoxGenereCubeIsChecked = args.Data.TE_GENERE_CUBE == StatusLiteral.Yes;
    }
    #endregion

    private void SfCheckBoxGenereCubeOnChange(ChangeEventArgs args)
    {
        //requis pour changer l etat visuel de la case a cocher
        SfCheckBoxGenereCubeIsChecked = (bool)args.Value!;
    }

    /// <summary>
    /// Check if exist TE_ETAT with upper version   
    /// Used for check with Insert / Update compliance
    /// </summary>
    /// <param name="TEM_ETAT_MASTERID">ID ETATMASTER </param>
    /// <param name="TE_ETATID">ID ETAT</param>
    /// <param name="TE_INDICE_REVISION_L1">1st digit of version</param>
    /// <param name="TE_INDICE_REVISION_L2">2nd digit of version</param>
    /// <param name="TE_INDICE_REVISION_L3">3 digit of version</param>
    private async Task<int?> GetTE_ETATDerniereVersion(int TEM_ETAT_MASTERID, int TE_ETATID, int TE_INDICE_REVISION_L1, int TE_INDICE_REVISION_L2, int TE_INDICE_REVISION_L3)
    {
        // verif présence d'une version + récente
        var EtatVersion = TE_INDICE_REVISION_L1 * 10000 + TE_INDICE_REVISION_L2 * 100 + TE_INDICE_REVISION_L3;
        var filter = $"TE_ETATID ne {TE_ETATID} and TEM_ETAT_MASTERID eq {TEM_ETAT_MASTERID}";
        var teEtatSlocal = (await ProxyCore.GetEnumerableAsync<TE_ETATS>($"?$filter={filter}"))
                       .AsEnumerable()
                       .Where(x => x.TE_INDICE_REVISION_L1 * 10000 + x.TE_INDICE_REVISION_L2 * 100 + x.TE_INDICE_REVISION_L3 >= EtatVersion)
                       .OrderByDescending(x => x.TE_INDICE_REVISION_L1 * 10000 + x.TE_INDICE_REVISION_L2 * 100 + x.TE_INDICE_REVISION_L3).ToList();

        //idealement extraire la version la + haute
        return teEtatSlocal.Any() ? teEtatSlocal.FirstOrDefault()!.TE_ETATID : null;
    }

    /// <summary>
    /// Check if exist available scenario for TE_ETAT  
    /// Used for check compliance while switch TE_ETAT status to Actif or Prototype 
    /// </summary>
    /// <param name="TE_ETATID">ID ETAT</param>
    private async Task<bool> GetTE_ETATScenarioStatutCompliance(int TE_ETATID)
    {
        // verif présence d'au moins une version active de scenario 
        var filter = $"TE_ETATID eq {TE_ETATID} and TRST_STATUTID eq '{StatusLiteral.Available}'";
        var TS_SCENARIOS = await ProxyCore.GetEnumerableAsync<TS_SCENARIOS>($"?$filter={filter}", useCache: false);

        return TS_SCENARIOS.Any();
    }

    /// <summary>
    /// Check if exist TE_ETAT with status A 
    /// Used for check compliance while switch TE_ETAT status to Actif or Prototype 
    /// </summary>
    /// <param name="TEM_ETAT_MASTERID">ID ETATMASTER</param>
    /// <param name="TE_ETATID">ID ETAT</param>
    private async Task<bool> GetTE_ETATExisteActif(int TEM_ETAT_MASTERID, int TE_ETATID)
    {
        // verif présence d'au moins une version active de scenario 

        var filter = $"TE_ETATID ne {TE_ETATID} and TEM_ETAT_MASTERID eq {TEM_ETAT_MASTERID} and TRST_STATUTID eq '{StatusLiteral.Available}'";
        var _teEtats = await ProxyCore.GetEnumerableAsync<TE_ETATS>($"?$top=1&$filter={filter}", useCache: false);

        return _teEtats.Any();
    }

    /// <summary>
    /// Final confirmation for Can't save
    /// </summary>
    private void OkInfoClick()
    {
        InfoDialogIsVisible = false;    //hide external dialog
    }

    /// <summary>
    /// Final confirmation for Activation then Inactivation of old version
    /// </summary>
    private async Task OkConfirmActiveDialogClickAsync()
    {
        ConfirmActive = true;
        ConfirmActiveDialogIsVisible = false;    // hide confirm dialog

        // on met a jour le statut de l'etat actif au sein de cette UTD  (ETAT_MASTER)
        var filter = $"TEM_ETAT_MASTERID eq {EtatMasterId} and TRST_STATUTID eq '{StatusLiteral.Available}' ";
        var Te_etats_update = await ProxyCore.GetEnumerableAsync<TE_ETATS>($"?$filter={filter}", useCache: false);

        if (Te_etats_update is not null)
        {
            var teEtatsUpdate = Te_etats_update.ToList();
            foreach (var el in teEtatsUpdate)
            {
                el.TRST_STATUTID = StatusLiteral.Deactivated;
            }

            // update item with status Active.
            var apiResult = await ProxyCore.UpdateAsync(teEtatsUpdate);

            // If the update failed.
            if (apiResult.Count.Equals(Litterals.NoDataRow))
            {
                await ProxyCore.SetLogException(new LogException(GetType(), Te_etats_update, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTM:ErrMsgVersionDeactivation"]);
            }
        }
    }

    /// <summary>
    /// Clean old values of demandes when switch etat from Prototype to Brouillon 
    /// </summary>
    /// <param name="TE_ETATID">ID ETAT</param>
    private async Task CleanEtatPrototypeToBrouillon(int TE_ETATID)
    {
        var filterDemande = $"TE_ETATID eq {TE_ETATID}";
        var Td_demandes_update = await ProxyCore.GetEnumerableAsync<TD_DEMANDES>($"?$filter={filterDemande}", useCache: false);

        var tdDemandesUpdate = Td_demandes_update.ToList();
        foreach (var el in tdDemandesUpdate)
        {
            if (el.TRST_STATUTID != StatusLiteral.ScheduleModel) el.TRST_STATUTID = StatusLiteral.CanceledRequest;  // annulation des demandes produite avec le prototype

            // on efface le scenario qui pourrait etre caduc par rapport aux futurs changement sur le projet prototype mis en brouillon	
            el.TS_SCENARIOID = null;

            ///* Desactivation des planifications */ => SEB no reccurring planif for prototyped demandes
            //var filterPlanif = $"TPF_DEMANDE_ORIGINEID eq {el.TD_DEMANDEID} and TRST_STATUTID eq '{StatusLiteral.Available}' ";

            //var tpfPlanifsUpdate = await ProxyCore.GetEnumerableAsync<TPF_PLANIFS>($"?$filter={filterPlanif}", useCache: false);
            //if (tpfPlanifsUpdate is not null)
            //{
            //    var elTpfPlanifsUpdates = tpfPlanifsUpdate.ToList();
            //    foreach (var elTpfPlanifsUpdate in elTpfPlanifsUpdates)
            //    {
            //        elTpfPlanifsUpdate.TRST_STATUTID = StatusLiteral.Deactivated;
            //    }

            //    var apiResult = await ProxyCore.UpdateAsync(elTpfPlanifsUpdates);
            //    // If the update failed.
            //    if (apiResult.Count.Equals(Litterals.NoDataRow))
            //    {
            //        await ProxyCore.SetLogException(new LogException(GetType(), tpfPlanifsUpdate, apiResult.Message));
            //        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            //    }
            //}

            /* Suppression des Batchs */
            List<string> idsBatchDemandesItemsToRemove = new();
            var filterBatch = $"TD_DEMANDEID eq {el.TD_DEMANDEID}";
            var TBD_BATCH_DEMANDES_delete = await ProxyCore.GetEnumerableAsync<TBD_BATCH_DEMANDES>($"?$filter={filterBatch}", useCache: false);
            if (TBD_BATCH_DEMANDES_delete is not null)
            {
                idsBatchDemandesItemsToRemove.AddRange(TBD_BATCH_DEMANDES_delete.Select(batch => batch.TBD_BATCH_DEMANDEID.ToString()));

                await ProxyCore.DeleteAsync<TBD_BATCH_DEMANDES>(idsBatchDemandesItemsToRemove.ToArray());
            }

            /* Suppression des Prerequis */
            List<string> idsPrerequisDemandesItemsToRemove = new();

            var filterPrerequis = $"TD_DEMANDEID eq {el.TD_DEMANDEID}";
            var TPD_PREREQUIS_DEMANDES_delete = await ProxyCore.GetEnumerableAsync<TPD_PREREQUIS_DEMANDES>($"?$filter={filterPrerequis}", useCache: false);
            if (TPD_PREREQUIS_DEMANDES_delete is not null)
            {
                idsPrerequisDemandesItemsToRemove.AddRange(TPD_PREREQUIS_DEMANDES_delete.Select(prerequis => prerequis.TPD_PREREQUIS_DEMANDEID.ToString()));

                await ProxyCore.DeleteAsync<TPD_PREREQUIS_DEMANDES>(idsPrerequisDemandesItemsToRemove.ToArray());
            }

            /* Desactivation des Ressources */
            List<string> idsRESSOURCE_DEMANDESItemsToRemove = new();
            List<string> fileRESSOURCE_DEMANDESItemsToRemove = new();

            var filterRessource = $"TD_DEMANDEID eq {el.TD_DEMANDEID}";
            var TRD_RESSOURCE_DEMANDES_Delete = await ProxyCore.GetEnumerableAsync<TRD_RESSOURCE_DEMANDES>($"?$filter={filterRessource}", useCache: false);
            if (TRD_RESSOURCE_DEMANDES_Delete is not null)
            {
                foreach (var ressource in TRD_RESSOURCE_DEMANDES_Delete)
                {
                    idsRESSOURCE_DEMANDESItemsToRemove.Add(ressource.TRD_RESSOURCE_DEMANDEID.ToString());
                    fileRESSOURCE_DEMANDESItemsToRemove.Add(ressource.TRD_NOM_FICHIER);
                }

                //  suppression fichier physique de ressource devenus obsoletes
                // exemple  var resultFiles = await ProxyCore.DeleteFiles(@"D:\Zob", new[] { "toto.txt", "Tutu.txt" });

                // la liste peut etre vide si on est deja passé de Proto a brouillon puis proto a rebrouillon sans avoir relancé de prod entre temps
                if (fileRESSOURCE_DEMANDESItemsToRemove.Count > 0)
                {
                    string pathRessources = Convert.ToString(await ProxyCore.GetApiUniversAppSettings("ParallelU:PathRessource"));
                    pathRessources = Path.Combine(pathRessources ?? string.Empty, el.TD_DEMANDEID.ToString().PadLeft(6, '0'));
                    await ProxyCore.DeleteFiles(pathRessources, fileRESSOURCE_DEMANDESItemsToRemove.ToArray());
                    await ProxyCore.DeleteAsync<TRD_RESSOURCE_DEMANDES>(idsRESSOURCE_DEMANDESItemsToRemove.ToArray());
                }
            }
        }

        // enregistrement effectif sur TD_DEMANDES
        if (tdDemandesUpdate.Any())
        {
            var apiResult = await ProxyCore.UpdateAsync(tdDemandesUpdate);
            // If the update failed.
            if (apiResult.Count.Equals(Litterals.NoDataRow))
            {
                await ProxyCore.SetLogException(new LogException(GetType(), Td_demandes_update, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            }
        }

        // on remet en Brouillon tous les scenarios qui étaient Actif
        var filterScenario = $"TE_ETATID eq {TE_ETATID} and TRST_STATUTID eq '{StatusLiteral.Available}'";
        var Ts_scenario_update = await ProxyCore.GetEnumerableAsync<TS_SCENARIOS>($"?$filter={filterScenario}", useCache: false);

        var tsScenarioUpdate = Ts_scenario_update.ToList();
        foreach (var el in tsScenarioUpdate)
        {
            el.TRST_STATUTID = StatusLiteral.Draft;
        }

        if (tsScenarioUpdate.Any())
        {
            var apiResult = await ProxyCore.UpdateAsync(tsScenarioUpdate);
            // If the update failed.
            if (apiResult.Count.Equals(Litterals.NoDataRow))
            {
                await ProxyCore.SetLogException(new LogException(GetType(), Ts_scenario_update, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            }
        }
    }

    /// <summary>
    /// Final confirmation for Cancel Activation 
    /// </summary>
    private void CancelConfirmActiveDialogClick()
    {
        ConfirmActiveDialogIsVisible = false;
        ConfirmActive = false;
    }

    #region Environment column
    /// <summary>
    /// Event triggers when environment changes.
    /// </summary>
    private Task OnEnvironmentChange()
    {
        // Reload grid with new data.
        return Ref_TE_ETATS.DataGrid.Refresh();
    }
    #endregion

    #region TL_LOGICIELS column
    /// <summary>
    /// List of TEL_ETAT_LOGICIELS (read from DB).
    /// </summary>
    private IList<TEL_ETAT_LOGICIELS> _etatLogiciels = new List<TEL_ETAT_LOGICIELS>();

    /// <summary>
    /// List of TL_LOGICIELS (read from DB).
    /// </summary>
    private IList<TL_LOGICIELS> _logiciels = new List<TL_LOGICIELS>();

    /// <summary>
    /// List of selected TL_LOGICIELS (binded with a multiselect component).
    /// </summary>
    private IList<TL_LOGICIELS> SelectedLogiciels { get; set; } = new List<TL_LOGICIELS>();

    /// <summary>
    /// TL_LOGICIELS multiselect error tooltip.
    /// </summary>
    private SfTooltip _logicielsTooltipObj;

    /// <summary>
    /// Get names of the TL_LOGICIELS linked to a TE_ETATS field.
    /// </summary>
    /// <param name="etatId">TE_ETATS id.</param>
    /// <returns>TL_LOGICIELS descriptions.</returns>
    private string GetLogicielsNames(int etatId)
    {
        // List of TL_LOGICIELS names.
        IList<string> logicielsNames = new List<string>();

        // Filters TEL_ETAT_LOGICIELS with TE_ETATS id.
        var etatLogiciels = _etatLogiciels.Where(el => el.TE_ETATID == etatId);

        // Browse through the junction table.
        foreach (var etatLogiciel in etatLogiciels)
        {
            // Get names for each linked TL_LOGICIELS.
            string logicielsName = _logiciels?.FirstOrDefault(l => l.TL_LOGICIELID == etatLogiciel.TL_LOGICIELID)?.TL_NOM_LOGICIEL;
            // Add description to the list.
            if (!string.IsNullOrEmpty(logicielsName))
            {
                logicielsNames.Add(logicielsName);
            }
        }

        return string.Join(", ", logicielsNames);
    }

    /// <summary>
    /// Save selected TL_LOGICIELS by updating TEL_ETAT_LOGICIELS junction table.
    /// Update both the database and the local copy.
    /// </summary>
    /// <param name="etat">TE_ETATS item.</param>
    private async Task UpdateLogicielsAsync(TE_ETATS etat)
    {
        // Get edited item id.
        int etatId = etat.TE_ETATID;

        // If Id is default, the item was just created and TEL_ETAT_LOGICIELS link table
        // doesn't need to be updated.
        if (etatId != default)
        {
            // Get TEL_ETAT_LOGICIELS linked to this TE_ETATS.
            var selectedEtatLogiciels = _etatLogiciels.Where(el => el.TE_ETATID == etatId);

            // Update junction table TEL_ETAT_LOGICIELS depending on selected TL_LOGICIELS and 
            // the edited TE_ETATS.
            // ***** Part 1: Remove unselected items from the database. *****
            // List of the ids of the (junction table) items to remove.
            List<string> idsItemsToRemove = new();
            IList<TEL_ETAT_LOGICIELS> itemsToRemove = new List<TEL_ETAT_LOGICIELS>();
            // Browse through junction table.
            var telEtatLogicielses = selectedEtatLogiciels.ToList();

            foreach (var el in telEtatLogicielses.Where(el => SelectedLogiciels.All(sl => sl.TL_LOGICIELID != el.TL_LOGICIELID)))
            {
                // Add this item to the list of elements to remove.
                idsItemsToRemove.Add(el.TEL_ETAT_LOGICIELID.ToString());
                itemsToRemove.Add(el);
            }

            if (itemsToRemove.Any())
            {
                try
                {
                    // Delete these items from the junction table.
                    await ProxyCore.DeleteAsync<TEL_ETAT_LOGICIELS>(idsItemsToRemove.ToArray());
                }
                catch (Exception ex)
                {
                    // Log error
                    await ProxyCore.SetLogException(new LogException(GetType(), ex));

                    /* Display error in a toast. */
                    await Toast.DisplayErrorAsync(Trad.Keys["GridSource:GridError"], Trad.Keys["DTM:SoftwaresSaveError"])
                        ;
                }
            }

            // ***** Part 2: Create newly selected items in the database. *****
            // List  of the (junction table) items to create.
            IList<TEL_ETAT_LOGICIELS> itemsToCreate = new List<TEL_ETAT_LOGICIELS>();
            // Browse through the selected TL_LOGICIELS.
            foreach (var logiciel in SelectedLogiciels)
            {
                // Search for selected TL_LOGICIELS that are not in the junction table.
                if (telEtatLogicielses.Any(sel => sel.TL_LOGICIELID == logiciel.TL_LOGICIELID))
                    continue;

                TEL_ETAT_LOGICIELS newItem = new()
                {
                    TE_ETATID = etatId,
                    TL_LOGICIELID = logiciel.TL_LOGICIELID
                };

                // Add this item to the list of elements to create.
                itemsToCreate.Add(newItem);
            }
            if (itemsToCreate.Any())
            {
                // Add these items to the junction table.
                var apiResult = await ProxyCore.InsertAsync(itemsToCreate);
                // If the insertion failed.
                if (apiResult.Count < 1)
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), itemsToCreate, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTM:SoftwaresSaveError"]);
                }
            }

            // ***** Part 3: Update the local copy of the junction table. *****
            // Delete items.
            foreach (var item in itemsToRemove)
            {
                _etatLogiciels.Remove(item);
            }
            // Add items.
            foreach (var item in itemsToCreate)
            {
                _etatLogiciels.Add(item);
            }
        }
    }
    #endregion

    #region Catalog
    /// <summary>
    /// Is catalog displayed ?
    /// </summary>
    private bool _isCatalogDisplayed;

    /// <summary>
    /// TE_ETATID of the displayed catalog.
    /// </summary>
    private int _catalogEtatId;

    /// <summary>
    /// TE_ETATID of the displayed catalog.
    /// </summary>
    private string _catalogEtatStatusId;
    #endregion

    /// <summary>
    /// Duplicate DataProcessingUnit
    /// </summary>
    private async void DataProcessingUnitDuplicate(TE_ETATS dpuIdToDuplicate)
    {
        if (await DialogService.ConfirmAsync(string.Format(Trad.Keys["DTM:DuplicateUtdQuestion"], dpuIdToDuplicate.TE_VERSION), Trad.Keys["COMMON:Confirm"],
            new()
            {
                AnimationSettings = new()
                {
                    Effect = DialogEffect.SlideTop,
                },
                ShowCloseIcon = false,
                CloseOnEscape = false,
            }))
        {
            if (await ProxyCore.VersionDuplicate(dpuIdToDuplicate.TE_ETATID))
            {
                Snackbar.Add(string.Format(Trad.Keys["DTM:DuplicateUtdConfirmation"], dpuIdToDuplicate.TE_VERSION), Severity.Success);

                // Refresh datagrid (strict minimum action expected), but indirectly
                await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, Ref_TE_ETATS.GetRefreshGridButtonId, 1500);

                // Again... get links between TE_ETATS table and TL_LOGICIELS table from DB.
                _etatLogiciels = (await ProxyCore.GetEnumerableAsync<TEL_ETAT_LOGICIELS>()).ToList();
            }
            else
                Snackbar.Add(string.Format(Trad.Keys["DTM:DuplicateUtdError"], dpuIdToDuplicate.TE_VERSION), Severity.Error);
        }
    }

    private async Task OnEnvironmentClosedAsync()
    {
        // Refresh datagrid (strict minimum action expected), but indirectly
        await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, Ref_TE_ETATS.GetRefreshGridButtonId, 1500);
    }
}