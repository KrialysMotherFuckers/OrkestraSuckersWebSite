using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.VisualBasic;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;
using System.Text;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd;

public partial class UtdVersionScenarios
{
    #region Parameters

    /// <summary>
    /// Specifies query to select data from DataSource.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    [Parameter] public TEM_ETAT_MASTERS Job { get; set; }
    [Parameter] public TE_ETATS JobVersion { get; set; }

    /// <summary>
    /// List of the selected objects in the component.
    /// </summary>
    [Parameter] public int EtatId { get; set; }
    [Parameter] public string StatutEtat { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    private OrkaGenericGridComponent<TS_SCENARIOS> Ref_TS_SCENARIOS;

    private IList<int> _scenarioIds = new List<int>();
    private IEnumerable<TBS_BATCH_SCENARIOS> _batchScenarios = Enumerable.Empty<TBS_BATCH_SCENARIOS>();
    private IEnumerable<TRS_RESSOURCE_SCENARIOS> _ressourceScenarios = Enumerable.Empty<TRS_RESSOURCE_SCENARIOS>();
    private IEnumerable<TPS_PREREQUIS_SCENARIOS> _prerequisScenarios = Enumerable.Empty<TPS_PREREQUIS_SCENARIOS>();

    private bool InfoDialogIsVisible { get; set; }
    private string ErrorMsg;

    private SfDialog InfoAccesImpossible; // gestion des acces a cmd 
    private bool InfoAccesImpossibleIsVisible { get; set; }
    private readonly string InfoAccesImpossibleMsg = string.Empty;

    private SfDialog SfGererAssociation;
    private bool ReadOnly;

    /// <summary>
    /// Give a unique id to each grid based on parent grid.
    /// </summary>
    private string GetSuffixId => EtatId.ToString();

    #region "param pour GererAssociationDisplayed"
    private string GererAssociationScenarioName;
    private int GererAssociationEtatId;
    private int GererAssociationScenarioID;
    private bool IsGererAssociationDisplayed;
    #endregion

    public string GetHeader(TS_SCENARIOS value) => value.TS_SCENARIOID == 0 ? Trad.Keys["DTM:CreateModule"] : Trad.Keys["DTM:EditModule"] + value.TS_NOM_SCENARIO;

    /// <summary>
    /// Initialize and open GererAssociation.
    /// </summary>
    /// <param name="data">scenario</param>
    private void OpenGererAssociation(TS_SCENARIOS data)
    {
        GererAssociationScenarioName = data.TS_NOM_SCENARIO;
        GererAssociationEtatId = data.TE_ETATID;
        GererAssociationScenarioID = data.TS_SCENARIOID;
        IsGererAssociationDisplayed = true;
    }

    #region Blazor life cycle
    private bool _afterDatabound;

    /// <summary>
    /// Loads / refresh batches, resources and scenarios
    /// </summary>
    /// <returns></returns>
    private async Task DataBoundHandlerAsync()
        => await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsInvokeCsharpCallback, DotNetObjectReference.Create(this), nameof(FeedScenariosAsync), 250);

    private bool _feeding;

    [JSInvokable]
    public async Task FeedScenariosAsync()
    {
        if (_feeding)
            return;

        _feeding = true;
        _scenarioIds = (await Ref_TS_SCENARIOS.DataGrid.GetCurrentViewRecordsAsync())!.Select(e => e.TS_SCENARIOID).ToList() ?? new();

        if (_scenarioIds.Any())
        {
            var filter = new StringBuilder("?$filter=");

            for (int i = 0; i < _scenarioIds.Count; i++)
            {
                filter.Append($"(TS_SCENARIOID eq {_scenarioIds[i]})");

                // Add " or " if it's not the last element
                if (i < _scenarioIds.Count - 1)
                    filter.Append(" or ");
            }

            _batchScenarios = await ProxyCore.GetEnumerableAsync<TBS_BATCH_SCENARIOS>(filter.ToString(), useCache: false);
            _ressourceScenarios = await ProxyCore.GetEnumerableAsync<TRS_RESSOURCE_SCENARIOS>(filter.ToString(), useCache: false);
            _prerequisScenarios = await ProxyCore.GetEnumerableAsync<TPS_PREREQUIS_SCENARIOS>(filter.ToString(), useCache: false);
        }

        _feeding = false;
        StateHasChanged();
    }
    #endregion

    private async Task OnActionBeginAsync(ActionEventArgs<TS_SCENARIOS> args)
    {
        // BeginEdit or Add action.
        if (Action.BeginEdit.Equals(args.RequestType) || Action.Add.Equals(args.RequestType))
        {
            // Get header of columns to hide/show based on UID.
            if (_editableColumnsHeaderText[0] is null)
                await ConvertUidToHeaderText();
            // Hide columns that can't be edited.
            await Ref_TS_SCENARIOS.DataGrid.HideColumnsAsync(_notEditableColumnsHeaderText);
            // Show columns that are editable only.
            await Ref_TS_SCENARIOS.DataGrid.ShowColumnsAsync(_editableColumnsHeaderText);

            // Add action.
            if (Action.Add.Equals(args.RequestType))
            {
                // Set default values.
                args.Data.TE_ETATID = EtatId;
                args.Data.TRST_STATUTID = StatusLiteral.Draft;
            }
        }
    }

    /// <summary>
    /// Event triggers when DataGrid actions are completed.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public async Task OnActionCompleteAsync(ActionEventArgs<TS_SCENARIOS> args)
    {
        if (Action.Save.Equals(args.RequestType) || Action.Cancel.Equals(args.RequestType))
        {
            // Hide columns that are editable only.
            await Ref_TS_SCENARIOS.DataGrid.HideColumnsAsync(_editableColumnsHeaderText);
            // Show columns that were hidden.
            await Ref_TS_SCENARIOS.DataGrid.ShowColumnsAsync(_notEditableColumnsHeaderText);
        }
    }

    /// <summary>
    /// Final confirmation for Can't save
    /// </summary>
    private void OkInfoClick()
    {
        InfoDialogIsVisible = false;    //hide external dialog
    }

    /// <summary>
    /// Info not allowed (status or authorisation granted)
    /// </summary>
    private void OkInfoAccesImpossibleClick()
    {
        InfoAccesImpossibleIsVisible = false;    //hide external dialog
    }

    private void OnCommandClicked(CommandClickEventArgs<TS_SCENARIOS> args)
    {
        if (args.CommandColumn.ID == "CommandGererAssociation")
        {
            if (JobVersion.TRST_STATUTID == StatusLiteral.Prototype && args.RowData.TRST_STATUTID == StatusLiteral.Draft)
                ReadOnly = false;
            else
                ReadOnly = args.RowData.TRST_STATUTID != StatusLiteral.Draft || JobVersion.TRST_STATUTID != StatusLiteral.Draft;

            OpenGererAssociation(args.RowData);
        }
    }

    /// <summary>
    /// Check if all batch are active for this Scenario  
    /// Used for check compliance while switch TS_SCENARIO status to Actif 
    /// </summary>
    /// <param name="TS_SCENARIOID">ID ETAT</param>
    private async Task<bool> ChkScenarioChildBatch(int TS_SCENARIOID)
    {
        var odata = $"?$expand={nameof(Data.EF.Univers.TBS_BATCH_SCENARIOS.TEB_ETAT_BATCH)}&$filter={nameof(Data.EF.Univers.TBS_BATCH_SCENARIOS.TS_SCENARIOID)} eq {TS_SCENARIOID}";

        var tbsBatchScenarios = (await ProxyCore.GetEnumerableAsync<Data.EF.Univers.TBS_BATCH_SCENARIOS>(odata)).ToList();

        return tbsBatchScenarios.All(e => e.TEB_ETAT_BATCH.TRST_STATUTID == StatusLiteral.Available);
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="Instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="Entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> Instance, object Entity) where TEntity : class, new()
    {
        if (Entity is TS_SCENARIOS tsScenario)
        {
            var scenarioId = tsScenario.TS_SCENARIOID;
            tsScenario.TS_NOM_SCENARIO = tsScenario.TS_NOM_SCENARIO.FirstCharToUpper();

            // verification existance d'au moins un bat associé
            if (scenarioId != 0 && tsScenario.TRST_STATUTID == StatusLiteral.Available)
            {
                var isValidScenario = await ChkScenarioChildBatch(scenarioId);
                var nbBatchs = _batchScenarios.Count(x => x.TS_SCENARIOID.Equals(scenarioId));

                if (isValidScenario && nbBatchs > 0)
                {
                    await Instance.DataGrid.EndEditAsync();
                    return;
                }
                else
                {
                    ErrorMsg = Trad.Keys["DTM:ErrMsgNoBat"];
                    InfoDialogIsVisible = true;
                    await Instance.DataGrid.CloseEditAsync();
                    return;
                }
            }

            await Instance.DataGrid.EndEditAsync();
        }
    }

    #region Grid hide/show columns
    /// <summary>
    /// Uids of columns to show when entering edit
    /// (and hide when exiting edit).
    /// </summary>
    private readonly string[] _editableColumnsUids = {
        "StatusColumn"
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
        "NbBatchsColumn",
        "NbResourcesColumn",
        "NbPrerequisitesColumn"
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
        for (int i = 0; i < _editableColumnsUids.Length; i++)
        {
            var col = await Ref_TS_SCENARIOS.DataGrid.GetColumnByUidAsync(_editableColumnsUids[i]);
            if (col is not null)
            {
                _editableColumnsHeaderText[i] = col.HeaderText;
            }
        }

        for (int i = 0; i < _notEditableColumnsUids.Length; i++)
        {
            var col = await Ref_TS_SCENARIOS.DataGrid.GetColumnByUidAsync(_notEditableColumnsUids[i]);
            if (col is not null)
            {
                _notEditableColumnsHeaderText[i] = col.HeaderText;
            }
        }
    }
    #endregion
}