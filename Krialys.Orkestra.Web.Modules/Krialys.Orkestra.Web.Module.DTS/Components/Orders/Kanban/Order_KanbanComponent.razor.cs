using Krialys.Data.EF.Univers;
using Syncfusion.Blazor.Kanban;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Kanban;

public partial class Order_KanbanComponent : IDisposable
{
    #region Properties
    /// <summary>
    /// List of order phases.
    /// </summary>
    private IEnumerable<TCMD_PH_PHASES> _phases { get; set; } = Enumerable.Empty<TCMD_PH_PHASES>();

    /// <summary>
    /// Selected order.
    /// </summary>
    private TCMD_COMMANDES _selectedOrder;
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Read phases from data base.
        _phases = await ProxyCore.GetEnumerableAsync<TCMD_PH_PHASES>();

        // Initialize Kanban columns.
        KanbanColumnsInit();

        // Register event when data need to be refreshed.
        OrderManagement.OrderChanged += RefreshKanbanAsync;

        // Register event when edit dialog need to be displayed (drag and drop).
        OrderManagement.OpenEditDialog += OpenEditDialogOnPhaseChange;
    }

    public void Dispose()
    {
        // Unregister events.
        OrderManagement.OrderChanged -= RefreshKanbanAsync;
        OrderManagement.OpenEditDialog -= OpenEditDialogOnPhaseChange;
    }
    #endregion

    #region Kanban
    /// <summary>
    /// Reference to the Kanban component.
    /// </summary>
    private SfKanban<TCMD_COMMANDES> KanbanReference;

    /// <summary>
    /// Refresh Kanban data.
    /// </summary>
    private async Task RefreshKanbanAsync()
        //=> await KanbanReference.AddCardAsync(new TCMD_COMMANDES());
        => await KanbanReference.RefreshAsync();

    /// <summary>
    /// Event triggered on double-clicking the Kanban cards.
    /// </summary>
    /// <param name="args">Card click event arguments.</param>
    public void CardDoubleClick(CardClickEventArgs<TCMD_COMMANDES> args)
    {
        // Open the custom editor dialog (instead of default one).
        OpenEditorDialog(order: args.Data);

        // Cancel default edit action.
        args.Cancel = true;
    }
    #endregion

    #region Kanban Columns
    /// <summary>
    /// List of the parameters for each kanban column.
    /// </summary>
    private List<KanbanColumn> _kanbanColumns = new();

    /// <summary>
    /// Build list of parameters for each kanban column.
    /// </summary>
    private void KanbanColumnsInit()
    {
        _kanbanColumns = new List<KanbanColumn>
        {
            new()
            {
                HeaderText = GetColumnHeaderText(Phases.ToAccept),
                KeyField = new() { Phases.ToAccept },
                TransitionColumns = new()
                {
                    Phases.InProgress,
                    Phases.Frost,
                    Phases.Rejected
                },
                AllowDrag = true
            },
            new()
            {
                HeaderText = GetColumnHeaderText(Phases.InProgress),
                KeyField = new() { Phases.InProgress },
                TransitionColumns = new()
                {
                    Phases.Frost,
                    Phases.Delivered,
                    Phases.Rejected
                },
                AllowDrag = true
            },
            new()
            {
                HeaderText = GetColumnHeaderText(Phases.Frost),
                KeyField = new() { Phases.Frost },
                TransitionColumns = new()
                {
                    Phases.InProgress,
                    Phases.Delivered,
                    Phases.Rejected
                },
                AllowDrag = true
            },
            new()
            {
                HeaderText = GetColumnHeaderText(Phases.Delivered),
                KeyField = new() { Phases.Delivered },
                TransitionColumns = new(),
                AllowDrag = false
            },
            new()
            {
                HeaderText = GetColumnHeaderText(Phases.Rejected),
                KeyField = new() { Phases.Rejected },
                TransitionColumns = new(),
                AllowDrag = false
            }
        };
    }

    /// <summary>
    /// Get title of the column from data base values and selected culture.
    /// </summary>
    /// <param name="phaseCode">Phase code on which the column depends.</param>
    /// <returns>Title of the column.</returns>
    private string GetColumnHeaderText(string phaseCode)
    {
        // Get phase corresponding to the code.
        var phase = _phases.FirstOrDefault(p => phaseCode.Equals(p.TCMD_PH_CODE));

        if (phase is not null)
            // Return phase label depending on culture.
            return Trad.IsCultureFr ? phase.TCMD_PH_LIB_FR : phase.TCMD_PH_LIB_EN;
        // No label found.
        return string.Empty;
    }

    /// <summary>
    /// Class describing the parameters of Kanban column.
    /// </summary>
    private class KanbanColumn
    {
        /// <summary>
        /// Column title.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Value of the key used to fill the column.
        /// </summary>
        public List<string> KeyField { get; set; }

        /// <summary>
        /// Name of the columns to which the transition is allowed.
        /// </summary>
        public List<string> TransitionColumns { get; set; }

        /// <summary>
        /// Is column drag enabled?
        /// </summary>
        public bool AllowDrag { get; set; } = true;
    }
    #endregion

    #region Kanban Swimlane
    /// <summary>
    /// List of codes for the creation modes displayed in the Kanban.
    /// Each creation modes is a swimlane.
    /// </summary>
    private readonly List<string> _kanbanCreationModeCodes = new()
    {
        CreationModes.UTD,
        CreationModes.Domain,
        CreationModes.Copy
    };

    /// <summary>
    /// Get TextField parameter for the swimlane.
    /// It depends on the selected culture.
    /// </summary>
    private string _swimlaneTextField => Trad.IsCultureFr ?
        $"{nameof(TCMD_COMMANDES.TCMD_MC_MODE_CREATION)}.{nameof(TCMD_MC_MODE_CREATIONS.TCMD_MC_LIB_FR)}"
        : $"{nameof(TCMD_COMMANDES.TCMD_MC_MODE_CREATION)}.{nameof(TCMD_MC_MODE_CREATIONS.TCMD_MC_LIB_EN)}";

    /// <summary>
    /// Event triggers before swimlane rows rendering on the page.
    /// </summary>
    /// <param name="args">Swimlane sort event arguments.</param>
    public void SwimlaneSorting(SwimlaneSortEventArgs args)
    {
        // Create a list of swimlanes.
        // It will contain swimlanes in the desired order.
        List<SwimlaneSettingsModel> orderedSwimlaneRows = new();

        // Reorder swimlanes according to the list of creation mode codes.
        foreach (var creationModeCode in _kanbanCreationModeCodes)
        {
            var swimlane = args.SwimlaneRows.FirstOrDefault(sl => sl.KeyField.Equals(creationModeCode));
            if (swimlane is not null)
            {
                orderedSwimlaneRows.Add(swimlane);
            }
        }

        // Overwrite default swimlanes with ordered swimlanes.
        args.SwimlaneRows = orderedSwimlaneRows;
    }
    #endregion

    #region Drag&Drop custom event
    /// <summary>
    /// Open edit dialog when there is a phase change and that
    /// some fields must be entered by the user.
    /// </summary>
    /// <param name="order">Order after drag and drop.</param>
    private void OpenEditDialogOnPhaseChange(TCMD_COMMANDES order)
    {
        // Open the custom editor dialog (instead of default one).
        OpenEditorDialog(order, isDragAndDrop: true);

        // Refresh UI.
        StateHasChanged();
    }
    #endregion

    #region Editor dialog
    /// <summary>
    /// Is order editor dialog displayed?
    /// </summary>
    private bool _isOrderEditorDisplayed;

    /// <summary>
    /// Is dialog opened after a drag & drop?
    /// </summary>
    private bool _isDragAndDrop;

    /// <summary>
    /// Open dialog used to edit an order.
    /// </summary>
    /// <param name="order">Edited order.</param>
    /// <param name="isDragAndDrop">Is dialog opened after a drag and drop?</param>
    private void OpenEditorDialog(TCMD_COMMANDES order, bool isDragAndDrop = false)
    {
        // Set selected order.
        _selectedOrder = order;

        _isDragAndDrop = isDragAndDrop;

        // Display order editor.
        _isOrderEditorDisplayed = true;
    }
    #endregion
}
