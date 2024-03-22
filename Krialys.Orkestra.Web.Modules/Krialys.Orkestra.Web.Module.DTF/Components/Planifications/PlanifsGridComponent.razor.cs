using Cronos;
using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.DTF.Components.Planifications;

public partial class PlanifsGridComponent
{
    #region Parameters
    /// <summary>
    /// Query used to select data from DataSource.
    /// </summary>
    [Parameter] public Query Query { get; set; }

    // pour déterminer si droit est donné sur les enregistrements d accéder aux boutons, dupliquer, annuler,....
    [Parameter] public bool AllowInteract { get; set; } = false;

    /// <summary>
    /// Id of expanded row in parent grid.
    /// </summary>
    [Parameter]
    public int ExpandedRowId { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Data of the planification being modified or duplicated.
    /// </summary>
    private TPF_PLANIFS _editedPlanif = new();
    #endregion

    #region Datagrid
    private OrkaGenericGridComponent<VPD_PLANIF_DETAILS> Ref_Grid;

    /// <summary>
    /// Event triggers every time a request is made to access row information, element, or data 
    /// and also before the row element is appended to the DataGrid element.
    /// </summary>
    /// <param name="args">Row data bound argument.</param>
    public void OnRowDataBound(RowDataBoundEventArgs<VPD_PLANIF_DETAILS> args)
    {
        if (!AllowInteract) // on n'autorise pas d'action sur les boutons  
        {
            args.Row.AddClass(new[] { "e-remove-deactivate-command e-remove-modify-command e-remove-duplicate-command" });
        }
        else // on régit les accès aux Btn selon les données
        {
            // If the planification is deactivated.
            if (!StatusLiteral.Available.Equals(args.Data.PLANIF_STATUTID))
            {
                // Hide "deactivate" and "modify" command buttons.
                args.Row.AddClass(new[] { "e-remove-deactivate-command e-remove-modify-command" });
            }
        }

        // If there is no resources.
        if (0.Equals(args.Data.NB_RESSOURCES))
        {
            // Hide "open resources" command button.
            args.Row.AddClass(new[] { "e-remove-open-resources-command" });
        }
    }

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click argument.</param>
    private void CommandClicked(CommandClickEventArgs<VPD_PLANIF_DETAILS> args)
    {
        // Prepare edited planification depending on the selected command.
        // Used by Cron dialog and deactivate dialog.
        _editedPlanif = new()
        {
            TPF_PLANIFID = CommandsIds.Duplicate.Equals(args.CommandColumn.ID) ? default : args.RowData.TPF_PLANIFID,
            TPF_DATE_DEBUT = args.RowData.TPF_DATE_DEBUT,
            TPF_DATE_FIN = args.RowData.TPF_DATE_FIN,
            TRST_STATUTID = CommandsIds.Deactivate.Equals(args.CommandColumn.ID) ? StatusLiteral.Deactivated : StatusLiteral.Available,
            TPF_DEMANDE_ORIGINEID = args.RowData.TPF_DEMANDE_ORIGINEID,
            TPF_PREREQUIS_DELAI_CHK = args.RowData.TPF_PREREQUIS_DELAI_CHK,
            TPF_CRON = args.RowData.TPF_CRON,
            TPF_TIMEZONE_INFOID = DateExtensions.GetLocalTimeZoneId,
            TRU_DECLARANTID = Session.GetUserId()
        };

        // Launch command.
        switch (args.CommandColumn.ID)
        {
            case CommandsIds.DisplayRessources:
                // DemandeId is used by ressources dialog.
                _selectedDemandeId = args.RowData.TD_DEMANDEID;

                OpenResourcesDialog();
                break;

            case CommandsIds.Modify:
            case CommandsIds.Duplicate:
                OpenCronDialog();
                break;

            case CommandsIds.Deactivate:
                OpenInactivateDialog();
                break;
        }
    }
    #endregion

    #region Grid commands
    /// <summary>
    /// Ids of the commands.
    /// </summary>
    private static class CommandsIds
    {
        public const string DisplayRessources = "DisplayRessourcesCommandId";
        public const string Modify = "ModifyCommandId";
        public const string Duplicate = "DuplicateCommandId";
        public const string Deactivate = "DeactivateCommandId";
    }

    /// <summary>
    /// Id of the selected TD_DEMANDES.
    /// </summary>
    private int _selectedDemandeId;

    /// <summary>
    /// Cancel "demandes" not launched yet.
    /// </summary>
    /// <param name="cancelationStatus">Status to apply on the canceled "demande".</param>
    /// <returns>True if success, false otherwise.</returns>
    private async Task<bool> CancelDemandesAsync(string cancelationStatus)
    {
        // Get TD_DEMANDES
        // where TPF_PLANIF_ORIGINEID is edited planif ID
        // and status is "DC" or StatusLiteral.ScheduledRequest.
        var filter = $"{nameof(TD_DEMANDES.TPF_PLANIF_ORIGINEID)} eq {_editedPlanif.TPF_PLANIFID} and ({nameof(TD_DEMANDES.TRST_STATUTID)} eq '{StatusLiteral.CreatedRequestAndWaitForExecution}' or {nameof(TD_DEMANDES.TRST_STATUTID)} eq 'DP')";
        var demandes = await ProxyCore.GetEnumerableAsync<TD_DEMANDES>($"?$filter={filter}", useCache: false);

        var tdDemandes = demandes.ToList();

        if (!tdDemandes.Any())
            return true;

        // Cancel TD_DEMANDES.
        foreach (var demande in tdDemandes)
        {
            demande.TRST_STATUTID = cancelationStatus;
            demande.TD_COMMENTAIRE_GESTIONNAIRE = "Planification annulée";
        }

        var apiResult = await ProxyCore.UpdateAsync(tdDemandes, convertToLocalDateTime: false);

        if (!apiResult.Count.Equals(Litterals.NoDataRow))
            return true;

        await ProxyCore.SetLogException(new LogException(GetType(), demandes, apiResult.Message));
        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTF:ScheduleDeactivateFailed"]);

        // Failure.
        return false;

        // Success.
    }

    /// <summary>
    /// Refresh grid data.
    /// Used after update.
    /// </summary>
    private Task RefreshGridDataAsync()
    {
        // Clear caches.
        ProxyCore.CacheRemoveEntities(typeof(VPD_PLANIF_DETAILS));

        // Refresh grid data.
        return Ref_Grid.DataGrid.Refresh();
    }

    /// <summary>
    /// Change selected row status to inactive.
    /// </summary>
    /// <returns>True if success, false otherwise.</returns>
    private async Task<bool> InactivateAsync()
    {
        if (_editedPlanif is not null && _editedPlanif.TPF_PLANIFID > 0)
        {
            /***** Step 1: Cancel requests already scheduled. *****/
            if (!await CancelDemandesAsync(StatusLiteral.CanceledRequest))
                // Failure.
                return false;

            /***** Step 2: Deactivate scheduling. *****/
            // Set status to inactive.
            _editedPlanif.TRST_STATUTID = StatusLiteral.Deactivated;

            // Update dates
            _editedPlanif.TPF_DATE_DEBUT = _editedPlanif.TPF_DATE_DEBUT.ToUniversalTime();
            _editedPlanif.TPF_DATE_FIN = _editedPlanif.TPF_DATE_FIN?.ToUniversalTime() ?? _editedPlanif.TPF_DATE_FIN;

            // Update grid data.
            var apiResult = await ProxyCore.UpdateAsync(new List<TPF_PLANIFS> { _editedPlanif }, convertToLocalDateTime: true);

            // If the update failed.
            if (apiResult.Count.Equals(Litterals.NoDataRow))
            {
                await ProxyCore.SetLogException(new LogException(GetType(), _editedPlanif, apiResult.Message));

                await Toast.DisplayErrorAsync(Trad.Keys["DTF:ScheduleDeactivation"], Trad.Keys["DTF:ScheduleDeactivateFailed"]);

                // Failure.
                return false;
            }

            /***** Step 3: Refresh grid data. *****/
            await RefreshGridDataAsync();
        }

        // Success.
        await Toast.DisplaySuccessAsync(Trad.Keys["DTF:ScheduleDeactivation"], Trad.Keys["DTF:ScheduleDeactivateSucceeded"]);

        return true;
    }

    /// <summary>
    /// Save modified row.
    /// </summary>
    private async Task<bool> ModifyAsync()
    {
        /***** Step 1: Stop requests already scheduled. *****/
        if (!await CancelDemandesAsync(StatusLiteral.PlanningCancelled))
            // Failure.
            return false;

        /***** Step 2: Update scheduling. *****/

        // Update dates
        _editedPlanif.TPF_DATE_DEBUT = _editedPlanif.TPF_DATE_DEBUT.ToUniversalTime();
        _editedPlanif.TPF_DATE_FIN = _editedPlanif.TPF_DATE_FIN?.ToUniversalTime() ?? _editedPlanif.TPF_DATE_FIN;

        // Update grid data.
        var apiResult = await ProxyCore.UpdateAsync(new List<TPF_PLANIFS> { _editedPlanif }, convertToLocalDateTime: true);

        // If the update failed.
        if (apiResult.Count.Equals(Litterals.NoDataRow))
        {
            await ProxyCore.SetLogException(new LogException(GetType(), _editedPlanif, apiResult.Message));

            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTF:ScheduleModifyFailed"]);

            // Failure.
            return false;
        }

        // Success.
        return true;
    }
    #endregion

    #region Grid Next cron occurence column
    /// <summary>
    /// Get next occurence from a Cron.
    /// </summary>
    /// <param name="planifDetails">Displayed item.</param>
    /// <returns>Date of next occurence as string.</returns>
    private string GetNextOccurence(VPD_PLANIF_DETAILS planifDetails)
    {
        // Start date is the maximum of scheduling start date and now.
        var startDate = planifDetails.TPF_DATE_DEBUT < DateTimeOffset.Now ? DateTimeOffset.Now : planifDetails.TPF_DATE_DEBUT;

        // Get Cron next occurence.
        var nextOccurence = CronExpression.Parse(planifDetails.TPF_CRON).GetNextOccurrence(startDate, TimeZoneInfo.Local);

        // Check if next occurence exist and that it doesn't exceed scheduling end date.
        if (nextOccurence is not null && (planifDetails.TPF_DATE_FIN is null || nextOccurence < planifDetails.TPF_DATE_FIN))
        {
            return nextOccurence.Value.ToString("g");
        }

        // No expected occurrence.
        return string.Empty;
    }
    #endregion

    #region Resources dialog
    /// <summary>
    /// Is the dialog used to display resources displayed ?
    /// </summary>
    private bool IsResourcesDialogDisplayed { get; set; }

    /// <summary>
    /// Open dialog used to display resources.
    /// </summary>
    private void OpenResourcesDialog() => IsResourcesDialogDisplayed = true;
    #endregion

    #region Cron dialog
    /// <summary>
    /// Is the CRON schedule expressions correct?
    /// </summary>
    private bool _isCronValid;

    /// <summary>
    /// Is "Cron" dialog visible?
    /// </summary>
    private bool IsCronDialogVisible { get; set; }

    /// <summary>
    /// Open dialog.
    /// </summary>
    private void OpenCronDialog() => IsCronDialogVisible = true;

    /// <summary>
    /// Close dialog.
    /// </summary>
    private void CloseCronDialog() => IsCronDialogVisible = false;

    private string GetCronDialogHeader() =>
        (_editedPlanif is not null && _editedPlanif.TPF_PLANIFID > 0) ? Trad.Keys["DTF:ScheduleModifyTitle"] : Trad.Keys["DTF:ScheduleDuplicateTitle"];

    /// <summary>
    /// Action launched when save button is clicked.
    /// </summary>
    private async Task SaveCronDialog()
    {
        // Is the save successful.
        bool isSaveSuccess = true;

        if (_isCronValid && _editedPlanif is not null)
        {

            // If command modify (Case where ID is assigned).
            if (_editedPlanif.TPF_PLANIFID > 0)
            {
                // Save modified row.
                isSaveSuccess = await ModifyAsync();
            }
            // If command duplicate (Case where ID is undefined).
            else
            {
                // Update dates
                _editedPlanif.TPF_DATE_DEBUT = _editedPlanif.TPF_DATE_DEBUT.ToUniversalTime();
                _editedPlanif.TPF_DATE_FIN = _editedPlanif.TPF_DATE_FIN?.ToUniversalTime() ?? _editedPlanif.TPF_DATE_FIN;

                // Insert new row.
                var apiResult = await ProxyCore.InsertAsync(new List<TPF_PLANIFS> { _editedPlanif }, convertToLocalDateTime: true);

                // If the insertion failed.
                if (apiResult.Count < 1)
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), _editedPlanif, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["DTF:ScheduleDuplicateFailed"]);

                    isSaveSuccess = false;
                }
            }

            if (isSaveSuccess)
            {
                CloseCronDialog();

                // Get CRON TimeZoneInfo.
                //TimeZoneInfo cronTZ = TimeZoneInfo.FindSystemTimeZoneById(_editedPlanif.TPF_TIMEZONE_INFOID);
                // Convert CRON start date from its TimeZone to local TimeZone.
                //DateTime localCronStartDate = TimeZoneInfo.ConvertTime(_editedPlanif.TPF_DATE_DEBUT, cronTZ, TimeZoneInfo.Local);

                var localCronStartDate = DateExtensions
                    .ConvertToTimeZoneFromUtc(_editedPlanif.TPF_DATE_DEBUT, _editedPlanif.TPF_TIMEZONE_INFOID);

                // Display an information message when the request is not created immediately.
                if (localCronStartDate > DateTime.Now)
                    await Toast.DisplayInfoAsync(Trad.Keys["COMMON:Info"], Trad.Keys["DTF:RequestCreatedWhenStartDateReached"]);

                await RefreshGridDataAsync();
            }
        }
    }
    #endregion

    #region Inactivate dialog
    /// <summary>
    /// Is "Inactivate" dialog displayed?
    /// </summary>
    private bool IsInactivateDialogDisplayed { get; set; }

    /// <summary>
    /// Open dialog used to inactivate.
    /// </summary>
    private void OpenInactivateDialog() => IsInactivateDialogDisplayed = true;

    /// <summary>
    /// Close dialog used to inactivate.
    /// </summary>
    private void CloseInactivateDialog() => IsInactivateDialogDisplayed = false;

    /// <summary>
    /// Action launched when confirm button is clicked.
    /// </summary>
    private async Task InactivateDialogOnConfirm()
    {
        // If inactivation is successful.
        if (await InactivateAsync())
        {
            CloseInactivateDialog();
        }
    }
    #endregion
}
