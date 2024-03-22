using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Mso;

public partial class Mso_Expected
{
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    private bool AllowModify { get; set; } = true;

    #region Datagrid
    private OrkaGenericGridComponent<TRA_ATTENDUS> Ref_Grid;

    /// <summary>
    /// Define Schedule button.
    /// </summary>
    private CommandButtonOptions ScheduleButtonOption => new()
    {
        CssClass = "e-success",
        IconCss = "e-icons e-clock",
        Content = Trad.Keys["MSO:Schedule"],
        Disabled = !AllowModify
    };

    /// <summary>
    /// Handler called when a command button is clicked.
    /// </summary>
    /// <param name="args">Click event argument with data of the clicked row.</param>
    private async Task CommandClickedAsync(CommandClickEventArgs<TRA_ATTENDUS> args)
    {
        await Schedule(args.RowData.TRA_ATTENDUID);
    }

    /// <summary>
    /// Schedule button action.
    /// Prepare and open planification dialog.
    /// </summary>
    /// <param name="attenduId">Id of the attendu to plan</param>
    private async Task Schedule(int attenduId)
    {
        /* Get planifications not linked to the selected attendu. */
        string expand = "TRAP_ATTENDUS_PLANIFS";
        string filter = $"TRAP_ATTENDUS_PLANIFS/all(trap: trap/TRA_ATTENDUID ne {attenduId})";
        _planifsNotLinked = await ProxyCore.GetEnumerableAsync<TRP_PLANIFS>($"?$expand={expand}&$filter={filter}");

        if (_planifsNotLinked.Any())
        {
            /* Keep selected attendu ID to use it in the dialog. */
            _selectedAttenduId = attenduId;

            OpenPlanifDialog();
        }
    }
    #endregion

    #region Blazor life cycle
    private IEnumerable<TRAPL_APPLICATIONS> Applications { get; set; }

    private IEnumerable<TRC_CRITICITES> Criticites { get; set; }

    private IEnumerable<TRR_RESULTATS> Resultats { get; set; }

    private IEnumerable<TRNF_NATURES_FLUX> NatureFluxes { get; set; }

    private IEnumerable<TRNT_NATURES_TRAITEMENTS> NatureTraitements { get; set; }

    private IEnumerable<TRTT_TECHNOS_TRAITEMENTS> TechnoTraitements { get; set; }

    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Check if the user is authrorized to modify the data.
        AllowModify = await Session.VerifyPolicy(PoliciesLiterals.AttendusEditor);
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        // Get foreign values.
        Applications = await ProxyCore.GetEnumerableAsync<TRAPL_APPLICATIONS>();
        Criticites = await ProxyCore.GetEnumerableAsync<TRC_CRITICITES>();
        Resultats = await ProxyCore.GetEnumerableAsync<TRR_RESULTATS>();
        NatureFluxes = await ProxyCore.GetEnumerableAsync<TRNF_NATURES_FLUX>();
        NatureTraitements = await ProxyCore.GetEnumerableAsync<TRNT_NATURES_TRAITEMENTS>();
        TechnoTraitements = await ProxyCore.GetEnumerableAsync<TRTT_TECHNOS_TRAITEMENTS>();
    }
    #endregion

    #region Planif dialog
    /// <summary>
    /// Is planif Dialog component displayed ?
    /// </summary>
    private bool IsPlanifDialogVisible { get; set; } = false;

    /// <summary>
    /// ID of the selected TRA_ATTENDUS.
    /// </summary>
    private int _selectedAttenduId;

    /// <summary>
    /// ID of the selected TRP_PLANIFS.
    /// </summary>
    private string _selectedPlanifId;

    /// <summary>
    /// List of all TRP_PLANIFS not linked to TRA_ATTENDUS.
    /// </summary>
    private IEnumerable<TRP_PLANIFS> _planifsNotLinked = Enumerable.Empty<TRP_PLANIFS>();

    /// <summary>
    /// Tooltip used to warn user when no TRP_PLANIFS is selected.
    /// </summary>
    private SfTooltip _planifTooltip;

    /// <summary>
    /// Method used to close planif dialog.
    /// </summary>
    private void OpenPlanifDialog()
    {
        // Open dialog
        IsPlanifDialogVisible = true;
    }

    /// <summary>
    /// Method used to close planif dialog.
    /// </summary>
    private async void ClosePlanifDialog()
    {
        // Reset selected planif.
        _selectedPlanifId = default;
        // Close dialog.
        IsPlanifDialogVisible = false;
        // Close tooltips.
        await _planifTooltip.CloseAsync();
    }

    /// <summary>
    /// Link TRP_PLANIFS to TRA_ATTENDUS with TRAP_ATTENDUS_PLANIFS table.
    /// </summary>
    private async Task LinkPlanif()
    {
        /* If a planification is selected. */
        if (int.TryParse(_selectedPlanifId, out int planifId))
        {
            /* Close planification dialog. */
            ClosePlanifDialog();

            /* Create new TRAP entry. */
            var newAttenduPlanif = new TRAP_ATTENDUS_PLANIFS
            {
                TRA_ATTENDUID = _selectedAttenduId,
                TRP_PLANIFID = planifId,
                TRAP_STATUT = StatusLiteral.Available,
                TRAP_DATE_MODIF = DateExtensions.GetUtcNow() // DateTime.Now
            };

            // Add entry to database.
            var apiResult = await ProxyCore.InsertAsync(new List<TRAP_ATTENDUS_PLANIFS> { newAttenduPlanif });

            // If the insertion failed.
            if (apiResult.Count < 1)
            {
                await ProxyCore.SetLogException(new LogException(GetType(), newAttenduPlanif, apiResult.Message));
                await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
            }

            /* Refresh parent grid to take into account the new values.
             * Will close all the opened hierarchy grids. */
            await Ref_Grid.DataGrid.Refresh();
        }
        /* No planification selected. */
        else
        {
            /* Open tooltip to warn the user. */
            await _planifTooltip.OpenAsync();
        }
    }
    #endregion
}