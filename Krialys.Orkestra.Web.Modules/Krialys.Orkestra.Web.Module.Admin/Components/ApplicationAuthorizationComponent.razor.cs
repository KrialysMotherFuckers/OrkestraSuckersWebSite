using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.Components;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.ADM.Components;

public partial class ApplicationAuthorizationComponent
{
    private OrkaGenericGridComponent<TRCLI_CLIENTAPPLICATIONS> Ref_Grid;

    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly string[] DisplayedFields = {
        nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_CLIENTAPPLICATIONID),
        nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_LABEL),
        nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_DESCRIPTION),
        nameof(TRCLI_CLIENTAPPLICATIONS.TRCLI_STATUS)
    };

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
            Bus.Publish(new SfTabBusEvent { Disabled = true });
        }

    }
}
