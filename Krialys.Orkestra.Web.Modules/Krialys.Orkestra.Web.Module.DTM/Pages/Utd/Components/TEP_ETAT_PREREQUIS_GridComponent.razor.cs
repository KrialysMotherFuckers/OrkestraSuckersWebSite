using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.DTM.Pages.Utd.Components;

public partial class TEP_ETAT_PREREQUIS_GridComponent
{
    #region Parameters
    /// <summary>
    /// Sf query applied to the grid.
    /// </summary>
    [Parameter] public Query GridQuery { get; set; }

    /// <summary>
    /// Id of selected TE_ETAT.
    /// </summary>
    [Parameter] public int EtatId { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    #region Grid
    /// <summary>
    /// Reference to the grid component.
    /// </summary>
    private OrkaGenericGridComponent<TEP_ETAT_PREREQUISS> Ref_TEP_ETAT_PREREQUISS;

    /// <summary>
    /// List of the displayed fields in grid columns.
    /// </summary>
    private readonly List<string> _displayedFields = new()
    {
        "TEP_ETAT_PREREQUISID",
        "TEP_FILEPATTERN",
        "TEP_PATH",
        "TRST_STATUTID",
        "TEP_IS_PATTERN",
        "TEP_NATURE",
        "TEP_DATE_MAJ",
        "TEP_COMMENTAIRE"
    };

    protected override Task OnInitializedAsync()
    {
        ProxyCore.CacheRemoveEntities(typeof(TEP_ETAT_PREREQUISS));

        return base.OnInitializedAsync();
    }

    /// <summary>
    /// Get header for grid edit template.
    /// </summary>
    /// <param name="data">Edited item.</param>
    /// <returns>Edit header text.</returns>
    public string GetEditHeader(TEP_ETAT_PREREQUISS data)
    {
        return data.TEP_ETAT_PREREQUISID == 0
            ? Trad.Keys["DTM:UTDCatalogPrerequisitesGridAddHeader"]
            : $"{Trad.Keys["DTM:UTDCatalogPrerequisitesGridEditHeader"]}{data.TEP_FILEPATTERN}";
    }

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database.
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="data">Incoming datas to be saved</param>
    private async Task SaveAsync(object data)
    {
        var etatPrerequis = data as TEP_ETAT_PREREQUISS;

        // Set TE_ETAT id (parent ID).
        if (etatPrerequis!.TE_ETATID == 0)
        {
            etatPrerequis.TE_ETATID = EtatId;
        }

        // End edition.
        await Ref_TEP_ETAT_PREREQUISS.DataGrid.EndEditAsync();
    }
    #endregion
}
