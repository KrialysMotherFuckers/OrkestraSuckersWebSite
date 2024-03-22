using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Mso;
using Microsoft.AspNetCore.Components;

namespace Krialys.Orkestra.Web.Module.Common.Components.DataManager;

public partial class ExpectedPlanningComponent
{
    #region Parameters
    /// <summary>
    /// Id of selected TRA_ATTENDU item.
    /// Only planifications linked to this item are displayed.
    /// </summary>
    [Parameter] public int TRA_ATTENDUID { get; set; }

    /// <summary>
    /// Is the user allowed to modify grid data ?
    /// </summary>
    [Parameter] public bool AllowModify { get; set; } = true;
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        await InitializeGridData();

        // Prepare status list.
        InitializeStatus();
    }
    #endregion

    #region Grid
    private OrkaGenericGridComponent<TRAP_ATTENDUS_PLANIFS> _refGrid;

    /// <summary>
    /// Grid data.
    /// </summary>
    private IEnumerable<TRAP_ATTENDUS_PLANIFS> _attendusPlanifsData = Enumerable.Empty<TRAP_ATTENDUS_PLANIFS>();

    /// <summary>
    /// Initialize Grid data.
    /// </summary>
    private async Task InitializeGridData()
    {
        if (!_attendusPlanifsData.Any())
        {
            // Get TRAP_ATTENDUS_PLANIFS with the selected TRA_ATTENDUID.
            // Expand it with TRP_PLANIF.
            string expand = "TRP_PLANIF";
            string filter = $"(TRA_ATTENDUID eq {TRA_ATTENDUID})";
            _attendusPlanifsData = await ProxyCore
                .GetEnumerableAsync<TRAP_ATTENDUS_PLANIFS>($"?$expand={expand}&$filter={filter}", useCache: false);
        }
    }

    /// <summary>
    /// Event called when user save an edition.
    /// </summary>
    /// <param name="attendusPlanifs">Item to update.</param>
    private async Task SaveAsync(TRAP_ATTENDUS_PLANIFS attendusPlanifs)
    {
        // Set modification date.
        attendusPlanifs.TRAP_DATE_MODIF = DateExtensions.GetUtcNow(); //  DateTimeOffset.Now;

        // Do not update expanded fields.
        attendusPlanifs.TRP_PLANIF = null;

        // Update item in database.
        var apiResult = await ProxyCore.UpdateAsync(new List<TRAP_ATTENDUS_PLANIFS> { attendusPlanifs });
        // If the update failed.
        if (apiResult.Count.Equals(Litterals.NoDataRow))
        {
            await ProxyCore.SetLogException(new LogException(GetType(), attendusPlanifs, apiResult.Message));

            await Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"], Trad.Keys["COMMON:DataBaseUpdateFailed"]);
        }

        // Update grid data.
        var gridDataToUpdate = _attendusPlanifsData.FirstOrDefault(ap => ap.TRAP_ATTENDU_PLANIFID == attendusPlanifs.TRAP_ATTENDU_PLANIFID);
        if (gridDataToUpdate != null)
        {
            gridDataToUpdate.TRAP_DATE_MODIF = attendusPlanifs.TRAP_DATE_MODIF;
            gridDataToUpdate.TRAP_STATUT = attendusPlanifs.TRAP_STATUT;
        }

        // Refresh grid.
        // Note: CloseEdit doesn't refresh TRAP_DATE_MODIF.
        await _refGrid.DataGrid.Refresh();
    }
    #endregion

    #region Status
    /// <summary>
    /// Status object, used for status columns.
    /// </summary>
    private class Statuts
    {
        public string StatusLib { get; set; }

        public string StatusValue { get; set; }
    }

    /// <summary>
    /// Dropdown values for status field.
    /// </summary>
    private readonly List<Statuts> _status = new();

    /// <summary>
    /// Initialize status list (depend on translations).
    /// </summary>
    private void InitializeStatus()
    {
        if (!_status.Any())
        {
            _status.Add(new Statuts { StatusLib = Trad.Keys["STATUS:A"], StatusValue = StatusLiteral.Available });
            _status.Add(new Statuts { StatusLib = Trad.Keys["STATUS:I"], StatusValue = StatusLiteral.Deactivated });
        }
    }

    /// <summary>
    /// Get style applied to status.
    /// </summary>
    /// <param name="status">Status value.</param>
    /// <returns>CSS style.</returns>
    private string GetStatusStyle(string status) =>
        StatusLiteral.Available.Equals(status) ? "color:green" : "color:red";
    #endregion
}
