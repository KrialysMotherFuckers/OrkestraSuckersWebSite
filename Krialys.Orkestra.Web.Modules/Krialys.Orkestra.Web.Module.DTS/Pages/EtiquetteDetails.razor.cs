using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.DTS.Pages;

public partial class EtiquetteDetails
{
    #region Injected services   
    [Inject] private ILogger<EtiquetteDetails> Logger { get; set; }
    #endregion

    #region Parameters
    /// <summary>
    /// Id of the TETQ_ETIQUETTES displayed.
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public string ID { get; set; }

    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Displayed TETQ_ETIQUETTES.
    /// </summary>
    private TETQ_ETIQUETTES _etiquette;

    /// <summary>
    /// VDE_DEMANDES_ETENDUES associated to this TETQ_ETIQUETTES.
    /// </summary>
    private VDE_DEMANDES_ETENDUES _demande;

    /// <summary>
    /// Show/hide toolbox
    /// </summary>
    private bool isChecked = true;

    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Get selected TETQ_ETIQUETTES (based on its ID)
        // Expanded to TPRCP_PRC_PERIMETRE
        // Expanded to TDOM_DOMAINES.
        string odataQuery = $"?$filter={nameof(TETQ_ETIQUETTES.TETQ_ETIQUETTEID)} eq {ID} " +
            $"&$expand={nameof(TETQ_ETIQUETTES.TPRCP_PRC_PERIMETRE)}($expand={nameof(TPRCP_PRC_PERIMETRES.TDOM_DOMAINES)})";
        _etiquette = (await ProxyCore.GetEnumerableAsync<TETQ_ETIQUETTES>($"{odataQuery}"))
            ?.FirstOrDefault();

        // Get VDE_DEMANDES_ETENDUES (based on TD_DEMANDEID).
        if (_etiquette?.DEMANDEID is not null)
        {
            odataQuery = $"?$filter={nameof(VDE_DEMANDES_ETENDUES.TD_DEMANDEID)} eq {_etiquette.DEMANDEID}";
            _demande = (await ProxyCore.GetEnumerableAsync<VDE_DEMANDES_ETENDUES>($"{odataQuery}"))
                ?.FirstOrDefault();
        }

        // Subscribe SfTabEvent
        Bus.Subscribe<SfTabBusEvent>(e =>
        {
            var result = e.GetMessage<SfTabBusEvent>().Disabled;
            if (Disabled != result)
            {
                Disabled = result;
                // Refresh UI
                StateHasChanged();
            }
        });
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            if (LocalStorage.ContainKey("tabPortail_ETQ_dts_etq_details"))
            {
                _showCheckBox = LocalStorage.GetItem<int>("tabPortail_ETQ_dts_etq_details") > 2;
                // Refresh UI
                StateHasChanged();
            }
        }

        base.OnAfterRender(firstRender);
    }

    #endregion

    #region Back button
    private void OnBackButtonClick()
    {
        NavigationManager.NavigateTo("dts_etq");
    }
    #endregion

    private SfTab _refTab;

    private bool _showCheckBox;

    private bool Disabled { get; set; }

    private void OnTabSelecting(SelectingEventArgs args)
    {
        _showCheckBox = args.SelectingIndex > 2;

        // Disable Tab navigation on Tab selection.
        if (args.IsSwiped)
        {
            args.Cancel = true;
        }
        else
        {
            // Set Disabled value then fire event to SfTab
            Bus.Publish(new SfTabBusEvent { Disabled = (args.SelectingIndex == 1) });
        }
    }
}
