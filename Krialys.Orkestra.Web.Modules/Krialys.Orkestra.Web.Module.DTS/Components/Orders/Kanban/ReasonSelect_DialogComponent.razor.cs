using Krialys.Data.EF.Univers;
using Krialys.Common.Literals;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Kanban;

/// <summary>
/// Dialog used to specify the reason for the phase change (of an order).
/// </summary>
public partial class ReasonSelect_DialogComponent : IDisposable
{
    #region Properties
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    private bool _isVisible;

    /// <summary>
    /// Code of the targeted phase.
    /// </summary>
    private string _targetedPhaseCode;
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override void OnInitialized()
    {
        // Register event which displays the dialog.
        OrderManagement.OpenReasonSelectDialog += OpenDialog;
    }

    public void Dispose()
    {
        // Unregister event.
        OrderManagement.OpenReasonSelectDialog -= OpenDialog;
    }
    #endregion

    #region Reason dropdown
    /// <summary>
    /// Id of the selected phase change reason.
    /// </summary>
    private int _selectedReasonId;

    /// <summary>
    /// Query applied on the data of the dropdown.
    /// </summary>
    private Query _dropdownQuery;

    /// <summary>
    /// Field (of database) displayed for each value.
    /// </summary>
    private string _dropdownTextField;

    /// <summary>
    /// Dropdown error tooltip.
    /// </summary>
    private SfTooltip _reasonTooltipObj;

    /// <summary>
    /// Initialize dropdown component used to select a phase change reason.
    /// </summary>
    private void InitializeDropdown()
    {
        // Initialize dropdown query.
        _dropdownQuery = new Query()
            .Where($"{nameof(TCMD_RP_RAISON_PHASES.TCMD_PH_PHASE)}.{nameof(TCMD_PH_PHASES.TCMD_PH_CODE)}",
                FiltersLiterals.Equal, _targetedPhaseCode);

        // Initialize dropdown text field.
        _dropdownTextField = Trad.IsCultureFr ?
            nameof(TCMD_RP_RAISON_PHASES.TCMD_RP_LIB_FR) :
            nameof(TCMD_RP_RAISON_PHASES.TCMD_RP_LIB_EN);
    }
    #endregion

    #region Comment TextBox
    /// <summary>
    /// User comment describing the change of phase.
    /// </summary>
    public string _comment;
    #endregion

    #region Dialog
    /// <summary>
    /// Title of the dialog.
    /// </summary>
    private string _dialogHeader;

    /// <summary>
    /// If a button is clicked, prevent double click.
    /// </summary>
    private bool _isButtonClicked;

    /// <summary>
    /// Close the dialog.
    /// </summary>
    private void CloseDialog()
    {
        // Hide dialog.
        _isVisible = false;

        StateHasChanged();
    }

    /// <summary>
    /// Open the dialog.
    /// </summary>
    /// <param name="targetedPhaseCode">Code of the phase in which we want to entrer.</param>
    /// <param name="orderId">Id of the order.</param>
    private void OpenDialog(string targetedPhaseCode, int orderId)
    {
        // Set dialog parameters.
        _targetedPhaseCode = targetedPhaseCode;

        // Set dialog header.
        _dialogHeader = string.Format(Trad.Keys["DTS:ChangePhaseDialogHeader"], orderId);

        // Reset selected reason and comment.
        _selectedReasonId = default;
        _comment = default;

        InitializeDropdown();

        // Show dialog.
        _isVisible = true;

        StateHasChanged();
    }

    /// <summary>
    /// Event triggers when "validate" button is clicked.
    /// Send API request to change the phase and then close the dialog.
    /// </summary>
    private async Task ChangePhaseValidateAsync()
    {
        // Are data valid?
        bool AreDataValid = true;

        // Prevent double click.
        if (!_isButtonClicked)
        {
            _isButtonClicked = true;

            // If no reason is selected.
            if (_selectedReasonId.Equals(default))
            {
                // Open tooltip.
                await _reasonTooltipObj.OpenAsync();

                // Data are invalid.
                AreDataValid = false;
            }
            else
            {
                // Close tooltip.
                await _reasonTooltipObj.CloseAsync();
            }

            // If data are valid.
            if (AreDataValid)
            {
                // Change order phase.
                await OrderManagement.OnPhaseChangeReasonSelectedAsync(_targetedPhaseCode,
                    _selectedReasonId, _comment);

                // Close dialog.
                CloseDialog();
            }

            _isButtonClicked = false;
        }
    }
    #endregion
}
