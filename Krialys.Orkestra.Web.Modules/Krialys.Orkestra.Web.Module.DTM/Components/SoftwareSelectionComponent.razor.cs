using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.DTM.Components;

public partial class SoftwareSelectionComponent
{
    #region Parameters
    /// <summary>
    /// List of the selected objects in the component.
    /// </summary>
    [Parameter]
    public IList<TL_LOGICIELS> SelectedLogiciels { get; set; } = new List<TL_LOGICIELS>();

    /// <summary>
    /// EventCallback used to pass the selected objects to the parent component.
    /// </summary>
    [Parameter]
    public EventCallback<IList<TL_LOGICIELS>> SelectedLogicielsChanged { get; set; }

    /// <summary>
    /// List of selectable objects for this component.
    /// </summary>
    [Parameter]
    public IEnumerable<TL_LOGICIELS> Logiciels { get; set; } = Enumerable.Empty<TL_LOGICIELS>();

    /// <summary>
    /// Junction table between TE_ETATS and TL_LOGICIELS.
    /// </summary>
    [Parameter]
    public IEnumerable<TEL_ETAT_LOGICIELS> EtatLogiciels { get; set; } = Enumerable.Empty<TEL_ETAT_LOGICIELS>();

    /// <summary>
    /// Id of the TE_ETATS edited by parent component.
    /// </summary>
    [Parameter]
    public int EtatId { get; set; }
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized
    /// after having received its initial parameters.
    /// </summary>
    protected override void OnInitialized()
    {
        //***** Initialize selected values. *****
        // Empty selected object list.
        SelectedLogiciels.Clear();

        // Get elements of the junction table corresponding to the Id of the edited object.
        var etatLogiciels = EtatLogiciels.Where(el => el.TE_ETATID == EtatId);

        // Browse through junction table.
        foreach (var etatLogiciel in etatLogiciels)
        {
            // Get selected object.
            var selectedLogiciel = Logiciels.FirstOrDefault(l => l.TL_LOGICIELID == etatLogiciel.TL_LOGICIELID);
            // Add it to a list.
            if (selectedLogiciel is not null)
            {
                SelectedLogiciels.Add(selectedLogiciel);
            }
        }

        // Update SfMultiSelect selected values.
        MultiselectValues = SelectedLogiciels.ToArray();
    }
    #endregion

    #region SfMultiSelect
    /// <summary>
    /// Array of the selected values.
    /// </summary>
    public TL_LOGICIELS[] MultiselectValues { get; set; } = Array.Empty<TL_LOGICIELS>();

    /// <summary>
    /// Action called when a new value is selected.
    /// </summary>
    /// <param name="args">Object selected.</param>
    private async Task OnValueChange(MultiSelectChangeEventArgs<TL_LOGICIELS[]> args)
    {
        // Empty selected object list.
        SelectedLogiciels.Clear();

        if (args.Value is not null)
        {
            // Browse through selected values.
            foreach (var selectedLogiciels in args.Value)
            {
                // Add selected object id to list.
                SelectedLogiciels.Add(selectedLogiciels);
            }
        }

        // Pass the selected value to the parent component.
        await SelectedLogicielsChanged.InvokeAsync(SelectedLogiciels);
    }
    #endregion
}
