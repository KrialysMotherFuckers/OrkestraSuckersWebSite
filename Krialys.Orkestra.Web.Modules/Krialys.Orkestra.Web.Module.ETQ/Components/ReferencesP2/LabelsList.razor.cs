using Krialys.Orkestra.Common.Shared;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.ReferencesP2;

public partial class LabelsList
{
    /// <summary>
    /// Number of displayed labels in grid.
    /// </summary>
    private int _labelsCount;

    /// <summary>
    /// Reference to the TETQ grid component.
    /// </summary>
    private TETQ_ETIQUETTES_GridComponent _TETQ_ETIQUETTES_GridComponentRef;

    /// <summary>
    /// Event triggered when TETQ_ETIQUETTES filters changed.
    /// </summary>
    /// <param name="filters">Filters to apply.</param>
    private Task OnFiltersChangedAsync(EtqFilters filters)
        => _TETQ_ETIQUETTES_GridComponentRef.ApplyFiltersAsync(filters);
}
