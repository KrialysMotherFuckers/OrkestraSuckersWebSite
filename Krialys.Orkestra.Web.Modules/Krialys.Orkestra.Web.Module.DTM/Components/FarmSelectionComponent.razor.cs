using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.DTM.Components;

public partial class FarmSelectionComponent
{
    #region Parameters
    /// <summary>
    /// List of the selected objects in the component.
    /// </summary>
    [Parameter]
    public IList<TF_FERMES> SelectedFarms { get; set; } = new List<TF_FERMES>();

    /// <summary>
    /// EventCallback used to pass the selected objects to the parent component.
    /// </summary>
    [Parameter]
    public EventCallback<IList<TF_FERMES>> SelectedFarmsChanged { get; set; }

    /// <summary>
    /// List of selectable objects for this component.
    /// </summary>
    [Parameter]
    public IEnumerable<TF_FERMES> Fermes { get; set; }

    /// <summary>
    /// Junction table between TEM_ETAT_MASTERS and TF_FERMES.
    /// </summary>
    [Parameter]
    public IList<TEMF_ETAT_MASTER_FERMES> EtatMasterFermes { get; set; }

    /// <summary>
    /// Id of the TEM_ETAT_MASTERS edited by parent component.
    /// </summary>
    [Parameter]
    public int EtatMasterId { get; set; }
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
        SelectedFarms.Clear();

        // Get elements of the junction table corresponding to the Id of the edited object.
        var etatMasterFermes = EtatMasterFermes.Where(emf => emf.TEM_ETAT_MASTERID == EtatMasterId);

        // Browse through junction table.
        foreach (var etatMasterFerme in etatMasterFermes)
        {
            // Get selected farm.
            var selectedFarm = Fermes.FirstOrDefault(f => f.TF_FERMEID == etatMasterFerme.TF_FERMEID);
            // Add it to a list.
            if (selectedFarm is not null)
            {
                SelectedFarms.Add(selectedFarm);
            }
        }

        // Update SfMultiSelect selected values.
        MultiselectValues = SelectedFarms.ToArray();
    }
    #endregion

    #region SfMultiSelect
    /// <summary>
    /// Array of the selected values.
    /// </summary>
    public TF_FERMES[] MultiselectValues { get; set; } = Array.Empty<TF_FERMES>();

    /// <summary>
    /// Action called when a new value is selected.
    /// </summary>
    /// <param name="args">Object selected.</param>
    private async Task OnValueChange(MultiSelectChangeEventArgs<TF_FERMES[]> args)
    {
        // Empty selected object list.
        SelectedFarms.Clear();

        if (args.Value is not null)
        {
            // Browse through selected values.
            foreach (var selectedFarm in args.Value)
            {
                // Add selected object id to list.
                SelectedFarms.Add(selectedFarm);
            }
        }

        // Pass the selected value to the parent component.
        await SelectedFarmsChanged.InvokeAsync(SelectedFarms);
    }
    #endregion
}
