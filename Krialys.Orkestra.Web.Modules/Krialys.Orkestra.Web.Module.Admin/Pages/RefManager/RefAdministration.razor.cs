using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Data.EF.Mso;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Common.Models;
using Krialys.Orkestra.Web.Module.Common.Models;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Grids;
using System.Collections;

namespace Krialys.Orkestra.Web.Module.ADM.Pages.RefManager;

public partial class RefAdministration
{

    [Inject] private IUserSessionStatus _userSession { get; set; }

    private SfGrid<TM_RFS_ReferentialSettings> _refGrid { get; set; }

    public class TypologyTypeItem
    {
        public Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType Code { get; set; }
        public string Label { get; set; }
    }

    private MudForm _mudForm;

    private IEnumerable<Krialys.Data.EF.RefManager.TR_CNX_Connections> _cnxList { get; set; } = Enumerable.Empty<Krialys.Data.EF.RefManager.TR_CNX_Connections>();
    private IEnumerable<Krialys.Data.EF.Univers.TRU_USERS> _userDataStewardList { get; set; } = Enumerable.Empty<Krialys.Data.EF.Univers.TRU_USERS>();
    private IEnumerable<Krialys.Data.EF.Univers.TS_SCENARIOS> _scenariosList { get; set; } = Enumerable.Empty<Krialys.Data.EF.Univers.TS_SCENARIOS>();
    private bool _isEditMode = false;

    private List<StatusItem> _statusList { get; set; } = new();
    private List<TypologyTypeItem> _typology { get; set; } = new();

    private bool _deleteAllowed = false;

    private TM_RFS_ReferentialSettings _currentRecord;
    private MudBlazor.MudCheckBox<bool> _rollBackChkBox;

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            new List<Task>()
                {
                    Task.Run(async () => { _cnxList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.RefManager.TR_CNX_Connections>($"?$filter= {nameof(Krialys.Data.EF.RefManager.TR_CNX_Connections.Cnx_IsActive)}", useCache: false); }),
                    Task.Run(async () => { _userDataStewardList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.Univers.TRU_USERS>($"?$orderby= {nameof(Krialys.Data.EF.Univers.TRU_USERS.TRU_FULLNAME)} & {nameof(Krialys.Data.EF.Univers.TRU_USERS.TRU_STATUS)} eq {StatusLiteral.Available}", useCache: false); }),
                    Task.Run(async () => { _scenariosList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.Univers.TS_SCENARIOS>($"?$expand= {nameof(Krialys.Data.EF.Univers.TS_SCENARIOS.TE_ETAT)} & {nameof(Krialys.Data.EF.Univers.TS_SCENARIOS.TRST_STATUTID)} eq {StatusLiteral.Available} $orderby= {nameof(Krialys.Data.EF.Univers.TS_SCENARIOS.TS_NOM_SCENARIO)}", useCache: false); }),
                });

        _statusList.Add(new StatusItem() { Label = @Trad.Keys[$"STATUS:{StatusLiteral.Draft}"], Code = StatusLiteral.Draft });
        _statusList.Add(new StatusItem() { Label = @Trad.Keys[$"STATUS:{StatusLiteral.Available}"], Code = StatusLiteral.Available });
        _statusList.Add(new StatusItem() { Label = @Trad.Keys[$"STATUS:{StatusLiteral.Deactivated}"], Code = StatusLiteral.Deactivated });

        _typology.Add(new TypologyTypeItem() { Label = @Trad.Keys[$"RefManager:Settings_Typology_None"], Code = Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType.None });
        _typology.Add(new TypologyTypeItem() { Label = @Trad.Keys[$"RefManager:Settings_Typology_WithLabel"], Code = Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType.WithLabel });
        _typology.Add(new TypologyTypeItem() { Label = @Trad.Keys[$"RefManager:Settings_Typology_WithoutLabelAddReplace"], Code = Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType.WithoutLabelAddReplace });
        _typology.Add(new TypologyTypeItem() { Label = @Trad.Keys[$"RefManager:Settings_Typology_WithoutLabelUpdate"], Code = Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType.WithoutLabelUpdate });
    }

    public void OnRowSelected(RowSelectEventArgs<TM_RFS_ReferentialSettings> args)
    {
        _deleteAllowed = (args.Data.Rfs_StatusCode == StatusLiteral.Draft);
    }

    public void OnActionBegin(ActionEventArgs<TM_RFS_ReferentialSettings> args)
    {
        _isEditMode = false;

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Add)
        {
            Task.Run(async () => { _cnxList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.RefManager.TR_CNX_Connections>("?$filter=Cnx_IsActive", useCache: false); });
            if (!_cnxList.Any())
            {
                Snackbar.Add(Trad.Keys["RefManager:Settings_ConnectionMissing"], Severity.Error);
                args.Cancel = true;
            }
            else
            {
                args.Data.Rfs_TableTypology = RefManagerTypologyType.None;
                args.Data.Cnx_Id = _cnxList.FirstOrDefault().Cnx_Id;
                args.Data.Rfs_ManagerId = string.Empty;
                args.Data.Rfs_StatusCode = StatusLiteral.Draft;
            }
        }

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.BeginEdit)
        {
            _isEditMode = true;
        }

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Save)
        {
            if (string.IsNullOrEmpty(args.Data.Rfs_CreatedBy))
            {
                args.Data.Rfs_CreationDate = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)); // mandatory SF is unable to deal with miliseconds
                args.Data.Rfs_CreatedBy = _userSession.GetUserName();
            }
            else
            {
                args.Data.Rfs_UpdateDate = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(1)); // mandatory SF is unable to deal with miliseconds
                args.Data.Rfs_UpdateBy = _userSession.GetUserName();
            }
        }
    }

    private async Task OnTypologyChanged(SelectEventArgs<TypologyTypeItem> args, TM_RFS_ReferentialSettings _currentRecord)
    {
        _refGrid.PreventRender(false);

        _currentRecord.Rfs_TableTypology = ((TypologyTypeItem)args.ItemData).Code;
        if (_currentRecord.Rfs_IsBackupNeeded && _currentRecord.Rfs_TableTypology == RefManagerTypologyType.WithLabel)
            _currentRecord.Rfs_IsBackupNeeded = false;

        if ((int)_currentRecord.Rfs_TableTypology != (int)Krialys.Orkestra.Web.Common.ApiClient.RefManagerTypologyType.WithLabel)
        {
            _currentRecord.Rfs_ParamLabelObjectCode = null;
            _currentRecord.Rfs_LabelCodeFieldName = null;
            _currentRecord.Rfs_ScenarioId = null;
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task OnStatusChanged(SelectEventArgs<StatusItem> args, TM_RFS_ReferentialSettings _currentRecord)
    {
        _refGrid.PreventRender(false);
        _currentRecord.Rfs_StatusCode = ((StatusItem)args.ItemData).Code;
        await InvokeAsync(StateHasChanged);
    }
}

#region Validator

public class RefAdministrationValidator : ComponentBase, IDisposable
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
        HandleValidation(CurrentEditContext.Field(nameof(TM_RFS_ReferentialSettings.Rfs_TableName)));
        HandleValidation(CurrentEditContext.Field(nameof(TM_RFS_ReferentialSettings.Rfs_TableFunctionalName)));
        HandleValidation(CurrentEditContext.Field(nameof(TM_RFS_ReferentialSettings.Cnx_Id)));
        HandleValidation(CurrentEditContext.Field(nameof(TM_RFS_ReferentialSettings.Rfs_TableQuerySelect)));
        HandleValidation(CurrentEditContext.Field(nameof(TM_RFS_ReferentialSettings.Rfs_TableSchema)));
    }

    protected void HandleValidation(FieldIdentifier identifier = default(FieldIdentifier))
    {
        var data = CurrentEditContext.Model as TM_RFS_ReferentialSettings;

        if (identifier.FieldName.Equals(nameof(TM_RFS_ReferentialSettings.Rfs_TableName)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableName), true, "");

            if (string.IsNullOrEmpty((context.Data as TM_RFS_ReferentialSettings).Rfs_TableName))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableName), false, _trad.Keys["COMMON:RequiredField"]);
            }
            else
            {
                if (!isEditMode)
                {

                    var list = Enumerable.Empty<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>();
                    Task.Run(async () => { list = await _proxy.GetEnumerableAsync<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>($"?$TRST_STATUTID eq {StatusLiteral.Available}", useCache: false); });

                    if (list.Any(x => x.Rfs_id != data.Rfs_id && x.Rfs_TableName.ToLower() == data.Rfs_TableName.ToLower()))
                    {
                        messageStore.Add(identifier, _trad.Keys["COMMON:DuplicateELement"]);
                        context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableName), false, _trad.Keys["COMMON:DuplicateELement"]);
                    }
                }
            }
        }

        if (identifier.FieldName.Equals(nameof(TM_RFS_ReferentialSettings.Rfs_TableFunctionalName)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableFunctionalName), true, "");

            if (string.IsNullOrEmpty((context.Data as TM_RFS_ReferentialSettings).Rfs_TableFunctionalName))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableFunctionalName), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TM_RFS_ReferentialSettings.Cnx_Id)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Cnx_Id), true, "");

            if ((context.Data as TM_RFS_ReferentialSettings).Cnx_Id <= 0)
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Cnx_Id), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TM_RFS_ReferentialSettings.Rfs_TableSchema)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableSchema), true, "");

            if (string.IsNullOrEmpty((context.Data as TM_RFS_ReferentialSettings).Rfs_TableSchema))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableSchema), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TM_RFS_ReferentialSettings.Rfs_TableQuerySelect)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableQuerySelect), true, "");

            if (string.IsNullOrEmpty((context.Data as TM_RFS_ReferentialSettings).Rfs_TableQuerySelect))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableQuerySelect), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }
    }
}

#endregion