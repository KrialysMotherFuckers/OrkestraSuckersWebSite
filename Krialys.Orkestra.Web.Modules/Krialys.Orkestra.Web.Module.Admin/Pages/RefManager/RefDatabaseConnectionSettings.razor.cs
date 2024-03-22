using FluentValidation;
using Krialys.Common.Literals;
using Krialys.Common.Localization;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.WebApi.Proxy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Syncfusion.Blazor.Grids;

namespace Krialys.Orkestra.Web.Module.ADM.Pages.RefManager;
public partial class RefDatabaseConnectionSettings
{
    private bool _isEditMode = false;
    private bool _deleteAllowed = false;
    private bool _canAlterStatus = false;

    private IEnumerable<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings> _settingsList { get; set; } = Enumerable.Empty<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>();

    protected override async Task OnInitializedAsync()
    {
        _settingsList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>("", useCache: false);
    }

    public void OnRowSelected(RowSelectEventArgs<TR_CNX_Connections> args)
    {
        _deleteAllowed = !args.Data.Cnx_IsActive;
    }

    public void OnActionBegin(ActionEventArgs<TR_CNX_Connections> args)
    {
        _isEditMode = (new[] { Syncfusion.Blazor.Grids.Action.BeginEdit, Syncfusion.Blazor.Grids.Action.BeforeBeginEdit }.Contains(args.RequestType));

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Add)
        {
            _canAlterStatus = true;
            args.Data.Cnx_DatabaseType = (int)DatabaseType.SqlServer;
            args.Data.Cnx_CreationDate = DateTime.UtcNow;
        }

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.BeginEdit)
        {
            _canAlterStatus = !_settingsList.Any(x => x.Cnx_Id == args.Data.Cnx_Id);
        }

        if (args.RequestType == Syncfusion.Blazor.Grids.Action.Delete)
        {
            var settingsList = Enumerable.Empty<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>();
            Task.Run(async () => { settingsList = await ProxyCore.GetEnumerableAsync<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>($"?$filter= {nameof(Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings.Cnx_Id)} eq {args.Data.Cnx_Id}", useCache: false); });
            if (settingsList.Any())
            {
                Snackbar.Add(Trad.Keys["RefManager:DbConnection_UnauthorizedDeletion"], MudBlazor.Severity.Warning);
                args.Cancel = true;
            }
        }
    }
}

#region Validator


public class RefDatabaseConnectionSettingsValidatorNew : AbstractValidator<TR_CNX_Connections>, ICustomFluentValidator
{
    public object Param { get; set; }

    public RefDatabaseConnectionSettingsValidatorNew()
    {
        RuleFor(e => e.Cnx_Code).NotEmpty().Must(IsUnique).WithMessage("Already Exist.");
        RuleFor(e => e.Cnx_Label).NotEmpty();
        RuleFor(e => e.Cnx_DatabaseType).GreaterThanOrEqualTo(0);
        RuleFor(e => e.Cnx_ServerName).NotEmpty();
        RuleFor(e => e.Cnx_Port).GreaterThanOrEqualTo(0);
        RuleFor(e => e.Cnx_DatabaseName).NotEmpty();
        RuleFor(e => e.Cnx_Login).NotEmpty();
        RuleFor(e => e.Cnx_Password).NotEmpty();
    }

    bool IsUnique(string code)
    {
        //if (Param != null)
        //{
        //    var list = Param as IEnumerable<Krialys.Data.EF.RefManager.TM_RFS_ReferentialSettings>;
        //}

        return true;
    }
}

public interface ICustomFluentValidator : FluentValidation.IValidator
{
    public object Param { get; set; }
}

public class OrkaFluentValidator<TValidator> : ComponentBase where TValidator : FluentValidation.IValidator, new()
{
    [Parameter] public object Param { get; set; }

    [CascadingParameter] private EditContext EditContext { get; set; }

    [Inject] private ILanguageContainerService _trad { get; set; }
    [Inject] private ISnackbar _snackbar { get; set; }

    private readonly static char[] separators = new[] { '.', '[' };
    private TValidator validator;

    protected override void OnInitialized()
    {
        validator = new TValidator();
        var messages = new ValidationMessageStore(EditContext);

        EditContext.OnFieldChanged += (sender, eventArgs) => ValidateModel((EditContext)sender, messages);
        EditContext.OnValidationRequested += (sender, eventArgs) => ValidateModel((EditContext)sender, messages);
    }

    private void ValidateModel(EditContext editContext, ValidationMessageStore messages)
    {
        var context = new ValidationContext<object>(editContext.Model);
        var validationResult = validator.Validate(context);
        messages.Clear();

        validationResult.Errors.ForEach(error =>
        {
            var fieldIdentifier = ToFieldIdentifier(editContext, error.PropertyName);
            messages.Add(fieldIdentifier, error.ErrorMessage);
        });
        editContext.NotifyValidationStateChanged();
    }

    private FieldIdentifier ToFieldIdentifier(EditContext editContext, string propertyPath)
    {
        var obj = editContext.Model;

        while (true)
        {
            var nextTokenEnd = propertyPath.IndexOfAny(separators);
            if (nextTokenEnd < 0) return new FieldIdentifier(obj, propertyPath);

            var nextToken = propertyPath.Substring(0, nextTokenEnd);
            propertyPath = propertyPath.Substring(nextTokenEnd + 1);

            object newObj;
            if (nextToken.EndsWith("]"))
            {
                nextToken = nextToken.Substring(0, nextToken.Length - 1);
                var prop = obj.GetType().GetProperty("Item");
                var indexerType = prop.GetIndexParameters()[0].ParameterType;
                var indexerValue = Convert.ChangeType(nextToken, indexerType);
                newObj = prop.GetValue(obj, new object[] { indexerValue });
            }
            else
            {
                var prop = obj.GetType().GetProperty(nextToken);
                if (prop == null)
                {
                    _snackbar.Add($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.", MudBlazor.Severity.Error);
                    throw new InvalidOperationException($"Could not find property named {nextToken} on object of type {obj.GetType().FullName}.");
                }
                newObj = prop.GetValue(obj);
            }

            if (newObj == null) return new FieldIdentifier(obj, nextToken);

            obj = newObj;
        }
    }
}

public class RefDatabaseConnectionSettingsValidator : ComponentBase, IDisposable
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
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_Code)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_Label)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_DatabaseType)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_ServerName)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_Port)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_DatabaseName)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_Login)));
        HandleValidation(CurrentEditContext.Field(nameof(TR_CNX_Connections.Cnx_Password)));
    }

    protected void HandleValidation(FieldIdentifier identifier = default(FieldIdentifier))
    {
        var data = CurrentEditContext.Model as TR_CNX_Connections;

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_Code)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Code), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_CNX_Connections).Cnx_Code))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Code), false, _trad.Keys["COMMON:RequiredField"]);
            }
            else
            {
                //if (!isEditMode)
                //{

                //    var list = Enumerable.Empty<Krialys.Data.EF.RefManager.TR_CNX_Connections>();
                //    Task.Run(async () => { list = await _proxy.GetEnumerableAsync<Krialys.Data.EF.RefManager.TR_CNX_Connections>("", useCache: false); });

                //    if (list.Any(x => x.Rfs_id != data.Rfs_id && x.Rfs_TableName.ToLower() == data.Rfs_TableName.ToLower()))
                //    {
                //    //    messageStore.Add(identifier, _trad.Keys["COMMON:DuplicateELement"]);
                //    //    context.ShowValidationMessage(nameof(TM_RFS_ReferentialSettings.Rfs_TableName), false, _trad.Keys["COMMON:DuplicateELement"]);
                //    }
                //}
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_Label)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Label), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_CNX_Connections).Cnx_Label))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Label), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_DatabaseType)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_DatabaseType), true, "");

            if ((context.Data as TR_CNX_Connections).Cnx_DatabaseType < 0)
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_DatabaseType), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_ServerName)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_ServerName), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_CNX_Connections).Cnx_ServerName))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_ServerName), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_Port)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Port), true, "");

            if ((context.Data as TR_CNX_Connections).Cnx_Port < 0)
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Port), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_DatabaseName)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_DatabaseName), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_CNX_Connections).Cnx_DatabaseName))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_DatabaseName), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

        if (identifier.FieldName.Equals(nameof(TR_CNX_Connections.Cnx_Password)))
        {
            messageStore.Clear(identifier);
            context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Password), true, "");

            if (string.IsNullOrEmpty((context.Data as TR_CNX_Connections).Cnx_Password))
            {
                messageStore.Add(identifier, _trad.Keys["COMMON:RequiredField"]);
                context.ShowValidationMessage(nameof(TR_CNX_Connections.Cnx_Password), false, _trad.Keys["COMMON:RequiredField"]);
            }
        }

    }
}

#endregion