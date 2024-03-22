using Krialys.Common.Literals;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.Common.Components.YesNoDropdown;

/// <summary>
/// A yes/no dropdown used to edit a field of an item.
/// </summary>
/// <typeparam name="TItem">Type of the edited item.</typeparam>
public partial class YesNoDropdownComponent<TItem> where TItem : class, new()
{
    #region Parameters
    /// <summary>
    /// Name of the edited field.
    /// Used to generate DataAnnotations and placeholder.
    /// </summary>
    [Parameter, EditorRequired] public string Field { get; set; }

    /// <summary>
    /// Selected value.
    /// </summary>
    [Parameter] public string Value { get; set; }

    /// <summary>
    /// EventCallback used to pass the value to the parent component.
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Is edition enabled ?
    /// </summary>
    [Parameter] public bool Enabled { get; set; } = true;
    #endregion

    #region Data
    /// <summary>
    /// Booleans are stored as text in DataBase.
    /// This class is used to translate DataBase values to localized text.
    /// </summary>
    private class BooleanAsText
    {
        /// <summary>
        /// Boolean label (depends on the localization).
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Boolean value as Text ("O" or "N").
        /// </summary>
        public string Value { get; set; }
    }

    /// <summary>
    /// List of localized data for booleans Yes/No.
    /// </summary>
    private List<BooleanAsText> _yesNoData
        => new List<BooleanAsText>
        {
            new BooleanAsText
            {
                Label = Trad.Keys["BOOL:O"],
                Value = StatusLiteral.Yes
            },
            new BooleanAsText
            {
                Label = Trad.Keys["BOOL:N"],
                Value = StatusLiteral.No
            }
        };
    #endregion

    #region Events
    /// <summary>
    /// this event triggers when the DropDown List value is changed.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private async Task ValueChangeAsync(ChangeEventArgs<string, BooleanAsText> args)
    {
        if (args.ItemData is not null)
            await ValueChanged.InvokeAsync(args.ItemData.Value);
        else
            await ValueChanged.InvokeAsync(null);
    }
    #endregion
}
