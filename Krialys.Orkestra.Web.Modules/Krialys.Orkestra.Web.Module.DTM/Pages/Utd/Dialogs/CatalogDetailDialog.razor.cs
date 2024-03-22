using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd.Dialogs;

public partial class CatalogDetailDialog
{
    #region Parameters
    /// <summary>
    /// Is catalog dialog visible.
    /// </summary>
    [Parameter] public bool IsDisplayed { get; set; }

    [Parameter]
    public EventCallback<bool> IsDisplayedChanged { get; set; }

    /// <summary>
    /// Id of selected TE_ETAT.
    /// </summary>
    [Parameter] public int EtatId { get; set; }

    /// <summary>
    /// StatusId of selected TE_ETAT.
    /// </summary>
    [Parameter] public string EtatStatusId { get; set; }

    /// <summary>
    /// Catalog header.
    /// </summary>
    [Parameter] public string Header { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    /// <summary>
    /// Sf query used to filter with selected EtatId.
    /// </summary>
    private Query _etatIdQuery;
    private Query _etatIdBatchQuery;

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component has received the parameter 
    /// from parent component.
    /// </summary>
    protected override void OnParametersSet()
    {
        _etatIdQuery = new Query().Where("TE_ETATID", "equal", EtatId);
        _etatIdBatchQuery = new Query().Where("TE_ETATID", "equal", EtatId).Where("TRST_STATUTID", "equal", StatusLiteral.Available);
    }
    #endregion

    /// <summary>
    /// Close the catalog dialog.
    /// </summary>
    private async Task CloseCatalog()
    {
        // Close catalog.
        IsDisplayed = false;

        // Update parent with new value.
        await IsDisplayedChanged.InvokeAsync(IsDisplayed);
    }

    private bool canModify()
        => AllowModify && (EtatStatusId == StatusLiteral.Draft || EtatStatusId == StatusLiteral.DraftMode);

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
        else
        {
            // Set Disabled value then fire event to SfTab
            Bus.Publish(new SfTabBusEvent { Disabled = args.SelectingIndex <= 2 });
        }
    }
    #endregion
}

