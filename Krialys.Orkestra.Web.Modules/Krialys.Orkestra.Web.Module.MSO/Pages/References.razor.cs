using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.MSO.Pages;

public partial class References
{
    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    private bool AllowModify { get; set; }

    #region Blazor life cycle
    private bool Disabled { get; set; }

    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// Assign event
    /// </summary>
    protected override async Task OnInitializedAsync()
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

        // Check if the user is authrorized to modify the data.
        AllowModify = await Session.VerifyPolicy(nameof(PoliciesLiterals.ReferencesEditor));
    }

    #endregion

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
