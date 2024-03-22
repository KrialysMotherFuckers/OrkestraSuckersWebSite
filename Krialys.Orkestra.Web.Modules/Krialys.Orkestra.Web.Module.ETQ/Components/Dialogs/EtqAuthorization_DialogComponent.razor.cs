using Krialys.Common.Literals;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Shared;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.Web.Module.ETQ.Components.Dialogs;

public partial class EtqAuthorization_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    /// <summary>
    /// Selected label.
    /// </summary>
    [Parameter] public TETQ_ETIQUETTES Label { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Dialog data.
    /// Used by the method applying authorizations on a label.
    /// </summary>
    private EtqAuthorizationArguments args = new();
    #endregion

    #region Blazor life cycle
    protected override void OnParametersSet()
    {
        // Set dialog data.
        args.IsAccessPublic = Label.etq_is_public_access;
    }
    #endregion

    #region IsAccessPublic RadioButton
    /// <summary>
    /// Name of the radio button component.
    /// </summary>
    private const string _IsAccessPublicRadioButtonName = "IsAccessPublic";
    #endregion

    #region User MultiSelect
    /// <summary>
    /// Query applied on user multi-select.
    /// Select active users, sorted alphabetically.
    /// </summary>
    private readonly Query _userQuery = new Query()
        .Where(nameof(TRU_USERS.TRU_STATUS), FiltersLiterals.Equal, StatusLiteral.Available)
        .Sort(nameof(TRU_USERS.TRU_FULLNAME), FiltersLiterals.Ascending);
    #endregion

    #region Authorization RadioButton
    /// <summary>
    /// Name of the radio button component.
    /// </summary>
    private const string _AuthorizeRadioButtonName = "Authorize";
    #endregion

    #region Dialog
    /// <summary>
    /// Close the dialog.
    /// </summary>
    private async Task CloseDialogAsync()
    {
        // Hide dialog.
        IsVisible = false;

        // Update parent with new value.
        await IsVisibleChanged.InvokeAsync(IsVisible);

        await OnClose.InvokeAsync(this);
    }

    /// <summary>
    /// Event called when save button is clicked.
    /// </summary>
    private async Task SaveAsync()
    {
        // Apply label authorizations.
        var response = await ProxyCore.SetEtiquetteAuthorizations(Label.TETQ_ETIQUETTEID, args);

        // If request failed.
        if (!response.IsSuccessStatusCode)
        {
            // Display error message.
            await Toast.DisplayErrorAsync(Trad.Keys["GridSource:GridError"],
                Trad.Keys["COMMON:RequestFailed"]);

            return;
        }

        // Close dialog.
        await CloseDialogAsync();
    }
    #endregion
}
