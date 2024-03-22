using BlazorComponentBus;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using static Krialys.Orkestra.Common.Shared.Logs;
using Action = Syncfusion.Blazor.Grids.Action;

namespace Krialys.Orkestra.Web.Module.Common.Components.DTF.UTD;
public partial class VDE_DEMANDES_ETENDUES_GridComponent
{
    #region Injected services
    [Inject] private ILogger<VDE_DEMANDES_ETENDUES_GridComponent> Logger { get; set; }
    [Inject] private ICpuClient _iCpuClient { get; set; }
    #endregion

    #region Parameters
    /// <summary>
	/// Productions displayed in the grid.
	/// </summary>
	[Parameter]
    public IEnumerable<Data.EF.Univers.VDE_DEMANDES_ETENDUES> Productions { get; set; }

    /// <summary>
	/// Is it enable to launch a new production?
	/// </summary>
	[Parameter]
    public bool CanLaunchProductions { get; set; }

    /// <summary>
    /// Displayed columns.
    /// </summary>
    [Parameter]
    public string[] DisplayedColumns { get; set; } =
    {
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_DEMANDEID),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.STATUT_DEMANDE_FR),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_QUALIF_BILAN),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.CATEGORIE),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TE_NOM_ETAT),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TS_NOM_SCENARIO),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.DEMANDEUR),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_DATE_PIVOT),
        nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_COMMENTAIRE_UTILISATEUR),
        RessourcesColumn,
        ResultsColumn
    };

    /// <summary>
    /// Is "new production" dialog visible?
    /// </summary>
    [Parameter]
    public bool IsNewProductionDisplayed { get; set; }

    [Parameter]
    public EventCallback<bool> IsNewProductionDisplayedChanged { get; set; }
    #endregion

    private bool isDataDriven;
    private bool isDTFAdm;
    private bool isDTFConsul;
    private Query gridQuery = new Query();

    private const string RessourcesColumn = "Ressources";
    private const string ResultsColumn = "Results";

    protected override async Task OnInitializedAsync()
    {
        InitializeStatusList();

        var _ = await Session.VerifyPolicies(new[] {
            PoliciesLiterals.DTFDataDriven, PoliciesLiterals.DTFAdm, PoliciesLiterals.DTFConsul
        });

        (isDataDriven, isDTFAdm, isDTFConsul) =
            (_[PoliciesLiterals.DTFDataDriven], _[PoliciesLiterals.DTFAdm], _[PoliciesLiterals.DTFConsul]);

        // If no data are passed as parameters, then construct grid query based on user rights.
        if (Productions is null)
        {
            string queryOptions = null;
            if (isDataDriven)
            {
                // le double filtre sur TRU_USERID
                // le 1er contribue  a ne ramener que les enregistrements de VDTFH_HABILITATION avec userID  =x
                // le 2nd pour permettre de ramener que les enregistrements avec userid pour lesquels on a au moins userID  =x
                // les 2 sont requis en tout cas
                queryOptions = $"?$expand=TS_SCENARIO($expand=VDTFH_HABILITATION($filter=TRU_USERID eq '{Session.GetUserId()}'))&$filter=TS_SCENARIO/VDTFH_HABILITATION/any(o: o/TRU_USERID eq '{Session.GetUserId()}')";
            }
            else if (isDTFAdm || isDTFConsul)
            {
                // à banir pas de $orderby, cf plus bas
            }
            else
            /* si pas un role attendu on force a ne pas ramener d'enregistrements */
            {
                queryOptions = "?$filter=TD_DEMANDEID eq 0";
            }

            gridQuery.AddParams(Litterals.OdataQueryParameters, queryOptions)
                .Sort(nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_DATE_PIVOT), "Descending");
        }
        else
        {
            gridQuery.Sort(nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.TD_DATE_PIVOT), "Descending");
        }

        // Subscribe to TrackedEntity via IComponentBus
        Bus.Subscribe<IList<TrackedEntity>>(OnTrackedVDEAsync);
    }

    /// <summary>
    /// Entity list to be observed
    /// </summary>
    private Type[] EntityTypeList { get; } = {
        typeof(Data.EF.Univers.VDE_DEMANDES_ETENDUES),
        typeof(Data.EF.Univers.TBD_BATCH_DEMANDES),
    };

    [ThreadStatic]
    private static int _prevId = 0;

    /// <summary>
    /// IComponentBus's callback for managing automatic smooth DataGrid refresh
    /// Retain the following Uuid: ConfirmePriseEnChargeDemande, SetAvancementBatchDemande and FinalizeDemande
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    private async Task OnTrackedVDEAsync(MessageArgs args, CancellationToken ct)
    {
        // Where UuidOrAny is not originated from ApplicationClientSessionId
        if (Ref_VDE_DEMANDES_ETENDUES.DataGrid != null)
        {
            // Get the tracking
            var tracked = args
                .GetMessage<IList<TrackedEntity>>()
                .FirstOrDefault(e => e.FullName.Equals(typeof(Data.EF.Univers.VDE_DEMANDES_ETENDUES).FullName)
                    && !string.IsNullOrEmpty(e.UuidOrAny)
                    && !e.UuidOrAny.Equals(ProxyCore.ApplicationClientSessionId, StringComparison.Ordinal)
                    // We only need to intercept these messages coming from CRUD via CpuServices and/or coming form UI as well
                    && (e.UuidOrAny.Equals("ConfirmePriseEnChargeDemande", StringComparison.Ordinal)
                        || e.UuidOrAny.Equals("SetAvancementBatchDemande", StringComparison.Ordinal)
                        || e.UuidOrAny.Equals("FinalizeDemande", StringComparison.Ordinal)
                        || e.UuidOrAny.Equals("CreateDemande", StringComparison.Ordinal)
                        || e.UuidOrAny.Equals("CancelDemande", StringComparison.Ordinal)
                        || e.UuidOrAny.Equals("PUCentralAnalysePrepareDemandesEligibles", StringComparison.Ordinal))
                    // OB-345: we need Insert + Update
                    && (e.Action.Equals("Insert", StringComparison.Ordinal) || e.Action.Equals("Update", StringComparison.Ordinal)));

            if (tracked != null)
            {
                if (_prevId != tracked.Id)
                {
                    _prevId = tracked.Id;

                    // Remove entity from cache, then refresh DataGrid to reflect changes
                    var entity = EntityTypeList.FirstOrDefault(e => e.FullName != null && e.FullName.Equals(tracked.FullName, StringComparison.Ordinal));

                    // Remove TEntity from cache, then refresh DataGrid to reflect changes
                    ProxyCore.CacheRemoveEntities(entity);

                    //#if DEBUG
                    // Trace to browser
                    Logger.LogWarning($"[TRK-UTD] Date: {tracked.Date:R}, Id: {tracked.Id}, Entity: {entity?.Name}, Action: {tracked.Action}, UserId: {tracked.UserId}, Uuid: {tracked.UuidOrAny}");
                    //#endif

                    // Refresh datagrid (strict minimum action expected), but indirectly
                    await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, ct, Ref_VDE_DEMANDES_ETENDUES.GetRefreshGridButtonId, 250);
                }
            }
        }
    }

    /// <summary>
    /// Used as callback to refill datagrid datasource
    /// Call when refresh is done by user himself or by Tracker
    /// Nettoyage du cache a l'initialisation et a chaque refresh pour garantir données a jour des modifs personnelles ou provenant d'autres utilisateurs ou système
    /// </summary>
    /// <returns></returns>
    //private void OnDataSourceLoad()
    //    => ProxyCore.CacheRemoveEntities(typeof(VDE_DEMANDES_ETENDUES));

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the rendering of all the references 
    /// to the component are populated.
    /// Use this stage to perform additional initialization steps with the rendered content,
    /// such as JS interop calls that interact with the rendered DOM elements.
    /// </summary>
    /// <param name="firstRender">Is the component rendered for the first time? </param>
    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && CanLaunchProductions)
        {
            // Add "Produce" button to the Toolbar.
            AddProduceToolbarButton();

            // Add "Cancel" button to the context menu.
            AddCancelContextMenuButton();
        }

        return base.OnAfterRenderAsync(firstRender);
    }
    #endregion

    #region Grid
    /// <summary>
    /// Reference to the grid component.
    /// </summary>
    private OrkaGenericGridComponent<Data.EF.Univers.VDE_DEMANDES_ETENDUES> Ref_VDE_DEMANDES_ETENDUES;

    /// <summary>
    /// Event triggers every time a request is made to access row information, element, or data 
    /// and also before the row element is appended to the DataGrid element.
    /// </summary>
    /// <param name="Args">Row data bound argument.</param>
    private void OnRowDataBound(RowDataBoundEventArgs<Data.EF.Univers.VDE_DEMANDES_ETENDUES> Args)
    {
        // If there is no results.
        if (Args.Data.TD_RESULT_EXIST_FILE != StatusLiteral.Yes)
        {
            // Hide "download result" command button.
            Args.Row.AddClass(new[] { "e-remove-download-result-command" });
        }

        // If there is no resources.
        if (Args.Data.NB_RESSOURCES.Equals(0))
        {
            // Hide "open resources" command button.
            Args.Row.AddClass(new[] { "e-remove-open-resources-command" });
        }

        // Change "qualifs" command icon depending on TD_QUALIF_BILAN.
        switch (Args.Data.TD_QUALIF_BILAN)
        {
            case null:
                // Hide "qualifs" command button.
                Args.Row.AddClass(new[] { "e-remove-qualifs-command" });
                break;
            case 1:
                Args.Row.AddClass(new[] { "e-greenlight" });
                break;
            case 2:
                Args.Row.AddClass(new[] { "e-orangelight" });
                break;
            case 3:
                Args.Row.AddClass(new[] { "e-redlight" });
                break;
        }
    }

    /// <summary>
    /// Event triggers when DataGrid actions starts.
    /// </summary>
    /// <param name="Args">Action event arguments.</param>
    private void OnActionBegin(ActionEventArgs<Data.EF.Univers.VDE_DEMANDES_ETENDUES> Args)
    {
        // Customization of the filters.
        if (!Action.FilterBeforeOpen.Equals(Args.RequestType))
        {
            return;
        }

        // For the request status column, restrict filtering operations.
        if (nameof(Data.EF.Univers.VDE_DEMANDES_ETENDUES.CODE_STATUT_DEMANDE).Equals(Args.ColumnName))
        {
            Args.FilterOperators = EqualityFilterOperatorList;
        }
    }
    #endregion

    #region Grid Toolbar
    /// <summary>
    /// "Produce" toolbar button.
    /// </summary>
    private readonly ItemModel ToolbarProduce = new()
    {
        PrefixIcon = "e-field-settings",
        Text = "[TRANSLATE]",
        TooltipText = "[TRANSLATE]",
        Id = "Produce",
        CssClass = "highlighted"
    };

    /// <summary>
    /// Add "Produce" button to the Toolbar.
    /// </summary>
    private void AddProduceToolbarButton()
    {
        // Translate inactivate button.
        ToolbarProduce.Text = Trad.Keys[$"DTF:GridToolbar{ToolbarProduce.Id}"];
        ToolbarProduce.TooltipText = Trad.Keys[$"DTF:GridToolbar{ToolbarProduce.Id}Tooltip"];
        // Add inactivate button to the toolbar.
        Ref_VDE_DEMANDES_ETENDUES.ToolbarListItems.Add(ToolbarProduce);
    }

    /// <summary>
    /// Event triggers when toolbar item is clicked.
    /// </summary>
    /// <param name="args">Click event argument.</param>
    private async Task OnToolbarClickAsync(ClickEventArgs args)
    {
        // Produce button clicked.
        if (args.Item.Id.Equals(ToolbarProduce.Id, StringComparison.OrdinalIgnoreCase))
        {
            // Display "NewProduction" component.
            IsNewProductionDisplayed = await _iCpuClient.GetCpuStatusAsync();

            if (!IsNewProductionDisplayed)
            {
                await Toast.DisplayWarningAsync("Attention", Trad.Keys["DTF:GridToolbarProduceAlert"]);
            }

            // Update parent with new value
            await IsNewProductionDisplayedChanged.InvokeAsync(IsNewProductionDisplayed);
        }
        else
        {
            await Ref_VDE_DEMANDES_ETENDUES.OnToolbarClickAsync(args);
        }
    }
    #endregion

    #region Grid Context Menu
    /// <summary>
    /// "Grid" main context menu.
    /// </summary>
    private readonly ContextMenuItemModel ContextMenuCancel = new()
    {
        Text = "[TRANSLATE]",
        Target = ".e-content",
        Id = "StopRequest",
        IconCss = "e-icons e-changes-reject"
    };

    /// <summary>
    /// Add "Cancel" button to the context menu.
    /// </summary>
    private void AddCancelContextMenuButton()
    {
        // Translate button text.
        ContextMenuCancel.Text = Trad.Keys[$"DTF:GridContextMenu{ContextMenuCancel.Id}"];
        // Add button to the context menu.
        Ref_VDE_DEMANDES_ETENDUES.ContextMenuListItems.Add(ContextMenuCancel);
    }

    /// <summary>
    /// Event triggers before opening the context menu.
    /// </summary>
    /// <param name="args">Context menu open event argument.</param>
    private void ContextMenuOpen(ContextMenuOpenEventArgs<Data.EF.Univers.VDE_DEMANDES_ETENDUES> args)
    {
        // Check if the user is authorized by Level 1 to interact with grid data.
        bool l1AllowModify = isDTFAdm || isDTFConsul;

        // Get "Cancel" context menu item.
        //var cancelMenuItem = args.ContextMenuObj.Items.FirstOrDefault(x => x.Id.Equals(ContextMenuCancel.Id));
        var cancelMenuItem = args.ContextMenu.Items.FirstOrDefault(x => x.Id.Equals(ContextMenuCancel.Id));

        if (cancelMenuItem is null || args.RowInfo.RowData == null)
        {
            return;
        }

        // User n'a pas la fonction de producteur sur ce module ( ni role admin) => pas le droit d'annuler
        if (!(args.RowInfo.RowData.TS_SCENARIO?.VDTFH_HABILITATION?.FirstOrDefault()?.PRODUCTEUR == 1 || l1AllowModify))
        {
            // Show cancel button.
            cancelMenuItem.Hidden = true;

            return;
        }

        // Request cancellation depends on request TD_IGNORE_RESULT.
        if (StatusLiteral.Yes.Equals(args.RowInfo.RowData.TD_IGNORE_RESULT))
        {
            // Hide cancel button.
            cancelMenuItem.Hidden = true;

            return;
        }

        // Request cancellation depends on request status.
        cancelMenuItem.Hidden = args.RowInfo.RowData.CODE_STATUT_DEMANDE switch
        {
            // Requests that can be cancelled.
            StatusLiteral.CreatedRequestAndWaitForExecution or StatusLiteral.ScheduledRequest or StatusLiteral.InProgress => false,// Show cancel button.
            // Requests that can't be cancelled.
            _ => true,// Hide cancel button.
        };
    }

    /// <summary>
    /// Event triggers when a context menu item is clicked.
    /// </summary>
    /// <param name="args">Context menu click event argument.</param>
    private Task ContextMenuItemClickedAsync(ContextMenuClickEventArgs<Data.EF.Univers.VDE_DEMANDES_ETENDUES> args)
    {
        // "Cancel" button clicked.
        return args.Item.Id.Equals(ContextMenuCancel.Id, StringComparison.OrdinalIgnoreCase)
            ? CancelRequest(args.RowInfo.RowData.TD_DEMANDEID)
            : Ref_VDE_DEMANDES_ETENDUES.OnContextMenuItemClickedAsync(args);
    }

    /// <summary>
    /// Cancel TD_DEMANDE.
    /// </summary>
    /// <param name="requestId">Id of TD_DEMANDE to cancel.</param>
    private async Task CancelRequest(int requestId)
    {
        // Get TD_DEMANDES
        // filtered by selected TD_DEMANDES ID.
        var filter = $"{nameof(Data.EF.Univers.TD_DEMANDES.TD_DEMANDEID)} eq {requestId}";
        var request = (await ProxyCore.GetEnumerableAsync<Data.EF.Univers.TD_DEMANDES>($"?$filter={filter}", useCache: false)).FirstOrDefault();

        if (request is not null)
        {
            // Does the request need to be updated ?
            bool updateRequest = false;

            // Prepare new request value.
            // Request cancellation depends on status.
            switch (request.TRST_STATUTID)
            {
                // Pending request.
                case StatusLiteral.CreatedRequestAndWaitForExecution:
                case StatusLiteral.ScheduledRequest:
                    // Change request statut to cancelled.
                    request.TRST_STATUTID = StatusLiteral.CanceledRequest;

                    updateRequest = true;
                    break;

                // Request in progress.
                case StatusLiteral.InProgress:
                    // Change request statut to cancelled.
                    request.TD_IGNORE_RESULT = StatusLiteral.Yes;

                    updateRequest = true;
                    break;
            }

            // Update request if needed.
            if (updateRequest)
            {
                // Update job, BUT don't convert DATES as they already are in UTC
                var apiResult = await ProxyCore.UpdateAsync(new List<Data.EF.Univers.TD_DEMANDES> { request });

                if (apiResult.Count.Equals(Litterals.NoDataRow))
                {
                    await ProxyCore.SetLogException(new LogException(GetType(), request, apiResult.Message));
                    await Toast.DisplayErrorAsync(Trad.Keys["DTF:RequestCancellation"], Trad.Keys["DTF:RequestCancellationFailed"]);
                }
                else
                {
                    // Send event to refresh grid for all clients.
                    await ProxyCore.AddTrackedEntity(new[] { typeof(Data.EF.Univers.VDE_DEMANDES_ETENDUES).FullName }, Litterals.Update, Session.GetUserId(), "CancelDemande");
                    await Toast.DisplaySuccessAsync(Trad.Keys["DTF:RequestCancellation"], Trad.Keys["DTF:RequestCancellationSucceed"]);
                }
            }
            else
            {
                await Toast.DisplayWarningAsync(Trad.Keys["DTF:RequestCancellation"], string.Format(Trad.Keys["DTF:RequestCancellationRefused"], Trad.Keys[$"STATUS:{request.TRST_STATUTID}"]));
            }
        }
    }
    #endregion

    #region Grid Command
    private Task NotAllowedToast()
        => Toast.DisplayWarningAsync("Info", "Not allowed");

    /// <summary>
    /// Data of the row where the command was launched via CommandClickedAsync.
    /// </summary>
    private Data.EF.Univers.VDE_DEMANDES_ETENDUES CommandData;

    /// <summary>
    /// Event triggers when command button is clicked.
    /// </summary>
    /// <param name="args">Command click argument.</param>
    private Task CommandClickedAsync(CommandClickEventArgs<Data.EF.Univers.VDE_DEMANDES_ETENDUES> args)
    {
        Task task = null;
        bool isProducteur = false;
        bool isControleur = false;

        CommandData = args.RowData;

        if (isDataDriven)
        {
            var habilitation = args.RowData.TS_SCENARIO?.VDTFH_HABILITATION?.ToList();

            if (habilitation is not null && habilitation.Any())
            {
                (isProducteur, isControleur) = habilitation
                    .Select(e => (e.PRODUCTEUR == 1, e.CONTROLEUR == 1))
                    .FirstOrDefault();
            }
        }

        // RG : limitation des acces pour qualifs et Ressources   
        bool AllowAction = isDTFAdm || isDTFConsul || isProducteur || isControleur;

        switch (args.CommandColumn.ID)
        {
            case "QualifsDialogCommand":

                if (!AllowAction)
                {
                    task = NotAllowedToast();
                    break;
                }

                OpenQualifsDialog();
                break;

            case "InfosDialogCommand":
                OpenInfosDialog();
                break;

            case "ResourcesDialogCommand":
                if (!AllowAction)
                {
                    task = NotAllowedToast();
                    break;
                }
                OpenResourcesDialog();
                break;

            case "DownloadResultCommand":
                Download.DownloadResult(args.RowData.TD_DEMANDEID);
                break;
        }

        return task ?? Task.CompletedTask;
    }
    #endregion

    #region Infos dialog button
    /// <summary>
    /// Is the dialog used to display infos displayed ?
    /// </summary>
    private bool _isInfosDialogDisplayed;

    /// <summary>
    /// Open dialog used to display infos.
    /// </summary>
    private void OpenInfosDialog() => _isInfosDialogDisplayed = true;
    #endregion

    #region Resources dialog button
    /// <summary>
    /// Is the dialog used to display resources displayed ?
    /// </summary>
    private bool _isResourcesDialogDisplayed;

    /// <summary>
    /// Open dialog used to display resources.
    /// </summary>
    private void OpenResourcesDialog() => _isResourcesDialogDisplayed = true;
    #endregion

    #region Qualifs dialog button
    /// <summary>
    /// Is the dialog used to display qualifs displayed ?
    /// </summary>
    private bool _isQualifsDialogDisplayed;

    /// <summary>
    /// Open dialog used to display qualifs.
    /// </summary>
    private void OpenQualifsDialog() => _isQualifsDialogDisplayed = true;
    #endregion

    #region CODE_STATUT_DEMANDE Column
    /// <summary>
    /// Requests status.
    /// This class is used to translate DataBase values to localized text.
    /// </summary>
    internal class Status
    {
        /// <summary>
        /// Value of the status in database.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Localized status.
        /// </summary>
        public string Label { get; set; }
    }

    /// <summary>
    /// List all requests status.
    /// </summary>
    private List<Status> StatusList { get; set; }

    /// <summary>
    /// Fill status list with status codes and translations.
    /// </summary>
    private void InitializeStatusList()
    {
        // List all status code.
        var statusCodeList = new List<string>
        {
            StatusLiteral.ScheduledRequest,
            StatusLiteral.CreatedRequestAndWaitForExecution,
            StatusLiteral.InProgress,
            StatusLiteral.RealizedRequest,
            StatusLiteral.InError,
            StatusLiteral.Stopping,
            StatusLiteral.CanceledRequest,
            StatusLiteral.WaitingTriggerFile,
            StatusLiteral.WaitingTriggerFileTimeout
        };

        // Fill status list based on status codes.
        StatusList = new List<Status>(statusCodeList.Select(s =>
            new Status { Code = s, Label = Trad.Keys[$"STATUS:{s}"] }));
    }

    /// <summary>
    /// A filter operator.
    /// </summary>
    private class FilterOperator
    {
        /// <summary>
        /// Value of the operator.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Localized text for this operator.
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// List of filter operator restricted to equality/inequality operators.
    /// </summary>
    private List<object> EqualityFilterOperatorList =>
        new List<object> {
            new FilterOperator
            {
                Text = Trad.Keys["COMMON:Equal"],
                Value = FiltersLiterals.Equal
            },
            new FilterOperator
            {
                Text = Trad.Keys["COMMON:NotEqual"],
                Value = FiltersLiterals.NotEqual
            }
        };
    #endregion
}