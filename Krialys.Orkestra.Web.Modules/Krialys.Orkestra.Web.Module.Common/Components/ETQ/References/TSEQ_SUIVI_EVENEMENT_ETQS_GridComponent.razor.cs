using Krialys.Data.EF.Etq;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.Common.Components.ETQ.References;

public partial class TSEQ_SUIVI_EVENEMENT_ETQS_GridComponent
{
    #region Parameters
    /// <summary>
    /// ID of a TETQ_ETIQUETTES.
    /// Used to filter grid.
    /// </summary>
    [Parameter] public int? EtiquetteId { get; set; }
    #endregion

    #region Blazor Life cycle
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Set the component parameters
        parameters.SetParameterProperties(this);

        if (parameters.TryGetValue<int?>(nameof(EtiquetteId), out var value))
        {
            if (value.HasValue)
            {
                GridQuery = GridQuery.Where(nameof(TSEQ_SUIVI_EVENEMENT_ETQS.TETQ_ETIQUETTEID), "equals", value.Value);
            }
        }

        // Pass an empty ParameterView, not parameters!
        await base.SetParametersAsync(ParameterView.Empty);
    }
    #endregion

    #region SfGrid
    /// <summary>
    /// Reference to the DataGrid component.
    /// </summary>
    private OrkaGenericGridComponent<TSEQ_SUIVI_EVENEMENT_ETQS> Ref_TSEQ_SUIVI_EVENEMENT_ETQS;

    /// <summary>
    /// Sf query used to filter and order grid.
    /// </summary>
    private Query GridQuery = new Query().Sort(nameof(TSEQ_SUIVI_EVENEMENT_ETQS.TSEQ_SUIVI_EVENEMENT_ETQID), "descending");
    #endregion
}
