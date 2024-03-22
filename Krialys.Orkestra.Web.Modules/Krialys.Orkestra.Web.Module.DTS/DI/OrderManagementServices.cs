using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Module.Common.DI;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.JsonPatch;
using Syncfusion.Blazor.Popups;
using System.Net;
using System.Text.Json;

namespace Krialys.Orkestra.Web.Module.DTS.DI;

public interface IOrderManagementServices
{
    public event OrderManagementServices.EventHandler OrderChanged;

    public event Action<string, int> OpenReasonSelectDialog;

    public event Action<Data.EF.Univers.TCMD_COMMANDES> OpenEditDialog;

    public void OnOpenEditDialog(Data.EF.Univers.TCMD_COMMANDES order);

    public Task OnOrderChangedAsync(bool isCacheCleared = true);

    public Task OnPhaseChangeReasonSelectedAsync(string phaseCode, int reasonId, string comment);

    public Task<bool> InsertOrderAsync(Data.EF.Univers.TCMD_COMMANDES order);

    public Task CancelOrderAsync(Data.EF.Univers.TCMD_COMMANDES order, bool isSuccessNotified = true);

    public Task<bool> StartProcessingOrderAsync(Data.EF.Univers.TCMD_COMMANDES order);

    public void FrostOrder(Data.EF.Univers.TCMD_COMMANDES order);

    public Task<bool> DeliverOrderAsync(Data.EF.Univers.TCMD_COMMANDES order);

    public void RejectOrder(Data.EF.Univers.TCMD_COMMANDES order);

    public Task CloseOrderAsync(Data.EF.Univers.TCMD_COMMANDES order);

    public Task<bool> ValidateOrderAsync(Data.EF.Univers.TCMD_COMMANDES order);

    public Task<bool> PatchOrderFromGridAsync(Data.EF.Univers.TCMD_COMMANDES order, bool isSuccessNotified = true);

    public Task<bool> PatchOrderFromKanbanAsync(Data.EF.Univers.TCMD_COMMANDES order, bool isSuccessNotified = true);

    public Task<Data.EF.Univers.TCMD_COMMANDES> DuplicateOrder(int duplicatedOrderId);
}

/// <summary>
/// Service used to manage requests related to orders (TCMD_COMMANDES).
/// </summary>
public sealed class OrderManagementServices : IOrderManagementServices
{
    #region Injected services
    /// <summary>
    /// Service used for translations.
    /// </summary>
    private readonly ILanguageContainerService _trad;

    /// <summary>
    /// Service used to communicate with the proxy.
    /// </summary>
    private readonly IHttpProxyCore _proxyCore;

    /// <summary>
    /// Service used to display Toasts.
    /// </summary>
    private readonly ISfToastServices _toast;

    /// <summary>
    /// Syncfusion service used to display predefined dialogs.
    /// </summary>
    private readonly SfDialogService _dialogService;

    private IEmailClient _iEmailClient { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OrderManagementServices"/> class.
    /// </summary>
    /// <param name="toast">Service used to display Toasts.</param>
    /// <param name="proxyCore">Service used to communicate with the proxy.</param>
    /// <param name="trad">Service used for translations.</param>
    /// <param name="dialogService">Syncfusion service used to display predefined dialogs.</param>
    public OrderManagementServices(ISfToastServices toast, IHttpProxyCore proxyCore, ILanguageContainerService trad, SfDialogService dialogService, IEmailClient iEmailClient)
    {
        _toast = toast;
        _proxyCore = proxyCore;
        _trad = trad;
        _dialogService = dialogService;
        _iEmailClient = iEmailClient;

        // Initialize built-in dialogs options.
        _confirmationDialogOptions = GetDialogOptions();
    }
    #endregion

    #region Events
    public delegate Task EventHandler();

    /// <summary>
    /// Event called when an operation on orders completes.
    /// Indicates to the user interface that it can refresh with updated data. 
    /// </summary>
    public event EventHandler OrderChanged;

    /// <summary>
    /// Used to refresh the UI when an order changed.
    /// Clear cache and raises OrderChanged event.
    /// </summary>
    /// <param name="isCacheCleared">Is the cache cleared?</param>
    public Task OnOrderChangedAsync(bool isCacheCleared = true)
    {
        if (isCacheCleared)
            // Clear order cache.
            _proxyCore.CacheRemoveEntities(typeof(Data.EF.Univers.TCMD_COMMANDES));

        // Raises OrderChanged event.
        return OrderChanged?.Invoke();
    }

    /// <summary>
    /// Event called when an edit dialog must be displayed so that the user
    /// can fill in the required fields before a phase change.
    /// </summary>
    public event Action<Data.EF.Univers.TCMD_COMMANDES> OpenEditDialog;

    /// <summary>
    /// Raise OpenEditDialog event.
    /// Open a dialog to fill in required fields for the phase change.
    /// </summary>
    /// <param name="order">Edited order.</param>
    public void OnOpenEditDialog(Data.EF.Univers.TCMD_COMMANDES order)
        => OpenEditDialog?.Invoke(order);

    /// <summary>
    /// Event called to open a dialog used to specify the reason for the phase change.
    /// </summary>
    public event Action<string, int> OpenReasonSelectDialog;

    /// <summary>
    /// Raise OpenReasonSelectDialog event.
    /// Open a dialog used to specify the reason for the phase change.
    /// </summary>
    /// <param name="targetedPhaseCode">Code of the phase in which we want to entrer.</param>
    /// <param name="orderId">Id of the order.</param>
    private void OnOpenReasonSelectDialog(string targetedPhaseCode, int orderId)
        => OpenReasonSelectDialog?.Invoke(targetedPhaseCode, orderId);
    #endregion

    #region Confirmation dialogs
    /// <summary>
    /// Options used to configure confirmation dialogs.
    /// </summary>
    private readonly DialogOptions _confirmationDialogOptions;

    /// <summary>
    /// Get options used to configure confirmation dialogs.
    /// </summary>
    /// <returns>Options to configure built-in dialogs.</returns>
    private DialogOptions GetDialogOptions()
        => new()
        {
            PrimaryButtonOptions = new DialogButtonOptions { Content = _trad.Keys["COMMON:Yes"] },
            CancelButtonOptions = new DialogButtonOptions { Content = _trad.Keys["COMMON:No"] },
            Width = "350px"
        };

    /// <summary>
    /// Open dialog to ask the user to confirm a cancel action.
    /// </summary>
    /// <param name="orderId">Id of the selected order.</param>
    /// <returns>True if the user confirmed, false otherwise.</returns>
    private Task<bool> ConfirmDeleteAsync(int orderId)
        => _dialogService.ConfirmAsync(
                _trad.Keys["DTS:OrderDeleteConfirmation"],
                string.Format($"{_trad.Keys["DTS:OrderDeleting"]}", orderId),
                _confirmationDialogOptions);

    /// <summary>
    /// Open dialog to ask the user to confirm a close action.
    /// </summary>
    /// <param name="orderId">Id of the selected order.</param>
    /// <returns>True if the user confirmed, false otherwise.</returns>
    private Task<bool> ConfirmCloseAsync(int orderId)
        => _dialogService.ConfirmAsync(
                _trad.Keys["DTS:OrderCloseConfirmation"],
                string.Format($"{_trad.Keys["DTS:OrderClosing"]}", orderId),
                _confirmationDialogOptions);
    #endregion

    #region Read order
    /// <summary>
    /// Read the value of a command in database.
    /// </summary>
    /// <param name="orderId">Id of the order to read.</param>
    private async Task<Data.EF.Univers.TCMD_COMMANDES> ReadOrderAsync(int orderId)
    {
        // Get TCMD_COMMANDES from DB,
        // expanded with TCMD_PH_PHASES, filtered by order id.
        string queryOptions = $"?$expand={nameof(Data.EF.Univers.TCMD_COMMANDES.TCMD_PH_PHASE)}" +
            $"&$filter={nameof(Data.EF.Univers.TCMD_COMMANDES.TCMD_COMMANDEID)} eq {orderId}";

        // Read selected order again.
        return (await _proxyCore.GetEnumerableAsync<Data.EF.Univers.TCMD_COMMANDES>(queryOptions, useCache: false))
            ?.FirstOrDefault();
    }
    #endregion

    #region Failures & Successes notifications
    /// <summary>
    /// Method called when an order request fails.
    /// </summary>
    /// <param name="title">Displayed title in toast.</param>
    /// <param name="message">Displayed message in toast.</param>
    private async Task OnOrderActionFailureAsync(string title, string message)
    {
        // Display error message.
        await _toast.DisplayErrorAsync(title, message);

        // Raise event to refresh data and UI.
        await OnOrderChangedAsync();
    }

    /// <summary>
    /// Method called when an order request succeeds.
    /// </summary>
    /// <param name="title">Displayed title in toast.</param>
    /// <param name="message">Displayed message in toast.</param>
    /// <param name="isToastDisplayed">Is toast displayed?</param>
    private async Task OnOrderActionSuccessAsync(string title, string message,
        bool isToastDisplayed)
    {
        if (isToastDisplayed)
            // Display success message.
            await _toast.DisplaySuccessAsync(title, message);

        // Raise event to refresh data and UI.
        await OnOrderChangedAsync();
    }
    #endregion

    #region Change order phase
    /// <summary>
    /// Method called when an order phase change fails.
    /// </summary>
    /// <param name="statusCode">Http status code returned by a phase change request.</param>
    /// <param name="errorTitle">Displayed error title.</param>
    private async Task OnChangePhaseFailedAsync(HttpStatusCode statusCode, string errorTitle)
    {
        switch (statusCode)
        {
            // If gone => if order was already deleted.
            case HttpStatusCode.Gone:
                // Notify failure.
                await OnOrderActionFailureAsync(errorTitle, _trad.Keys["DTS:OrderGone"]);
                break;

            // If conflict => if phase was no longer draft.
            case HttpStatusCode.Conflict:
                // Notify failure.
                await OnOrderActionFailureAsync(errorTitle, _trad.Keys["DTS:OrderConflict"]);
                break;

            default:
                // Notify failure.
                await OnOrderActionFailureAsync(errorTitle, _trad.Keys["COMMON:RequestFailed"]);
                break;
        }
    }

    /// <summary>
    /// Change phase of selected order.
    /// </summary>
    /// <param name="order">Order whose phase is changed.</param>
    /// <param name="phaseCode">Code of the phase in which we want to entrer.</param>
    /// <param name="reasonId">Id of the selected phase change reason.</param>
    /// <param name="comment">User comment explaining why he canceled the order.</param>
    /// <param name="isSuccessNotified">Is successful action reported?</param>
    /// <returns>True if success, false otherwise.</returns>
    private async Task<bool> ChangeOrderPhaseAsync(Data.EF.Univers.TCMD_COMMANDES order, string phaseCode,
        int reasonId = default, string comment = default,
        bool isSuccessNotified = true)
    {
        // If no phase change reason is selected.
        // Get it from database.
        if (reasonId.Equals(default))
        {
            // Read phase change reasons from data base.
            string reasonPhaseQuery = $"?$expand={nameof(Data.EF.Univers.TCMD_RP_RAISON_PHASES.TCMD_PH_PHASE)}";
            var _reasonPhases = await _proxyCore.GetEnumerableAsync<Data.EF.Univers.TCMD_RP_RAISON_PHASES>(reasonPhaseQuery);

            // Get reason for the phase we want to enter.
            var reason = _reasonPhases
                .FirstOrDefault(rp => phaseCode.Equals(rp.TCMD_PH_PHASE.TCMD_PH_CODE));

            if (reason != null)
            {
                reasonId = reason.TCMD_RP_RAISON_PHASEID;
            }
        }

        // Change phase request.
        ChangeOrderPhaseArguments args = new()
        {
            PhaseCode = phaseCode,
            ReasonId = reasonId,
            Comment = comment
        };
        var response = await _proxyCore.ChangeOrderPhase(order.TCMD_COMMANDEID, args);

        // If success.
        if (response.IsSuccessStatusCode)
        {
            if (isSuccessNotified)
            {
                // Depending of the entered phase, a toast is displayed.
                bool isToastDisplayed = Phases.Completed.Equals(phaseCode)
                    || Phases.Canceled.Equals(phaseCode);

                // Notify success.
                await OnOrderActionSuccessAsync(_trad.Keys[$"DTS:Order{phaseCode}Title"],
                    _trad.Keys[$"DTS:Order{phaseCode}Success"], isToastDisplayed);
            }

            if (Phases.Canceled.Equals(phaseCode))
                await _iEmailClient.SendAutomatedMailForOrderAsync(order.TCMD_COMMANDEID, OrderStatus.Canceled);
            else if (Phases.Rejected.Equals(phaseCode))
                await _iEmailClient.SendAutomatedMailForOrderAsync(order.TCMD_COMMANDEID, OrderStatus.Rejected);
            else if (Phases.Completed.Equals(phaseCode))
                await _iEmailClient.SendAutomatedMailForOrderAsync(order.TCMD_COMMANDEID, OrderStatus.EndOfProduction);

            // Success.
            return true;
        }

        // Handle phase change failures.
        await OnChangePhaseFailedAsync(response.StatusCode,
            _trad.Keys[$"DTS:Order{phaseCode}Title"]);

        // Failure.
        return false;
    }
    #endregion

    #region On change phase reason selected
    /// <summary>
    /// Event called when the user selects a reason for a phase change.
    /// Apply phase change (after optional user confirmation).
    /// </summary>
    /// <param name="phaseCode">Code of the phase in which we want to entrer.</param>
    /// <param name="reasonId">Id of the selected phase change reason.</param>
    /// <param name="comment">User comment explaining why he canceled the order.</param>
    public async Task OnPhaseChangeReasonSelectedAsync(string phaseCode, int reasonId,
        string comment)
    {
        // Is the phase change confirmed?
        bool isConfirmed = true;

        // If it is a cancellation, ask the user to confirm their action.
        if (Phases.Canceled.Equals(phaseCode))
            isConfirmed = await ConfirmDeleteAsync(_selectedOrder.TCMD_COMMANDEID);

        // Change phase.
        if (isConfirmed)
            await ChangeOrderPhaseAsync(_selectedOrder, phaseCode, reasonId, comment);
    }
    #endregion

    #region Create order
    /// <summary>
    /// Insert an order in database.
    /// </summary>
    /// <param name="order">Order to insert in database.</param>
    /// <returns>True if success.</returns>
    public async Task<bool> InsertOrderAsync(Data.EF.Univers.TCMD_COMMANDES order)
    {
        // Don't overwrite foreign values.
        order.TCMD_PH_PHASE = null;
        order.TCMD_MC_MODE_CREATION = null;

        // Save item in database.
        var apiResult = await _proxyCore.InsertAsync(new List<Data.EF.Univers.TCMD_COMMANDES> { order });

        // If the insertion failed.
        if (apiResult.Count < 1)
        {
            // Write failure in log.
            await _proxyCore.SetLogException(new LogException(GetType(), order, apiResult.Message));

            // Display error message.
            await _toast.DisplayErrorAsync(_trad.Keys["COMMON:Error"], _trad.Keys["COMMON:DataBaseUpdateFailed"] + apiResult.Message);

            // Failure.
            return false;
        }

        // Update order Id.
        order.TCMD_COMMANDEID = (int)apiResult.Count;

        // Success.
        return true;
    }
    #endregion

    #region Cancel order
    /// <summary>
    /// Edited order.
    /// </summary>
    private Data.EF.Univers.TCMD_COMMANDES _selectedOrder;

    /// <summary>
    /// Cancel selected order.
    /// </summary>
    /// <param name="order">Order to cancel.</param>
    /// <param name="isSuccessNotified">Is successful action reported?</param>
    public async Task CancelOrderAsync(Data.EF.Univers.TCMD_COMMANDES order, bool isSuccessNotified = true)
    {
        // If phase is draft.
        if (Phases.Draft.Equals(order.TCMD_PH_PHASE.TCMD_PH_CODE))
        {
            if (isSuccessNotified)
            {
                // Ask the user to confirm their action.
                bool isConfirm = await ConfirmDeleteAsync(order.TCMD_COMMANDEID);

                // If user didn't confirm, then exit.
                if (!isConfirm)
                {
                    return;
                }
            }

            // Try delete order.
            var response = await _proxyCore.DeleteOrder(order.TCMD_COMMANDEID);

            // If success.
            if (response.IsSuccessStatusCode)
            {
                if (isSuccessNotified)
                {
                    // Notify success.
                    await OnOrderActionSuccessAsync(_trad.Keys[$"DTS:Order{Phases.Canceled}Title"],
                        _trad.Keys[$"DTS:Order{Phases.Canceled}Success"], true);
                }

                // Task done.
                return;
            }

            // Set error message title.
            string errorTitle = _trad.Keys[$"DTS:Order{Phases.Canceled}Title"];

            switch (response.StatusCode)
            {
                // If gone => if order was already deleted.
                case HttpStatusCode.Gone:
                    // Notify failure.
                    await OnOrderActionFailureAsync(errorTitle,
                        _trad.Keys["DTS:OrderGone"]);
                    break;

                // If conflict => if phase was no longer draft.
                case HttpStatusCode.Conflict:
                    // Read selected order to verify that the phase changes.
                    order = await ReadOrderAsync(order.TCMD_COMMANDEID);

                    // If oder is not found or phase hasn't changed (still draft).
                    if (order is null ||
                        Phases.Draft.Equals(order.TCMD_PH_PHASE.TCMD_PH_CODE))
                    {
                        // Notify failure.
                        await OnOrderActionFailureAsync(errorTitle,
                            _trad.Keys["COMMON:RequestFailed"]);
                    }
                    break;

                default:
                    // Notify failure.
                    await OnOrderActionFailureAsync(errorTitle,
                        _trad.Keys["COMMON:RequestFailed"]);
                    break;
            }
        }

        // If phase isn't draft.
        if (!Phases.Draft.Equals(order?.TCMD_PH_PHASE.TCMD_PH_CODE))
        {
            // Save selected order (to use it again when dialog is closed).
            _selectedOrder = order;

            // Open dialog to select a reason for the cancellation.
            if (order != null)
            {
                OnOpenReasonSelectDialog(Phases.Canceled, order.TCMD_COMMANDEID);                
            }
        }
    }
    #endregion

    #region Begin order: Enter "In progress" phase
    /// <summary>
    /// Start processing an order.
    /// => Place order in "In progress" phase.
    /// </summary>
    /// <param name="order">Selected order.</param>
    /// <returns>True if success, false otherwise.</returns>
    public Task<bool> StartProcessingOrderAsync(Data.EF.Univers.TCMD_COMMANDES order)
        => ChangeOrderPhaseAsync(order, Phases.InProgress);
    #endregion

    #region Frost order
    /// <summary>
    /// Frost an order.
    /// => Place order in "Frost" phase.
    /// </summary>
    /// <param name="order">Selected order.</param>
    public void FrostOrder(Data.EF.Univers.TCMD_COMMANDES order)
    {
        // Save selected order (to use it again when dialog is closed)
        _selectedOrder = order;

        // Open dialog to select a reason for the phase change.
        OnOpenReasonSelectDialog(Phases.Frost, order.TCMD_COMMANDEID);
    }
    #endregion

    #region Deliver order
    /// <summary>
    /// Deliver an order.
    /// => Place order in "Delivered" phase.
    /// </summary>
    /// <param name="order">Order to deliver.</param>
    /// <returns>True if success, false otherwise.</returns>
    public Task<bool> DeliverOrderAsync(Data.EF.Univers.TCMD_COMMANDES order)
        => ChangeOrderPhaseAsync(order, Phases.Delivered);
    #endregion

    #region Reject order

    /// <summary>
    /// Reject an order.
    /// => Place order in "Rejected" phase.
    /// </summary>
    /// <param name="order">Selected order.</param>
    public void RejectOrder(Data.EF.Univers.TCMD_COMMANDES order)
    {
        // Save selected order (to use it again when dialog is closed)
        _selectedOrder = order;

        // Open dialog to select a reason for the phase change.
        OnOpenReasonSelectDialog(Phases.Rejected, order.TCMD_COMMANDEID);
    }

    #endregion

    #region Close order: Enter "completed" phase
    /// <summary>
    /// Close selected order.
    /// => Place order in "completed" phase.
    /// </summary>
    /// <param name="order">Order to close.</param>
    public async Task CloseOrderAsync(Data.EF.Univers.TCMD_COMMANDES order)
    {
        // Ask the user to confirm their action.
        bool isConfirm = await ConfirmCloseAsync(order.TCMD_COMMANDEID);

        if (isConfirm)
        {
            await ChangeOrderPhaseAsync(order, Phases.Completed);

        }
    }
    #endregion

    #region Validate order: Enter "To accept" phase
    /// <summary>
    /// Validate selected order.
    /// => Place order in "To accept" phase.
    /// </summary>
    /// <param name="order">Validated order.</param>
    /// <returns>True if success.</returns>
    public Task<bool> ValidateOrderAsync(Data.EF.Univers.TCMD_COMMANDES order)
        => ChangeOrderPhaseAsync(order, Phases.ToAccept, isSuccessNotified: false);
    #endregion

    #region Save order
    /// <summary>
    /// Order data are patched based on grid editable fields.
    /// </summary>
    /// <param name="order">Order to save.</param>
    /// <param name="isSuccessNotified">Is successful action reported?</param>
    /// <returns>True if success.</returns>
    public Task<bool> PatchOrderFromGridAsync(Data.EF.Univers.TCMD_COMMANDES order,
        bool isSuccessNotified = true)
    {
        // Prepare patch.
        var patch = new JsonPatchDocument<Data.EF.Univers.TCMD_COMMANDES>()
            .With(p =>
            {
                p.Replace(cmd => cmd.TCMD_MC_MODE_CREATIONID, order.TCMD_MC_MODE_CREATIONID);
                p.Replace(cmd => cmd.TDOM_DOMAINEID, order.TDOM_DOMAINEID);
                p.Replace(cmd => cmd.TE_ETATID, order.TE_ETATID);
                p.Replace(cmd => cmd.TS_SCENARIOID, order.TS_SCENARIOID);
                p.Replace(cmd => cmd.TCMD_COMMENTAIRE, order.TCMD_COMMENTAIRE);
                p.Replace(cmd => cmd.TCMD_DATE_LIVRAISON_SOUHAITEE, order.TCMD_DATE_LIVRAISON_SOUHAITEE);
                p.Replace(cmd => cmd.TCMD_DATE_MODIF, DateExtensions.GetUtcNow());
            });

        return PatchOrderAsync(order, patch, isSuccessNotified);
    }

    /// <summary>
    /// Order data are patched based on Kanban editable fields.
    /// </summary>
    /// <param name="order">Order to save.</param>
    /// <param name="isSuccessNotified">Is successful action reported?</param>
    /// <returns>True if success.</returns>
    public Task<bool> PatchOrderFromKanbanAsync(Data.EF.Univers.TCMD_COMMANDES order,
        bool isSuccessNotified = true)
    {
        // Prepare patch.
        var patch = new JsonPatchDocument<Data.EF.Univers.TCMD_COMMANDES>()
            .With(p =>
            {
                p.Replace(cmd => cmd.TRU_EXPLOITANTID, order.TRU_EXPLOITANTID);
                p.Replace(cmd => cmd.TCMD_DATE_PREVISIONNELLE_LIVRAISON, order.TCMD_DATE_PREVISIONNELLE_LIVRAISON);
                p.Replace(cmd => cmd.TCMD_DATE_MODIF, DateExtensions.GetUtcNow());
            });

        return PatchOrderAsync(order, patch, isSuccessNotified);
    }

    /// <summary>
    /// Patch selected order.
    /// </summary>
    /// <param name="order">Order to save.</param>
    /// <param name="patch">Json patch applied on orders.</param>
    /// <param name="isSuccessNotified">Is successful action reported?</param>
    /// <returns>True if success.</returns>
    private async Task<bool> PatchOrderAsync(Data.EF.Univers.TCMD_COMMANDES order,
        JsonPatchDocument<Data.EF.Univers.TCMD_COMMANDES> patch,
        bool isSuccessNotified = true)
    {
        // Apply patch.
        var apiResult = await _proxyCore.PatchAsync(new object[] { order.TCMD_COMMANDEID }, new[] { patch });

        // If the patch failed.
        if (!apiResult.HttpStatusCode.Equals(HttpStatusCode.OK.ToString()))
        {
            await _proxyCore.SetLogException(new LogException(GetType(), order,
                apiResult.Message));

            await _toast.DisplayErrorAsync(_trad.Keys["COMMON:Error"],
                _trad.Keys["COMMON:DataBaseUpdateFailed"]);

            return false;
        }

        if (isSuccessNotified)
        {
            // Raise event to refresh data and UI.
            await OnOrderChangedAsync();
        }

        return true;
    }
    #endregion

    #region Duplicate Order
    /// <summary>
    /// Duplicate an order:
    /// Create a new order based on an existing order.
    /// </summary>
    /// <param name="duplicatedOrderId">Id of the order to duplicate.</param>
    /// <returns>Created order or null if creation failed.</returns>
    public async Task<Data.EF.Univers.TCMD_COMMANDES> DuplicateOrder(int duplicatedOrderId)
    {
        // Call API to duplicate an order based on its original ID.
        var response = await _proxyCore.DuplicateOrder(duplicatedOrderId);

        // If duplication is successful.
        if (response.IsSuccessStatusCode)
        {
            // Get created order by deserializing response content.
            string content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Data.EF.Univers.TCMD_COMMANDES>(content);
        }
        // If duplication failed.
        else
        {
            // Get title of the error message.
            string errorTitle = string.Format(_trad.Keys["DTS:OrderDuplication"], duplicatedOrderId);

            switch (response.StatusCode)
            {
                // If gone => if order was already deleted.
                case HttpStatusCode.Gone:
                    // Notify failure.
                    await OnOrderActionFailureAsync(errorTitle, _trad.Keys["DTS:OrderGone"]);
                    break;

                // If conflict => if order is not in a phase where duplication is allowed.
                case HttpStatusCode.Conflict:
                    // Notify failure.
                    await OnOrderActionFailureAsync(errorTitle, _trad.Keys["DTS:OrderConflict"]);
                    break;

                default:
                    // Notify failure.
                    await OnOrderActionFailureAsync(errorTitle, _trad.Keys["COMMON:RequestFailed"]);
                    break;
            }

            // No order created.
            return null;
        }
    }
    #endregion
}
