using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Kanban;

/// <summary>
/// Component used to associate orders to notable productions.
/// </summary>
public partial class NotableProductions_MultiselectComponent
{
    #region Parameters
    /// <summary>
    /// Id of the selected order.
    /// </summary>
    [Parameter]
    public int OrderId { get; set; }

    /// <summary>
    /// Is the selection of a production required?
    /// </summary>
    [Parameter]
    public bool IsRequired { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Productions associated to the selected order.
    /// </summary>
    private IEnumerable<TCMD_DA_DEMANDES_ASSOCIEES> _associatedProductions { get; set; } = Enumerable.Empty<TCMD_DA_DEMANDES_ASSOCIEES>();
    #endregion

    #region Blazor life cycle
    protected override Task OnInitializedAsync()
        => InitializeMultiSelectAsync();
    #endregion

    #region Tooltip
    /// <summary>
    /// Reference to the error Tooltip applied on MultiSelect.
    /// </summary>
    private SfTooltip _tooltipReference;
    #endregion

    #region MultiSelect
    /// <summary>
    /// Reference to the multiselect object.
    /// </summary>
    private SfMultiSelect<TCMD_DA_DEMANDES_ASSOCIEES[], TCMD_DA_DEMANDES_ASSOCIEES> _multiselectReference;

    /// <summary>
    /// Array of the selected values.
    /// </summary>
    private TCMD_DA_DEMANDES_ASSOCIEES[] _multiselectValues { get; set; } = Array.Empty<TCMD_DA_DEMANDES_ASSOCIEES>();

    /// <summary>
    /// Multiselect placeholder/title.
    /// </summary>
    private string _multiselectPlaceholder;

    /// <summary>
    /// Initialize multiselect component.
    /// </summary>
    private async Task InitializeMultiSelectAsync()
    {
        // Initialize multiselect placeholder.
        _multiselectPlaceholder = Trad.Keys["DTS:NotableAssociatedProductions"];
        // A star is added if the field is required.
        if (IsRequired)
            _multiselectPlaceholder += " *";

        // Read productions associated to this order and where production is in status realized.
        string query = $"?$filter={nameof(TCMD_DA_DEMANDES_ASSOCIEES.TCMD_COMMANDEID)} eq {OrderId}" +
            $"and {nameof(TCMD_DA_DEMANDES_ASSOCIEES.TD_DEMANDE)}/{nameof(TD_DEMANDES.TRST_STATUTID)} eq '{StatusLiteral.RealizedRequest}'";
        _associatedProductions = await ProxyCore.GetEnumerableAsync<TCMD_DA_DEMANDES_ASSOCIEES>(query, useCache: false);

        // Refresh multiselect.
        // Without refresh, the selected value sometimes does not appear.
        await _multiselectReference.RefreshDataAsync();
        StateHasChanged();

        // Get selected values: i.e. get notable productions.
        _multiselectValues = _associatedProductions
            .Where(ap => 1.Equals(ap.TCMD_DA_VERSION_NOTABLE)).ToArray();

        // Refresh multiselect.
        await _multiselectReference.RefreshDataAsync();
    }

    /// <summary>
    /// Action called when a new value is selected.
    /// </summary>
    /// <param name="args">Multiselect change event arguments.</param>
    private async Task ValueChangeAsync(MultiSelectChangeEventArgs<TCMD_DA_DEMANDES_ASSOCIEES[]> args)
    {
        // Close tooltip.
        if (_tooltipReference is not null)
            await _tooltipReference.CloseAsync();

        // If no value is selected.
        if (args.Value is null)
        {
            // Reset all notable productions.
            _associatedProductions.Select(ap => { ap.TCMD_DA_VERSION_NOTABLE = 0; return ap; })
                .ToList();

            return;
        }

        // Browse through associated productions.
        foreach (var production in _associatedProductions)
        {
            // If associated production is selected.
            production.TCMD_DA_VERSION_NOTABLE = args.Value.Any(v =>
                v.TCMD_DA_DEMANDES_ASSOCIEEID.Equals(production.TCMD_DA_DEMANDES_ASSOCIEEID))
                ? 1  // associated production as notable.
                : 0; // associated production is not notable.
        }
    }
    #endregion

    #region Public methods
    /// <summary>
    /// Save productions associated with orders.
    /// </summary>
    /// <returns>True if save is successful, false otherwise.</returns>
    public async Task<bool> SaveNotableProductionsAsync()
    {
        // If the selection of a production is required
        // and if there isn't any selected production.
        if (IsRequired && !_associatedProductions.Any(da => 1.Equals(da.TCMD_DA_VERSION_NOTABLE)))
        {
            // Show error.
            if (_tooltipReference is not null)
                await _tooltipReference.OpenAsync();

            return false;
        }

        if (_associatedProductions.Any())
        {
            // Save associated productions in database.
            var apiResult = await ProxyCore.UpdateAsync(_associatedProductions);

            // If failure.
            if (apiResult.Count <= 0)
            {
                await ProxyCore.SetLogException(
                    new LogException(GetType(), _associatedProductions, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"],
                    Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                return false;
            }
        }

        // Default: Success.
        return true;
    }
    #endregion
}
