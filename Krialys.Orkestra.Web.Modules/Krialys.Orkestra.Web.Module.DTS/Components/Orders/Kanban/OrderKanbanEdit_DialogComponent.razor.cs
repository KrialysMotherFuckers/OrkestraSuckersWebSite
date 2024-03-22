using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Kanban;

public partial class OrderKanbanEdit_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Selected order.
    /// </summary>
    [Parameter]
    public TCMD_COMMANDES Order { get; set; }

    /// <summary>
    /// Is opened after a drag & drop?
    /// </summary>
    [Parameter]
    public bool IsDragAndDrop { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Domain of the selected order.
    /// </summary>
    private TDOM_DOMAINES _domain;

    /// <summary>
    /// Productions associated to the selected order.
    /// </summary>
    private IEnumerable<TCMD_DA_DEMANDES_ASSOCIEES> _associatedProductions { get; set; } = Enumerable.Empty<TCMD_DA_DEMANDES_ASSOCIEES>();

    /// <summary>
    /// Is information on edited order displayed?
    /// </summary>
    private bool _isInfoDisplayed;
    #endregion

    #region Blazor life cycle
    protected override async Task OnInitializedAsync()
    {
        // Get order phase code.
        string phaseCode = Order.TCMD_PH_PHASE?.TCMD_PH_CODE;

        // Set dialog header.
        // Case of an edition.
        _dialogHeader = string.Format(IsDragAndDrop
            ? Trad.Keys["DTS:ChangePhaseDialogHeader"]
            : Trad.Keys["DTS:OrderEdition"], Order.TCMD_COMMANDEID);

        // Infos on UTD/Domain is not displayed after drag and drop.
        _isInfoDisplayed = !IsDragAndDrop;

        // Order handler is editable only in phases "To Accept" and "In Progress".
        _isHandlerDropdownDisplayed = Phases.ToAccept.Equals(phaseCode)
            || Phases.InProgress.Equals(phaseCode);

        // Estimated delivery date is editable only in phases "To Accept" and "In Progress".
        _isDeliveryDatePickerDisplayed = _isHandlerDropdownDisplayed;

        // A production can be associated to the order only in phases "In Progress" and "Delivered".
        _isProductionMultiSelectDisplayed = (Phases.InProgress.Equals(phaseCode) && !IsDragAndDrop)
            || Phases.Delivered.Equals(phaseCode);

        // The selection of a notable production is required in phase "Delivered".
        _isProductionSelectionRequired = Phases.Delivered.Equals(phaseCode);

        // Save button is disabled if there are no editable fields.
        _isSaveButtonDisabled = !_isHandlerDropdownDisplayed
            && !_isDeliveryDatePickerDisplayed
            && !_isProductionMultiSelectDisplayed;

        // Read domain.
        if (Order.TDOM_DOMAINEID.HasValue)
        {
            string query = $"?$filter={nameof(TCMD_COMMANDES.TDOM_DOMAINEID)} eq {Order.TDOM_DOMAINEID}";

            _domain = (await ProxyCore.GetEnumerableAsync<TDOM_DOMAINES>(query)).FirstOrDefault();
        }
    }
    #endregion

    #region Spinner
    /// <summary>
    /// Show spinner when true.
    /// Prevents the user from performing actions when loading a page.
    /// </summary>
    private bool _isBusy;
    #endregion

    #region Dialog
    /// <summary>
    /// Title of the dialog.
    /// </summary>
    private string _dialogHeader;

    /// <summary>
    /// Is save button disabled?
    /// </summary>
    private bool _isSaveButtonDisabled;

    /// <summary>
    /// Close the dialog.
    /// </summary>
    private async Task CloseDialogAsync()
    {
        // Hide dialog.
        IsVisible = false;

        // Refresh orders.
        // Without this refresh, there is a bug where the cards of the Kanban can't be moved.
        await OrderManagement.OnOrderChangedAsync(isCacheCleared: false);

        // Update parent with new value.
        await IsVisibleChanged.InvokeAsync(IsVisible);
    }

    /// <summary>
    /// Event called when save button is clicked.
    /// </summary>
    private async Task SaveOrderAsync()
    {
        // Is the save operation successful?
        bool success = true;

        // Prevent double click.
        if (!_isBusy)
        {
            _isBusy = true;

            // Handler field is mandatory.
            if (_isHandlerDropdownDisplayed
                && Order.TRU_EXPLOITANTID is null)
            {
                success = false;

                if (_handlerDropdownReference is not null)
                {
                    await _handlerDropdownReference.ShowTooltipAsync();
                }
            }

            // Delevery date field is mandatory.
            if (_isDeliveryDatePickerDisplayed
                && Order.TCMD_DATE_PREVISIONNELLE_LIVRAISON is null)
            {
                success = false;

                if (_deleveryDateTooltipReference is not null)
                    await _deleveryDateTooltipReference.OpenAsync();
            }

            if (success && _isProductionMultiSelectDisplayed)
                success = await _productionMultiselectReference.SaveNotableProductionsAsync();

            // Save changes in database.
            if (success)
                success = await OrderManagement.PatchOrderFromKanbanAsync(Order);

            // Change phase.
            if (success && IsDragAndDrop)
            {
                // Change phase depending on the new phase value (set by drag and drop).
                switch (Order?.TCMD_PH_PHASE?.TCMD_PH_CODE)
                {
                    case Phases.InProgress:
                        success = await OrderManagement.StartProcessingOrderAsync(Order);
                        break;

                    case Phases.Delivered:
                        success = await OrderManagement.DeliverOrderAsync(Order);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }

            _isBusy = false;

            if (success)
                await CloseDialogAsync();
        }
    }
    #endregion

    #region Order Documents Button & Dialog
    /// <summary>
    /// Additional HTML attributes applied on button to open documents dialog.
    /// </summary>
    private Dictionary<string, object> _documentsButtonHtmlAttributes
        => new Dictionary<string, object>
        {
            { "title", Trad.Keys["DTS:DocumentsTooltip"]}
        };

    /// <summary>
    /// Is order documents dialog displayed?
    /// </summary>
    private bool _isOrderDocumentsDialogDisplayed;

    /// <summary>
    /// Display dialog showing details about uploaded documents.
    /// </summary>
    private void DisplayOrderDocumentsDialog()
        // Open dialog.
        => _isOrderDocumentsDialogDisplayed = true;
    #endregion

    #region Handler DropDown
    /// <summary>
    /// Is handler dropdown displayed?
    /// </summary>
    private bool _isHandlerDropdownDisplayed;

    /// <summary>
    /// Reference to the handler dropdown component used to select the user processing the order.
    /// </summary>
    private OrderHandler_DropDownComponent _handlerDropdownReference;
    #endregion

    #region Delivery Date Picker
    /// <summary>
    /// Is delivery DatePicker displayed?
    /// </summary>
    private bool _isDeliveryDatePickerDisplayed;

    /// <summary>
    /// Reference to the error Tooltip applied on delevery DatePicker.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _deleveryDateTooltipReference;

    /// <summary>
    /// Minimum of selectable dates.
    /// </summary>
    private DateTime _minDate = DateTimeOffset.Now.Date;

    /// <summary>
    /// Date picker placeholder/title.
    /// </summary>
    private string _datePickerPlaceholder
        => $"{DataAnnotations.Display<TCMD_COMMANDES>(
            nameof(TCMD_COMMANDES.TCMD_DATE_PREVISIONNELLE_LIVRAISON))} *";

    /// <summary>
    /// Event triggered when the Calendar value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    public async void OnDeliveryDateValueChange(ChangedEventArgs<DateTime?> args)
    {
        if (_deleveryDateTooltipReference is not null
            && Order.TCMD_DATE_PREVISIONNELLE_LIVRAISON is not null)
            await _deleveryDateTooltipReference.CloseAsync();
    }
    #endregion

    #region Production multiselect
    // Used to select "notable" productions.

    /// <summary>
    /// Is production multiselect displayed?
    /// </summary>
    private bool _isProductionMultiSelectDisplayed;

    /// <summary>
    /// Reference to the multiselect component used to associate orders to notable productions.
    /// </summary>
    private NotableProductions_MultiselectComponent _productionMultiselectReference;

    /// <summary>
    /// Is it required to select at least one notable production?
    /// </summary>
    private bool _isProductionSelectionRequired;
    #endregion
}
