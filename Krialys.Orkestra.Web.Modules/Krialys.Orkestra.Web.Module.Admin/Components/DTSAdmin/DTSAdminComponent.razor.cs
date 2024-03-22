using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.ADM.Components.DTSAdmin;
public partial class DTSAdminComponent
{
    #region Tab
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
            Bus.Publish(new SfTabBusEvent { Disabled = (args.SelectingIndex != 1) });
        }
    }
    #endregion
}
