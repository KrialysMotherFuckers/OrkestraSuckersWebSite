using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Module.MSO.DI;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Schedule;

namespace Krialys.Orkestra.Web.Module.MSO.Pages;

public partial class RapprochementsScheduler
{
    #region SfMultiSelect Application-Code
    /// <summary>
    /// Reference to the multiselect object.
    /// </summary>
    private SfMultiSelect<TRA_ATTENDUS[], TRA_ATTENDUS> MultiselectRef;

    /// <summary>
    /// Query applied to the multi-select object.
    /// </summary>
    private Query ApplicationSelectQuery { get; set; } = new Query().Expand(new List<string> { "TRAPL_APPLICATION" });

    /// <summary>
    /// Action called when code selection is cleared.
    /// </summary>
    /// <param name="args">Information about the mouse event.</param>
    private async Task OnApplicationCleared(MouseEventArgs args)
    {
        // Reset filters on appointments.
        Appointments.Filters.Clear();

        // Refresh scheduler content.
        await ScheduleRef.RefreshEventsAsync();
    }

    /// <summary>
    /// Action called when a new application is selected.
    /// </summary>
    /// <param name="args">Object selected.</param>
    private async Task OnApplicationChange(MultiSelectChangeEventArgs<TRA_ATTENDUS[]> args)
    {
        // Reset filters on appointments.
        Appointments.Filters.Clear();

        // Filter to apply to TRA_ATTENDUS.
        WhereFilter attendus = null;

        // Filter to apply to TTL_LOGS.
        WhereFilter logs = null;

        // Construct filters based on selected values.
        if (args.Value is not null)
        {
            foreach (var attendu in args.Value)
            {
                // Generate attendu filter.
                attendus = attendus is null
                    ? (new()
                    {
                        Field = "TRA_ATTENDU.TRA_ATTENDUID",
                        Operator = "equal",
                        value = attendu.TRA_ATTENDUID
                    })
                    : attendus.Or("TRA_ATTENDU.TRA_ATTENDUID", "equal", attendu.TRA_ATTENDUID);

                // Generate log filter.
                logs = logs is null
                    ? (new()
                    {
                        Field = "TRA_CODE",
                        Operator = "equal",
                        value = attendu.TRA_CODE
                    })
                    : logs.Or("TRA_CODE", "equal", attendu.TRA_CODE);
            }

            // Add generated filters to filter service.
            Appointments.Filters.Attendus.Add(attendus);
            Appointments.Filters.Logs.Add(logs);

            // Refresh scheduler content.
            await ScheduleRef.RefreshEventsAsync();
        }
    }
    #endregion SfMultiSelect Application-Code

    #region Rapprochements Scheduler
    /// <summary>
    /// Reference to the scheduler object.
    /// Used to call scheduler methods.
    /// </summary>
    private SfSchedule<AppointmentServices.Appointment> ScheduleRef;

    /// <summary>
    /// Get CSS style for event header.
    /// </summary>
    /// <param name="Appointment">Data from selected event.</param>
    /// <returns>Css style.</returns>
    private string GetSchedulerHeaderStyles(AppointmentServices.Appointment Appointment)
    {
        string backgroundColor = default;

        // Get background color from scheduler ressource.
        if (Appointment.ResultId != 0)
        {
            backgroundColor = Results.FirstOrDefault(item => item.Id.Equals(Appointment.ResultId))?.ResultColor;
        }
        else if (Appointment.EntityId != 0)
        {
            backgroundColor = Entities.FirstOrDefault(item => item.Id.Equals(Appointment.EntityId))?.EntityColor;
        }

        // Style = background color and white characters
        return !string.IsNullOrEmpty(backgroundColor) ? $"background:{backgroundColor}; color: white;" : string.Empty;
    }

    /// <summary>
    /// Get event duration in text format.
    /// </summary>
    /// <param name="Appointment">Data from selected event.</param>
    /// <returns>Duration text.</returns>
    private static string GetDurationText(AppointmentServices.Appointment Appointment)
    {
        return $"{Appointment.StartTime:dddd dd MMMM yyyy} ({Appointment.StartTime:hh:mm tt}-{Appointment.RealEndTime:hh:mm tt})";
    }
    #endregion Rapprochement scheduler

    #region Rapprochements Scheduler Ressources
    /// <summary>
    /// Resources table used for scheduler grouping.
    /// </summary>
    private static readonly string[] Resources = { "Entities" };

    /// <summary>
    /// Result ressource, differenciate logs by results.
    /// </summary>
    private static readonly IList<AppointmentServices.Ressource> Results = new List<AppointmentServices.Ressource>
    {
        new() { ResultText = "OK", Id = 1, ResultColor = "#009E73" },
        new() { ResultText = "KO", Id = 2, ResultColor = "#D55E00" },
        new() { ResultText = "NA", Id = 3, ResultColor = "#56B4E9" },
        new() { ResultText = "", Id = 5, ResultColor = "#56B4E9" }
    };

    /// <summary>
    /// Entity ressource, differenciate data by entities (attendus or logs).
    /// </summary>
    private IList<AppointmentServices.Ressource> Entities => new List<AppointmentServices.Ressource>
    {
        new() {
            EntityText = Trad.Keys["MSO:Attendus"],
            Id = 1,
            EntityGroupId = 1,
            EntityColor = "#334193"
        },
        new() {
            EntityText = Trad.Keys["MSO:Performed"],
            Id = 2,
            EntityGroupId = 2,
            EntityColor = "#334193"
        },
    };
    #endregion

    #region Rapprochements Scheduler Events
    /// <summary>
    /// Handler called when the events are double clicked.
    /// </summary>
    /// <param name="args">Click event containing selected event data.</param>
    public async Task OnEventDoubleClick(EventClickArgs<AppointmentServices.Appointment> args)
    {
        // Cancel double click default action.
        args.Cancel = true;

        // Set selected appointment in a dictionary.
        // This dictionary will be displayed in "event details" dialog.
        SelectedAppointmentDico = args.Event.AsText(DataAnnotations);

        // Open "event details" dialog.
        OpenDialog();

        // Close scheduler quick info pop-up.
        // Sometimes the quick info templates opens after the double click event.
        // The quick info pop-up isn't closed in this case.
        await ScheduleRef.CloseQuickInfoPopupAsync();
    }

    /// <summary>
    /// Handler called when any Scheduler action failed to achieve the desired results.
    /// </summary>
    /// <param name="args">Error details and its cause.</param>
    public void OnActionFailure(ActionEventArgs<AppointmentServices.Appointment> args)
    {
        /* Retrieve error message. */
        string ErrorDetails = args.Error.Message;

        /* Limit error message length. */
        if (ErrorDetails.Length > 4096)
            ErrorDetails = ErrorDetails[..4096] + "...";

        /* Display error in a toast. */
        Toast.DisplayErrorAsync("Scheduler error", ErrorDetails);
    }
    #endregion

    #region Event details dialog
    /// <summary>
    /// Selected appointment as a dictionary.
    /// Key = property display name.
    /// </summary>
    private Dictionary<string, string> SelectedAppointmentDico { get; set; }

    /// <summary>
    /// Show or hide "event details" dialog.
    /// </summary>
    private bool IsDialogVisible { get; set; }

    /// <summary>
    /// Open "event details" dialog .
    /// </summary>
    private void OpenDialog()
    {
        IsDialogVisible = true;
    }

    /// <summary>
    /// Close "event details" dialog.
    /// </summary>
    private void CloseDialog()
    {
        // Reset dico.
        SelectedAppointmentDico = null;

        // Hide dialog.
        IsDialogVisible = false;
    }
    #endregion
}
