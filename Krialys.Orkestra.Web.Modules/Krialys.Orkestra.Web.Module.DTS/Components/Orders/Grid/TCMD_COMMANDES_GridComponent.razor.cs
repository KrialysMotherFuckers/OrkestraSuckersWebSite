using Krialys.Common.Extensions;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Grid;

public partial class TCMD_COMMANDES_GridComponent : IDisposable
{
    #region Properties
    /// <summary>
    /// List of order phases.
    /// </summary>
    private IEnumerable<TCMD_PH_PHASES> _phases { get; set; } = Enumerable.Empty<TCMD_PH_PHASES>();

    /// <summary>
    /// List of order creation modes.
    /// </summary>
    private IEnumerable<TCMD_MC_MODE_CREATIONS> _creationModes { get; set; } = Enumerable.Empty<TCMD_MC_MODE_CREATIONS>();
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        // Register event when data need to be refreshed.
        OrderManagement.OrderChanged += RefreshOrderGridAsync;
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
            // Add "Order" button to the Toolbar.
            AddOrderToolbarButton();
        }
    }

    public void Dispose()
    {
        // Unregister event.
        OrderManagement.OrderChanged -= RefreshOrderGridAsync;
    }
    #endregion

    #region Spinner
    /// <summary>
    /// Is an operation ongoing?
    /// </summary>
    private bool _isBusy;
    #endregion

    #region Datagrid
    /// <summary>
    /// Reference to the WasmDataGrid component.
    /// </summary>
    private OrkaGenericGridComponent<TCMD_COMMANDES> Ref_Grid;

    /// <summary>
    /// OData query applied to the grid.
    /// </summary>
    private const string _oDataQuery = $"?$expand={nameof(TCMD_COMMANDES.TCMD_PH_PHASE)}," +
        $"{nameof(TCMD_COMMANDES.TCMD_DOC_DOCUMENTS)},{nameof(TCMD_COMMANDES.TCMD_MC_MODE_CREATION)}," +
        $"{nameof(TCMD_COMMANDES.TE_ETAT)}($expand={nameof(TE_ETATS.TEM_ETAT_MASTER)})," +
        $"{nameof(TCMD_COMMANDES.TS_SCENARIO)},{nameof(TCMD_COMMANDES.TRU_COMMANDITAIRE)}," +
        $"{nameof(TCMD_COMMANDES.TRU_EXPLOITANT)}";

    /// <summary>
    /// Sf query used to filter and order grid.
    /// </summary>
    private readonly Query _gridQuery = new Query()
        .Sort(nameof(TCMD_COMMANDES.TCMD_DATE_CREATION), "descending")
        .AddParams(Litterals.OdataQueryParameters, _oDataQuery);

    /// <summary>
    /// Refresh order grid data.
    /// </summary>
    private Task RefreshOrderGridAsync()
        // Refresh grid data.
        => Ref_Grid.DataGrid.Refresh();

    /// <summary>
    /// Indicate if an order is editable based on its phase code.
    /// </summary>
    /// <param name="phaseCode">Phase code of an order.</param>
    /// <returns>True if edition is allowed, false otherwise.</returns>
    private bool IsOrderEditable(string phaseCode)
    {
        switch (phaseCode)
        {
            case Phases.Rejected:
            case Phases.Completed:
            case Phases.Canceled:
            case Phases.Archived:
                return false;

            default:
                return true;
        }
    }

    /// <summary>
    /// Event triggers every time a request is made to access row information, element, or data 
    /// and also before the row element is appended to the DataGrid element.
    /// </summary>
    /// <param name="args">Row data bound argument.</param>
    public void OnRowDataBound(RowDataBoundEventArgs<TCMD_COMMANDES> args)
    {
        // Hide command buttons according to the phase of the order.
        switch (args.Data?.TCMD_PH_PHASE?.TCMD_PH_CODE)
        {
            case Phases.Draft:
                // Hide "duplicate" and "close" command button.
                args.Row.AddClass(new[] { "e-remove-duplicate-command", "e-remove-close-command" });
                break;

            case Phases.ToAccept:
            case Phases.InProgress:
            case Phases.Frost:
                // Hide "close" command button.
                args.Row.AddClass(new[] { "e-remove-close-command" });
                break;

            case Phases.Rejected:
            case Phases.Completed:
            case Phases.Canceled:
            case Phases.Archived:
                // Hide "cancel" and "close" command buttons.
                args.Row.AddClass(new[] {
                    "e-remove-cancel-command", "e-remove-close-command"
                });
                break;
        }

        if (!IsOrderEditable(args.Data?.TCMD_PH_PHASE?.TCMD_PH_CODE))
            // Hide "edit" command button.
            args.Row.AddClass(new[] { "e-remove-edit-command" });

        // If there are no attached documents.
        if (!args.Data.TCMD_DOC_DOCUMENTS.Any())
        {
            // Hide "documents" command button.
            args.Row.AddClass(new[] { "e-remove-documents-command" });
        }

        // If order is not delivered.
        if (args.Data.TCMD_DATE_LIVRAISON is null)
        {
            // Hide "associations" command button.
            args.Row.AddClass(new[] { "e-remove-associations-command" });
        }
    }

    /// <summary>
    /// Event triggers when a record is double clicked.
    /// </summary>
    /// <param name="args">Record double click event arguments.</param>
    public void OnRecordDoubleClick(RecordDoubleClickEventArgs<TCMD_COMMANDES> args)
    {
        if (IsOrderEditable(args.RowData?.TCMD_PH_PHASE?.TCMD_PH_CODE))
            DisplayOrderEditorDialog(order: args.RowData);
    }
    #endregion

    #region Datagrid toolbar
    /// <summary>
    /// "Order" toolbar button.
    /// </summary>
    private readonly ItemModel _toolbarOrderButton = new()
    {
        PrefixIcon = "e-field-settings",
        Text = "[TRANSLATE]",
        TooltipText = "[TRANSLATE]",
        Id = "Order",
        CssClass = "highlighted"
    };

    /// <summary>
    /// Add "Order" button to the Toolbar.
    /// </summary>
    private void AddOrderToolbarButton()
    {
        // Translate toolbar buttons.
        _toolbarOrderButton.Text = Trad.Keys[$"DTS:{_toolbarOrderButton.Id}Button"];
        _toolbarOrderButton.TooltipText = Trad.Keys[$"DTS:{_toolbarOrderButton.Id}ButtonTooltip"];
        // Add inactivate button to the toolbar.
        Ref_Grid.ToolbarListItems.Add(_toolbarOrderButton);
    }

    /// <summary>
    /// Event triggers when toolbar item is clicked.
    /// </summary>
    /// <param name="args">Click event argument.</param>
    public async Task OnToolbarClickAsync(ClickEventArgs args)
    {
        // Prevent double click.
        if (!_isBusy)
        {
            _isBusy = true;

            // Order button clicked.
            if (args.Item.Id.Equals(_toolbarOrderButton.Id))
            {
                // New order.
                TCMD_COMMANDES order = await InitializeNewOrderAsync();

                // Open order editor dialog.
                DisplayOrderEditorDialog(order);
            }
            else
            {
                await Ref_Grid.OnToolbarClickAsync(args);
            }

            _isBusy = false;
        }
    }
    #endregion

    #region Datagrid commands
    /// <summary>
    /// Selected order.
    /// </summary>
    private TCMD_COMMANDES _selectedOrder;

    /// <summary>
    /// Ids of the commands.
    /// </summary>
    private static class CommandsIds
    {
        public const string DisplayDocuments = "DisplayDocumentsCommandId";
        public const string DisplayDetails = "DisplayDetailsCommandId";
        public const string DisplayAssociations = "DisplayAssociationsCommandId";
        public const string Edit = "EditCommandId";
        public const string Cancel = "CancelCommandId";
        public const string Duplicate = "DuplicateCommandId";
        public const string Close = "CloseCommandId";
    }

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click arguments.</param>
    private async Task CommandClickedAsync(CommandClickEventArgs<TCMD_COMMANDES> args)
    {
        // Prevent double command.
        if (!_isBusy)
        {
            // Command begin.
            _isBusy = true;

            // Launch command.
            switch (args.CommandColumn.ID)
            {
                case CommandsIds.DisplayDocuments:
                    DisplayOrderDocumentsDialog(order: args.RowData);
                    break;

                case CommandsIds.DisplayDetails:
                    DisplayOrderDetailsDialog(order: args.RowData);
                    break;

                case CommandsIds.DisplayAssociations:
                    DisplayOrderAssociationsDialog(order: args.RowData);
                    break;

                case CommandsIds.Edit:
                    DisplayOrderEditorDialog(order: args.RowData);
                    break;

                case CommandsIds.Cancel:
                    await OrderManagement.CancelOrderAsync(order: args.RowData);
                    break;

                case CommandsIds.Duplicate:
                    // Get Id of the duplicated order.
                    int duplicatedOrderId = args.RowData.TCMD_COMMANDEID;

                    // Ask confirmation before duplicating the order.
                    bool isConfirm = await DialogService.ConfirmAsync(
                        content: null,
                        title: string.Format(Trad.Keys["DTS:OrderDuplication"], duplicatedOrderId),
                        new DialogOptions()
                        {
                            ChildContent = GetDuplicationConfirmation(
                                string.Format(Trad.Keys["DTS:OrderDuplicationConfirmation"], duplicatedOrderId),
                                string.Format(Trad.Keys["DTS:OrderDuplicationConfirmationMessage"], duplicatedOrderId)
                            )
                        });

                    if (isConfirm)
                    {
                        // Duplicate order.
                        TCMD_COMMANDES order = await OrderManagement.DuplicateOrder(duplicatedOrderId);

                        // Display edit dialog.
                        DisplayOrderEditorDialog(order);
                    }

                    break;

                case CommandsIds.Close:
                    await OrderManagement.CloseOrderAsync(order: args.RowData);
                    break;
            }

            // Command complete.
            _isBusy = false;

            // Re-render component to close spinner.
            StateHasChanged();
        }
    }
    #endregion

    #region Initialize Order
    /// <summary>
    /// Initialize a new order.
    /// </summary>
    /// <returns>Initialized order.</returns>
    private async Task<TCMD_COMMANDES> InitializeNewOrderAsync()
    {
        // If phases list is empty.
        if (!_phases.Any())
        {
            // Read phases from data base.
            _phases = await ProxyCore.GetEnumerableAsync<TCMD_PH_PHASES>();
        }

        // If creation mode list is empty.
        if (!_creationModes.Any())
        {
            // Read creation modes from data base.
            _creationModes = await ProxyCore.GetEnumerableAsync<TCMD_MC_MODE_CREATIONS>();
        }

        // Get draft phase.
        var draftPhase = _phases.FirstOrDefault(p => Phases.Draft.Equals(p.TCMD_PH_CODE));
        // Get domain creation mode.
        var domainCreationMode = _creationModes.FirstOrDefault(c => CreationModes.Domain.Equals(c.TCMD_MC_CODE));

        // If base data was recovered correctly.
        if (draftPhase is not null && domainCreationMode is not null)
        {
            // Initialize new order.
            return new TCMD_COMMANDES()
            {
                TCMD_DATE_CREATION = DateExtensions.GetLocaleNow(),
                TCMD_DATE_MODIF = DateExtensions.GetLocaleNow(),
                TRU_COMMANDITAIREID = Session.GetUserId(),
                TCMD_PH_PHASEID = draftPhase.TCMD_PH_PHASEID,
                TCMD_MC_MODE_CREATIONID = domainCreationMode.TCMD_MC_MODE_CREATIONID,
                TCMD_PH_PHASE = draftPhase,
                TCMD_MC_MODE_CREATION = domainCreationMode
            };
        }

        // Order initialization failed.
        return null;
    }
    #endregion

    #region Order editor dialog
    /// <summary>
    /// Is order editor dialog displayed?
    /// </summary>
    private bool _isOrderEditorDisplayed;

    /// <summary>
    /// Display order editor dialog.
    /// </summary>
    /// <param name="order">Edited order.</param>
    private void DisplayOrderEditorDialog(TCMD_COMMANDES order = null)
    {
        // Open editor if there is an order to edit.
        if (order is not null)
        {
            _selectedOrder = order;
            _isOrderEditorDisplayed = true;
        }
    }
    #endregion

    #region Order Documents Dialog
    /// <summary>
    /// Is order documents dialog displayed?
    /// </summary>
    private bool _isOrderDocumentsDialogDisplayed;

    /// <summary>
    /// Display dialog showing details about uploaded documents.
    /// </summary>
    /// <param name="order">Selected order.</param>
    private void DisplayOrderDocumentsDialog(TCMD_COMMANDES order)
    {
        _selectedOrder = order;

        // Open dialog.
        _isOrderDocumentsDialogDisplayed = true;
    }
    #endregion

    #region Order Details Dialog
    /// <summary>
    /// Is order details dialog displayed?
    /// </summary>
    private bool _isOrderDetailsDialogDisplayed;

    /// <summary>
    /// Display dialog showing details about selected order.
    /// </summary>
    /// <param name="order">Selected order.</param>
    private void DisplayOrderDetailsDialog(TCMD_COMMANDES order)
    {
        _selectedOrder = order;

        // Open dialog.
        _isOrderDetailsDialogDisplayed = true;
    }
    #endregion

    #region Order Associations Details Dialog
    /// <summary>
    /// Is order associations detail dialog displayed?
    /// </summary>
    private bool _isOrderAssociationsDialogDisplayed;

    /// <summary>
    /// Display dialog showing details about associated productions.
    /// </summary>
    /// <param name="order">Selected order.</param>
    private void DisplayOrderAssociationsDialog(TCMD_COMMANDES order)
    {
        _selectedOrder = order;

        // Open dialog.
        _isOrderAssociationsDialogDisplayed = true;
    }
    #endregion
}
