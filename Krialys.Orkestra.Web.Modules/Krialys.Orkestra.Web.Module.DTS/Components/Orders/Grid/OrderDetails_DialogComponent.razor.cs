using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Grid;

public partial class OrderDetails_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Selected order.
    /// </summary>
    [Parameter]
    public TCMD_COMMANDES Order { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// History of the phase changes.
    /// </summary>
    private IEnumerable<TCMD_SP_SUIVI_PHASES> _phaseChangesHistory { get; set; } = Enumerable.Empty<TCMD_SP_SUIVI_PHASES>();

    /// <summary>
    /// Domain of the order.
    /// </summary>
    private TDOM_DOMAINES _domain { get; set; }
    #endregion

    #region Blazor life cycle
    protected override async Task OnInitializedAsync()
    {
        // Get phase change history for the selected order
        // expanded with phases (TCMD_PH_PHASES).
        string expand = $"{nameof(TCMD_SP_SUIVI_PHASES.TCMD_PH_PHASE_AVANT)}," +
            $"{nameof(TCMD_SP_SUIVI_PHASES.TCMD_PH_PHASE_APRES)}";
        string filter = $"{nameof(TCMD_SP_SUIVI_PHASES.TCMD_COMMANDEID)} eq {Order.TCMD_COMMANDEID}";
        _phaseChangesHistory = await ProxyCore.GetEnumerableAsync<TCMD_SP_SUIVI_PHASES>(
            $"?$expand={expand}&$filter={filter}", useCache: false);

        // Get domain for selected order.
        if (Order.TDOM_DOMAINEID is not null)
        {
            var domains = await ProxyCore.GetEnumerableAsync<TDOM_DOMAINES>(
                $"?$filter={nameof(TDOM_DOMAINES.TDOM_DOMAINEID)} eq {Order.TDOM_DOMAINEID}");
            _domain = domains.FirstOrDefault();
        }
    }
    #endregion

    #region Dialog
    /// <summary>
    /// Close the dialog.
    /// </summary>
    private Task CloseDialogAsync()
    {
        // Hide dialog.
        IsVisible = false;

        // Update parent with new value.
        return IsVisibleChanged.InvokeAsync(IsVisible);
    }
    #endregion
}

