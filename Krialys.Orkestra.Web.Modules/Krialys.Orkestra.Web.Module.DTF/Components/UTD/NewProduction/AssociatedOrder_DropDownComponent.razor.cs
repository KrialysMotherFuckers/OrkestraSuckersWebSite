using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.DTF.Components.UTD.NewProduction;
public partial class AssociatedOrder_DropDownComponent
{
    #region Parameters
    /// <summary>
    /// Id of associated order.
    /// </summary>
    [Parameter] public int? OrderId { get; set; }
    [Parameter] public EventCallback<int?> OrderIdChanged { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Comment for the selected Order.
    /// </summary>
    private string _orderComment;
    #endregion

    #region Dropdown
    /// <summary>
    /// OData query applied to the dropdown.
    /// </summary>
    private const string _oDataQuery = $"?$expand={nameof(TCMD_COMMANDES.TRU_COMMANDITAIRE)}";

    /// <summary>
    /// Sf query used to filter and order dropdown list:
    ///  - Sorted by descending creation date,
    ///  - Where order phase is "in progress",
    ///  - Expanded with order creator (TRU_COMMANDITAIRE).
    /// </summary>
    private readonly Query _dropdownQuery = new Query()
        .Sort(nameof(TCMD_COMMANDES.TCMD_DATE_CREATION), "descending")
        .Where($"{nameof(TCMD_COMMANDES.TCMD_PH_PHASE)}.{nameof(TCMD_PH_PHASES.TCMD_PH_CODE)}",
            FiltersLiterals.Equal, Phases.InProgress)
        .AddParams(Litterals.OdataQueryParameters, _oDataQuery);

    /// <summary>
    /// Event triggers when selected value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private Task ValueChangeAsync(ChangeEventArgs<int, TCMD_COMMANDES> args)
    {
        // Update comment.
        _orderComment = args.ItemData?.TCMD_COMMENTAIRE;

        // Update parent with new value.
        return OrderIdChanged.InvokeAsync(args.ItemData?.TCMD_COMMANDEID);
    }
    #endregion
}
