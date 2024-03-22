using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTF.Components.UTD.NewProduction;

public partial class ProdSchedulingComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Is selected UTD a prototype?
    /// </summary>
    [Parameter]
    public bool IsPrototype { get; set; }

    /// <summary>
    /// Callback triggered when a deferred production is validated.
    /// Passed parameters: Desired execution date.
    /// </summary>
    [Parameter] public EventCallback<DateTime?> LaunchDeferredProduction { get; set; }

    /// <summary>
    /// Callback triggered when a recurrent production is validated.
    /// Passed parameters: CRON, start date, end date.
    /// </summary>
    [Parameter] public EventCallback<(string, DateTime, DateTime?)> LaunchRecurrentProduction { get; set; }
    #endregion

    #region Dialog
    /// <summary>
    /// True if a button of the table is clicked, prevent double click.
    /// </summary>
    private bool _isButtonClicked;

    /// <summary>
    /// Close the dialog.
    /// </summary>
    private Task CloseDialogAsync()
    {
        // Close catalog.
        IsVisible = false;

        // Update parent with new value.
        return IsVisibleChanged.InvokeAsync(IsVisible);
    }

    /// <summary>
    /// Event triggers when "validate" button is clicked.
    /// </summary>
    private async Task ProductionValidateAsync()
    {
        // Prevent double click.
        if (!_isButtonClicked)
        {
            _isButtonClicked = true;

            // Selected scheduling
            switch (SelectedSchedulingMode)
            {
                // Launch at selected date.
                case SchedulingMode.Deferred:
                    // Verify if date is selected.
                    if (await IsScheduledDateSelected())
                    {
                        // Triggers the parent component to launch production.
                        await LaunchDeferredProduction.InvokeAsync(_executionDate);
                    }
                    break;

                case SchedulingMode.Recurrent:
                    // Verify if CRON is valid.
                    if (_isCronValid)
                    {
                        // Triggers the parent component to launch production.
                        await LaunchRecurrentProduction.InvokeAsync((_cron, _cronStartDate, _cronEndDate));
                    }
                    break;
            }

            _isButtonClicked = false;
        }
    }
    #endregion

    #region Radio button
    /// <summary>
    /// Name of the radio button (to select scheduling type).
    /// </summary>
    private const string SchedulerRbName = "SchedulerRadioButton";

    /// <summary>
    /// Scheduling modes.
    /// </summary>
    private enum SchedulingMode
    {
        Deferred,
        Recurrent
    }

    /// <summary>
    /// Selected scheduling mode.
    /// </summary>
    private SchedulingMode SelectedSchedulingMode = SchedulingMode.Deferred;
    #endregion

    #region DateTime picker
    /// <summary>
    /// Execution date picked.
    /// </summary>
    private DateTime? _executionDate;

    /// <summary>
    /// Reference to the error Tooltip applied on "scheduled" DateTimePicker.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _refScheduledDateTooltip;

    /// <summary>
    /// Checks if scheduled date is selected.
    /// </summary>
    /// <returns>True if date is selected, false otherwise.</returns>
    private async Task<bool> IsScheduledDateSelected()
    {
        // Schedule date not selected.
        if (_executionDate is null || _executionDate == default)
        {
            if (_refScheduledDateTooltip is not null)
                await _refScheduledDateTooltip.OpenAsync();

            return false;
        }

        // Schedule date selected.
        return true;
    }

    /// <summary>
    /// Event triggers when the scheduled date is changed.
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private Task OnScheduledDateChangeAsync(ChangedEventArgs<DateTime?> args)
        // Close error tooltip.
        => _refScheduledDateTooltip.CloseAsync();
    #endregion

    #region CRON Manager
    /// <summary>
    /// CRON schedule expression.
    /// </summary>
    private string _cron;

    /// <summary>
    /// Is the CRON schedule expression correct?
    /// </summary>
    private bool _isCronValid;

    /// <summary>
    /// Schedule start date.
    /// </summary>
    private DateTime _cronStartDate;

    /// <summary>
    /// Schedule end date.
    /// </summary>
    private DateTime? _cronEndDate;
    #endregion
}
