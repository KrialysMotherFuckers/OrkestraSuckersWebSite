using Krialys.Common.Literals;
using Krialys.Orkestra.Web.Module.Common.DI;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.ADM.Components.Claims;

public partial class SelectClaimComponent
{
    #region Parameters
    /// <summary>
    /// Data of the dropdown component.
    /// </summary>
    [Parameter] public IEnumerable<SfGridColumnParameterServices.ForeignValue> DataSource { get; set; }

    private IEnumerable<SfGridColumnParameterServices.ForeignValue> _dataSource;

    /// <summary>
    /// Id of the selected claim.
    /// </summary>
    [Parameter] public int SelectedClaimId { get; set; }

    /// <summary>
    /// EventCallback used to pass SelectedClaimId to parent component.
    /// </summary>
    [Parameter] public EventCallback<int> SelectedClaimIdChanged { get; set; }

    /// <summary>
    /// Value of the claim.
    /// </summary>
    [Parameter] public string ClaimValue { get; set; }

    /// <summary>
    /// EventCallback used to pass the claim value to the parent component.
    /// </summary>
    [Parameter] public EventCallback<string> ClaimValueChanged { get; set; }
    #endregion

    /// <summary>
    /// We only keep Role, ignoring TokenLifetime since it can be considered private or internal, mainly used by Krialys.Etl 'user'
    /// </summary>
    protected override void OnInitialized()
        => _dataSource = DataSource.Where(e => e.Label.Equals(ClaimsLiterals.Role, StringComparison.OrdinalIgnoreCase));

    #region DropdownList events
    /// <summary>
    /// Action called when a new value is selected.
    /// </summary>
    /// <param name="args">Object selected.</param>
    private async Task ValueChange(ChangeEventArgs<int, SfGridColumnParameterServices.ForeignValue> args)
    {
        // Pass the new value to the parent component.
        await SelectedClaimIdChanged.InvokeAsync(SelectedClaimId);

        // Reset claim value.
        ClaimValue = string.Empty;

        await ClaimValueChanged.InvokeAsync(ClaimValue);
    }
    #endregion
}
