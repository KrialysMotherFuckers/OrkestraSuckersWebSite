using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Mso;

public partial class Mso_Planning
{
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    private bool AllowModify { get; set; } = true;

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Check if the user is authrorized to modify the data.
        AllowModify = await Session.VerifyPolicy(PoliciesLiterals.PlanifsEditor);
    }
    #endregion

    #region CRON
    /// <summary>
    /// Is the CRON schedule expressions correct?
    /// </summary>
    private bool _isCronValid;
    #endregion

    #region Datagrid
    private OrkaGenericGridComponent<TRP_PLANIFS> _refGrid;

    /// <summary>
    /// Grid Query.
    /// Deactivate UTC/Local dates conversions.
    /// </summary>
    private readonly Query _query = new Query().AddParams(Litterals.ConvertToUTtc, false);

    /// <summary>
    /// Dialog settings.
    /// </summary>
    public static DialogSettings GridDialogSettings => new()
    {
        Width = "65%",
        Height = "auto",
        ShowCloseIcon = true,
        EnableResize = false,
        CloseOnEscape = true,
        AllowDragging = false
    };

    /// <summary>
    /// Event triggered when a DataGrid action starts.
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public void OnActionBegin(ActionEventArgs<TRP_PLANIFS> args)
    {
        if (args.RequestType == Action.Add)
        {
            // Reset Cron.
            _isCronValid = false;
        }
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database.
    /// Some business rules can be applied here using EndEdit(), or reject globally using CloseEdit().
    /// </summary>
    /// <param name="planification">Saved data.</param>
    private async Task SaveAsync(TRP_PLANIFS planification)
    {
        // If CRON is valid.
        if (_isCronValid)
        {
            // Set planification.
            planification.TRP_STATUT = StatusLiteral.Available;
            planification.TRP_TIMEZONE_INFOID = DateExtensions.GetLocalTimeZoneId;

            if (planification.TRP_PLANIFID == default)
            {
                // Creator.
                planification.TTU_CREATEURID = Session.GetUserId();
            }
            else
            {
                // Set editor.
                planification.TTU_MODIFICATEURID = Session.GetUserId();
            }

            // Save.
            await _refGrid.DataGrid.EndEditAsync();
        }
    }
    #endregion
}
