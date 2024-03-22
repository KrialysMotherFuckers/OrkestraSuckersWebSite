using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using System.Text.RegularExpressions;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.OBJET; //Krialys.Orkestra.Web.Module.ETQ.ETQ.Pages

public partial class EtqModeleComponent
{
    // Parent instances
    private OrkaGenericGridComponent<TOBJE_OBJET_ETIQUETTES> Ref_TOBJE_OBJET_ETIQUETTES;
    private const int EtqStatusDefault = 1; // statut de la version  1 = Actif, 2 proto,  0 archive // Faudrait peut être faire un Enum si tu as plusieurs valeurs ??
    private string UserId => Session.GetUserId();  // id user courant
    private IEnumerable<TRU_USERS> TruUsersData { get; set; } = Enumerable.Empty<TRU_USERS>();
    private int _etqImpose;    // identifiant de la codification imposée (parametre optionnel fichier de conf) 
    private string _msgDelete, _msgDelete2; // pour suppression

    //  private bool IsEditOrAdd { get; set; } = false;
    private bool IsAdd { get; set; } = false;

    private TOBJE_OBJET_ETIQUETTES _selectedData = new();

    private bool DeleteDialogIsVisible { get; set; }

    private bool OkDeleteDisabled { get; set; } = false;

    private bool DeleteConfirm { get; set; }

    private SfDialog _infoDialog;

    private bool InfoDialogIsVisible { get; set; }

    private string CheckedValueStatut { get; set; } = "Unchanged";  // choix du nouveau statut (inchangé ou pas)

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var etq = Config["Metier:EtqImpose"];

            if (!string.IsNullOrEmpty(etq))
                _etqImpose = int.Parse(etq);
        }
        catch
        {
            // not used yet
        }

        // recup liste globale des users
        TruUsersData = await ProxyCore.GetEnumerableAsync<TRU_USERS>();
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(TEntity entity) where TEntity : class, new()
    {
        if (entity is TOBJE_OBJET_ETIQUETTES tobje)
        {// l'enregistrement d'un nouvel objet  ne passe pas par le endedit donc les dataannotation ne sont pas exploitées 
            var etiquetteId = tobje.TOBJE_OBJET_ETIQUETTEID;
            if (tobje.TOBJE_CODE is not null && tobje.TOBJE_CODE_ETIQUETTAGE is not null) // minimum syndical , le control via les dataannotations ne se fait pas avant de rentrer dans le save
            {// cas ou on fait save sans avoir rien saisi

                bool match = Regex.IsMatch(tobje.TOBJE_CODE_ETIQUETTAGE, "^[a-zA-Z0-9]+$");
                if (match)
                {
                    // ajout nouvel enregistrement
                    if (etiquetteId == 0)
                    {   // sample :  await GetCodeVersionSuivante("LISTE_TRP");
                        var newVersion = await GetCodeVersionSuivante(tobje.TOBJE_CODE);

                        tobje.TOBJE_VERSION = newVersion;  // indice de version
                        tobje.TOBJE_VERSION_ETQ_STATUT = 2;  // 0: ancienne version, 1 : unique version active, 2 : prototype
                        tobje.TRU_ACTEURID = UserId;//   id user de la personne connectée 
                        tobje.TOBJE_DATE_CREATION = DateExtensions.GetUtcNow();
                    }

                    //------------------
                    if (etiquetteId != 0)  // ==>On est sur un EDIT d'un enregistrement qu'on modifie 
                    {
                        // analyse du changement de statut pour appliquer regles au besoin
                        switch (CheckedValueStatut)
                        {
                            case "Unchanged":
                                break;
                            case "0":
                                tobje.TOBJE_VERSION_ETQ_STATUT = 0;  //CheckedValueStatut
                                break;
                            case "1":
                                tobje.TOBJE_VERSION_ETQ_STATUT = 1;
                                break;
                            case "2":
                                tobje.TOBJE_VERSION_ETQ_STATUT = 2;
                                break;
                            default:
                                tobje.TOBJE_VERSION_ETQ_STATUT = 2;  // force en proto si valeur autre
                                break;
                        }
                    }

                    //------------------
                    // ajout ou modif 
                    //concerne uniquement la version active  // on n'autorise qu'une seule version "active" 
                    var isRecordable = true;
                    if (tobje.TOBJE_VERSION_ETQ_STATUT == EtqStatusDefault)
                    {
                        var sameVersion = await GetObjetVersionIdentique(tobje.TOBJE_OBJET_ETIQUETTEID, tobje.TOBJE_CODE);

                        if (sameVersion.HasValue)
                        {
                            isRecordable = false;
                            await _infoDialog.ShowAsync();
                        }
                    }
                    if (isRecordable)
                    {
                        if (etiquetteId == 0)
                        {
                            //  enregistrement en bdd via api pour récupérer l'id de l'enregistrement inséré
                            //  requis pour insertion dans table complémentaire
                            var apiResult = await ProxyCore.InsertAsync(new List<TOBJE_OBJET_ETIQUETTES> { tobje }, convertToLocalDateTime: true);

                            // If the insertion failed.
                            if (apiResult.Count < 1)
                            {
                                await ProxyCore.SetLogException(new LogException(GetType(), tobje, apiResult.Message));
                                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                            }
                            else
                            {
                                // bien utiliser CloseEditAsync et pas EndEdit car sauvegarde faite via InsertAsync ET ne pas le mettre à la fin !!
                                await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.CloseEditAsync();

                                // le count est l'ID de l'enregistrement retourné 
                                // et pas réeelleemnt un count du nb d'enregistrements en bdd
                                tobje.TOBJE_OBJET_ETIQUETTEID = (int)apiResult.Count;

                                // alimentation de table complémentaire si etape prealable reussie
                                await AlimTablesSatellite(tobje.TOBJE_OBJET_ETIQUETTEID);

                                // permet de forcer un "refresh de la grille"
                                ProxyCore.CacheRemoveEntities(typeof(TOBJE_OBJET_ETIQUETTES));
                                //await InvokeAsync(StateHasChanged);
                                await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.Refresh();
                            }
                        }
                        else  // simple mise a jour 
                        {
                            var t = tobje.TOBJE_OBJET_ETIQUETTEID;
                            await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.EndEditAsync();
                        }
                    }
                    else
                        await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.CloseEditAsync();
                }
                else
                {
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], "Format de CODE ETIQUETTAGE incorrect");
                }
            }
            // await InvokeAsync(StateHasChanged);
        }
    }

    #region Customisable datagrid events

    public string GetHeader(TOBJE_OBJET_ETIQUETTES value)
    {
        return value.TOBJE_OBJET_ETIQUETTEID == 0
            ? "Créer une nouvelle version d'un objet d'étiquette"
            : "Editer une version d'un objet d'étiquette : " + value.TOBJE_CODE;
    }

    private async Task ActionBeginHandler(ActionEventArgs<TOBJE_OBJET_ETIQUETTES> args)
    {
        args.PreventRender = true;

        if (args.RequestType is Action.BeginEdit or Action.Add)
        {
            // IsEditOrAdd = true;
            CheckedValueStatut = "Unchanged"; // reinitialise au cas ou on etait sur un autre enregistrement auparavant
        }
        else if (args.RequestType != Action.Refresh)
        {
            // IsEditOrAdd = false;
            IsAdd = false;
        }

        if (args.RequestType == Action.Delete && !DeleteConfirm)
        {
            args.Cancel = true;
            bool locked = false;

            // reinitialisation
            _msgDelete = string.Empty;
            _msgDelete2 = string.Empty;
            // rendre visible la dialogbox avant le GetButtonItems sinon considéré comme non initialisé lors de l exécution
            DeleteDialogIsVisible = true;

            //foreach (var btn in DeleteDialog.GetButtonItems())
            //{
            //    if (btn.Content == "OK")
            //        btn.Disabled = locked;
            //}

            int nbEtiquettes = await GetNbEtiquette(_selectedData.TOBJE_OBJET_ETIQUETTEID);
            if (nbEtiquettes > 0)
            {
                //Suppression non possible : {nbEtiquettes} étiquettes existantes";
                _msgDelete = string.Format(Trad.Keys["ETQ:TOBJE_OBJET_ETIQUETTESDelete1"], nbEtiquettes);
                locked = true;
            }

            if (_selectedData.TOBJE_VERSION_ETQ_STATUT == EtqStatusDefault)
            {
                if (string.IsNullOrEmpty(_msgDelete))
                    _msgDelete = Trad.Keys["ETQ:TOBJE_OBJET_ETIQUETTESDelete2"];
                else
                    _msgDelete2 = Trad.Keys["ETQ:TOBJE_OBJET_ETIQUETTESDelete2"];

                locked = true;
            }

            //if (locked) // desactivation bouton OK
            //{
            //    foreach (var btn in DeleteDialog.GetButtonItems())
            //    {
            //        if (btn.Content == "OK")
            //            btn.Disabled = locked;
            //    }
            //}
            OkDeleteDisabled = locked;

            //await DeleteDialog.Show();   //show external dialog 
        }
        // -----------------------ADD-------------
        else if (args.RequestType == Action.Add)
        {
            IsAdd = true;

            // valeur par defaut selon conditions 
            args.RowData.TOBJE_VERSION_ETQ_STATUT = 2;//proto  EtqStatusDefault;

            if (_etqImpose != 0) // on ne force la valeur QUE si <> 0 
                args.RowData.TEQC_ETQ_CODIFID = _etqImpose;
        }
        else if (args.RequestType != Action.Refresh) IsAdd = false;
        // }

        args.PreventRender = false;
    }

    private async Task OnActionFailureAsync(FailureEventArgs args)
    {
        DeleteConfirm = false;  // requis si echec!!

        await Ref_TOBJE_OBJET_ETIQUETTES.ActionFailureAsync(args);
    }

    private void OnActionComplete(ActionEventArgs<TOBJE_OBJET_ETIQUETTES> args)
    {
        args.PreventRender = true;

        if (args.RequestType.ToString() == "Delete" && DeleteConfirm)
        {
            DeleteConfirm = false;
        }

        if (args.RequestType == Action.Add)
        {
            foreach (var col in Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.Columns)
            {
                ////cache colonne en mode ajout
                //if (col.Field == nameof(TOBJE_OBJET_ETIQUETTES.TRU_ACTEURID) ||
                //    col.Field == nameof(TOBJE_OBJET_ETIQUETTES.TOBJE_DATE_CREATION))
                //{
                //    col.Visible = false;
                //}

                //positionnement sur valeur par defaut si existe dans la conf de l'installation
                if (_etqImpose != 0 && col.Field.Equals(nameof(TOBJE_OBJET_ETIQUETTES.TEQC_ETQ_CODIFID), StringComparison.OrdinalIgnoreCase))
                {
                    col.Visible = false;
                }
            }
        }
        //else if (args.RequestType == Syncfusion.Blazor.Grids.Action.BeginEdit)
        //{
        //    foreach (var col in Ref_TOBJE_OBJET_ETIQUETTES.Columns)
        //    {
        //        // invisibilité
        //        if (col.Field == nameof(TOBJE_OBJET_ETIQUETTES.TRU_ACTEURID) ||
        //            col.Field == nameof(TOBJE_OBJET_ETIQUETTES.TOBJE_DATE_CREATION))
        //            col.Visible = false;
        //    }
        //}

        args.PreventRender = false;
    }

    private void RowSelectHandler(RowSelectEventArgs<TOBJE_OBJET_ETIQUETTES> args) => _selectedData = args.Data;

    /// <summary>
    /// Final confirmation for Delete
    /// </summary>
    private async Task OkDeleteClickAsync()
    {
        DeleteConfirm = true;
        DeleteDialogIsVisible = false;    // hide Delete dialog

        // suppression dans tables filles 
        List<string> idsTOBJE_OBJET_ETIQUETTItemsToRemove = new();

        var filterObjetq = $"TOBJE_OBJET_ETIQUETTEID eq {_selectedData.TOBJE_OBJET_ETIQUETTEID}";
        var tobjrObjetReglesDelete = (await ProxyCore.GetEnumerableAsync<TOBJR_OBJET_REGLES>($"?$filter={filterObjetq}", useCache: false));
        if (tobjrObjetReglesDelete is not null)
        {
            idsTOBJE_OBJET_ETIQUETTItemsToRemove.AddRange(tobjrObjetReglesDelete.Select(record => record.TOBJR_OBJET_REGLEID.ToString()));

            await ProxyCore.DeleteAsync<TOBJR_OBJET_REGLES>(idsTOBJE_OBJET_ETIQUETTItemsToRemove.ToArray());
        }

        // suppression dans table principale
        await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.DeleteRecordAsync();   //delete the record while clicking OK button
        await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.EndEditAsync();
    }

    /// <summary>
    /// Final confirmation for Cancel Delete 
    /// </summary>
    private void CancelDeleteClick()
    {
        DeleteDialogIsVisible = false;
    }

    /// <summary>
    /// Final confirmation for Can't save
    /// </summary>
    private void OkInfoClick()
    {
        InfoDialogIsVisible = false;    //hide external dialog
    }

    private async Task OnCommandClickedAsync(CommandClickEventArgs<TOBJE_OBJET_ETIQUETTES> args)
    {
        _selectedData = args.RowData;

        if (args.CommandColumn.ID == "CommandDupliquer")
        {
            TOBJE_OBJET_ETIQUETTES TOBJE_OBJET_ETIQUETTE = new();

            var newVersion = await GetCodeVersionSuivante(_selectedData.TOBJE_CODE);
            /* reprise des données de l objet de référence */
            TOBJE_OBJET_ETIQUETTE.TDOM_DOMAINEID = _selectedData.TDOM_DOMAINEID;
            TOBJE_OBJET_ETIQUETTE.TEQC_ETQ_CODIFID = _selectedData.TEQC_ETQ_CODIFID;
            TOBJE_OBJET_ETIQUETTE.TOBF_OBJ_FORMATID = _selectedData.TOBF_OBJ_FORMATID;
            TOBJE_OBJET_ETIQUETTE.TOBJE_CODE = _selectedData.TOBJE_CODE;
            TOBJE_OBJET_ETIQUETTE.TOBJE_CODE_ETIQUETTAGE = _selectedData.TOBJE_CODE_ETIQUETTAGE;
            TOBJE_OBJET_ETIQUETTE.TOBJE_DESC = _selectedData.TOBJE_DESC;
            TOBJE_OBJET_ETIQUETTE.TOBJE_LIB = _selectedData.TOBJE_LIB;
            TOBJE_OBJET_ETIQUETTE.TOBJE_VERSION_ETQ_DESC = _selectedData.TOBJE_VERSION_ETQ_DESC;
            TOBJE_OBJET_ETIQUETTE.TOBN_OBJ_NATUREID = _selectedData.TOBN_OBJ_NATUREID;

            /* nouvelle valeurs */
            TOBJE_OBJET_ETIQUETTE.TOBJE_VERSION = newVersion;  // indice de version
            TOBJE_OBJET_ETIQUETTE.TOBJE_VERSION_ETQ_STATUT = 2;  // 0: ancienne version, 1 : unique version active, 2 : prototype
            TOBJE_OBJET_ETIQUETTE.TRU_ACTEURID = UserId;//   id user de la personne connectée 
            TOBJE_OBJET_ETIQUETTE.TOBJE_DATE_CREATION = DateExtensions.GetUtcNow();

            //  requis pour insertion dans table complémentaire
            var apiResult = await ProxyCore.InsertAsync(new List<TOBJE_OBJET_ETIQUETTES> { TOBJE_OBJET_ETIQUETTE }, convertToLocalDateTime: true);

            // If the insertion failed.                
            if (apiResult.Count < 1)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), TOBJE_OBJET_ETIQUETTE, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            }

            TOBJE_OBJET_ETIQUETTE.TOBJE_OBJET_ETIQUETTEID = (int)apiResult.Count;

            // le count est l'ID de l'enregistrement retourné 
            // et pas réeelleemnt un count du nb d'enregistrements en bdd
            if (TOBJE_OBJET_ETIQUETTE.TOBJE_OBJET_ETIQUETTEID > 0)
            {   // alimentation de table complémentaire si etape prealable reussie
                await AlimTablesSatellite(TOBJE_OBJET_ETIQUETTE.TOBJE_OBJET_ETIQUETTEID);

                /* copie des valeurs de regles pour les regles communes */
                await RepriseValeursRegles(_selectedData.TOBJE_OBJET_ETIQUETTEID, TOBJE_OBJET_ETIQUETTE.TOBJE_OBJET_ETIQUETTEID);

                // permet de forcer un "refresh de la grille"
                ProxyCore.CacheRemoveEntities(typeof(TOBJE_OBJET_ETIQUETTES));
                await InvokeAsync(StateHasChanged);
                await Ref_TOBJE_OBJET_ETIQUETTES.DataGrid.Refresh();
            }
            else
            {
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], "Echec de duplication");
            }

            // DUPLICATE(SelectedData);
            // duplique objet + regles 
        }
    }
    #endregion

    #region autres methodes

    /// <summary>
    /// Retrive next version for a specific Code d'objet d'etiquette 
    /// </summary>
    /// <param name="code">Code</param>
    private async Task<int> GetCodeVersionSuivante(string code)
    {
        var filter = $"TOBJE_CODE eq '{code}'";
        var sort = "TOBJE_VERSION desc";
        var tobjeObjetEtiquette = (await ProxyCore.GetEnumerableAsync<TOBJE_OBJET_ETIQUETTES>($"?$filter={filter}&$orderby={sort}"))?.FirstOrDefault();

        return tobjeObjetEtiquette?.TOBJE_VERSION != null ? (int)(tobjeObjetEtiquette.TOBJE_VERSION + 1) : 1;
    }

    /// <summary>
    /// Retrive actual last version for a specific Code d'objet d'etiquette 
    /// </summary>
    /// <param name="code">Code</param>
    ///  <returns>Derniere version sinon 0</returns>
    private async Task<int> GetLastCodeVersion(string code)
    {
        var filter = $"TOBJE_CODE eq '{code}'";
        var sort = "TOBJE_VERSION desc";
        var tobjeObjetEtiquette = (await ProxyCore.GetEnumerableAsync<TOBJE_OBJET_ETIQUETTES>($"?$filter={filter}&$orderby={sort}"))?.FirstOrDefault();

        return (tobjeObjetEtiquette is not null && tobjeObjetEtiquette.TOBJE_VERSION.HasValue) ? (int)(tobjeObjetEtiquette.TOBJE_VERSION) : 0;
    }

    /// <summary>
    /// Check if child exist for specific OBJET_ETIQUETTEID 
    /// Used for check with Delete compliance
    /// </summary>
    /// <param name="TOBJE_OBJET_ETIQUETTEID">ID OBJET ETIQUETTE</param>
    private async Task<int> GetNbEtiquette(int TOBJE_OBJET_ETIQUETTEID)
    {
        var filter = $"TOBJE_OBJET_ETIQUETTEID eq {TOBJE_OBJET_ETIQUETTEID}";
        var TETQ_ETIQUETTES = await ProxyCore.GetEnumerableAsync<TETQ_ETIQUETTES>($"?$filter={filter}");

        return TETQ_ETIQUETTES.Count();
    }

    /// <summary>
    /// Check if exist same signature for a Code  
    /// Used for check with Insert / Update compliance
    /// </summary>
    /// <param name="TOBJE_OBJET_ETIQUETTEID">ID OBJET ETIQUETTE</param>
    /// <param name="TOBJE_CODE">CODE OBJET ETIQUETTE</param>
    private async Task<int?> GetObjetVersionIdentique(int TOBJE_OBJET_ETIQUETTEID, string TOBJE_CODE)
    {
        // un unique exemplaire code + TOBJE_VERSION_ETQ_STATUT=1 ne peut exister 
        var filter = $"TOBJE_OBJET_ETIQUETTEID ne {TOBJE_OBJET_ETIQUETTEID} and TOBJE_CODE eq '{TOBJE_CODE}' and TOBJE_VERSION_ETQ_STATUT eq {EtqStatusDefault}";
        var tobjeObjetEtiquettes = await ProxyCore.GetEnumerableAsync<TOBJE_OBJET_ETIQUETTES>($"?$top=1&$filter={filter}");
        var tobjeObjetEtiquetteses = tobjeObjetEtiquettes.ToList();

        return tobjeObjetEtiquetteses.Any() ? tobjeObjetEtiquetteses.FirstOrDefault()!.TOBJE_OBJET_ETIQUETTEID : null;
    }

    /// <summary>
    /// Retrieve ALL users from db_univers for local mapping
    /// </summary>
    private async Task GetAllUser() => TruUsersData ??= await ProxyCore.GetEnumerableAsync<TRU_USERS>();

    #endregion

    #region "regles d objet"

    /// <summary>
    ///  Fill child tables with default records
    /// </summary>
    /// <param name="tobjId"> ID de l'objet d'etiquette nouvellement créé</param>
    /// <returns></returns>
    private async Task AlimTablesSatellite(int tobjId)
    {
        try
        {
            // alimentation de TOBJR_OBJET_REGLES
            // http://localhost:8000/api/etq/v1/TRGL_REGLES?$expand=TRGLRV_REGLES_VALEURS($filter=TRGLRV_VALEUR_DEFAUT eq 'O' or TRGLRV_VALEUR_DEFAUT eq 'F' )

            // la creation est elle issue d'une action de duplication ou d une création ?
            // TODO si duplication on reprend les param pas de la version précédente mais de la version initiale selectionnée par l'utilisateur

            string queryOptions = "?$expand=TRGLRV_REGLES_VALEURS($filter=TRGLRV_VALEUR_DEFAUT eq 'O' or TRGLRV_VALEUR_DEFAUT eq 'F')";
            var regles = await ProxyCore.GetEnumerableAsync<TRGL_REGLES>(queryOptions, useCache: false);

            if (regles != null)
            {
                var listObjRegles = new List<TOBJR_OBJET_REGLES>();

                foreach (var regle in regles)
                {
                    // init toutes les regles pour ce nouvel objet d etiquette parmis le catalogue, avec valeur par defaut
                    var objRegles = new TOBJR_OBJET_REGLES()
                    {
                        TOBJE_OBJET_ETIQUETTEID = tobjId,
                        TRGL_REGLEID = regle.TRGL_REGLEID,
                        TOBJR_APPLICABLE = StatusLiteral.No,
                        TRGLRV_REGLES_VALEURID = regle.TRGLRV_REGLES_VALEURS.Any()
                            ? regle.TRGLRV_REGLES_VALEURS.FirstOrDefault().TRGLRV_REGLES_VALEURID
                            : throw new Exception($"TRGLRV_REGLES_VALEURS::TRGLRV_REGLES_VALEURID has no Id set on TRGL_REGLEID '{regle.TRGL_REGLEID}'")
                    };
                    //TOBJR_OBJET_REGLE.TOBJR_VALEUR = regle.TRGLRV_REGLES_VALEURS.FirstOrDefault()?.TRGLRV_VALEUR;
                    // TOBJR_OBJET_REGLE.TOBJR_ECHEANCE_DUREE = null; //par defaut est a laisser a null en bdd

                    listObjRegles.Add(objRegles);
                }

                var apiResult = await ProxyCore.InsertAsync(listObjRegles);

                // If the insertion failed.
                if (apiResult.Count < 1)
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), listObjRegles, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                }
            }
        }
        catch (Exception ex)
        {
            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], ex.Message);
        }
    }

    /// <summary>
    ///  Copy common rule values from original Objet etq
    /// </summary>
    /// <param name="tobjIdSrc"> ID de l'objet d'etiquette de référence</param>
    /// <param name="tobjIdDest">ID de l'objet d'etiquette nouvellement créé</param>
    /// <returns></returns>

    private async Task RepriseValeursRegles(int tobjIdSrc, int tobjIdDest)
    {
        // mise à jour  de TOBJR_OBJET_REGLES basée sur les enregistrements de l'objet qui aura servi a la duplication
        // toutes les regles peuvent ne pas etre présentes dans un sens ou un autre

        // string queryOptionsSrc = $"?$expand=TRGLRV_REGLES_VALEURS($filter=TRGLRV_VALEUR_DEFAUT eq 'O' or TRGLRV_VALEUR_DEFAUT eq 'F')";

        string queryOptionsSrc = $"?$filter=TOBJE_OBJET_ETIQUETTEID eq {tobjIdSrc}";
        var regleObjetSrc = await ProxyCore.GetEnumerableAsync<TOBJR_OBJET_REGLES>(queryOptionsSrc, useCache: false);

        string queryOptionsDest = $"?$filter=TOBJE_OBJET_ETIQUETTEID eq {tobjIdDest}";
        var regleObjetDest = await ProxyCore.GetEnumerableAsync<TOBJR_OBJET_REGLES>(queryOptionsDest, useCache: false);

        var tobjrObjetRegleses = regleObjetDest.ToList();
        foreach (var regleSrc in regleObjetSrc)
        {
            var item = tobjrObjetRegleses.FirstOrDefault(a => a.TRGL_REGLEID == regleSrc.TRGL_REGLEID);
            if (item is null)
                continue;

            item.TOBJR_APPLICABLE = regleSrc.TOBJR_APPLICABLE;
            item.TOBJR_ECHEANCE_DUREE = regleSrc.TOBJR_ECHEANCE_DUREE;
            item.TRGLRV_REGLES_VALEURID = regleSrc.TRGLRV_REGLES_VALEURID;
        }

        // TODO Seb : tu en fais quoi du resultat en cas d'erreur ? Adopter la proposition de François serait un bonus ?
        var apiResult = await ProxyCore.UpdateAsync(tobjrObjetRegleses);
        // If the update failed.
        if (apiResult.Count.Equals(Litterals.NoDataRow))
        {
            await ProxyCore.SetLogException(new LogException(GetType(), regleObjetDest, apiResult.Message));

            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
        }
    }

    #endregion "regles d objet"

    #region "changement statut objet"
    private void SfRadioButtonStatutOnChange(ChangeEventArgs args)
    {
        CheckedValueStatut = (string)args.Value;
    }
    #endregion
}