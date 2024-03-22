using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.DTF.Components.UTD.NewProduction;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTF.Components.UTD;

public partial class NewProductionComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }

    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Is selected UTD a prototype ?
    /// </summary>
    private bool _isPrototype;
    #endregion

    #region Dialog
    /// <summary>
    /// Delete draft demand, then close the dialog.
    /// </summary>
    private async Task DeleteProdAndCloseDialogAsync()
    {
        // Prevent double click.
        if (_isButtonClicked)
            return;

        _isButtonClicked = true;

        // Aborts the current production.
        await DeleteProductionAsync();

        // Close catalog.
        IsVisible = false;
        // Update parent with new value.
        await IsVisibleChanged.InvokeAsync(IsVisible);

        _isButtonClicked = false;
    }

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
    #endregion

    #region UTD select Component
    /// <summary>
    /// Event triggered when a new scenario is selected.
    /// </summary>
    /// <param name="etatId">Selected TE_ETATS (UTD).</param>
    /// <param name="scenarioId">Selected TS_SCENARIOS (module).</param>
    /// <param name="isPrototype">Is the selected UTD a prototype?</param>
    private async Task OnScenarioChangedAsync(int? etatId, int? scenarioId, bool isPrototype)
    {
        _isButtonClicked = true;

        // Is the new selected UTD a prototype?
        _isPrototype = isPrototype;

        // If Etat (UTD) changed.
        if (!Demande.TE_ETATID.Equals(etatId ?? default))
        {
            // Update Etat.
            Demande.TE_ETATID = etatId ?? default;

            // If demand exists.
            if (!Demande.TD_DEMANDEID.Equals(default))
            {
                // Delete existing demand.
                await DeleteProductionAsync();
                // Reset demand ID.
                Demande.TD_DEMANDEID = default;

                // Note: Update existing demand does not works
                // because Demande.TE_ETATID can't be updated.
            }
        }

        // If Scenario changed.
        if (scenarioId != Demande.TS_SCENARIOID)
        {
            // If scenario is selected.
            if (!Demande.TS_SCENARIOID.Equals(default))
            {
                // Reset of scenario ID.
                // This forces the dispose of the SfUpload component (inside ProdFileUploadComponent).
                // This is a way to reload the SfUpload component from zero.
                // Otherwise SfUpload is difficult to clean/update
                // (because all SfUpload must have the same ID: UploadFiles).
                Demande.TS_SCENARIOID = default;
                StateHasChanged();

                // Clean demand.
                await ResetProductionAsync();
            }

            // Update scenario.
            Demande.TS_SCENARIOID = scenarioId;

            // If scenario is selected.
            if (!Demande.TS_SCENARIOID.Equals(default))
            {
                // If demand is not created yet.
                if (Demande.TD_DEMANDEID.Equals(default))
                    // Create demand.
                    await CreateProductionAsync();
                else
                    // Update demand.
                    await UpdateProductionAsync();
            }
        }

        _isButtonClicked = false;
    }
    #endregion

    #region Files Upload Component
    /// <summary>
    /// Reference to the production file upload component.
    /// </summary>
    private ProdFileUploadComponent Ref_ProdFileUploadComponent;
    #endregion

    #region Comment TextBox
    /// <summary>
    /// Reference to the error Tooltip applied on "user comment" TextBox.
    /// Used to show errors.
    /// </summary>
    private SfTooltip Ref_CommentTooltip;

    /// <summary>
    /// HTML attributes applied to the comment TextBox.
    /// </summary>
    private Dictionary<string, object> _commentHtmlAttributes = new Dictionary<string, object>
    {
        // Specifies the visible height of a text area, in lines.
        {"rows", "3" }
    };

    /// <summary>
    /// Checks that the required fields on the comment tab are filled in.
    /// Display tooltips on unfilled fields.
    /// </summary>
    /// <returns>True, if required fields are filled, false otherwise. </returns>
    private async Task<bool> IsCommentFilledIn()
    {
        // User comment field is mandatory.
        if (string.IsNullOrWhiteSpace(Demande.TD_COMMENTAIRE_UTILISATEUR))
        {
            if (Ref_CommentTooltip is not null)
                await Ref_CommentTooltip.OpenAsync();

            return false;
        }

        return true;
    }

    /// <summary>
    /// Event triggers when the content of "user comment" TextBox has changed and gets focus-out.
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private Task OnCommentChangeAsync(ChangedEventArgs args)
        // Close error tooltip.
        => Ref_CommentTooltip.CloseAsync();
    #endregion

    #region Associated order
    /// <summary>
    /// Id of the associated order.
    /// </summary>
    private int? _associatedOrderId;
    #endregion

    #region Scheduling Dialog (and CRON)
    /// <summary>
    /// Scheduling modes.
    /// </summary>
    private enum SchedulingMode
    {
        Immediate,
        Deferred,
        Recurrent
    }

    /// <summary>
    /// Selected scheduling mode.
    /// </summary>
    private SchedulingMode _selectedSchedulingMode = SchedulingMode.Immediate;

    /// <summary>
    /// CRON schedule expressions.
    /// </summary>
    private string _cron;

    /// <summary>
    /// Schedule start date.
    /// </summary>
    private DateTime _cronStartDate;

    /// <summary>
    /// Schedule end date.
    /// </summary>
    private DateTime? _cronEndDate;

    /// <summary>
    /// Is the dialog to schedule the production opened?
    /// </summary>
    private bool IsProdSchedulingDisplayed;

    /// <summary>
    /// Open dialog used to schedule the production.
    /// </summary>
    private async Task OpenProdSchedulingDialogAsync()
    {
        // Verify all required files are uploaded.
        // Verify that a comment is written.
        if (Ref_ProdFileUploadComponent is not null
            && await Ref_ProdFileUploadComponent.AreRequiredFilesUploadedAsync()
            && await IsCommentFilledIn())
        {
            // Open dialog.
            IsProdSchedulingDisplayed = true;
        }
    }

    /// <summary>
    /// Event triggered when deferred production is launched.
    /// </summary>
    /// <param name="executionDate">Desired execution date.</param>
    private Task LaunchDeferredProductionAsync(DateTime? executionDate)
    {
        _selectedSchedulingMode = SchedulingMode.Deferred;
        Demande.TD_DATE_EXECUTION_SOUHAITEE = executionDate;

        return ProductionValidateAsync();
    }

    /// <summary>
    /// Event triggered when recurrent production is launched.
    /// </summary>
    /// <param name="cron">CRON schedule expressions.</param>
    /// <param name="startDate">Schedule start date.</param>
    /// <param name="endDate">Schedule end date.</param>
    private Task LaunchRecurrentProductionAsync(string cron, DateTime startDate, DateTime? endDate)
    {
        _selectedSchedulingMode = SchedulingMode.Recurrent;

        _cron = cron;
        _cronStartDate = startDate;
        _cronEndDate = endDate;

        return ProductionValidateAsync();
    }
    #endregion

    #region Dialog buttons
    /// <summary>
    /// True if a button of the table is clicked, prevent double click.
    /// Used to display spinner too.
    /// </summary>
    private bool _isButtonClicked;

    /// <summary>
    /// Is validate button disabled ?
    /// </summary>
    /// <returns>True if validate button is disabled.</returns>
    private bool IsValidateDisabled()
        => (Demande.TS_SCENARIOID is null || Demande.TD_DEMANDEID.Equals(default));

    /// <summary>
    /// Event triggered when "validate" button is clicked.
    /// </summary>
    private async Task ProductionValidateAsync()
    {
        // Prevent double click.
        if (!_isButtonClicked)
        {
            _isButtonClicked = true;

            // Verify all required files are uploaded.
            // Verify that a comment is written.
            if (Ref_ProdFileUploadComponent is null
                || !await Ref_ProdFileUploadComponent.AreRequiredFilesUploadedAsync()
                || !await IsCommentFilledIn())
            {
                _isButtonClicked = false;
                return;
            }

            // Is the validation successful?
            bool success = true;

            // Step 1: Prepare demand, associate them with batches and planifications.
            // Selected scheduling
            switch (_selectedSchedulingMode)
            {
                // Launch immediately.
                case SchedulingMode.Immediate:
                    // Execution date is now.
                    Demande.TD_DATE_EXECUTION_SOUHAITEE = DateExtensions.GetLocaleNow();

                    // Link Batchs and Demands.
                    success &= await AssociateBatchsToDemande();

                    // Status: demand created => Ready to launch.
                    Demande.TRST_STATUTID = StatusLiteral.CreatedRequestAndWaitForExecution;
                    break;

                // Launch at selected date.
                case SchedulingMode.Deferred:
                    // Link Batchs and Demands.
                    success &= await AssociateBatchsToDemande();

                    // Status: Demand created => Ready to launch.
                    Demande.TRST_STATUTID = StatusLiteral.CreatedRequestAndWaitForExecution;
                    break;

                case SchedulingMode.Recurrent:
                    // Status: Scheduling template.
                    Demande.TRST_STATUTID = StatusLiteral.ScheduleModel;
                    // Erase execution date
                    // In case the user entered one in deferred mode.
                    Demande.TD_DATE_EXECUTION_SOUHAITEE = null;

                    // Create a planification.
                    var planif = new TPF_PLANIFS
                    {
                        TPF_DATE_DEBUT = DateExtensions.ConvertToUTC(_cronStartDate, DateExtensions.GetLocalTimeZoneId),
                        TPF_DATE_FIN = _cronEndDate.HasValue ? DateExtensions.ConvertToUTC(_cronEndDate.Value, DateExtensions.GetLocalTimeZoneId) : _cronEndDate,
                        TPF_TIMEZONE_INFOID = DateExtensions.GetLocalTimeZoneId,
                        TRST_STATUTID = StatusLiteral.Available,
                        TPF_DEMANDE_ORIGINEID = Demande.TD_DEMANDEID,
                        TPF_CRON = _cron,
                        TRU_DECLARANTID = Session.GetUserId()
                    };

                    // Save planification in database.
                    var apiResult = await ProxyCore.InsertAsync(new List<TPF_PLANIFS> { planif }, convertToLocalDateTime: true);
                    // If the insertion failed.
                    if (apiResult.Count < 0)
                    {
                        success = false;

                        await ProxyCore.SetLogException(new LogException(GetType(), planif, apiResult.Message));
                        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
                    }

                    // Note: Association of batchs to demands is done by server (ParallelU)
                    // for each recurent demand launched.
                    break;
            }

            // Step 2: Associate order and demand.
            if (success && _associatedOrderId is not null)
                success = await AssociateOrderToProductionAsync();

            // Step 3: Save demand.
            if (success)
            {
                // Update created production.
                success = await UpdateProductionAsync();

                // Real time update: Send an event to all users to be able to refresh their data. 
                // Only for immediate and deferred productions, because recurrent ones will be
                // launched by server (ParallelU).
                if (_selectedSchedulingMode.Equals(SchedulingMode.Immediate) || _selectedSchedulingMode.Equals(SchedulingMode.Deferred))
                {
                    // Entity cleared from cache by event
                    bool sent = await ProxyCore.AddTrackedEntity(new[] { typeof(VDE_DEMANDES_ETENDUES).FullName }, Litterals.Insert, Session.GetUserId(), "CreateDemande");
                    if (!sent)
                    {
                        await ProxyCore.SetLogException(new LogException(GetType()));
                        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], "AddTrackedEntity -> CreateDemande");
                    }
                }

                // With scheduled requests, display an information message
                // when the request is not created immediately.
                // Note: Cron start date and "Now" are on the same TZ, no date conversions needed.
                if (success && StatusLiteral.ScheduleModel.Equals(Demande.TRST_STATUTID) && _cronStartDate > DateTime.Now)
                    await Toast.DisplayInfoAsync(Trad.Keys["COMMON:Info"], Trad.Keys["DTF:RequestCreatedWhenStartDateReached"]);

                // Clear affected caches.
                ProxyCore.CacheRemoveEntities(typeof(VPD_PLANIF_DETAILS), typeof(VPE_PLANIF_ENTETES));

                // End new production.
                await CloseDialogAsync();
            }

            _isButtonClicked = false;
        }
    }
    #endregion

    #region Production CRUD (Demande)
    /// <summary>
    /// Demande created by launching new production.
    /// </summary>
    private TD_DEMANDES Demande { get; set; } = new();

    /// <summary>
    /// Save a new production in DB.
    /// => Create a new entry in TD_DEMANDES.
    /// </summary>
    /// <returns>True if creation succeed.</returns>
    private async Task<bool> CreateProductionAsync()
    {
        // Status draft.
        Demande.TRST_STATUTID = StatusLiteral.DraftMode;
        // User creating the production.
        Demande.TRU_DEMANDEURID = Session.GetUserId();
        // Date where production is created.
        Demande.TD_DATE_DEMANDE = DateExtensions.GetLocaleNow();

        // Save in db.
        var apiResult = await ProxyCore.InsertAsync(new List<TD_DEMANDES> { Demande });

        if (apiResult.Count > Litterals.NoDataRow)
        {
            Demande.TD_DEMANDEID = (int)apiResult.Count;

            // Success.
            return true;
        }
        // If the insertion failed.

        await ProxyCore.SetLogException(new LogException(GetType(), Demande, apiResult.Message));
        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], $"{Trad.Keys["DTF:NewProductionFailed"]}");

        // Fail.
        return false;
    }

    /// <summary>
    /// Update created production in DB.
    /// => Update TD_DEMANDES entry.
    /// </summary>
    /// <returns>True if update succeed.</returns>
    private async Task<bool> UpdateProductionAsync()
    {
        var apiResult = await ProxyCore.UpdateAsync(new List<TD_DEMANDES> { Demande });
        // If the update failed.
        if (apiResult.Count.Equals(Litterals.NoDataRow))
        {
            await ProxyCore.SetLogException(new LogException(GetType(), Demande, apiResult.Message));
            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

            // Fail.
            return false;
        }

        // Success.
        return true;
    }

    /// <summary>
    /// Reset production (Demand). Used when a new scenario is selected.
    /// Clear entries in related tables and erase uploaded files.
    /// </summary>
    /// <returns>True if success.</returns>
    private Task<bool> ResetProductionAsync()
        => ClearResourcesAsync();

    /// <summary>
    /// Delete production (Demand).
    /// Clear entries in related tables and erase uploaded files.
    /// </summary>
    /// <returns>True if success.</returns>
    private async Task<bool> DeleteProductionAsync()
    {
        // Clear resources associated to the demand.
        bool success = await ClearResourcesAsync();

        // If a demand exists.
        if (success && !Demande.TD_DEMANDEID.Equals(default))
        {
            // Delete demand.
            var apiResult = await ProxyCore.DeleteAsync<TD_DEMANDES>(new List<string> { Demande.TD_DEMANDEID.ToString() });

            // Api delete failed.
            if (apiResult.Count < 1)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), Demande, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                return false;
            }
        }

        return success;
    }
    #endregion

    #region Erase uploaded files (resources)
    /// <summary>
    /// Path of the resources on the server.
    /// Corresponds to a parameter from ApiUnivers configuration file (appsettings.json).
    /// </summary>
    private const string _resourcePath = "ParallelU:PathRessource";

    /// <summary>
    /// Delete resource file on server.
    /// </summary>
    /// <param name="fileName">name of the file to delete.</param>
    /// <returns>True if success.</returns>
    private async Task<bool> EraseResourceFileAsync(string fileName)
    {
        // Returned value.
        bool isEraseSuccessful = true;

        // Path of the file to remove.
        string filePath = $"{{{_resourcePath}}}{Demande.TD_DEMANDEID.ToString().PadLeft(6, '0')}/";
        try
        {
            // Delete file.
            var deleteResult = await ProxyCore
                .DeleteFiles(filePath, new[] { fileName });

            // Delete result.
            if (!"OK".Equals(deleteResult.Key))
            {
                isEraseSuccessful = false;

                await ProxyCore.SetLogException(new LogException(GetType(), fileName, deleteResult.Value));
            }
        }
        // Delete failed with error.
        catch (Exception ex)
        {
            isEraseSuccessful = false;

            // Log error
            await ProxyCore.SetLogException(new LogException(GetType(), ex));
        }

        if (!isEraseSuccessful)
        {
            // Display error.
            await Toast.DisplayErrorAsync($"{Trad.Keys["COMMON:Error"]}",
                $"{string.Format(Trad.Keys["DTF:FileDeleteFailed"], fileName)}")
                ;
        }

        return isEraseSuccessful;
    }
    #endregion

    #region Ressources-Demande link
    /// <summary>
    /// Clear resources entries and files associated to a production.
    /// </summary>
    /// <returns>True if success.</returns>
    private async Task<bool> ClearResourcesAsync()
    {
        // If demand doesn't exist, nothing to clear.
        if (Demande.TD_DEMANDEID.Equals(default))
            return true;

        // Search if resources were attached to the demand.
        string queryOptions = $"?$filter=(TD_DEMANDEID eq {Demande.TD_DEMANDEID})&$expand=TER_ETAT_RESSOURCE";
        var ressourceDemandes = await ProxyCore.GetEnumerableAsync<TRD_RESSOURCE_DEMANDES>(queryOptions, useCache: false);

        var trdRessourceDemandeses = ressourceDemandes as TRD_RESSOURCE_DEMANDES[] ?? ressourceDemandes.ToArray();
        if (trdRessourceDemandeses.Any())
        {
            // Delete resource files.
            foreach (var ressourceDemande in trdRessourceDemandeses)
            {
                // Get file name.
                // If resource is a mask (is a pattern), take the name of the uploaded file (original name).
                // If it isn't a mask, take the resource file name.
                string fileName = StatusLiteral.Yes.Equals(ressourceDemande.TER_ETAT_RESSOURCE.TER_IS_PATTERN) ?
                    ressourceDemande.TRD_NOM_FICHIER_ORIGINAL : ressourceDemande.TRD_NOM_FICHIER;
                // Delete ressource file.
                await EraseResourceFileAsync(fileName);
            }

            // Delete link between the resources and the demands.
            var ids = trdRessourceDemandeses.Select(rd => rd.TRD_RESSOURCE_DEMANDEID.ToString()).ToList();
            var apiResult = await ProxyCore.DeleteAsync<TRD_RESSOURCE_DEMANDES>(ids);

            // Api delete failed.
            if (apiResult.Count < 1)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), ressourceDemandes, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                return false;
            }
        }

        // Success
        return true;
    }
    #endregion

    #region Batchs-Demande link
    /// <summary>
    /// Associate batchs with a demand.
    /// Complete TBD_BATCH_DEMANDES table.
    /// </summary>
    /// <returns>True if association succeed.</returns>
    private async Task<bool> AssociateBatchsToDemande()
    {
        // Get TEB_ETAT_BATCHS from DB,
        // expanded with TBS_BATCH_SCENARIOS,
        // filtered to display only TEB associated to the selected scenario.
        string queryOptions = $"?$expand=TBS_BATCH_SCENARIOS($filter=TS_SCENARIOID eq {Demande.TS_SCENARIOID})" +
            $"&$filter=TBS_BATCH_SCENARIOS/any(TBS: TBS/TS_SCENARIOID eq {Demande.TS_SCENARIOID})";
        var etatBatchs = await ProxyCore.GetEnumerableAsync<TEB_ETAT_BATCHS>(queryOptions, useCache: false);

        foreach (var etatBatch in etatBatchs)
        {
            foreach (var batchScenario in etatBatch.TBS_BATCH_SCENARIOS)
            {
                // Link batchs to the demand.
                // Create a new TBD_BATCH_DEMANDES.
                TBD_BATCH_DEMANDES batchDemande = new()
                {
                    TD_DEMANDEID = Demande.TD_DEMANDEID,
                    TE_ETATID = Demande.TE_ETATID,
                    TBD_ORDRE_EXECUTION = batchScenario.TBS_ORDRE_EXECUTION,
                    TBD_EXECUTION = StatusLiteral.Yes, // Not used.
                    TEB_ETAT_BATCHID = etatBatch.TEB_ETAT_BATCHID
                };

                // Save in db.
                var apiResult = await ProxyCore.InsertAsync(new List<TBD_BATCH_DEMANDES> { batchDemande });

                // If the insertion failed.                    
                if (apiResult.Count < 1)
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), batchDemande, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);

                    // Failed.
                    return false;
                }
            }
        }

        // Succeed.
        return true;
    }
    #endregion

    #region Order-Production link
    /// <summary>
    /// Associate an order to the production.
    /// => Create a new entry in TDC_DEMANDES_COMMANDES table.
    /// </summary>
    /// <returns>True if association succeed.</returns>
    private async Task<bool> AssociateOrderToProductionAsync()
    {
        // Create association.
        TDC_DEMANDES_COMMANDES demandesCommandes = new()
        {
            TD_DEMANDEID = Demande.TD_DEMANDEID,
            TCMD_COMMANDEID = (int)_associatedOrderId
        };

        // Save in db.
        var apiResult = await ProxyCore.InsertAsync(new List<TDC_DEMANDES_COMMANDES> { demandesCommandes });

        if (apiResult.Count > Litterals.NoDataRow)
            // Success.
            return true;

        // If the insertion failed.
        await ProxyCore.SetLogException(new LogException(GetType(), demandesCommandes, apiResult.Message));
        await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], $"{Trad.Keys["DTF:NewProductionFailed"]}");

        // Fail.
        return false;
    }
    #endregion
}
