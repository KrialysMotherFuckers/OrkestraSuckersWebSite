using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.Authentication;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Buttons;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;

namespace Krialys.Orkestra.Web.Module.DTS.Components.Orders.Grid;

public partial class OrderEdit_DialogComponent
{
    #region Parameters
    /// <summary>
    /// Is dialog visible?
    /// </summary>
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    /// <summary>
    /// Edited order.
    /// </summary>
    [Parameter]
    public TCMD_COMMANDES Order { get; set; }
    #endregion

    #region Properties
    /// <summary>
    /// Is the order phase draft?
    /// </summary>
    private bool _isDraft;

    /// <summary>
    /// Can the creation mode be changed?
    /// </summary>
    private bool _canChangeCreationMode;

    /// <summary>
    /// Id of the creation mode domain.
    /// </summary>
    private int _creationModeDomainId;

    /// <summary>
    /// Id of the creation mode UTD.
    /// </summary>
    private int _creationModeUtdId;
    #endregion

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Set titles and placeholders.
        _domainTitle = $"{DataAnnotations.Display<TCMD_COMMANDES>(nameof(TCMD_COMMANDES.TDOM_DOMAINEID))}*";
        _domainDescriptionTitle = DataAnnotations.Display<TDOM_DOMAINES>(nameof(TDOM_DOMAINES.TDOM_DESC));
        _commentPlaceholder = $"{DataAnnotations.Display<TCMD_COMMANDES>(nameof(TCMD_COMMANDES.TCMD_COMMENTAIRE))}*";
        _datePickerPlaceholder = $"{DataAnnotations.Display<TCMD_COMMANDES>(nameof(TCMD_COMMANDES.TCMD_DATE_LIVRAISON_SOUHAITEE))}*";

        // Is the order in draft phase?
        _isDraft = Phases.Draft.Equals(Order.TCMD_PH_PHASE.TCMD_PH_CODE);

        // Creation mode can be changed if the order is in draft phase
        // and if it was not created by duplication.
        _canChangeCreationMode = _isDraft &&
            !CreationModes.Copy.Equals(Order.TCMD_MC_MODE_CREATION.TCMD_MC_CODE);

        // Set radio button.
        _checkedValue = Order?.TE_ETATID is not null ? CreationModes.UTD : CreationModes.Domain;

        // Set buttons states.
        UpdateButtonsStates();

        await InitializeDocumentUploaderAsync();

        await InitializeCreationModesAsync();

        await InitializeDomainAsync();

        // Hide spinner.
        _isBusy = false;
    }
    #endregion

    #region Dialog
    /// <summary>
    /// Close the dialog.
    /// </summary>
    private async Task CloseDialogAsync()
    {
        // Don't close if processing isn't completed.
        if (_isBusy)
            return;

        // Hide dialog.
        IsVisible = false;

        // Update parent with new value.
        await IsVisibleChanged.InvokeAsync(IsVisible);
    }
    #endregion

    #region Spinner
    /// <summary>
    /// Show spinner when true.
    /// Prevents the user from performing actions when loading a page.
    /// </summary>
    private bool _isBusy = true;
    #endregion

    #region RadioButton
    /// <summary>
    /// Name of radio button component.
    /// All elements of a radio button component must share this name.
    /// </summary>
    private const string _buttonName = "SelectCreationMode";

    /// <summary>
    /// Value of the checked radio button.
    /// </summary>
    private string _checkedValue;

    /// <summary>
    /// Get Ids of the creation modes.
    /// </summary>
    private async Task InitializeCreationModesAsync()
    {
        // Read creation modes from data base.
        var creationModes = await ProxyCore.GetEnumerableAsync<TCMD_MC_MODE_CREATIONS>();

        // Get creation modes by domain and by UTD.
        var tcmdMcModeCreationses = creationModes as TCMD_MC_MODE_CREATIONS[] ?? creationModes.ToArray();
        var domainCreationMode = tcmdMcModeCreationses.FirstOrDefault(c => CreationModes.Domain.Equals(c.TCMD_MC_CODE));
        var utdCreationMode = tcmdMcModeCreationses.FirstOrDefault(c => CreationModes.UTD.Equals(c.TCMD_MC_CODE));

        // Check if creation modes was read correctly.
        if (domainCreationMode is not null && utdCreationMode is not null)
        {
            // Get ids of the creation modes.
            _creationModeDomainId = domainCreationMode.TCMD_MC_MODE_CREATIONID;
            _creationModeUtdId = utdCreationMode.TCMD_MC_MODE_CREATIONID;
        }
        else
        {
            // Log error.
            var errorMessage = "Creations modes could not be read in Database.";
            _ = ProxyCore.SetLogException(new LogException(GetType(), string.Empty, errorMessage));

            // Display error toast.
            _ = Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"],
                Trad.Keys["COMMON:RequestFailed"]);

            // Close dialog.
            _ = CloseDialogAsync();
        }
    }

    /// <summary>
    /// Event triggered when the radio button value change.
    /// I.e., when a creation mode is selected.
    /// </summary>
    /// <param name="args">Button change event arguments.</param>
    private void OnCheckedValueChange(ChangeArgs<string> args)
    {
        CloseTooltips();

        // Update order creation modes ids.
        Order.TCMD_MC_MODE_CREATIONID = CreationModes.Domain.Equals(args.Value) ? _creationModeDomainId : _creationModeUtdId;
    }
    #endregion

    #region Domain AutoCompete
    /// <summary>
    /// Label of the selected domain (TDOM_LIB).
    /// </summary>
    private string _domainLabel;

    /// <summary>
    /// Sf query applied on domain table (TDOM_DOMAINES).
    /// Only domains with at least one labeled object are selected.
    /// </summary>
    private readonly Query _domainQuery = new Query()
        .AddParams(Litterals.OdataQueryParameters, $"?$filter={nameof(TOBJE_OBJET_ETIQUETTES)}/any()");

    /// <summary>
    /// Domain autocomplete title.
    /// </summary>
    private string _domainTitle;

    /// <summary>
    /// Description of the selected domain.
    /// </summary>
    private string _domainDescription;

    /// <summary>
    /// Domain description title.
    /// </summary>
    private string _domainDescriptionTitle;

    /// <summary>
    /// Initialize Domain autocomplete data.
    /// </summary>
    private async Task InitializeDomainAsync()
    {
        if (Order.TDOM_DOMAINEID is not null)
        {
            // Read selected domain in database.
            string query = $"?$filter=TDOM_DOMAINEID eq {Order.TDOM_DOMAINEID}";
            var domain = (await ProxyCore.GetEnumerableAsync<TDOM_DOMAINES>(query)).FirstOrDefault();

            // Set domain label.
            _domainLabel = domain?.TDOM_LIB;

            // Set desciption because ValueChange event won't be called.
            _domainDescription = domain?.TDOM_DESC;
        }
    }

    /// <summary>
    /// Event triggers when a domain is selected.
    /// </summary>
    /// <param name="args">Change event arguments.</param>
    private void DomainValueChange(ChangeEventArgs<string, TDOM_DOMAINES> args)
    {
        CloseTooltips();

        // Reset selected UTD.
        Order.TE_ETATID = default;
        Order.TS_SCENARIOID = default;

        // Set selected domain.
        Order.TDOM_DOMAINEID = args.ItemData?.TDOM_DOMAINEID;

        // Set domain description.
        _domainDescription = args.ItemData?.TDOM_DESC;

        // Set buttons states.
        UpdateButtonsStates();
    }
    #endregion

    #region UTD select Component
    /// <summary>
    /// Event triggered when a new etat is selected.
    /// </summary>
    /// <param name="etatId">Selected TE_ETATS (UTD).</param>
    private void OnEtatChanged(int? etatId)
    {
        CloseTooltips();

        // Reset selected domain.
        Order.TDOM_DOMAINEID = default;
        _domainLabel = default;
        _domainDescription = default;

        // Set order etat.
        Order.TE_ETATID = etatId;

        // Set buttons states.
        UpdateButtonsStates();
    }

    /// <summary>
    /// Event triggered when a new scenario is selected.
    /// </summary>
    /// <param name="scenarioId">Selected TS_SCENARIOS (module).</param>
    private void OnScenarioChanged(int? scenarioId)
    {
        CloseTooltips();

        // Set order scenario.
        Order.TS_SCENARIOID = scenarioId;
    }
    #endregion

    #region Documents uploader
    /// <summary>
    /// Reference to the uploader component.
    /// </summary>
    SfUploader _uploaderReference;

    /// <summary>
    /// Extensions of the file types allowed.
    /// </summary>
    private static string _uploaderAllowedExtensions;

    /// <summary>
    /// Maximum allowed file size to be uploaded in bytes.
    /// </summary>
    private static double _upladerMaxFileSize;

    /// <summary>
    /// URL of save action (POST request) that will receive the upload files
    /// and handle the save operation in server.
    /// </summary>
    private string _uploaderSaveUrl;

    /// <summary>
    /// Get the URL of remove action (POST request) that receives the file information 
    /// and handle the remove operation in server.
    /// </summary>
    private string _uploaderRemoveUrl;

    /// <summary>
    /// Status code of the upload.
    /// </summary>
    enum UploaderStatusCode
    {
        Failure, // Upload failed.
        Ready, // Ready to upload.
        Success // Upload succeed.
    }

    /// <summary>
    /// Configure document uploader endpoints.
    /// Read uploader parameters in database.
    /// </summary>
    private async Task InitializeDocumentUploaderAsync()
    {
        // Set endpoints.
        _uploaderSaveUrl = $"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/Orders/SaveDocument";
        _uploaderRemoveUrl = $"{Config[Litterals.ProxyUrl]}{Litterals.UniversRootPath}/Orders/RemoveDocument";

        // Get settings
        var settings = await ProxyCore.GetEnumerableAsync<TR_WST_WebSite_Settings>(useCache: false);

        // Read uploader maximum size in database.
        var documentMaxSizeValue = settings.Any()
            ? settings.FirstOrDefault(e => e.Wst_Code == ParametersIds.Command_DocumentMaxSize)?.Wst_Value
            : "";

        // Convert maximum size from string to double.
        double.TryParse(documentMaxSizeValue, out _upladerMaxFileSize);

        // Check the read parameter.
        string incorrectParameter = default;
        string incorrectValue = default;
        if (_upladerMaxFileSize.Equals(default))
        {
            incorrectParameter = ParametersIds.Command_DocumentMaxSize;
            incorrectValue = documentMaxSizeValue;
        }

        // Read uploader allowed extensions in database.
        _uploaderAllowedExtensions = settings.Any()
            ? settings.FirstOrDefault(e => e.Wst_Code == ParametersIds.Command_DocumentExtensions)?.Wst_Value
            : "";

        // Check the read parameter.
        if (string.IsNullOrWhiteSpace(_uploaderAllowedExtensions))
        {
            incorrectParameter = ParametersIds.Command_DocumentExtensions;
            incorrectValue = _uploaderAllowedExtensions;
        }

        // If a parameter is incorrect.
        if (incorrectParameter is not null)
        {
            // Display error toast.
            _ = Toast.DisplayErrorAsync(Trad.Keys["COMMON:Error"],
                string.Format(Trad.Keys["COMMON:IncorrectParameter"],
                incorrectParameter));

            // Log error.
            string errorMessage = $"The parameter {incorrectParameter} from {nameof(TR_WST_WebSite_Settings)} is incorrect.";
            _ = ProxyCore.SetLogException(
                    new LogException(GetType(), incorrectValue, errorMessage)
                );

            // Close dialog.
            _ = CloseDialogAsync();
        }
    }

    /// <summary>
    /// Event triggered before the upload process.
    /// This event is used to add additional parameters to the upload request.
    /// </summary>
    /// <param name="args">BeforeUpload event arguments.</param>
    private async Task OnBeforeUploadAsync(BeforeUploadEventArgs args)
    {
        // Get "ready to upload" status code.
        string ReadyStatusCode = ((int)UploaderStatusCode.Ready).ToString();
        // If there aren't any files to upload.
        // => Complete save/validate workflow.
        if (!args.FilesData.Any(f => ReadyStatusCode.Equals(f.StatusCode)))
            await EndSaveAsync();

        // Get authentication token.
        string token = ((AuthStateProvider)StateProvider).GetAuthorizationHeader;

        // Add authentication token to the current request authorization header.
        args.CurrentRequest = new List<object>
        {
            new { Authorization = token }
        };

        // Add parameters in data form.
        args.CustomFormData = new List<object> {
            new { orderId = Order.TCMD_COMMANDEID }
        };
    }

    /// <summary>
    /// Event triggered before removing the file from the server.
    /// </summary>
    /// <param name="args">BeforeRemove event arguments.</param>
    private void OnBeforeRemove(BeforeRemoveEventArgs args)
    {
        // Get authentication token.
        string token = ((AuthStateProvider)StateProvider).GetAuthorizationHeader;

        // Add authentication token to the current request authorization header.
        args.CurrentRequest = new List<object>
        {
            new { Authorization = token }
        };

        // Add parameters in data form.
        args.CustomFormData = new List<object> {
            new { orderId = Order.TCMD_COMMANDEID }
        };

        // If PostRawFile is set to false, it will post only the file name to the remove action
        // instead of the file representation (IFormFile object).
        args.PostRawFile = false;
    }

    /// <summary>
    /// Event triggered after all selected files have been processed for upload (successfully or not).
    /// </summary>
    /// <param name="args">ActionComplete event arguments.</param>
    private async Task OnActionCompleteAsync(Syncfusion.Blazor.Inputs.ActionCompleteEventArgs args)
    {
        bool success = true;

        // Browse through uploaded files.
        foreach (var uploadedFileInfo in args.FileData)
        {
            // Check if all files uploaded successfully.
            if (!((int)UploaderStatusCode.Success).ToString()
                .Equals(uploadedFileInfo.StatusCode))
            {
                success = false;
            }
        }

        // If uploads were successful.
        // => Complete save/validate workflow.
        if (success)
            await EndSaveAsync();

        // End of action.
        _isBusy = false;
    }

    /// <summary>
    /// Event triggered after selecting or dropping the files.
    /// </summary>
    /// <param name="args">Selected event arguments.</param>
    private void FileSelected(SelectedEventArgs args)
        // As the position of the tooltips is absolute,
        // they must be erased each time the UI changes.
        => CloseTooltips();

    /// <summary>
    /// Event triggered on removing the uploaded file.
    /// </summary>
    /// <param name="args">Removing event arguments.</param>
    private void OnRemove(RemovingEventArgs args)
        // As the position of the tooltips is absolute,
        // they must be erased each time the UI changes.
        => CloseTooltips();
    #endregion

    #region Comment TextBox
    /// <summary>
    /// Comment TextBox placeholder/title.
    /// </summary>
    private string _commentPlaceholder;

    /// <summary>
    /// HTML attributes applied to the comment TextBox.
    /// </summary>
    private Dictionary<string, object> _commentHtmlAttributes = new Dictionary<string, object>
    {
        // Specifies the visible height of a text area, in lines.
        {"rows", "3" }
    };

    /// <summary>
    /// Reference to the error Tooltip applied on "comment" TextBox.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _refCommentTooltip;

    /// <summary>
    /// Check that the comment TextBox is filled in.
    /// </summary>
    /// <returns>True, if a comment is written. False, otherwise.</returns>
    private async Task<bool> IsCommentFilledInAsync()
    {
        // If comment is not filled in.
        if (string.IsNullOrWhiteSpace(Order.TCMD_COMMENTAIRE))
        {
            // Open tooltip.
            if (_refCommentTooltip is not null)
                await _refCommentTooltip.OpenAsync();

            return false;
        }

        return true;
    }

    /// <summary>
    /// Event triggers when the content of "comment" TextBox has changed and gets focus-out.
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private async Task OnCommentChangeAsync(ChangedEventArgs args)
    {
        // If comment is not filled in.
        if (string.IsNullOrWhiteSpace(Order.TCMD_COMMENTAIRE))
        {
            // Open tooltip.
            if (_refCommentTooltip is not null)
                await _refCommentTooltip.OpenAsync();
        }
        else
        {
            // Close tooltip.
            if (_refCommentTooltip is not null)
                await _refCommentTooltip.CloseAsync();
        }
    }
    #endregion

    #region Delivery DatePicker
    /// <summary>
    /// Minimum of selectable dates => Tomorrow.
    /// </summary>
    private DateTime _minDate = DateTimeOffset.Now.Date.AddDays(1);

    /// <summary>
    /// Date picker placeholder/title.
    /// </summary>
    private string _datePickerPlaceholder;

    /// <summary>
    /// Reference to the error Tooltip applied on "delivery" DatePicker.
    /// Used to show errors.
    /// </summary>
    private SfTooltip _refDeliveryDateTooltip;

    /// <summary>
    /// Check that a delivery date is selected.
    /// </summary>
    /// <returns>True, if a delivery date is selected. False, otherwise.</returns>
    private async Task<bool> IsDeliveryDateSelectedAsync()
    {
        // Delivery date not selected.
        if (Order.TCMD_DATE_LIVRAISON_SOUHAITEE is null)
        {
            // Open tooltip.
            if (_refDeliveryDateTooltip is not null)
                await _refDeliveryDateTooltip.OpenAsync();

            return false;
        }

        return true;
    }

    /// <summary>
    /// Event triggers when the delivery date is changed.
    /// </summary>
    /// <param name="args">Changed event arguments.</param>
    private Task OnDeliveryDateChangeAsync(ChangedEventArgs<DateTime?> args)
        // Close error tooltip.
        => _refDeliveryDateTooltip.CloseAsync();
    #endregion

    #region Buttons
    /// <summary>
    /// Are buttons disabled?
    /// </summary>
    private bool _areButtonsDisabled = true;

    /// <summary>
    /// Update states enabled/disabled of the buttons.
    /// </summary>
    private void UpdateButtonsStates()
    {
        // Buttons are disabled, if neither a Domain nor an UTD is selected.
        _areButtonsDisabled = Order.TDOM_DOMAINEID.Equals(default)
            && Order.TE_ETATID.Equals(default);
    }

    /// <summary>
    /// Event called when save button is clicked.
    /// </summary>
    private async Task SaveOrderAsync()
    {
        // Prevent double click.
        if (!_isBusy)
        {
            _isBusy = true;

            await StartSaveAsync();
        }
    }

    /// <summary>
    /// Event called when validate button is clicked.
    /// </summary>
    private async Task ValidateOrderAsync()
    {
        // Prevent double click.
        if (!_isBusy)
        {
            _isBusy = true;

            // Verify that a comment is written.
            bool isOrderComplete = await IsCommentFilledInAsync();

            // Verify that a delivery date is selected.
            isOrderComplete &= await IsDeliveryDateSelectedAsync();

            // If the order is completely filled in by the user.
            if (isOrderComplete)
                await StartSaveAsync(isValidation: true);
            else
                // End of button action.
                _isBusy = false;
        }
    }
    #endregion

    #region Save/Validate actions
    /// <summary>
    /// Used to identify the validation action. 
    /// </summary>
    private bool _needValidation;

    /// <summary>
    /// Start save/validate workflow.
    /// </summary>
    private async Task StartSaveAsync(bool isValidation = false)
    {
        // Case where we are not in draft phase.
        // => We only need to upload files.
        if (!_isDraft)
        {
            // Launch file upload.
            await _uploaderReference.UploadAsync();

            return;
        }

        // Is the action a success?
        bool isSuccess = true;

        // If order is not saved yet.
        if (Order.TCMD_COMMANDEID.Equals(default))
            // Save new order in database.
            isSuccess = await OrderManagement.InsertOrderAsync(Order);
        else
            // Patch existing order in database.
            isSuccess = await OrderManagement.PatchOrderFromGridAsync(Order);

        if (isSuccess)
        {
            // Validation will be done when files are uploaded.
            _needValidation = isValidation;

            // Launch file upload.
            await _uploaderReference.UploadAsync();
        }
        else
            // End of action.
            _isBusy = false;
    }

    /// <summary>
    /// Complete save/validate workflow.
    /// </summary>
    private async Task EndSaveAsync()
    {
        // Validate order if needed.
        if (_needValidation)
        {
            await OrderManagement.ValidateOrderAsync(Order);

            _needValidation = false;
        }

        // Refresh orders.
        await OrderManagement.OnOrderChangedAsync();

        _isBusy = false;

        await CloseDialogAsync();
    }
    #endregion

    #region Tooltips
    /// <summary>
    /// Close all the tooltips.
    /// </summary>
    private void CloseTooltips()
    {
        _refCommentTooltip.CloseAsync();
        _refDeliveryDateTooltip.CloseAsync();
    }
    #endregion
}
