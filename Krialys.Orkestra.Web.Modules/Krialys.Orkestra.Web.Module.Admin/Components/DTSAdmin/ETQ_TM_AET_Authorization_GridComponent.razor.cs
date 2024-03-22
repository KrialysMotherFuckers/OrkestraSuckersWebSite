using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Components;
using Krialys.Orkestra.Web.Module.Common.Components.ETQ;
using Krialys.Orkestra.Web.Module.Common.Components.Users;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.ADM.Components.DTSAdmin;
public partial class ETQ_TM_AET_Authorization_GridComponent
{
    #region DataGrid
    /// <summary>
    /// Reference to the WasmDataGrid component.
    /// </summary>
    private OrkaGenericGridComponent<ETQ_TM_AET_Authorization> _gridRef;

    /// <summary>
    /// OData query applied to the grid.
    /// </summary>
    private const string _oDataQuery = $"?$expand={nameof(ETQ_TM_AET_Authorization.TETQ_ETIQUETTE)}";

    /// <summary>
    /// Sf query used to filter and order grid.
    /// </summary>
    private readonly Query _gridQuery = new Query()
        .Sort(nameof(ETQ_TM_AET_Authorization.aet_initializing_date), FiltersLiterals.Descending)
        .AddParams(Litterals.OdataQueryParameters, _oDataQuery);
    #endregion

    #region DataGrid Edit Settings and Events
    /// <summary>
    /// Name of the edited user.
    /// </summary>
    string _userName;

    /// <summary>
    /// Get header for grid edit template.
    /// </summary>
    /// <param name="itemId">Id of edited item.</param>
    /// <returns>Edit header text.</returns>
    public string GetEditHeader(int itemId)
        => itemId.Equals(0)
            ? Trad.Keys["Administration:AddAuthorization"]
            : Trad.Keys["Administration:EditAuthorization"];

    /// <summary>
    /// Event triggered before the record is to be edited.
    /// </summary>
    /// <param name="args">Begin edit arguments.</param>
    private async Task OnBeginEditAsync(BeginEditArgs<ETQ_TM_AET_Authorization> args)
    {
        // Reset radio button value.
        _statusRadioButtonValue = _statusRadioButtonNoChangeValue;

        // Get name of the user for the selected record.
        string filter = $"{nameof(TRU_USERS.TRU_USERID)} eq '{args.RowData.aet_user_id}'";
        string select = $"{nameof(TRU_USERS.TRU_FULLNAME)}";
        var users = await ProxyCore.GetEnumerableAsync<TRU_USERS>(
            $"?$filter={filter}&$select={select}", useCache: false);

        _userName = users.FirstOrDefault()?.TRU_FULLNAME;
    }

    /// <summary>
    /// Event called when user save an edition.
    /// </summary>
    /// <param name="item">Edited item.</param>
    private async Task SaveEditAsync(ETQ_TM_AET_Authorization item)
    {
        #region Check data
        // Is the data incomplete?
        bool isDataIncomplete = false;

        // If no user is selected.
        if (item.aet_user_id is null)
        {
            // Display a tooltip.
            await _userDropDownRef.ShowTooltipAsync();

            isDataIncomplete = true;
        }

        // If no label is selected.
        if (item.aet_etiquette_id.Equals(0))
        {
            // Display a tooltip.
            await _etiquetteDropDownRef.ShowTooltipAsync();

            isDataIncomplete = true;
        }

        // If data is incomplete, don't save.
        if (isDataIncomplete)
            return;
        #endregion

        #region Prepare data
        // Don't update child items.
        item.TETQ_ETIQUETTE = null;

        // If it is an insertion.
        if (item.aet_id.Equals(0))
        {
            item.aet_initializing_user_id = Session.GetUserId();
            item.aet_initializing_date = DateExtensions.GetLocaleNowSecond();
        }
        // If it is an edition.
        else
        {
            item.aet_update_by = Session.GetUserId();
            item.aet_update_date = DateExtensions.GetLocaleNowSecond();

            // If "status" radio button value changed.
            if (!_statusRadioButtonValue.Equals(_statusRadioButtonNoChangeValue))
            {
                // Apply selected status.
                item.aet_status_id = _statusRadioButtonValue;
            }
        }
        #endregion

        // Save modified values.
        await _gridRef.DataGrid.EndEditAsync();
    }
    #endregion

    #region User DropDown
    /// <summary>
    /// Reference to the User_DropDownComponent component.
    /// </summary>
    private User_DropDownComponent _userDropDownRef;
    #endregion

    #region Label (ETQ) DropDown
    /// <summary>
    /// Reference to the Etiquette_DropDownComponent component.
    /// </summary>
    private Etiquette_DropDownComponent _etiquetteDropDownRef;
    #endregion

    #region Status RadioButton
    /// <summary>
    /// Name of the status RadioButton.
    /// </summary>
    private const string _statusRadioButtonName = "status";

    /// <summary>
    /// Default value of the status RadioButton.
    /// </summary>
    private const string _statusRadioButtonNoChangeValue = "Unchanged";

    /// <summary>
    /// Value of the status RadioButton.
    /// </summary>
    private string _statusRadioButtonValue = _statusRadioButtonNoChangeValue;

    /// <summary>
    /// Event triggered when selected radio button changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private void OnStatusChange(ChangeEventArgs args)
    {
        // Update radio button value.
        _statusRadioButtonValue = (string)args.Value;
    }
    #endregion

    #region Comment TextBox
    /// <summary>
    /// HTML attributes applied to the comment TextBox.
    /// </summary>
    private Dictionary<string, object> _commentHtmlAttributes = new Dictionary<string, object>
    {
        // Specifies the visible height of a text area, in lines.
        {"rows", "3" }
    };
    #endregion
}
