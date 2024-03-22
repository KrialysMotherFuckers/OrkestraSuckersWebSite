using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Module.Common.Models;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.ADM.Pages.Settings;
public partial class EmailTemplateSettings
{
    [Inject] private ITR_MEL_EMail_TemplatesClient _iTR_MEL_EMail_TemplatesClient { get; set; }
    [Inject] private IUserSessionStatus _userSession { get; set; }

    private SfGrid<Krialys.Orkestra.Web.Common.ApiClient.TR_MEL_EMail_Templates> _ref_Grid;
    private bool _deleteAllowed = false;

    private IEnumerable<Krialys.Data.EF.Univers.TR_LNG_Languages> _languageList { get; set; } = Enumerable.Empty<Krialys.Data.EF.Univers.TR_LNG_Languages>();
    private List<Tuple<string, string>> _statusList { get; set; } = new();
    private List<Tuple<string, string>> _importanceList { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        _languageList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.Univers.TR_LNG_Languages>();

        _statusList.Add(new Tuple<string, string>(StatusLiteral.Available, Trad.Keys[$"STATUS:{StatusLiteral.Available}"]));
        _statusList.Add(new Tuple<string, string>(StatusLiteral.Deactivated, Trad.Keys[$"STATUS:{StatusLiteral.Deactivated}"]));

        _importanceList.Add(new Tuple<string, string>("H", Trad.Keys[$"Email:Importance_H"]));
        _importanceList.Add(new Tuple<string, string>("N", Trad.Keys[$"Email:Importance_N"]));
        _importanceList.Add(new Tuple<string, string>("L", Trad.Keys[$"Email:Importance_L"]));
    }

    public void OnRowSelected(RowSelectEventArgs<Krialys.Orkestra.Web.Common.ApiClient.TR_MEL_EMail_Templates> args)
    {
        _deleteAllowed = (args.Data.Sta_code == StatusLiteral.Draft);
    }

    public void OnActionBegin(ActionEventArgs<Krialys.Orkestra.Web.Common.ApiClient.TR_MEL_EMail_Templates> args)
    {
        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Add)
        {
            args.Data.Lng_code = _languageList.First().lng_code;
            args.Data.Mel_email_importance = "N";
            args.Data.Sta_code = StatusLiteral.Available;
        }

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Save)
        {
            if (string.IsNullOrEmpty(args.Data.Mel_created_by))
            {
                args.Data.Mel_creation_date = DateTime.UtcNow;
                args.Data.Mel_created_by = _userSession.GetUserName();
            }
            else
            {
                args.Data.Mel_update_date = DateTime.UtcNow;
                args.Data.Mel_update_by = _userSession.GetUserName();
            }
        }
    }

    private async Task OnStatusChanged(SelectEventArgs<Tuple<string, string>> args, Krialys.Orkestra.Web.Common.ApiClient.TR_MEL_EMail_Templates _currentRecord)
    {
        _ref_Grid.PreventRender(false);
        _currentRecord.Sta_code = args.ItemData.Item1;
        await InvokeAsync(StateHasChanged);
    }
}

#region Validator

public class EmailTemplateSettingsValidator : ComponentBase, IDisposable
{
    [Parameter] public ValidatorTemplateContext context { get; set; }
    [Parameter] public bool isEditMode { get; set; }
    [CascadingParameter] private EditContext CurrentEditContext { get; set; }

    [Inject] IHttpProxyCore _proxy { get; set; }
    [Inject] private ILanguageContainerService _trad { get; set; }

    private ValidationMessageStore messageStore;

    protected override void OnInitialized()
    {
        messageStore = new ValidationMessageStore(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += ValidateRequested;
        CurrentEditContext.OnFieldChanged += ValidateField;
    }

    public void Dispose()
    {
        CurrentEditContext.OnValidationRequested -= ValidateRequested;
        CurrentEditContext.OnFieldChanged -= ValidateField;
        GC.SuppressFinalize(this);
    }

    protected void ValidateField(object editContext, FieldChangedEventArgs fieldChangedEventArgs) => HandleValidation(fieldChangedEventArgs.FieldIdentifier);
    private void ValidateRequested(object editContext, ValidationRequestedEventArgs validationEventArgs)
    {
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_code)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_description)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_email_recipients)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_email_subject)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_email_body)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_MEL_EMail_Templates.Mel_email_footer)));
    }

    protected void HandleValidation(FieldIdentifier identifier = default(FieldIdentifier))
    {
        var data = CurrentEditContext.Model as TR_MEL_EMail_Templates;

        if (identifier.FieldName.Equals(nameof(TR_MEL_EMail_Templates.Mel_code)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_code), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_MEL_EMail_Templates).Mel_code))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_code), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_MEL_EMail_Templates.Mel_description)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_description), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_MEL_EMail_Templates).Mel_description))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_description), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_MEL_EMail_Templates.Mel_email_subject)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_subject), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_MEL_EMail_Templates).Mel_email_subject))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_subject), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_MEL_EMail_Templates.Mel_email_body)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_body), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_MEL_EMail_Templates).Mel_email_body))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_body), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_MEL_EMail_Templates.Mel_email_footer)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_footer), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_MEL_EMail_Templates).Mel_email_footer))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_MEL_EMail_Templates.Mel_email_footer), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }
    }
}

#endregion