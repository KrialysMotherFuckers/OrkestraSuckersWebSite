using Krialys.Data.EF.Univers;
using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.Common.Components.Users;

/// <summary>
/// Component used to select a user among those active.
/// Customize user list through OData query parameter.
/// </summary>
public partial class User_DropDownComponent
{
    #region Parameters
    /// <summary>
    /// Id of the selected user.
    /// </summary>
    [Parameter] public string UserId { get; set; }
    [Parameter] public EventCallback<string> UserIdChanged { get; set; }

    /// <summary>
    /// Dropdown placeholder/title.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; }

    /// <summary>
    /// Is the selection of a value required?
    /// </summary>
    [Parameter]
    public bool IsValueSelectionRequired { get; set; }

    /// <summary>
    /// Facultative OData query applied on users.
    /// Default: Active user in alphabetical order.
    /// </summary>
    [Parameter]
    public string ODataQuery { get; set; } =
        $"?$filter={nameof(TRU_USERS.TRU_STATUS)} eq '{StatusLiteral.Available}' " +
        $"&$orderby={nameof(TRU_USERS.TRU_FULLNAME)} asc";

    /// <summary>
    /// Width of the dropdown.
    /// </summary>
    [Parameter]
    public string Width { get; set; } = "100%";
    #endregion

    #region Properties
    /// <summary>
    /// Selectable users.
    /// </summary>
    private IEnumerable<TRU_USERS> _users { get; set; } = Enumerable.Empty<TRU_USERS>();
    #endregion

    #region Blazor life cycle
    protected override Task OnInitializedAsync()
        => InitializeDropdownAsync();
    #endregion

    #region Tooltip
    /// <summary>
    /// Reference to the error Tooltip applied on DropDown.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _dropdownTooltipReference;

    /// <summary>
    /// Show tooltip.
    /// </summary>
    public Task ShowTooltipAsync()
    {
        if (_dropdownTooltipReference is not null)
            return _dropdownTooltipReference.OpenAsync();
        return Task.CompletedTask;
    }
    #endregion

    #region User DropDown
    /// <summary>
    /// Initialize the dropdown.
    /// Get list of available users.
    /// </summary>
    private async Task InitializeDropdownAsync()
    {
        // If placeholder is undefined, initialize it with a default value.
        if (string.IsNullOrWhiteSpace(Placeholder))
            Placeholder = DataAnnotations.Display<TRU_USERS>(nameof(TRU_USERS.TRU_FULLNAME));
        // If the selection of a value required, add an asterix to the placeholder.
        if (IsValueSelectionRequired)
            Placeholder += "*";

        _users = await ProxyCore.GetEnumerableAsync<TRU_USERS>(ODataQuery, useCache: false);
    }

    /// <summary>
    /// Event triggered when the DropDown value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private async Task ValueChangeAsync(ChangeEventArgs<string, TRU_USERS> args)
    {
        // Close tooltip if a value is selected.
        if (_dropdownTooltipReference is not null
            && args.Value is not null)
            await _dropdownTooltipReference.CloseAsync();

        // If value changed.
        if (UserId != args.Value)
        {
            // Update parameter.
            UserId = args.Value;

            // Update parent with new value.
            await UserIdChanged.InvokeAsync(UserId);
        }
    }
    #endregion
}
