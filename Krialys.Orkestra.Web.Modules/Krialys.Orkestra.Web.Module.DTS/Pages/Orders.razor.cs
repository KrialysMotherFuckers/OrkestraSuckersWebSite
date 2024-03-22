using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.DTS.Pages;

public partial class Orders
{
    #region Tab
    /// <summary>
    /// Event triggers before the tab item gets selected.
    /// </summary>
    /// <param name="args">Selecting event arguments.</param>
    private void OnTabSelecting(SelectingEventArgs args)
    {
        // Disable Tab navigation by swapping on touch screens.
        if (args.IsSwiped)
        {
            args.Cancel = true;
        }
    }
    #endregion
}
