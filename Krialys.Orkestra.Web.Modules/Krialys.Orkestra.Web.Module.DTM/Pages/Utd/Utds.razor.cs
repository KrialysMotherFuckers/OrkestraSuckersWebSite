using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;
using static MudBlazor.CategoryTypes;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd;

public partial class Utds
{
    public new RolesEnums.RolesValues PolicyApplied = PoliciesLiterals.UTDEditor;

    #region Categories column
    /// <summary>
    /// List of TC_CATEGORIES (read from DB).
    /// </summary>
    private readonly List<TC_CATEGORIES> _categories = new();

    /// <summary>
    /// List only TC_CATEGORIES with "Active" status. 
    /// </summary>
    private IList<TC_CATEGORIES> ActiveCategories => _categories.Where(c => c.TRST_STATUTID.Equals(StatusLiteral.Available)).ToList();

    #endregion

    #region Farms column
    /// <summary>
    /// List of TEMF_ETAT_MASTER_FERMES (read from DB).
    /// </summary>
    private List<TEMF_ETAT_MASTER_FERMES> _etatMasterFermes = new List<TEMF_ETAT_MASTER_FERMES>();

    /// <summary>
    /// List of TF_FERMES (read from DB).
    /// </summary>
    private List<TF_FERMES> _fermes = new List<TF_FERMES>();

    /// <summary>
    /// List only TF_FERMES with "Active" status. 
    /// </summary>
    private IList<TF_FERMES> ActiveFermes => _fermes.Where(f => f.TRST_STATUTID.Equals(StatusLiteral.Available)).ToList();

    /// <summary>
    /// List of selected farms (binded with SelectFarmComponent).
    /// </summary>
    public IList<TF_FERMES> SelectedFarms { get; set; } = new List<TF_FERMES>();

    /// <summary>
    /// Farm multiselect error tooltip.
    /// </summary>
    private SfTooltip _fermesTooltipObj;

    /// <summary>
    /// Get descriptions of the farms linked to a TEM_ETAT_MASTER field.
    /// </summary>
    /// <param name="etatMasterId">TEM_ETAT_MASTER id.</param>
    /// <returns>Farms' descriptions.</returns>
    private string GetFarmsDescriptions(int etatMasterId)
        => string.Join(", ", from etatMasterFerme in _etatMasterFermes.Where(emf => emf.TEM_ETAT_MASTERID == etatMasterId).ToList()
                             let farmDescription = _fermes?.FirstOrDefault(f => f.TF_FERMEID == etatMasterFerme.TF_FERMEID)?.TF_DESCR
                             where !string.IsNullOrEmpty(farmDescription)
                             select farmDescription);

    /// <summary>
    /// Save selected farms by updating TEMF_ETAT_MASTER_FERMES junction table.
    /// Update both the database and the local copy.
    /// </summary>
    /// <param name="etatMaster">TEM_ETAT_MASTER item.</param>
    private async Task SaveFarmsAsync(TEM_ETAT_MASTERS etatMaster)
    {
        // Get item id.
        int etatMasterId = etatMaster.TEM_ETAT_MASTERID;

        // If the action is an insertion, the id is not defined before being created in database.
        if (etatMasterId == 0)
        {
            // Read id of the inserted item from database.
            // Filter with item GUID to get inserted item.
            string filter = $"(TEM_GUID eq '{etatMaster.TEM_GUID}')";
            etatMasterId = (await ProxyCore.GetEnumerableAsync<TEM_ETAT_MASTERS>($"?$filter={filter}")
                ).FirstOrDefault()?
                .TEM_ETAT_MASTERID ?? 0;
        }

        // Verify id is defined now.
        if (etatMasterId != 0)
        {
            // Get farms linked to this etat master.
            _etatMasterFermes = (await ProxyCore.GetEnumerableAsync<TEMF_ETAT_MASTER_FERMES>(useCache: false)).ToList();
            var selectedEtatMasterFermes = _etatMasterFermes.Where(emf => emf.TEM_ETAT_MASTERID == etatMasterId);

            // Update junction table TEMF_ETAT_MASTER_FERMES depending on selected TF_FERMES and 
            // the edited TEM_ETAT_MASTERS.
            // ***** Part 1: Remove unselected items from the database. *****
            // List of the ids of the TEMF entries to remove.
            List<string> idsEmfToRemove = new();
            IList<TEMF_ETAT_MASTER_FERMES> emfToRemove = new List<TEMF_ETAT_MASTER_FERMES>();
            // Browse through junction table.
            var temfEtatMasterFermeses = selectedEtatMasterFermes.ToList();
            foreach (var emf in temfEtatMasterFermeses)
            {
                // Search elements of TEMF that are not in selected farms.
                if (SelectedFarms.Any(sf => sf.TF_FERMEID == emf.TF_FERMEID))
                    continue;

                // Add this item to the list of elements to remove.
                idsEmfToRemove.Add(emf.TEMF_ETAT_MASTER_FERMEID.ToString());
                emfToRemove.Add(emf);
            }
            if (idsEmfToRemove.Any())
            {
                try
                {
                    // Delete these items from the junction table.
                    await ProxyCore.DeleteAsync<TEMF_ETAT_MASTER_FERMES>(idsEmfToRemove.ToArray());
                }
                catch (Exception ex)
                {
                    // Log error
                    await ProxyCore.SetLogException(new LogException(this.GetType(), ex));

                    // Display error in a toast.
                    await Toast.DisplayErrorAsync(Trad.Keys["GridSource:GridError"], Trad.Keys["DTM:FarmsSaveError"])
                        ;
                }
            }

            // ***** Part 2: Create newly selected items in the database. *****
            // List  of the TEMF entries to create.
            IList<TEMF_ETAT_MASTER_FERMES> emfToCreate = new List<TEMF_ETAT_MASTER_FERMES>();

            // Browse through the selected farms.
            foreach (var farm in SelectedFarms)
            {
                // Search for selected farms that are not in TEMF.
                if (!temfEtatMasterFermeses.Any(semf => semf.TF_FERMEID == farm.TF_FERMEID))
                {
                    TEMF_ETAT_MASTER_FERMES newEmf = new()
                    {
                        TEM_ETAT_MASTERID = etatMasterId,
                        TF_FERMEID = farm.TF_FERMEID,
                        TEMF_DATE_AJOUT = DateExtensions.GetLocaleNow()
                    };

                    // Add this item to the list of elements to create.
                    emfToCreate.Add(newEmf);
                }
            }
            if (emfToCreate.Any())
            {
                // Add these items to the junction table.
                var apiResult = await ProxyCore.InsertAsync(emfToCreate);
                // If the insertion failed.
                if (apiResult.Count < 1)
                {
                    await ProxyCore.SetLogException(new LogException(this.GetType(), emfToCreate, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTM:FarmsSaveError"]);
                }
            }

            // ***** Part 3: Update the local copy of the junction table. *****
            // Delete items.
            foreach (var emf in emfToRemove)
            {
                _etatMasterFermes.Remove(emf);
            }

            // Add items.
            foreach (var emf in emfToCreate)
            {
                _etatMasterFermes.Add(emf);
            }
        }
    }
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        if (Enum.IsDefined(typeof(RolesEnums.RolesValues), PolicyApplied))
            AllowModify = await IsInPolicy(nameof(PoliciesLiterals.UTDEditor));

        // Get links between EtatMaster table and farm table from DB
        _etatMasterFermes.AddRange(await ProxyCore.GetEnumerableAsync<TEMF_ETAT_MASTER_FERMES>(useCache: false));

        // Get list of servers farms from DB
        _fermes.AddRange(await ProxyCore.GetEnumerableAsync<TF_FERMES>(useCache: true));

        // Get categories from DB
        _categories.AddRange(await ProxyCore.GetEnumerableAsync<TC_CATEGORIES>(useCache: true));
    }

    /// <summary>
    /// Method called by the framework when the rendering of all the references 
    /// to the component are populated.
    /// Use this stage to perform additional initialization steps with the rendered content,
    /// such as JS interop calls that interact with the rendered DOM elements.
    /// </summary>
    /// <param name="firstRender">Is the component rendered for the first time? </param>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // Add "Inactivate" button to the Toolbar.
            // Note: Why is this method called by OnAfterRender?
            // This method uses the reference of the component WasmDataGrid.
            // The reference is available after the render.
            // Besides, WasmDataGrid toolbar has already been initialized by WasmDataGrid OnInitialized
            // because the child lifecycle happens before render.
            AddInactivateToolbarButton();
        }
    }

    private async Task OnGridRefresh()
    {
        // Refresh datagrid (strict minimum action expected), but indirectly
        await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, Ref_TEM_ETAT_MASTERS.GetRefreshGridButtonId, 750);
    }

    #endregion

    #region
    /// <summary>
    /// Propage changement de nom de l'UTD master sur les UTD enfant (selon leurs statuts)
    /// </summary>
    /// <param name="etatMaster">TEM_ETAT_MASTER item.</param>
    private async Task SaveUTDNameChildAsync(TEM_ETAT_MASTERS etatMaster)
    {
        int etatMasterId = etatMaster.TEM_ETAT_MASTERID;

        if (etatMasterId != 0)
        {
            string filter = $"TEM_ETAT_MASTERID eq {etatMasterId} and TRST_STATUTID in ('{StatusLiteral.Available}','{StatusLiteral.Prototype}','{StatusLiteral.Draft}')";

            var _etats = await ProxyCore.GetEnumerableAsync<TE_ETATS>($"?$filter={filter}");
            var idx = new List<object>();
            var sta = new List<JsonPatchDocument<TE_ETATS>>();

            foreach (var etat in _etats)
            {
                idx.Add(etat.TE_ETATID);
                sta.Add(new JsonPatchDocument<TE_ETATS>().Replace(teEtats => teEtats.TE_NOM_ETAT, etatMaster.TEM_NOM_ETAT_MASTER));
            }

            var result = await ProxyCore.PatchAsync(idx, sta);
            if (result.Count > 0)
            {
                ProxyCore.CacheRemoveEntities(typeof(TE_ETATS));
            }
        }
    }
    #endregion

    #region TEM_ETAT_MASTERS Grid
    /// <summary>
    /// Reference to the grid component.
    /// </summary>
    OrkaGenericGridComponent<TEM_ETAT_MASTERS> Ref_TEM_ETAT_MASTERS;

    /// <summary>
    /// Data of the selected row.
    /// </summary>
    private TEM_ETAT_MASTERS _selectedRecord;

    /// <summary>
    /// Is the save button of the edit dialog disabled?
    /// </summary>
    private bool ButtonSaveDisabled { get; set; }

    ///// <summary>
    ///// Is the user allowed to modify grid data ?
    ///// </summary>
    //private bool AllowModify { get; set; } = true;

    /// <summary>
    /// Hidden fields when adding/editing.
    /// </summary>
    private readonly string[] _editingHiddenFields = {
        nameof(TEM_ETAT_MASTERS.TEM_DATE_CREATION),
        nameof(TEM_ETAT_MASTERS.TRST_STATUTID)
    };

    /// <summary>
    /// Get header for grid edit template.
    /// </summary>
    /// <param name="etatMaster">Edited item.</param>
    /// <returns>Edit header text.</returns>
    public string GetEditHeader(TEM_ETAT_MASTERS etatMaster)
        => etatMaster.TEM_ETAT_MASTERID == 0 ? Trad.Keys["DTM:UTDGridAddHeader"] : Trad.Keys["DTM:UTDGridEditHeader"];

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database.
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit().
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        if (entity is TEM_ETAT_MASTERS etatMaster)
        {
            etatMaster.TEM_NOM_ETAT_MASTER = etatMaster.TEM_NOM_ETAT_MASTER.FirstCharToUpper();

            // If no farms are selected, display tooltip and don't save.
            if (!SelectedFarms.Any())
            {
                await _fermesTooltipObj.OpenAsync();
                return;
            }

            await _fermesTooltipObj.CloseAsync();

            // If new item is added.
            if (etatMaster!.TEM_ETAT_MASTERID == 0)
            {
                // Set creation date.
                etatMaster.TEM_DATE_CREATION = DateExtensions.GetLocaleNow();
            }
        }

        // Save grids record.
        await instance.DataGrid.EndEditAsync();
    }

    /// <summary>
    /// Event triggers when DataGrid actions starts.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public Task OnActionBeginAsync(ActionEventArgs<TEM_ETAT_MASTERS> args)
    {
        // When an edition is started.
        if (Action.BeginEdit.Equals(args.RequestType))
        {
            // If statut is active.
            ButtonSaveDisabled = !args.Data.TRST_STATUTID.Equals(StatusLiteral.Available);

            // Hide columns that can't be edited.
            return Ref_TEM_ETAT_MASTERS.DataGrid.HideColumnsAsync(_editingHiddenFields, hideBy: "Field");
        }
        // When an add is started.

        if (Action.Add.Equals(args.RequestType))
        {
            ButtonSaveDisabled = false;

            // Hide columns that can't be added.
            return Ref_TEM_ETAT_MASTERS.DataGrid.HideColumnsAsync(_editingHiddenFields, hideBy: "Field");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Event triggers when DataGrid actions are completed.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public async Task OnActionCompleteAsync(ActionEventArgs<TEM_ETAT_MASTERS> args)
    {
        // When save is done.
        if (Action.Save.Equals(args.RequestType))
        {
            // Save UTD name to all child Active/Brouillon/Prototype
            await SaveUTDNameChildAsync(args.Data);

            // Save farm selection.
            await SaveFarmsAsync(args.Data);

            // Show again hidden columns.
            await Ref_TEM_ETAT_MASTERS.DataGrid.ShowColumnsAsync(_editingHiddenFields, showBy: "Field");
        }
        // When cancel is done.
        else if (Action.Cancel.Equals(args.RequestType))
        {
            // Show again hidden columns.
            await Ref_TEM_ETAT_MASTERS.DataGrid.ShowColumnsAsync(_editingHiddenFields, showBy: "Field");
        }
    }

    /// <summary>
    /// Event triggers when toolbar item is clicked.
    /// </summary>
    /// <param name="args">Click event argument.</param>
    public Task OnToolbarClickAsync(ClickEventArgs args)
    {
        // Inactivate button clicked.
        if (args.Item.Id.Equals(_toolbarInactivate.Id, StringComparison.OrdinalIgnoreCase))
        {
            if (_selectedRecord != null)
                OpenInactivateDialog();
        }
        else
        {
            return Ref_TEM_ETAT_MASTERS.OnToolbarClickAsync(args);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Event triggers when a row is selected.
    /// </summary>
    /// <param name="args">Row select event argument.</param>
    public void RowSelected(RowSelectEventArgs<TEM_ETAT_MASTERS> args)
    {
        // Get selected row data.
        _selectedRecord = args.Data;

        // If row status is active.
        if (StatusLiteral.Available.Equals(_selectedRecord.TRST_STATUTID))
        {
            // Enable inactivate button.
            _toolbarInactivate.Disabled = !AllowModify;
        }
    }

    /// <summary>
    /// Event triggers when a selected row is deselected.
    /// </summary>
    /// <param name="args">Row deselect event argument.</param>
    public void RowDeselected(RowDeselectEventArgs<TEM_ETAT_MASTERS> args)
    {
        // Clear selected row data.
        _selectedRecord = null;

        // Disable inactivate button.
        _toolbarInactivate.Disabled = true;
    }
    #endregion    

    #region TEM_ETAT_MASTERS Grid Toolbar
    /// <summary>
    /// "Inactivate" toolbar button.
    /// </summary>
    private readonly ItemModel _toolbarInactivate = new()
    {
        PrefixIcon = "e-error-treeview",
        Text = "[TRANSLATE]",
        TooltipText = "[TRANSLATE]",
        Id = "Inactivate",
        Disabled = true
    };

    /// <summary>
    /// Add "Inactivate" button to the Toolbar.
    /// </summary>
    private void AddInactivateToolbarButton()
    {
        // Translate inactivate button.
        _toolbarInactivate.Text = Trad.Keys[$"DTM:GridToolbar{_toolbarInactivate.Id}"];
        _toolbarInactivate.TooltipText = Trad.Keys[$"DTM:GridToolbar{_toolbarInactivate.Id}Tooltip"];

        // Add inactivate button to the toolbar.
        Ref_TEM_ETAT_MASTERS.ToolbarListItems.Add(_toolbarInactivate);
    }

    /// <summary>
    /// Change selected row status to inactive.
    /// </summary>
    private async Task InactivateAsync()
    {
        if (_selectedRecord is not null)
        {
            // Set status to inactive.
            _selectedRecord.TRST_STATUTID_OLD = _selectedRecord.TRST_STATUTID;
            _selectedRecord.TRST_STATUTID = StatusLiteral.Deactivated;

            var _etats = await ProxyCore.GetEnumerableAsync<TE_ETATS>($"?$filter=TEM_ETAT_MASTERID eq {_selectedRecord.TEM_ETAT_MASTERID}");
            if (_etats.Any())
            {
                foreach (var etat in _etats)
                {
                    etat.TRST_STATUTID_OLD = etat.TRST_STATUTID;

                    if (etat.TRST_STATUTID == StatusLiteral.Available)
                        etat.TRST_STATUTID = StatusLiteral.Deactivated;
                    else if (new[] { StatusLiteral.Draft, StatusLiteral.Prototype }.Contains(etat.TRST_STATUTID))
                        etat.TRST_STATUTID = StatusLiteral.Canceled;
                }
                await ProxyCore.UpdateAsync(_etats, convertToLocalDateTime: true);
            }

            // Disable inactivate button.
            _toolbarInactivate.Disabled = true;

            // Update grid data.
            await Ref_TEM_ETAT_MASTERS.DataGrid.SetRowDataAsync(_selectedRecord.TEM_ETAT_MASTERID, _selectedRecord);
        }
    }

    #endregion

    #region Inactivate dialog
    /// <summary>
    /// Is "Inactivate UTD" dialog displayed?
    /// </summary>
    private bool IsInactivateDialogDisplayed { get; set; }

    /// <summary>
    /// Open dialog used to inactivate an UTD.
    /// </summary>
    private void OpenInactivateDialog() => IsInactivateDialogDisplayed = true;

    /// <summary>
    /// Close dialog used to inactivate an UTD.
    /// </summary>
    private void CloseInactivateDialog() => IsInactivateDialogDisplayed = false;

    /// <summary>
    /// Action launched when confirm button is clicked.
    /// </summary>
    private Task InactivateDialogOnConfirm()
    {
        CloseInactivateDialog();

        return InactivateAsync();
    }
    #endregion
}
