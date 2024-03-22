using Krialys.Common.Enums;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Kanban;

/// <summary>
/// Used to select the user handling/processing the order (Exploitant in french).
/// </summary>
public partial class OrderHandler_DropDownComponent
{
    #region Parameters
    /// <summary>
    /// Id of the user handling the order.
    /// </summary>
    [Parameter] public string HandlerId { get; set; }
    [Parameter] public EventCallback<string> HandlerIdChanged { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Users allowed to process the order.
    /// </summary>
    private IEnumerable<TRU_USERS> _handlers { get; set; } = Enumerable.Empty<TRU_USERS>();
    #endregion

    #region Blazor life cycle
    protected override Task OnInitializedAsync()
        => InitializeHandlerDropdownAsync();
    #endregion

    #region Tooltip
    /// <summary>
    /// Reference to the error Tooltip applied on handler DropDown.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _handlerDropdownTooltipReference;

    /// <summary>
    /// Show tooltip.
    /// </summary>
    public Task ShowTooltipAsync()
    {
        if (_handlerDropdownTooltipReference is not null)
            return _handlerDropdownTooltipReference.OpenAsync();
        return Task.CompletedTask;
    }
    #endregion

    #region Handler DropDown
    /// <summary>
    /// Handler dropdown placeholder/title.
    /// </summary>
    private string _dropdownPlaceholder;

    /// <summary>
    /// Initialize the dropdown.
    /// Get list of available users for the handler dropdown.
    /// </summary>
    private async Task InitializeHandlerDropdownAsync()
    {
        // Initialize placeholder.
        _dropdownPlaceholder = $"{DataAnnotations.Display<TCMD_COMMANDES>(nameof(TCMD_COMMANDES.TRU_EXPLOITANTID))} *";

        // Get value of the DataDriven role.
        int dataDrivenClaimValue = (int)RolesEnums.RolesValues.DataDriven;

        // Read users allowed to process the order :
        //  - Filtered on active user,
        //      - with claim value equal to DataDriven,
        //      - with active user-claims,
        //      - with client application "DTF",
        //      - with claim of type "Role".
        //  - Sorted alphabetically.
        string query = $"?$filter={nameof(TRU_USERS.TRU_STATUS)} eq '{StatusLiteral.Available}' " +
            $"and {nameof(TRU_USERS.TRUCL_USERS_CLAIMS)}/any(TRUCL: " +
                $"TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRUCL_CLAIM_VALUE)} eq '{dataDrivenClaimValue}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRUCL_STATUS)} eq '{StatusLiteral.Available}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRCLI_CLIENTAPPLICATION)}/{nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_LABEL)} eq '{Litterals.ApplicationDTF}' " +
                $"and TRUCL/{nameof(TRUCL_USERS_CLAIMS.TRCL_CLAIM)}/{nameof(TRCL_CLAIMS.TRCL_CLAIM_NAME)} eq '{ClaimsLiterals.Role}')" +
            $"&$orderby={nameof(TRU_USERS.TRU_FULLNAME)} asc";
        _handlers = await ProxyCore.GetEnumerableAsync<TRU_USERS>(query, useCache: false);
    }

    /// <summary>
    /// Event triggered when the DropDown value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private async Task HandlerValueChangeAsync(ChangeEventArgs<string, TRU_USERS> args)
    {
        if (_handlerDropdownTooltipReference is not null
            && args.Value is not null)
            await _handlerDropdownTooltipReference.CloseAsync();

        // If value changed.
        if (HandlerId != args.Value)
        {
            // Update parameter.
            HandlerId = args.Value;

            // Update parent with new value.
            await HandlerIdChanged.InvokeAsync(HandlerId);
        }
    }
    #endregion
}
