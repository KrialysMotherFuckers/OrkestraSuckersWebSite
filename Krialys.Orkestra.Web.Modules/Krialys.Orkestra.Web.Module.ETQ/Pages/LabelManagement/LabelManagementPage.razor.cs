using Krialys.Data.EF.Etq;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.Components;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.ETQ.Pages.LabelManagement;

public partial class LabelManagementPage
{
    #region SfTab
    private bool Disabled { get; set; }

    protected override void OnInitialized()
    {
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

    private void OnTabSelecting(SelectingEventArgs args)
    {
        // Disable Tab navigation on Tab selection.
        if (args.IsSwiped)
        {
            args.Cancel = true;
        }
        else
        {
            // Set Disabled value then fire event to SfTab
            Bus.Publish(new SfTabBusEvent { Disabled = (args.SelectingIndex != 0) });
        }
    }
    #endregion

    #region TSR_SUIVI_RESSOURCES Grid
    /// <summary>
    /// Reference to the data grid component.
    /// </summary>
    private OrkaGenericGridComponent<TSR_SUIVI_RESSOURCES> Ref_TSR_SUIVI_RESSOURCES;
    #endregion
}