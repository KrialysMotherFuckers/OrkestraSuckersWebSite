using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.Common.Components.ETQ;

/// <summary>
/// Component used to select a label (etiquette).
/// </summary>
public partial class Etiquette_DropDownComponent
{
    #region Parameters
    /// <summary>
    /// Id of the selected label.
    /// </summary>
    [Parameter] public int EtqId { get; set; }
    [Parameter] public EventCallback<int> EtqIdChanged { get; set; }

    /// <summary>
    /// Is the selection of a value required?
    /// </summary>
    [Parameter]
    public bool IsValueSelectionRequired { get; set; }
    #endregion

    #region Blazor life cycle
    protected override void OnInitialized()
        => InitializeDropdown();
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

    #region DropDown
    /// <summary>
    /// OData query applied to the dropdown.
    /// </summary>
    private const string _odataQueryParameters =
        $"?$select={nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID)}," +
        $"{nameof(TETQ_ETIQUETTES.TETQ_CODE)}";

    /// <summary>
    /// Sf Query applied to the data of the dropdown.
    /// </summary>
    private readonly Query _query = new Query()
        .AddParams(Litterals.OdataQueryParameters, _odataQueryParameters)
        .Sort(nameof(TETQ_ETIQUETTES.TETQ_CODE), FiltersLiterals.Ascending)
        .Take(10)
        .RequiresCount();

    /// <summary>
    /// Dropdown placeholder/title.
    /// </summary>
    private string _placeholder;

    /// <summary>
    /// Initialize the dropdown.
    /// </summary>
    private void InitializeDropdown()
    {
        // Initialize dropdown title.
        _placeholder = DataAnnotations.Display<TETQ_ETIQUETTES>(nameof(TETQ_ETIQUETTES.TETQ_CODE));
        // If the selection of a value required, add an asterix to the placeholder.
        if (IsValueSelectionRequired)
            _placeholder += "*";
    }

    /// <summary>
    /// Event triggered when the DropDown value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private async Task ValueChangeAsync(ChangeEventArgs<int, TETQ_ETIQUETTES> args)
    {
        // Close tooltip if a value is selected.
        if (_dropdownTooltipReference is not null
            && !args.Value.Equals(0))
            await _dropdownTooltipReference.CloseAsync();

        // If value changed.
        if (EtqId != args.Value)
        {
            // Update parameter.
            EtqId = args.Value;

            // Update parent with new value.
            await EtqIdChanged.InvokeAsync(EtqId);
        }
    }
    #endregion
}
