using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.ADM.Components.DTFAdmin
{
    public partial class DataDrivenDTFComponent
    {
        #region Properties
        /// <summary>
        /// Specifies query to select data from DataSource.
        /// </summary>
        Query GridQuery;

        /// <summary>
        /// Sort on module/scenario.
        /// On trie  en 1er par le nom du scenario et ensuite par le nom du module, donc ne pas changer l'ordre du sort suivant.
        /// </summary>
        private readonly Query ScenarioQuery = new Query()
           .Sort(nameof(TS_SCENARIOS.TS_NOM_SCENARIO), "Ascending")
           .Sort($"{nameof(TS_SCENARIOS.TE_ETAT)}.{nameof(TE_ETATS.TE_FULLNAME)}", "Ascending");

        private string UserId { get; set; }   // id user courant

        private string CheckedValueStatut { get; set; } = "Unchanged";  // choix du nouveau statut (inchangé ou pas)

        public IEnumerable<TH_HABILITATIONS.ModeDroit> ListeProfil;
        #endregion

        #region Blazor life cycle
        protected override async Task OnInitializedAsync()
        {
            UserId = Session.GetUserId();

            InitializeGridQuery();

            ListeProfil = new TH_HABILITATIONS.ModeDroit().DroitList();

            var categories_QueryOption = $"?$filter=TRST_STATUTID eq '{StatusLiteral.Available}'";
            _categories = await ProxyCore.GetEnumerableAsync<TC_CATEGORIES>(categories_QueryOption, useCache: false);
            _categories = _categories.OrderBy(d => d.TC_NOM);

            // Read authorized users.
            _authorizedUsers = await ProxyCore.GetEnumerableAsync<TRU_USERS>(_authorizedUserOdataQuery, useCache: false);

            //http://localhost:8000/api/univers/v1/TRU_USERS?$expand=TRUCL_USERS_CLAIMS($filter=TRUCL_STATUS eq '{StatusLiteral.Available}' and TRCLI_CLIENTAPPLICATIONID eq 7 and TRUCL_CLAIM_VALUE eq '4' )&$filter=TRU_STATUS eq 'A' and TRUCL_USERS_CLAIMS/any(TRS: TRS/TRUCL_CLAIM_VALUE  eq '4')
        }
        #endregion

        #region Grid
        /// <summary>
        /// Reference to the grid component.
        /// </summary>
        private OrkaGenericGridComponent<TH_HABILITATIONS> Ref_TH_HABILITATIONS;

        /// <summary>
        /// Initialize Sf query applied on the grid.
        /// </summary>
        protected void InitializeGridQuery()
        {
            var queryOptions = $"?$expand=TS_SCENARIO($expand=TE_ETAT),TRU_USER, TRST_STATUT,TRU_INITIALISATION_AUTEUR,TRU_MAJ_AUTEUR";
            //http://localhost:8000/api/univers/v1/TH_HABILITATIONS?$expand=TS_SCENARIO($expand=TE_ETAT),TRU_USER

            GridQuery = new Query()
                    .AddParams(Litterals.OdataQueryParameters, queryOptions) // Ne mettre QUE les $expand & $filter dans ce slot !
                    .Sort(nameof(TH_HABILITATIONS.TH_MAJ_DATE), "descending");
        }

        /// <summary>
        /// Get header for grid edit template.
        /// </summary>
        /// <param name="th_habilitation">Edited item.</param>
        /// <returns>Edit header text.</returns>
        public string GetEditHeader(TH_HABILITATIONS th_habilitation)
            => th_habilitation.TH_HABILITATIONID == 0 ? Trad.Keys["Administration:AddAuthorizations"] : Trad.Keys["Administration:EditAuthorization"];
        #endregion

        #region Categories column
        /// <summary>
        /// List of TC_CATEGORIES (read from DB).
        /// </summary>
        private IEnumerable<TC_CATEGORIES> _categories = Enumerable.Empty<TC_CATEGORIES>();

        /// <summary>
        /// List only TC_CATEGORIES with "Active" status. 
        /// </summary>
        private IEnumerable<TC_CATEGORIES> ActiveCategories
        {
            get
            {
                return _categories;
            }
        }

        /// <summary>
        /// Event triggered when a category is selected.
        /// </summary>
        /// <param name="args">Change event arguments.</param>
        private Task CategoryValueChangeAsync(ChangeEventArgs<int?, TC_CATEGORIES> args)
            // Update available scenarios depending on selected category.
            => SetScenariosAsync(args.ItemData.TC_CATEGORIEID);
        #endregion

        #region SfGrid Events
        /// <summary>
        /// Event triggered when DataGrid actions are completed.
        /// </summary>
        private async Task ActionBeginAsync(ActionEventArgs<TH_HABILITATIONS> args)
        {
            if (Syncfusion.Blazor.Grids.Action.BeginEdit.Equals(args.RequestType) ||
                Syncfusion.Blazor.Grids.Action.Add.Equals(args.RequestType))
            {
                // Reset checked value.
                CheckedValueStatut = "Unchanged";

                // Clear selected modules.
                MultiselectValues.Clear();

                // Reset list of available modules (scenarios).
                await SetScenariosAsync();
            }
        }

        /// <summary>
        /// Event triggered when DataGrid actions are completed.
        /// </summary>
        private void OnActionComplete(ActionEventArgs<TH_HABILITATIONS> args)
        {
            /* Disable prevent render.
             * https://blazor.syncfusion.com/documentation/datagrid/webassembly-performance/#avoid-unnecessary-component-renders*/
            if (args.RequestType is Syncfusion.Blazor.Grids.Action.Add or Syncfusion.Blazor.Grids.Action.BeginEdit)
            {
                args.PreventRender = false;  // requis ici et pas ailleurs utilisé pour effet cascade entre selection de catégorie et de module
            }
        }

        /// <summary>
        /// Event triggered when selected radio button changed.
        /// </summary>
        /// <param name="args">Change event arguments.</param>
        private void SfRadioButtonStatutOnChange(ChangeEventArgs args)
        {
            CheckedValueStatut = (string)args.Value;
        }
        #endregion

        #region Authorized user column
        /// <summary>
        /// Users who can be authorized.
        /// </summary>
        private IEnumerable<TRU_USERS> _authorizedUsers = Enumerable.Empty<TRU_USERS>();

        /// <summary>
        // Odata query used to read users who can be authorized.
        //  - Active users,
        //      - with claim value equal to DataDriven,
        //      - with active user-claims,
        //      - with client application "DTF",
        //      - with claim of type "Role".
        //  - Sorted alphabetically.
        /// </summary>
        private string _authorizedUserOdataQuery = $"?$filter={nameof(TRU_USERS.TRU_STATUS)} eq '{StatusLiteral.Available}' " +
            $"and {nameof(TRU_USERS.TRUCL_USERS_CLAIMS)}/any(TRUCL: " +
                $"TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRUCL_CLAIM_VALUE)} eq '{(int)RolesEnums.RolesValues.DataDriven}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRUCL_STATUS)} eq '{StatusLiteral.Available}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRCLI_CLIENTAPPLICATION)}/{nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_LABEL)} eq '{Litterals.ApplicationDTF}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRCL_CLAIM)}/{nameof(TRCL_CLAIMS.TRCL_CLAIM_NAME)} eq '{ClaimsLiterals.Role}')" +
            $"&$orderby={nameof(TRU_USERS.TRU_FULLNAME)} asc";
        #endregion

        #region Scenario MultiSelect
        /// <summary>
        /// Array of the selected values.
        /// </summary>
        public IList<TS_SCENARIOS> MultiselectValues { get; set; } = new List<TS_SCENARIOS>();

        /// <summary>
        /// List of TS_SCENARIOS  (read from DB).
        /// </summary>
        private IEnumerable<TS_SCENARIOS> _scenarios = Enumerable.Empty<TS_SCENARIOS>();

        /// <summary>
        /// OData query used to read scenarios/modules:
        /// - Expanded to versions (TE_ETATS),
        /// - With an active scenarios,
        ///     - With active or prototype versions (TE_ETATS),
        ///     - With active UTD (TEM_ETAT_MASTERS).
        /// </summary>
        private const string _scenariosOdataQuery = $"?$expand={nameof(TS_SCENARIOS.TE_ETAT)}" +
            $"&$filter={nameof(TS_SCENARIOS.TRST_STATUTID)} eq '{StatusLiteral.Available}' " +
            $"and ({nameof(TS_SCENARIOS.TE_ETAT)}/{nameof(TE_ETATS.TRST_STATUTID)} eq '{StatusLiteral.Prototype}' " +
                $"or {nameof(TS_SCENARIOS.TE_ETAT)}/{nameof(TE_ETATS.TRST_STATUTID)} eq '{StatusLiteral.Available}') " +
            $"and {nameof(TS_SCENARIOS.TE_ETAT)}/{nameof(TE_ETATS.TEM_ETAT_MASTER)}/{nameof(TEM_ETAT_MASTERS.TRST_STATUTID)} eq '{StatusLiteral.Available}'";

        /// <summary>
        /// Update available scenarios depending on selected category.
        /// </summary>
        /// <param name="categoryId">Id of the category. Used to filter scenarios by selected category.</param>
        private async Task SetScenariosAsync(int categoryId = default)
        {
            // Prepare optional category filter.
            string categoryFilter = string.Empty;
            if (!categoryId.Equals(default))
                categoryFilter = $" and {nameof(TS_SCENARIOS.TE_ETAT)}/{nameof(TE_ETATS.TEM_ETAT_MASTER)}/{nameof(TEM_ETAT_MASTERS.TC_CATEGORIEID)} eq {categoryId}";

            // Read scenarios/modules.
            _scenarios = await ProxyCore.GetEnumerableAsync<TS_SCENARIOS>($"{_scenariosOdataQuery}{categoryFilter}", useCache: false);
        }

        /// <summary>
        /// Add 1 to N entries
        /// </summary>
        /// <param name="args">Select event arguments.</param>
        private void OnValueSelectHandler(SelectEventArgs<TS_SCENARIOS> args)
        {
            if (!MultiselectValues.Contains(args.ItemData))
                MultiselectValues.Add(args.ItemData);
        }

        /// <summary>
        /// Remove 1 to N entries
        /// </summary>
        /// <param name="args">Remove event arguments.</param>
        private void OnValueRemoveHandler(RemoveEventArgs<TS_SCENARIOS> args)
        {
            if (MultiselectValues.Contains(args.ItemData))
                MultiselectValues.Remove(args.ItemData);
        }
        #endregion

        #region Save
        /// <summary>
        /// Custom event to handle and manipulate datas before saving to database.
        /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit().
        /// </summary>
        /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
        /// <param name="entity">Incoming datas to be saved</param>
        private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
        {
            Ref_TH_HABILITATIONS.DataGrid.PreventRender(true);

            // cas modif prendre en compte chgt de statut si autorisé
            var habilitation = entity as TH_HABILITATIONS;

            if (habilitation!.TH_HABILITATIONID == 0)
            {
                // verif si user sélectionné
                // verif si au moins un module est sélectionné
                // verif si un droit est sélectionné

                // risque potentiel si un droit que l on souhaite rajouter existe déjà meme si pas d effet
                if (!MultiselectValues.Any() || habilitation.TH_DROIT_CONCERNE is null || habilitation.TRU_USERID is null)
                {
                    //"Veuillez sélectionner au moins un module,  un profil et un utilisateur "
                    await Toast.DisplayWarningAsync("Info", @Trad.Keys["Administration:DataDrivenDTFAssociationChk"]);
                    return;
                }

                IList<TH_HABILITATIONS> itemsToCreate = new List<TH_HABILITATIONS>();
                foreach (var Module in MultiselectValues)
                {
                    TH_HABILITATIONS newItem = new()
                    {
                        TRU_USERID = habilitation.TRU_USERID,
                        TS_SCENARIOID = Module.TS_SCENARIOID,
                        TH_COMMENTAIRE = habilitation.TH_COMMENTAIRE,
                        TH_DROIT_CONCERNE = habilitation.TH_DROIT_CONCERNE,

                        TH_DATE_INITIALISATION = DateExtensions.GetLocaleNow(),
                        TRU_INITIALISATION_AUTEURID = UserId,
                        TH_MAJ_DATE = DateExtensions.GetLocaleNow(),
                        TRU_MAJ_AUTEURID = UserId,
                        TH_EST_HABILITE = 1,
                        TRST_STATUTID = StatusLiteral.Available
                        //champs non exploité liés aux groupes
                        //TTE_TEAMID
                        //TSG_SCENARIO_GPEID
                        //TH_HERITE_HABILITATIONID
                    };

                    // Add this item to the list of elements to create.
                    itemsToCreate.Add(newItem);
                }

                if (itemsToCreate.Any())
                {
                    // Add these items to the table.
                    var apiResult = await ProxyCore.InsertAsync(itemsToCreate);
                    // If the insertion failed.
                    if (apiResult.Count < 1)
                    {
                        await ProxyCore.SetLogException(new LogException(GetType(), itemsToCreate, apiResult.Message));
                        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"] + apiResult.Message);
                        return;
                    }
                    else
                    // forcer refresh de la grille suite a ajout en masse
                    {
                        // bien utiliser CloseEditAsync et pas EndEdit car sauvegarde faite via InsertAsync
                        await Ref_TH_HABILITATIONS.DataGrid.CloseEditAsync();

                        // Refresh grid after close, otherwise it can broke the SfGrid edit state machine.
                        ProxyCore.CacheRemoveEntities(typeof(TEntity));
                        await Ref_TH_HABILITATIONS.DataGrid.Refresh();
                    }
                }
            }
            else  // mode mise a jour uniquement
            {
                switch (CheckedValueStatut)
                {
                    case "Unchanged":
                        break;

                    case StatusLiteral.Available:
                        // verification que le user est toujours habilité en datadriven sur DTF
                        if (!(_authorizedUsers.Where(a => a.TRU_USERID == habilitation.TRU_USERID).Any()))
                        { //"Action impossible, l'Utilisateur ne dispose plus du rôle Datadriven ou est Inactif"

                            // Ne pas s amuser a changer les commentaires des autres
                            // surtout en en mettant un autre qui ne correspond pas a la réalité

                            await Toast.DisplayWarningAsync(Trad.Keys["COMMON:Error"], @Trad.Keys["Administration:DataDrivenDTFAssociationInvalid"]);
                            return;
                        }
                        else
                        {
                            habilitation.TH_EST_HABILITE = 1;
                            habilitation.TRST_STATUTID = StatusLiteral.Available;
                            break;
                        }

                    case StatusLiteral.Deactivated:
                        habilitation.TH_EST_HABILITE = 0;
                        habilitation.TRST_STATUTID = StatusLiteral.Deactivated;
                        // autre actions eventuelles a déterminer MOA ( ex: désactiver demandes / planif qui aurient pu etre définies)
                        break;
                }

                // ATTENTION il est obligatoire de resetter le rattachement des  entities associés aux  FK pour lesquelles on souhaite modifier la valeur
                // sinon génération erreur, lié a l'expand, le systeme va essayer de mettre a jour à tort l entities rattaché sinon
                habilitation.TRST_STATUT = null;
                habilitation.TRU_MAJ_AUTEUR = null;
                habilitation.TRU_INITIALISATION_AUTEUR = null;

                habilitation.TS_SCENARIO = null;
                habilitation.TRU_USER = null;
                habilitation.TSG_SCENARIO_GPE = null;
                habilitation.TRU_INITIALISATION_AUTEUR = null;
                habilitation.TTE_TEAM = null;

                habilitation.TH_MAJ_DATE = DateExtensions.GetLocaleNow();
                habilitation.TRU_MAJ_AUTEURID = UserId;

                // Save.
                await Ref_TH_HABILITATIONS.DataGrid.EndEditAsync();
            }
        }
        #endregion
    }
}