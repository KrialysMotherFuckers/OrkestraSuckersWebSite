using BlazorComponentBus;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.DI;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Action = Syncfusion.Blazor.Grids.Action;
using FailureEventArgs = Syncfusion.Blazor.Grids.FailureEventArgs;

namespace Krialys.Orkestra.Web.Module.Common.Components;

public partial class OrkaGenericGridComponent<TEntity> where TEntity : class, new()
{
    /// <summary>
    /// Dynamic Datagrid Id based on AppPrefix
    /// Rule: Portail_{DBMS}_{PAGE}_{ENTITY}(_{SuffixId})
    /// </summary>
    private string GetGridId
    {
        get
        {
            // DBMS
            var dbmsName = typeof(TEntity).Namespace!.Replace("Krialys.Data.EF.", string.Empty);

            // PAGE (without any parameters)
            var pageName = NavigationManager.Uri.Split('/').Last().ToLower();
            var pos = pageName.IndexOf("?", StringComparison.Ordinal);
            pageName = pos > 0 ? pageName[..pos] : pageName;

            return $"{Litterals.Portail}{dbmsName}_{pageName}_{typeof(TEntity).Name}{(string.IsNullOrEmpty(SuffixId) ? string.Empty : $"_{SuffixId}")}";
        }
    }

    /// <summary>
    /// Dynamic refresh button based on GetGridId with a pre
    /// </summary>
    public string GetRefreshGridButtonId => $"RefreshGridButton_{GetGridId}";

    #region Parameters
    /// <summary>
    /// Are grid columns auto-generated?
    /// True by default.
    /// </summary>
    [Parameter] public bool AutoGenerateColumns { get; set; } = true;

    /// <summary>
    /// Are columns properties auto-generated?
    /// </summary>
    [Parameter] public bool AutoGenerateColumnProperties { get; set; } = true;

    /// <summary>
    /// Do we add WasmHub component?
    /// </summary>
    [Parameter] public bool AllowTracking { get; set; }

    /// <summary>
    /// Allow Csv export option (Csv export depends on AllowExcelExport flag)
    /// </summary>
    [Parameter] public bool AllowCsvExport { get; set; }

    /// <summary>
    /// Is the toolbar displayed?
    /// Default true.
    /// </summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// Add grid columns to the SfDataGridComponent.
    /// </summary>
    [Parameter] public RenderFragment GridColumns { get; set; }

    /// <summary>
    /// Enable/disable grouping + overloads AllowGrouping flag as well as of 8th of Feb. 2022, default is no grouping allowed
    /// </summary>
    [Parameter] public bool EnableGrouping { get; set; }

    /// <summary>
    /// Show or hide column chooser, by default, the column chooser is hidden
    /// </summary>
    [Parameter] public bool EnableColumnChooser { get; set; }

    /// <summary>
    /// Enable/disable and overloads ShowColumnMenu
    /// </summary>
    [Parameter] public bool EnableColumnMenu { get; set; }

    /// <summary>
    /// Show required fields when datagrid is loaded first (optional)
    /// </summary>
    [Parameter] public string[] OnLoadFields { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Parameter] public FilterType GridFilterType { get; set; } = Syncfusion.Blazor.Grids.FilterType.Excel;

    private List<string> _onLoadFields = new();

    /// <summary>
    /// Show required fields when edit dialog is open in Edit mode (optional)
    /// </summary>
    [Parameter] public string[] OnEditFields { get; set; }

    /// <summary>
    /// Show required fields when edit dialog is open in Add mode (optional)
    /// </summary>
    [Parameter] public string[] OnAddFields { get; set; }

    /// <summary>
    /// Add suffix to DataGrid ID.
    /// </summary>
    [Parameter] public string SuffixId { get; set; }

    /// <summary>
    /// Does the datagrid appear within a tab?
    /// </summary>
    [Parameter] public bool IsWithinTab { get; set; }

    /// <summary>
    /// Is the default Data Manager used to get grid data.
    /// </summary>
    [Parameter] public bool UseDataManager { get; set; } = true;

    private const string ActionMode = "Field";
    #endregion

    // Edit button translation
    public string EditButtonText => Trad.Keys["GridSource:EditButtonText"];

    // Cancel button translation
    public string CancelButtonText => Trad.Keys["GridSource:CancelButtonText"];

    /// <summary>
    /// Reference to the grid object, used to call grid methods.
    /// </summary>
    public SfGrid<TEntity> DataGrid;

    public IList<ISfGridColumnParameterServices> GridColumnParametersList { get; set; } = new List<ISfGridColumnParameterServices>();

    [Inject] private ILogger<OrkaGenericGridComponent<TEntity>> Logger { get; set; }

    [Inject] private SfDialogService DialogService { get; set; }

    [Inject] private ISnackbar snackbar { get; set; }

    [Inject] private NavigationManager Navigation { get; set; }

    // Fields attributes
    private IReadOnlyDictionary<string, object> Properties { get; set; }

    /// <summary>
    /// Call translation service.
    /// </summary>
    /// <param name="key">Key of the element to translate.</param>
    private string Translate(string key) => Trad.Keys[key];

    /// <summary>
    /// Menu kind
    /// </summary>
    private static FilterSettings FilterType(ColumnType columnType) => columnType switch
    {
        ColumnType.None or ColumnType.String
            => new FilterSettings { Type = Syncfusion.Blazor.Grids.FilterType.Excel },

        ColumnType.Date or ColumnType.DateTime or ColumnType.Integer or ColumnType.Long or ColumnType.DateTime or ColumnType.Decimal
            => new FilterSettings { Type = Syncfusion.Blazor.Grids.FilterType.Excel },

        ColumnType.CheckBox or ColumnType.Boolean
            => new FilterSettings { Type = Syncfusion.Blazor.Grids.FilterType.CheckBox },

        _ => new FilterSettings { Type = Syncfusion.Blazor.Grids.FilterType.FilterBar },
    };

    /// <summary>
    /// Event triggers when DataGrid actions starts : Add / Edit
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public Task OnActionBeginAsync(ActionEventArgs<TEntity> args)
    {
        try
        {
            switch (args.RequestType)
            {
                case Action.BeginEdit:
                    // Hide columns that can't be edited
                    if (OnEditFields is not null)
                    {
                        var delta = _onLoadFields.Except(OnEditFields).ToArray();

                        return DataGrid.HideColumnsAsync(delta, hideBy: ActionMode);
                    }
                    break;

                case Action.Add:
                    // Hide columns that can't be added
                    if (OnAddFields is not null)
                    {
                        var delta = _onLoadFields.Except(OnAddFields).ToArray();

                        return DataGrid.HideColumnsAsync(delta, hideBy: ActionMode);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OnActionBegin] Error: {0}", ex.InnerException?.Message ?? ex.Message);
        }


        return Task.CompletedTask;
    }

    /// <summary>
    /// Event triggers when DataGrid actions ends : Add / Edit
    /// </summary>
    /// <param name="args">Action event argument.</param>
    public Task OnActionCompleteAsync(ActionEventArgs<TEntity> args)
    {
        try
        {
            switch (args.RequestType)
            {
                case Action.Save:
                case Action.Cancel:
                    // Show again hidden columns
                    if (_onLoadFields is not null)
                    {
                        return DataGrid.ShowColumnsAsync(_onLoadFields.ToArray(), showBy: ActionMode);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[OnActionComplete] Error: {0}", ex.InnerException?.Message ?? ex.Message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Allow to build dynamically columns
    /// Can only be used when AutoGenerateColumns == false 
    /// </summary>
    public void AddColumn(string field)
    {
        // Get column attributes
        var prop = GridColumnParametersList.FirstOrDefault(x => x.IsInGrid && x.Field.Equals(field, StringComparison.OrdinalIgnoreCase));

        // Add columns with their respective attributes + avoid tuples
        if (prop is not null && DataGrid.Columns.Find(x => x.Field.Equals(prop.Field)) is null)
        {
            DataGrid.Columns.Add(new GridColumn
            {
                Field = prop.Field,
                HeaderText = prop.HeaderText,
                IsPrimaryKey = prop.IsPrimaryKey,
                IsIdentity = prop.IsIdentity,
                Visible = prop.Visible,
                Format = prop.Format,
                Type = prop.ColumnType,
                EnableGroupByFormat = (prop.Format == default),
                AutoFit = prop.Autofit,
                MinWidth = prop.Width,
            });
        }
    }

    #region Context menu customization

    /// <summary>
    /// Context menu configuration. This menu is displayed by right click on the grid.
    /// </summary>
    public List<ContextMenuItemModel> ContextMenuListItems { get; set; } = new();

    /// <summary>
    /// "Grid" main context menu.
    /// </summary>
    private readonly ContextMenuItemModel _contextMenuGridItemModel = new()
    {
        Text = "[TRANSLATE]",
        Target = ".e-content",
        Id = "GridMain",
        Items = new List<MenuItem>()
    };

    /// <summary>
    /// "Reload" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuReloadItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "ReloadGrid",
        IconCss = "e-icons e-refresh"
    };

    /// <summary>
    /// "Reload" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuClearFiltersItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "ClearFilters",
        IconCss = "e-icons e-filter-clear-2"
    };

    /// <summary>
    /// "Copy" main context menu.
    /// </summary>
    private readonly ContextMenuItemModel _contextMenuCopyItemModel = new()
    {
        Text = "[TRANSLATE]",
        Target = ".e-content",
        Id = "CopyMain",
        Items = new List<MenuItem>()
    };

    /// <summary>
    /// "Copy raw context menu.
    /// </summary>
    private readonly MenuItem _contextMenuCopyRawItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "CopyRaw",
        IconCss = "e-icons e-copy"
    };

    /// <summary>
    /// "Copy with headers" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuCopyWithHeadersItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "CopyWithHeaders",
        IconCss = "e-icons e-copy"
    };

    /// <summary>
    /// "Copy without headers" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuCopyWithoutHeadersItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "CopyWithoutHeaders",
        IconCss = "e-icons e-copy"
    };

    /// <summary>
    /// "Export" main context menu.
    /// </summary>
    private readonly ContextMenuItemModel _contextMenuExportItemModel = new()
    {
        Text = "[TRANSLATE]",
        Target = ".e-content",
        Id = "ExportMain",
        Items = new List<MenuItem>()
    };

    /// <summary>
    /// "Excel export" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuExcelExportItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "ExportToExcel",
        IconCss = "e-icons e-excelexport"
    };

    /// <summary>
    /// "CSV export" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuCsvExportItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "ExportToCsv",
        IconCss = "e-icons e-csvexport"
    };

    /// <summary>
    /// "Pdf export" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuPdfExportItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "ExportToPdf",
        IconCss = "e-icons e-pdfexport"
    };

    /// <summary>
    /// Separator for context menu.
    /// </summary>
    private readonly MenuItem _contextMenuSeparatorItem = new()
    {
        Separator = true
    };

    /// <summary>
    /// "Print" context menu.
    /// </summary>
    private readonly MenuItem _contextMenuPrintItem = new()
    {
        Text = "[TRANSLATE]",
        Id = "Print",
        IconCss = "e-icons e-print"
    };

    #endregion Context menu customization

    #region Toolbar customization

    /// <summary>
    /// List of the items in the toolbar.
    /// </summary>
    public List<object> ToolbarListItems = new();

    /// <summary>
    /// "Clear filters" toolbar button.
    /// </summary>
    private readonly ItemModel _toolbarReloadGrid/*ClearFilters*/ = new()
    {
        PrefixIcon = "e-icons e-refresh", //"e-filter-clear-2",
        Text = "[TRANSLATE]",
        TooltipText = "[TRANSLATE]",
        Id = "ReloadGrid" //"ClearFilters"
    };

    /// <summary>
    /// Initialize toolbar based on parameters and translations.
    /// </summary>
    private void InitializeToolbar()
    {
        if (!ShowToolbar)
        {
            ToolbarListItems = null;
        }
        else
        {
            if (!ToolbarListItems.Any())
            {
                // Add toolbar items according to parameters.
                ToolbarListItems.Add(_toolbarReloadGrid /*ClearFilters*/);
                if (EnableColumnChooser)
                {
                    ToolbarListItems.Add("ColumnChooser");
                }

                ToolbarListItems.Add("Add");
                ToolbarListItems.Add("Edit");
                ToolbarListItems.Add("Delete");
                // ToolbarListItems.Add("Search");

                if (AllowCsvExport)
                    ToolbarListItems.Add("CsvExport");

                if (AllowExcelExport)
                    ToolbarListItems.Add("ExcelExport");

                if (AllowPdfExport)
                    ToolbarListItems.Add("PdfExport");

                // Apply translation on the fly
                foreach (var el in ToolbarListItems)
                {
                    if (el is not ItemModel model)
                    {
                        continue;
                    }

                    model.Text = Translate($"GridSource:Menu{model.Id}");
                    model.TooltipText = Translate($"GridSource:Menu{model.Id}Tooltip");
                }
            }
        }
    }

    #endregion Toolbar customization

    /// <summary>
    /// Initialize toolbar based on parameters and translations.
    /// </summary>
    private void InitializeContextMenu()
    {
        if (ContextMenuListItems.Any())
        {
            return;
        }

        // Grid item model
        _contextMenuGridItemModel.Items.Add(_contextMenuReloadItem);
        _contextMenuGridItemModel.Items.Add(_contextMenuClearFiltersItem);
        _contextMenuGridItemModel.Items.Add(_contextMenuSeparatorItem);
        _contextMenuGridItemModel.Items.Add(new MenuItem { Id = "AutoFit" });
        _contextMenuGridItemModel.Items.Add(new MenuItem { Id = "AutoFitAll" });
        //_contextMenuGridItemModel.Items.Add(_contextMenuSeparatorItem);
        //_contextMenuGridItemModel.Items.Add(new MenuItem { Id = "FirstPage" });
        //_contextMenuGridItemModel.Items.Add(new MenuItem { Id = "PrevPage" });
        //_contextMenuGridItemModel.Items.Add(new MenuItem { Id = "NextPage" });
        //_contextMenuGridItemModel.Items.Add(new MenuItem { Id = "LastPage" });
        ContextMenuListItems.Add(_contextMenuGridItemModel);

        // Copy item model
        _contextMenuCopyItemModel.Items.Add(_contextMenuCopyRawItem);
        _contextMenuCopyItemModel.Items.Add(_contextMenuCopyWithHeadersItem);
        _contextMenuCopyItemModel.Items.Add(_contextMenuCopyWithoutHeadersItem);
        ContextMenuListItems.Add(_contextMenuCopyItemModel);

        // Export Item model
        if (AllowExcelExport)
        {
            _contextMenuExportItemModel.Items.Add(_contextMenuExcelExportItem);
            if (AllowCsvExport)
            {
                _contextMenuExportItemModel.Items.Add(_contextMenuCsvExportItem);
            }
        }

        if (AllowPdfExport)
        {
            _contextMenuExportItemModel.Items.Add(_contextMenuPdfExportItem);
        }

        //_contextMenuExportItemModel.Items.Add(_contextMenuSeparatorItem);
        //_contextMenuExportItemModel.Items.Add(_contextMenuPrintItem);

        ContextMenuListItems.Add(_contextMenuExportItemModel);

        // Apply translation on the fly
        foreach (var contextMenuItem in ContextMenuListItems)
        {
            contextMenuItem.Text = Translate($"GridSource:Menu{contextMenuItem.Id}");
            foreach (var menuItem in contextMenuItem.Items)
            {
                menuItem.Text = string.IsNullOrEmpty(menuItem.Text)
                    ? menuItem.Id
                    : Translate($"GridSource:Menu{menuItem.Id}");
            }
        }
    }

    private bool _onlyOnce = true;

    /// <summary>
    /// Assign additional parameters
    /// https://shauncurtis.github.io/articles/Blazor-Components.html#some-important-less-documented-information-and-lessons-learned
    /// </summary>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // Set the component parameters
        parameters.SetParameterProperties(this);

        Properties = parameters.ToDictionary();

        // Optimize code here
        if (_onlyOnce)
        {
            _onlyOnce = false;

            // If the data is provided, there is no need of to get the data using Data Manager.
            if (Properties.TryGetValue("DataSource", out var datas))
            {
                if (datas is not null)
                {
                    UseDataManager = false;
                }
            }

            // Grid initializations.
            InitializeToolbar();
            InitializeContextMenu();

            //if (AutoGenerateColumnProperties)
            {
                // Decode Data Annotations + set FK 
                DecodeDataAnnotations();
            }
        }

        // Pass an empty ParameterView, not parameters!
        await base.SetParametersAsync(ParameterView.Empty);
    }

    /// <summary>
    /// Value of the foreign key.
    /// </summary>
    private object _foreignId;

    /// <summary>
    /// Text displayed instead of the foreign key.
    /// </summary>
    private string _foreignLabel;

    /// <summary>
    /// Map column types
    /// </summary>
    private static IDictionary<Type, ColumnType> _columnTypeMap = new Dictionary<Type, ColumnType>
    {
        { typeof(int), ColumnType.Integer },
        { typeof(int?), ColumnType.Integer },
        { typeof(uint), ColumnType.Integer },
        { typeof(uint?), ColumnType.Integer },
        { typeof(short), ColumnType.Integer },
        { typeof(short?), ColumnType.Integer },
        { typeof(ushort), ColumnType.Integer },
        { typeof(ushort?), ColumnType.Integer },
        { typeof(byte), ColumnType.Integer },
        { typeof(byte?), ColumnType.Integer },

        { typeof(long), ColumnType.Long },
        { typeof(long?), ColumnType.Long },
        { typeof(ulong), ColumnType.Long },
        { typeof(ulong?), ColumnType.Long },

        { typeof(float), ColumnType.Double },
        { typeof(float?), ColumnType.Double },
        { typeof(double), ColumnType.Double },
        { typeof(double?), ColumnType.Double },

        { typeof(decimal), ColumnType.Decimal },
        { typeof(decimal?), ColumnType.Decimal },

        { typeof(bool), ColumnType.Boolean },
        { typeof(bool?), ColumnType.Boolean },

        { typeof(DateTime), ColumnType.DateTime },
        { typeof(DateTime?), ColumnType.DateTime },
        { typeof(DateTimeOffset), ColumnType.DateTime },
        { typeof(DateTimeOffset?), ColumnType.DateTime },
    };

    /// <summary>
    /// Default 'grid' parameters below (but don't do it for [Parameter] ones !!)
    /// Rules :
    /// </summary>
    /// <returns></returns>
    private void DecodeDataAnnotations()
    {
        //var watch = new System.Diagnostics.Stopwatch();
        //watch.Start();
        try
        {
            /***** Part 1 : Set Grid parameters. *****/
            // If GridColumnParameters list is empty, complete it.
            if (SessionStatus.IsConnected && !GridColumnParametersList.Any())
            {
                /***** Part 1 : Extract GridColumnParameters from TEntity properties and attributes *****/
                // Browse through TEntity properties.
                //Parallel.ForEach(GenericCacheInvoke.GetPropertyInfosAsArray, prop =>
                GenericCacheInvoke.GetPropertyInfosAsArray.AsParallel().ForAll(prop =>
                {
                    // Set default grid column parameters (using TEntity properties).
                    GridColumnParameters = new SfGridColumnParameterServices
                    {
                        Type = prop.PropertyType,
                        Field = prop.Name,
                        HeaderText = prop.Name,
                        ColumnType = _columnTypeMap.TryGetValue(prop.PropertyType, out var type) ? type : ColumnType.String,
                    };

                    // Browse through properties attributes.
                    foreach (var attr in prop.GetCustomAttributesData())
                    {
                        switch (attr.AttributeType)
                        {
                            // If the property is a primary key.
                            case { } t when t == typeof(KeyAttribute):
                                GridColumnParameters.IsPrimaryKey = true;
                                GridColumnParameters.IsIdentity = true;
                                GridColumnParameters.Visible = false;
                                break;

                            // If the property is a navigation property.
                            case { } t when t == typeof(InversePropertyAttribute):
                                // Navigation properties are not rendered.
                                GridColumnParameters.IsInGrid = false;
                                break;

                            // If the property is a foreign key.
                            case { } t when t == typeof(ForeignKeyAttribute):
                                // Get name of the foreign key from foreign key attribute.
                                if (attr.ConstructorArguments.Count > 0)
                                {
                                    GridColumnParameters.ForeignKeyName = attr.ConstructorArguments[0].Value?.ToString();
                                }

                                // Get type of foreign key. It's the property type.
                                GridColumnParameters.ForeignKeyType = prop.PropertyType;
                                break;

                            // Dynamic translation support (based on DataAnnotationsResources.xx-XX.resx)
                            case { } t when t == typeof(DisplayAttribute):
                                {
                                    GridColumnParameters.HeaderText = DataAnnotations.Display<TEntity>(GridColumnParameters.Field);

                                    const int maxLength = 160;
                                    var length = GridColumnParameters.HeaderText.Length + 10;
                                    GridColumnParameters.Autofit = length > maxLength;
                                    length = length < maxLength ? maxLength : length;
                                    GridColumnParameters.Width = length.ToString();
                                }
                                break;

                            case { } t when t == typeof(DisplayFormatAttribute):
                                {
                                    var arg = (attr.NamedArguments ?? throw new InvalidOperationException())
                                        .FirstOrDefault(arg => arg.MemberName.Equals("DataFormatString", StringComparison.Ordinal));

                                    GridColumnParameters.Format = arg.TypedValue.Value?.ToString() ?? string.Empty;
                                }
                                break;

                            default:
                                _ = nameof(attr.AttributeType.Name);
                                break;
                        }
                    }

                    /* Add in grid column parameters list.  */
                    GridColumnParametersList.Add(GridColumnParameters);

                    /***** Part 2 : Prepare GridForeignColumn parameters. *****/
                    /* Problem:
                     * It's not possible to define the grid column parameters (gridColParam) of a foreign key property 
                     * from its attributes only.
                     * In fact, there is no information on its attributes indicating if a property is indeed a foreign key.
                     * 
                     * Solution:
                     * The informations on foreign key properties must be retrived from the navigation properties.
                     * 
                     * In part 1, each property of TEntity were converted in a gridColParam entry.
                     * In part 2, gridColParam of foreign key properties will be completed using gridColParam of navigation properties.
                     * The link between a navigation property and a foreign key property is the ForeignKey attribute. */

                    // Browse through TEntity which have foreign key.
                    // This value is called src (source) because navigation properties will be extracted from it.
                    foreach (var srcGridColParam in GridColumnParametersList.Where(x => x.ForeignKeyName is not null))
                    {
                        // Browse through all grid column properties. 
                        // This value is called dst (destination) because its foreign key properties will be completed.
                        foreach (var dstGridColParam in GridColumnParametersList.Where(fk => srcGridColParam.ForeignKeyName.Equals(fk.Field, StringComparison.Ordinal)))
                        {
                            // 1- Get type of the foreign key, coming from the navigation property. 
                            dstGridColParam.ForeignKeyType = srcGridColParam.ForeignKeyType;

                            // 2- Get name of the foreign key.
                            dstGridColParam.ForeignKeyName = GenericCacheInvoke.GetPropertyInfoFirstOrDefault(dstGridColParam.ForeignKeyType)?.Name;

                            // 3- Get the value of the foreign key (not the real one but the one displayed), coming from DisplayColumn attribute of the foreign entity.
                            dstGridColParam.ForeignKeyValue = GenericCacheInvoke.GetDisplayColumnAttribute(dstGridColParam.ForeignKeyType);
                            dstGridColParam.IsForeignKey = true;
                        }
                    }
                });
            }

            /***** Part 3 : Read values of the foreign keys. *****/
            /* In parts 1 and 2, the grid column parameters were prepared based on the properties of generic type TEntity.
             * These parts were synchronous.
             * 
             * In part 3, the values of the foreign keys are read (from database).
             * These values are stored in a list inside grid column parameters.
             * This list is composed of the value of a foreign key (Id) and the text displayed instead of it (Label).
             * They will be displayed in a dropdown list when a foreign key is edited. */
            // Browse through grid column parameters which are foreign keys
            // and which have foreign values list is not completed yet.
            // TODO : optimize this repetitive 99.99% time will have same datas
            async void ActionColumnParameters(ISfGridColumnParameterServices el)
            {
                // Browse through foreign data.
                // Each foreign data is a dicionnary where Key is the name of the property and Value is the value of the property.
                var dicoFks = await ProxyCore.GetDictionaryAsync<TEntity>(el.ForeignKeyType.Name);

                foreach (var dico in dicoFks.ToList())
                {
                    // Browse through pairs of the dictionary
                    foreach (var kvp in dico)
                    {
                        // If the property is the foreign key, get value of foreign key/id
                        if (kvp.Key.Equals(el.ForeignKeyName, StringComparison.Ordinal))
                        {
                            // Convert foreign value to the correct type.
                            if (typeof(string) == el.Type)
                            {
                                _foreignId = kvp.Value.ToString();
                            }
                            else if (typeof(int) == el.Type || typeof(int?) == el.Type)
                            {
                                if (int.TryParse(kvp.Value?.ToString(), out int tmp))
                                {
                                    _foreignId = tmp;
                                }
                            }
                            else
                            {
                                throw new NotSupportedException($"DataGrid: Foreign value type not supported: {el.ForeignKeyType.Name}");
                            }
                        }

                        // If the property is the text displayed instead of the foreign key, get value of displayed text/label
                        if (kvp.Key.Equals(el.ForeignKeyValue, StringComparison.OrdinalIgnoreCase))
                        {
                            _foreignLabel = kvp.Value?.ToString();
                        }
                    }

                    // Trick : add if concurrent dictionary allows it!
                    if (el.ForeignValuesDico.TryAdd($"{typeof(TEntity).Name}_{el.ForeignKeyType.Name}_{_foreignId}_{_foreignLabel}", null))
                    {
                        // Add read values to foreign values list.
                        el.ForeignValuesList.Add(new SfGridColumnParameterServices.ForeignValue
                        {
                            Id = _foreignId, // <= add .ToString() cast as of version 19.4.0.41 (after Jan., 5th 2022), remove it as of 19.4.0.42
                            Label = _foreignLabel,
                        });
                    }
                }

                // OR-650: evolution, as of now all foreign keys will be ordered by their labels
                el.ForeignValuesList = el.ForeignValuesList.OrderBy(x => x.Label).ToList();
            }

            GridColumnParametersList
                .Where(x => !string.IsNullOrEmpty(x.ForeignKeyName)
                && !string.IsNullOrEmpty(x.ForeignKeyValue)
                && !x.ForeignValuesList.Any())
            .ToList()
            .AsParallel()
            .ForAll(ActionColumnParameters);

            // Part 4: Get or prepare DisplayedFields
            if (!_onLoadFields.Any())
            {
                _onLoadFields = OnLoadFields is null
                    ? GenericCacheInvoke.DisplayedFieldsAsList
                    : OnLoadFields.ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error: {Message}", ex.Message);
            GridColumnParametersList.Clear();
        }

        //watch.Stop();
        //var elapsed = watch.ElapsedMilliseconds;
        //if (elapsed > 1)
        //{
        //    var stop = elapsed;
        //}
    }

    /// <summary>
    /// Generic cache for reflected types
    /// https://danielwertheim.se/generic-method-cache-vs-reflection/
    /// Important to return as well List or Array, don't make it evaluated after
    /// </summary>
    internal static class GenericCacheInvoke
    {
        /// <summary>
        /// Get DisplayedFields as Array (get rid of 'InversePropertyAttribute' fields) (cached)
        /// </summary>
        internal static readonly string[] DisplayedFieldsAsArray = typeof(TEntity)
            .GetProperties()
            .Where(pi => Attribute.GetCustomAttributes(pi).All(x => x.GetType() != typeof(InversePropertyAttribute)))
            .Select(pi => pi.Name).ToArray();

        /// <summary>
        /// Get DisplayedFields as List (get rid of 'InversePropertyAttribute' fields) (cached)
        /// </summary>
        internal static readonly List<string> DisplayedFieldsAsList = typeof(TEntity)
            .GetProperties()
            .Where(pi => Attribute.GetCustomAttributes(pi).All(x => x.GetType() != typeof(InversePropertyAttribute)))
            .Select(pi => pi.Name).ToList();

        /// <summary>
        /// Get PropertyInfo as Array (cached)
        /// </summary>
        internal static readonly PropertyInfo[] GetPropertyInfosAsArray = typeof(TEntity)
            .GetProperties().ToArray();

        /// <summary>
        /// Get PropertyInfo FirstOrDefault FKI (not cached)
        /// </summary>
        internal static PropertyInfo GetPropertyInfoFirstOrDefault(Type type) => type
            .GetProperties()
            .FirstOrDefault(p => p.GetCustomAttributesData().Any(x => x.AttributeType == typeof(KeyAttribute)));

        /// <summary>
        /// Get DisplayColumnAttribute for FK (not cached)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static string GetDisplayColumnAttribute(Type type) => type
            ?.GetCustomAttributesData()
            .FirstOrDefault(attr => attr.AttributeType == typeof(DisplayColumnAttribute))
            ?.ConstructorArguments
            .FirstOrDefault().Value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Don't remove this event!
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //Check session
            if (SessionStatus.IsTimeOut())
            {
                await Session.GetAuthorizationAsync();
            }
            else if (DataGrid is not null)
            {
                // If disabled cache mode has been set, then we can free the cache
                if (!ProxyCore.IsCacheEnabled)
                {
                    ProxyCore.CacheClear();
                }

                // Remove capacities that are determined once the grid is loading (exception are functional rules that can disable)
                // These cases are appliable if we default 'AllowModify' parameters variable to true, then a business rule decides what to allow at runtime
                if (!DataGrid.EditSettings.AllowAdding)
                    ToolbarListItems.Remove("Add");

                if (!DataGrid.EditSettings.AllowEditing)
                    ToolbarListItems.Remove("Edit");

                if (!DataGrid.EditSettings.AllowDeleting)
                    ToolbarListItems.Remove("Delete");

                if (AutoGenerateColumns)
                    OnLoadFields = GenericCacheInvoke.DisplayedFieldsAsArray;
            }
        }
    }

    /// <summary>
    /// Special event managing the datagrid page settings
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task OnLoadHandler(object args)
    {
#if DEBUG
        if (DataGrid is not null)
            Logger.LogWarning($"[DBG-GRID] OnLoadHandler GRID Name='{DataGrid.ID}'");
#endif
        // Avoid direct DataGrid refresh here, else you'll loose Filters and all parmeters linked to  the grid!
        //await DataGrid.Refresh();
        // Instead, invoke indirect datagrid Refresh (strict minimum action expected), but indirectly
        await JsInProcessRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, GetRefreshGridButtonId, 250);
    }

    private int _pageSize = 12;

    /// <summary>
    /// Adjust datagrid matrix, then refresh datagrid
    /// </summary>
    /// <returns></returns>
    private async Task RefreshDatagridAsync(bool clearCachedEntity)
    {
        // Here, you can customize your code.
        int browserHeight = await JsInteropRuntime.InvokeAsync<int>(Litterals.JsGetBrowserHeight);
        int top = await JsInteropRuntime.InvokeAsync<int>(Litterals.JsGetElementOffsetTop, GetGridId, IsWithinTab || DataGrid.AllowGrouping ? 2 : 1);
        int rows = ((browserHeight - (Litterals.RowHeight / 2) - top) / Litterals.RowHeight) - (IsWithinTab || DataGrid.AllowGrouping ? 4 : 3);
        rows = rows < 1 ? 1 : rows;

        //if (DataGrid.TotalItemCount > 0)
        //    rows = rows < DataGrid.TotalItemCount ? rows : DataGrid.TotalItemCount;

        bool needRefresh = clearCachedEntity || DataGrid.PageSettings.PageSize != rows;

        if (needRefresh)
        {
            // DataGrid.PageSettings.PageSize = rows;
            if (clearCachedEntity)
            {
                // Clear cache 1st
                ProxyCore.CacheRemoveEntities(typeof(TEntity));
            }
            DataGrid.ForceUpdate = true;
            DataGrid.SoftRefresh = false;
        }
        else
        {
            DataGrid.ForceUpdate = false;
            DataGrid.SoftRefresh = true;
        }

        _pageSize = rows;

        // Datagrid refresh invokes a new call to ReadAsync whatever if the Pagesize is the same as the 1st call
        await DataGrid.Refresh();
    }
    #region HANDLERS

    private bool _toolbarClicked = false;

    /// <summary>
    /// Toolbar events
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public async Task OnToolbarClickAsync(ClickEventArgs args)
    {
        if (!_toolbarClicked)
        {
            _toolbarClicked = true;

            // Excel export
            if (AllowExcelExport && args.Item.Id.Equals($"{DataGrid.ID}_excelexport", StringComparison.OrdinalIgnoreCase))
            {
                await ExportToExcelAsync();
            }
            //// Csv export
            //else if (AllowCsvExport && args.Item.Id.Equals($"{DataGrid.ID}_csvexport", StringComparison.OrdinalIgnoreCase))
            //{
            //    await ExportToCsvAsync();
            //}
            // Pdf export (current page only)
            else if (AllowPdfExport && args.Item.Id.Equals($"{DataGrid.ID}_pdfexport", StringComparison.OrdinalIgnoreCase))
            {
                await ExportToPdfAsync();
            }
            // Reload grid
            else if (args.Item.Id.Equals("ReloadGrid" /*"ClearFilters"*/, StringComparison.OrdinalIgnoreCase))
            {
                await RefreshDatagridAsync(true);
            }

            _toolbarClicked = false;
        }
    }

    private async Task ExportToExcelAsync()
    {
        if (DataGrid.TotalItemCount > 0)
        {
            if (await DialogService.ConfirmAsync(string.Format(Trad.Keys["COMMON:ConfirmExport"], "Excel"), "Export"))
            {
                await DataGrid.ExportToExcelAsync(false, new()
                {
                    IncludeCommandColumn = true,
                    IncludeHeaderRow = true,
                    IncludeHiddenColumn = true,
                    IncludeTemplateColumn = true,
                    FileName = $"{DataGrid.ID}_{DateTime.Now:dd-MM-yyyy-HH:mm:ss}.xlsx"
                });
            }
        }
        else
            await Toast.DisplayWarningAsync("EXCEL", Trad.Keys["COMMON:Export"]);
    }

    private async Task ExportToCsvAsync()
    {
        if (DataGrid.TotalItemCount > 0)
        {
            if (await DialogService.ConfirmAsync(string.Format(Trad.Keys["COMMON:ConfirmExport"], "Csv"), "Export"))
            {
                //await DataGrid.ExportToCsvAsync(new()
                //{
                //    IncludeCommandColumn = true,
                //    IncludeHeaderRow = true,
                //    IncludeHiddenColumn = true,
                //    IncludeTemplateColumn = true,
                //    FileName = $"{DataGrid.ID}_{DateTime.Now:dd-MM-yyyy-HH:mm:ss}.csv"
                //});
            }
        }
        else
            await Toast.DisplayWarningAsync("CSV", Trad.Keys["COMMON:Export"]);
    }

    private async Task ExportToPdfAsync()
    {
        if (DataGrid.TotalItemCount > 0)
        {
            if (await DialogService.ConfirmAsync(string.Format(Trad.Keys["COMMON:ConfirmExport"], "Pdf"), "Export"))
            {
                await DataGrid.ExportToPdfAsync(false, new()
                {
                    IncludeCommandColumn = true,
                    IncludeHeaderRow = true,
                    IncludeHiddenColumn = true,
                    IncludeTemplateColumn = true,
                    FileName = $"{DataGrid.ID}_{DateTime.Now:dd-MM-yyyy-HH:mm:ss}.pdf"
                });
            }
        }
        else
            await Toast.DisplayWarningAsync("PDF", Trad.Keys["COMMON:Export"]);
    }

    /// <summary>
    /// Start datagrid: subscribe to dedicated events
    /// </summary>
    protected override void OnInitialized()
    {
        // Subscribe to TrackedEntity via IComponentBus
        if (AllowTracking)
            Bus.Subscribe<IList<TrackedEntity>>(OnTrackedTEntityAsync);
        //else
        //    Bus.UnSubscribe<IList<TrackedEntity>>(OnTrackedTEntityAsync);
    }

    /// <summary>
    /// Destroy datagrid: Unsubscribe to dedicated events
    /// </summary>
    public void OnDestroyed()
    {
        // Set Disabled value then fire event to SfTab
        Bus.Publish(new SfTabBusEvent { Disabled = false });

        // Remove cached entity?
        //ProxyCore.CacheRemoveEntities(typeof(TEntity));

        // Factory mode: re-enabling cache
        if (!ProxyCore.IsCacheEnabled)
            ProxyCore.GetOrSetDisablingCacheStatus(disableCache: false);

        //if (AllowTracking)
        //    Bus.UnSubscribe<IList<TrackedEntity>>(OnTrackedTEntityAsync);
#if DEBUG
        if (DataGrid is not null)
            Logger.LogWarning($"[DBG-GRID] OnDestroyed GRID Name='{DataGrid.ID}'");
#endif
    }

    /// <summary>
    /// Destroy datagrid: Unsubscribe to dedicated events
    /// </summary>
    /// <param name="extraCachedEntities">(Optional) Entities to remove from cache.</param>
    public void OnDestroyed(Type[] extraCachedEntities)
    {
        OnDestroyed();

        _ = ProxyCore.CacheRemoveEntities(extraCachedEntities);
    }

    [ThreadStatic]
    private static int _prevId = 0;

    /// <summary>
    /// Callback for managing automatic smooth DataGrid refresh
    /// </summary>
    /// <param name="args"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    //public Task OnTrackedEnties(IList<TrackedEntity> trackedEntities)
    private async Task OnTrackedTEntityAsync(MessageArgs args, CancellationToken ct)
    {
        if (DataGrid != null)
        {
            // Get the tracking
            var tracked = args
                .GetMessage<IList<TrackedEntity>>()
                .FirstOrDefault(e => e.FullName.Equals(typeof(TEntity).FullName)
                    && !string.IsNullOrEmpty(e.UuidOrAny)
                    && !e.UuidOrAny.Equals(ProxyCore.ApplicationClientSessionId, StringComparison.Ordinal));

            if (tracked != null)
            {
                if (_prevId != tracked.Id)
                {
                    _prevId = tracked.Id;

                    // Remove TEntity from cache, then refresh DataGrid to reflect changes
                    ProxyCore.CacheRemoveEntities(typeof(TEntity));

                    //#if DEBUG
                    // Trace to browser
                    Logger.LogWarning($"[TRK-GRID] Date: {tracked.Date:R}, Id: {tracked.Id}, Entity: {typeof(TEntity).Name}, Action: {tracked.Action}, UserId: {tracked.UserId}, Uuid: {tracked.UuidOrAny}");
                    //#endif

                    // Indirect call to ReloadGridAsync
                    await JsInteropRuntime.InvokeVoidAsync(Litterals.JsSendClickToRefreshDatagrid, ct, GetRefreshGridButtonId, 250);
                }
            }
        }
    }

    private bool _isClearing;

    /// <summary>
    /// RAZ: Clear searches, groupings, filters, sorts and focuses.
    /// </summary>
    private async Task ClearFiltersAsync()
    {
        if (!_isClearing)
        {
            _isClearing = true;

            // Clear search box
            if (DataGrid.SearchSettings.Key is not null)
            {
                DataGrid.SearchSettings.Key = null;
            }

            try
            {
                await DataGrid.SearchAsync("");
                await DataGrid.ClearGroupingAsync();
                await DataGrid.ClearFilteringAsync();
                await DataGrid.ClearSortingAsync();
                await DataGrid.ClearSelectionAsync();
            }
            finally
            {
                await RefreshDatagridAsync(false);
                _isClearing = false;
            }
        }
    }

    /// <summary>
    /// Handler called when a context menu item is clicked.
    /// </summary>
    /// <param name="args">Options to control the context menu click action.</param>
    public async Task OnContextMenuItemClickedAsync(ContextMenuClickEventArgs<TEntity> args)
    {
        if (DataGrid is not null)
        {
            switch (args.Item.Id)
            {
                case "ReloadGrid":
                    await RefreshDatagridAsync(true);
                    break;

                case "ClearFilters":
                    await ClearFiltersAsync();
                    break;

                case "CopyRaw":
                    var cellValue = string.IsNullOrEmpty(args.Column.Field)
                        ? null
                        : args.RowInfo.RowData.GetType().GetProperty(args.Column.Field)?.GetValue(args.RowInfo.RowData, null);
                    if (cellValue != null)
                    {
                        try
                        {
                            await JsInteropRuntime.InvokeVoidAsync("navigator.clipboard.writeText", cellValue.ToString());
                        }
                        catch
                        {
                            await Toast.DisplayWarningAsync("Attention", "Clipboard is only supported on pages served over HTTPS or from Localhost.");
                        }
                    }
                    break;

                case "CopyWithHeaders":
                    await DataGrid.CopyAsync(true);
                    break;

                case "CopyWithoutHeaders":
                    await DataGrid.CopyAsync(false);
                    break;

                case "ExportToExcel":
                    await ExportToExcelAsync();
                    break;

                //case "ExportToCsv":
                //    await ExportToCsvAsync();
                //    break;

                case "ExportToPdf":
                    await ExportToPdfAsync();
                    break;

                case "Print":
                    await DataGrid.PrintAsync();
                    break;
            }
        }
    }

    /// <summary>
    /// Internally handles: Disabled flag from SfTab
    /// </summary>
    public void DataBoundHander()
    {
        // Set Disabled value then fire event to SfTab
        Bus.Publish(new SfTabBusEvent { Disabled = false });
    }

    #region SfGrid OnActionFailure event

    /// <summary>
    /// Handler called when any DataGrid action failed to achieve the desired results.
    /// </summary>
    /// <param name="args">Error details and its cause.</param>
    public Task ActionFailureAsync(FailureEventArgs args)
    {
        /* Retrieve error message. */
        string errorDetails = args.Error.Message;

        /* Limit error message length. */
        if (errorDetails.Length > 4096)
            errorDetails = errorDetails[..4096] + "...";

        snackbar.Add($"{Trad.Keys["GridSource:GridError"]}.{Environment.NewLine}{errorDetails}", MudBlazor.Severity.Error);

        return Task.CompletedTask;
    }

    #endregion SfGrid OnActionFailure event

    #endregion

    #region Search settings

    /// <summary>
    /// Search fields must be filled only once with GetSearchFields on after rendering datagrid
    /// It seems to be a bug in SF if we don't get fields immediately... :/
    /// </summary>
    private string[] _searchFields = Array.Empty<string>();

    /// <summary>
    /// Get all searchable 'string' fields
    /// </summary>
    /// <returns></returns>
    private void GetSearchFields()
    {
        try
        {
            _searchFields = _searchFields.Length switch
            {
                0 => (from p in GridColumnParametersList
                      where (p.ColumnType == ColumnType.String
                          /*|| p.ColumnType.Equals(ColumnType.DateTime)*/) && p.IsInGrid && p.ForeignKeyName is null
                      select p.Field).ToArray(),
                _ => _searchFields
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error: {Message}", ex.Message);
        }
    }

    #endregion Search settings
}