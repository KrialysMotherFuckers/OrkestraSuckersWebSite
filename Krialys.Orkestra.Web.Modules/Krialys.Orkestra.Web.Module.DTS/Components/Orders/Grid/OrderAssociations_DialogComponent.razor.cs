using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using System.Text.Json;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Grid;
public partial class OrderAssociations_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Selected order Id.
    /// </summary>
    [Parameter]
    public int OrderId { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Productions associated with selected order.
    /// </summary>
    private IEnumerable<VDE_DEMANDES_ETENDUES> _productions { get; set; } = Enumerable.Empty<VDE_DEMANDES_ETENDUES>();
    #endregion

    #region Blazor life cycle
    protected override async Task OnInitializedAsync()
    {
        // Call API to read productions associated with selected order.
        var response = await ProxyCore.GetProductionsAssociatedWithAnOrderAsync(OrderId);

        // If request is successful.
        if (response.IsSuccessStatusCode)
        {
            // Get data by deserializing response content.
            string content = await response.Content.ReadAsStringAsync();
            _productions = JsonSerializer.Deserialize<List<VDE_DEMANDES_ETENDUES>>(content);
        }
        else
        {
            // Display error message.
            await Toast.DisplayErrorAsync(Trad.Keys["GridSource:GridError"],
                Trad.Keys["COMMON:RequestFailed"]);
        }
    }
    #endregion

    #region Grid
    /// <summary>
    /// Displayed columns.
    /// </summary>
    public string[] _displayedColumns { get; set; } =
    {
        nameof(VDE_DEMANDES_ETENDUES.TD_DEMANDEID),
        nameof(VDE_DEMANDES_ETENDUES.TE_NOM_ETAT),
        nameof(VDE_DEMANDES_ETENDUES.TS_NOM_SCENARIO),
        nameof(VDE_DEMANDES_ETENDUES.TD_DATE_PIVOT),
        nameof(VDE_DEMANDES_ETENDUES.STATUT_DEMANDE_FR),
        nameof(VDE_DEMANDES_ETENDUES.TD_QUALIF_BILAN),
        nameof(VDE_DEMANDES_ETENDUES.TD_COMMENTAIRE_UTILISATEUR)
    };
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
