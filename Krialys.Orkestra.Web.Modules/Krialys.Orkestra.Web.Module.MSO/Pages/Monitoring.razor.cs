using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.MSO.Pages
{
    public partial class Monitoring
    {
        private bool Disabled { get; set; }

        private void OnTabSelecting(SelectingEventArgs args)
        {
            // Disable Tab navigation on Tab selection.
            if (args.IsSwiped)
            {
                args.Cancel = true;
            }
        }
    }
}
