using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD
{
    public partial class UTDSelectComponent
    {
        #region Parameters
        /// <summary>
        /// Id of selected Etat.
        /// </summary>
        [Parameter]
        public int? _etatId { get; set; }

        /// <summary>
        /// Id of selected scenario.
        /// </summary>
        [Parameter]
        public int? _scenarioId { get; set; }

        /// <summary>
        /// Is the selection of an application module mandatory?
        /// </summary>
        [Parameter]
        public bool IsModuleRequired { get; set; }

        /// <summary>
        /// Is UTD selection enabled?
        /// </summary>
        [Parameter]
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Is the selection made among all UTDs?
        /// (Otherwise, the selection is made according to the user rights.)
        /// </summary>
        [Parameter]
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Callback triggers when a new Etat (UTD) is selected.
        /// Passed parameter EtatId.
        /// </summary>
        [Parameter]
        public EventCallback<int?> OnEtatChanged { get; set; }

        /// <summary>
        /// Callback triggers when a new scenario is selected.
        /// Passed parameters: EtatId, ScenarioId, IsPrototype.
        /// </summary>
        [Parameter]
        public EventCallback<(int?, int?, bool)> OnScenarioChanged { get; set; }
        #endregion

        #region Properties
        /// <summary>
        /// Id of selected TC_CATEGORIES.
        /// </summary>
        public int? _categoryId { get; set; }

        /// <summary>
        /// Is the UTD in prototype status.
        /// </summary>
        public bool _isPrototype { get; set; }
        #endregion

        #region Blazor life cycle
        /// <summary>
        /// Is component initialization ongoing?
        /// </summary>
        private bool _isInitializing = true;

        /// <summary>
        /// Called after the component has finished rendering.
        /// </summary>
        /// <param name="firstRender">True if first render.</param>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await VerifyUserRightsAsync();

                await InitializeCategoryAsync();
            }
        }
        #endregion

        #region User rights
        /// <summary>
        /// Does the user verify the DataDriven policy for DTF?
        /// </summary>
        private bool _isDataDriven;

        /// <summary>
        /// Does the user verify the Admin policy for DTF?
        /// </summary>
        private bool _isDTFAdm;

        /// <summary>
        /// Control the rights of the user. 
        /// </summary>
        private async Task VerifyUserRightsAsync()
        {
            // Force admin rights to have a complete list of Etats.
            if (IsAdmin)
            {
                _isDTFAdm = true;
                return;
            }

            // Get rights from user policy.
            _isDataDriven = await Session.VerifyPolicy(PoliciesLiterals.DTFDataDriven);
            _isDTFAdm = await Session.VerifyPolicy(PoliciesLiterals.DTFAdm);
        }
        #endregion

        #region Category select
        /// <summary>
        /// List of available Categories.
        /// </summary>
        private IEnumerable<TC_CATEGORIES> CategoryList = Enumerable.Empty<TC_CATEGORIES>();

        /// <summary>
        /// Filters applied on Category selection.
        /// </summary>
        private readonly Query CategoryQuery = new Query()
            .Where(nameof(TC_CATEGORIES.TRST_STATUTID), "equal", StatusLiteral.Available)
            .Sort(nameof(TC_CATEGORIES.TC_NOM), "Ascending")
            .Take(20);

        /// <summary>
        /// Comment of the selected Category.
        /// </summary>
        private string CategoryComment;

        /// <summary>
        /// Initialize Category autocomplete data.
        /// </summary>
        private async Task InitializeCategoryAsync()
        {
            // Initialize UTD list depending on user rights.
            await ReadEtatAsync();

            // Based on UTD list (Etats) available for this user,
            // select distinct categories.
            CategoryList = EtatsList
                .Select(e => e.TEM_ETAT_MASTER.TC_CATEGORIE)
                    .DistinctBy(c => c.TC_CATEGORIEID)
                    .ToList();

            // Go to next initialization step.
            await InitializeEtatAsync();
        }

        /// <summary>
        /// Event triggers when the AutoComplete value is changed.
        /// </summary>
        /// <param name="args">Change event arguments.</param>
        private async Task CategoryValueChangeAsync(ChangeEventArgs<string, TC_CATEGORIES> args)
            => await SetCategoryAsync(args.ItemData);

        /// <summary>
        /// Update component with selected Category.
        /// If no Category is selected, clear component.
        /// </summary>
        /// <param name="category">Selected Category.</param>
        private async Task SetCategoryAsync(TC_CATEGORIES category)
        {
            // Update comment.
            CategoryComment = category?.TC_COMMENTAIRE;

            // Set selected etat id and inform parent.
            _categoryId = category?.TC_CATEGORIEID;

            // Reset Etat.
            if (!_isInitializing)
                _etatId = 0;

            // Update available Etats depending on selected Categorie.
            if (category is null)
            {
                await InitializeEtatAsync();
            }
            else
            {
                await InitializeEtatAsync(category.TC_CATEGORIEID);
            }
        }
        #endregion

        #region Versions (TE_ETAT)
        /// <summary>
        /// Reference to the AutoComplete component.
        /// </summary>
        private SfAutoComplete<string, TE_ETATS> EtatAutoCompleteReference;

        /// <summary>
        /// List of available Etats.
        /// </summary>
        private IEnumerable<TE_ETATS> EtatsList = Enumerable.Empty<TE_ETATS>();

        /// <summary>
        /// Filters applied on Etats selection.
        /// </summary>
        private readonly Query EtatsQuery = new Query()
            .Take(20);

        /// <summary>
        /// Value of autocomplete.
        /// </summary>
        private string SelectedEtatValue { get; set; }

        /// <summary>
        /// Comment from the EtatMaster associated of the selected Etat.
        /// </summary>
        private string EtatMasterComment;

        /// <summary>
        /// Comment of the selected Etat.
        /// </summary>
        private string EtatComment;

        /// <summary>
        /// Information about the selected revision.
        /// </summary>
        private string EtatInfoRevision;

        /// <summary>
        /// Highlight the searched characters on suggested list items.
        /// </summary>
        /// <param name="etatFullName">Text to highlight.</param>
        /// <returns>Highlighten text as markup.</returns>
        private MarkupString EtatAutoCompleteHighlightSearch(string etatFullName)
            => (MarkupString)EtatAutoCompleteReference
            .HighLightSearch(etatFullName, true, Syncfusion.Blazor.DropDowns.FilterType.Contains);

        /// <summary>
        /// Read Etat values based on user rights.
        /// </summary>
        /// <param name="categoryId">Category Id by which the data will be filtered.</param>
        private async Task ReadEtatAsync(int categoryId = 0)
        {
            // If the user is an admin of DTF.
            if (_isDTFAdm)
                EtatsList = await ProxyCore.GetExecutableTeEtatsForDTFAsync<TE_ETATS>(true, Session.GetUserId(), categoryId);
            // If the user has DataDriven rights for DTF.
            else if (_isDataDriven)
                EtatsList = await ProxyCore.GetExecutableTeEtatsForDTFAsync<TE_ETATS>(false, Session.GetUserId(), categoryId);
        }

        /// <summary>
        /// Initialize Etat autocomplete data.
        /// </summary>
        /// <param name="categoryId">Category Id by which the data will be filtered.</param>
        private async Task InitializeEtatAsync(int categoryId = 0)
        {
            if (!_isInitializing)
                // Initialize UTD list depending on user rights.
                await ReadEtatAsync(categoryId);

            // Get selected Etat.
            TE_ETATS selectedEtat = EtatsList.FirstOrDefault(e => _etatId.Equals(e.TE_ETATID));

            // Get AutoComplete value.
            SelectedEtatValue = selectedEtat?.TE_FULLNAME;

            if (SelectedEtatValue is null)
                // Component initialization end here.
                _isInitializing = false;

            // Re-render component.
            StateHasChanged();

            // If component is deactivated, forces the update of Etat information
            // because ValueChange event won't be called.
            if (!IsEnabled)
                await SetEtatAsync(selectedEtat);
        }

        /// <summary>
        /// Event triggers when the AutoComplete value is changed.
        /// </summary>
        /// <param name="args">Change event arguments.</param>
        private async Task EtatValueChangeAsync(ChangeEventArgs<string, TE_ETATS> args)
            => await SetEtatAsync(args.ItemData);

        /// <summary>
        /// Update component with selected Etat.
        /// If no Etat is selected, clear component.
        /// </summary>
        /// <param name="etat">Selected Etat.</param>
        private async Task SetEtatAsync(TE_ETATS etat)
        {
            // Update comments.
            EtatComment = etat?.TE_COMMENTAIRE;
            EtatMasterComment = etat?.TEM_ETAT_MASTER?.TEM_COMMENTAIRE;
            EtatInfoRevision = etat?.TE_INFO_REVISION;

            // Activate/Deactivate scenario selection.
            ScenarioSelectionEnabled = etat is not null && IsEnabled;

            // Set selected etat id and inform parent.
            _etatId = etat?.TE_ETATID;

            // Triggers callback.
            await OnEtatChanged.InvokeAsync(_etatId);

            // Update boolean indicating if selected Etat is a prototype.
            if (etat is not null)
            {
                _isPrototype = StatusLiteral.Prototype.Equals(etat?.TRST_STATUTID);
            }

            // Reset Scenario.
            if (!_isInitializing)
                _scenarioId = default;

            await InitializeScenarioAsync();
        }
        #endregion

        #region Application module (TS_SCENARIO)
        /// <summary>
        /// List of available Scenarios.
        /// </summary>
        private IEnumerable<TS_SCENARIOS> ScenariosList = Enumerable.Empty<TS_SCENARIOS>();

        /// <summary>
        /// Value of autocomplete.
        /// </summary>
        private string SelectedScenarioValue { get; set; }

        /// <summary>
        /// Description of the selected Scenario.
        /// </summary>
        private string ScenarioDescription;

        /// <summary>
        /// Can the user select a Scenario?
        /// </summary>
        private bool ScenarioSelectionEnabled;

        /// <summary>
        /// Event triggers when the AutoComplete value is changed.
        /// </summary>
        /// <param name="args">Change event arguments.</param>
        private async Task ScenarioValueChangeAsync(ChangeEventArgs<string, TS_SCENARIOS> args)
            => await SetScenarioAsync(args.ItemData);

        /// <summary>
        /// Initialize Scenario Autocomplete data.
        /// </summary>
        private async Task InitializeScenarioAsync()
        {
            //***** Get Scenario DataSource *****//
            // If Etat is selected, initialize Scenario list depending on user rights.
            if (_etatId > 0)
            {
                // Construct odata query to apply on scenarios.
                string queryOptions;
                if (_isDataDriven) // restriction datadriven
                {
                    // http://localhost:8000/api/univers/v1/TS_SCENARIOS?$expand=VDTFH_HABILITATION($filter=TRU_USERID eq '1' and PRODUCTEUR eq 1)&$filter=VDTFH_HABILITATION/any(o: o/TRU_USERID eq '1' and o/PRODUCTEUR eq 1)
                    queryOptions = $"$expand=VDTFH_HABILITATION($filter=TRU_USERID eq '{Session.GetUserId()}' and PRODUCTEUR eq 1)&$filter=VDTFH_HABILITATION / any(o: o/TRU_USERID eq '{Session.GetUserId()}' and o/PRODUCTEUR eq 1) and ";
                }
                else
                { // admin pas de restriction lié a la table d'habilitation
                    queryOptions = $"$filter=";
                }
                // Filter with active status and with selected Etat.
                queryOptions += $"TRST_STATUTID eq '{StatusLiteral.Available}' and TE_ETATID eq {_etatId}";

                // Read data from db.
                ScenariosList = await ProxyCore.GetEnumerableAsync<TS_SCENARIOS>($"?{queryOptions}", useCache: false);
            }

            //***** Get Sceario Value *****//
            // Get selected scenario.
            TS_SCENARIOS selectedScenario = ScenariosList.FirstOrDefault(s => _scenarioId.Equals(s.TS_SCENARIOID));

            // Set AutoComplete value.
            SelectedScenarioValue = selectedScenario?.TS_NOM_SCENARIO;

            //***** Set Scenario Informations *****//
            // If component is deactivated, forces the update of scenario information
            // because ValueChange event won't be called.
            if (!ScenarioSelectionEnabled)
                await SetScenarioAsync(selectedScenario);
        }

        /// <summary>
        /// Update component with selected Scenario.
        /// If no Scenario is selected, clear component.
        /// </summary>
        /// <param name="scenario">Selected Scenario.</param>
        private async Task SetScenarioAsync(TS_SCENARIOS scenario)
        {
            // Set scenario description.
            ScenarioDescription = scenario?.TS_DESCR;

            // Set selected scenario id and inform parent.
            _scenarioId = scenario?.TS_SCENARIOID;

            // Triggers callback.
            await OnScenarioChanged.InvokeAsync((_etatId, _scenarioId, _isPrototype));

            // Component initialization end here.
            if (_isInitializing)
                _isInitializing = false;
        }
        #endregion
    }
}
