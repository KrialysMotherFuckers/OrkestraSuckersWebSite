using Krialys.Common.Extensions;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Diagram;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.SplitButtons;
using System.Text.Json;
using Orientation = Syncfusion.Blazor.Diagram.Orientation;

namespace Krialys.Orkestra.Web.Module.Common.Components.ETQ;

public partial class ArboEtiquette
{
    [Parameter] public int? EtiquetteId { get; set; }

    [Parameter] public ArboEtiquettes.Mode Mode { get; set; }

    [Parameter] public bool SidebarToggle { get; set; }
    #region AUTOCOMPLETE

    //private SfAutoComplete<int?, TETQ_ETIQUETTES> autoObj;
    //private string targetValue = string.Empty;

    //private const int minLength = 3;
    //private const int maxTop = 15;

    //private const string sourceField = "TETQ_CODE";
    //private const string targetField = "TETQ_ETIQUETTEID";
    //private const string orderField = "TETQ_DATE_CREATION";

    //private const string verb = "contains";

    //private async Task OnFiltering(FilteringEventArgs args)
    //{
    //    args.PreventDefaultAction = true;

    //    if (args.Text.Length >= minLength)
    //    {
    //        // Filter datas via ODATA
    //        var filter = $"?$top={maxTop}&$filter={verb}(tolower({sourceField}), tolower('{args.Text}')) eq true&$orderby={orderField} desc";
    //        var datas = await ProxyCore.GetEnumerableAsync<TETQ_ETIQUETTES>(filter, useCache: false);

    //        //ProxyCore.GetAllSqlRaw

    //        if (datas.Any())
    //        {
    //            // Filter rules
    //            var query = new Query().Where(new WhereFilter()
    //            {
    //                Field = sourceField,
    //                Operator = verb,
    //                value = args.Text,
    //                IgnoreCase = true
    //            });
    //            query = !string.IsNullOrEmpty(args.Text) ? query : new Query();

    //            // Apply filter
    //            await autoObj.FilterAsync(datas, query);
    //        }
    //    }
    //}

    //private async Task OnValueChange(ChangeEventArgs<int?, TETQ_ETIQUETTES> args)
    //{
    //    //targetValue = args.IsInteracted ? $"Etiquette Id : {args.Value}" : "No data found!";

    //    if (args.Value.HasValue && args.IsInteracted)
    //    {
    //        DataSource = (await GetArboEtiquettes(args.Value.Value, Mode))?.ToList();
    //    }
    //}

    #endregion

    // ORGANIZATION CHART
    #region CHART PARAMETERS

    private const string sourceId = nameof(ArboEtiquettes.ComputedId);
    private const string sourceParentID = nameof(ArboEtiquettes.ComputedParentId);
    private const string _diagramId = "diagram-space";

    ScrollLimitMode scrollLimit { get; set; } = ScrollLimitMode.Infinity;
    private readonly Orientation subTreeOrientation = Orientation.Vertical;
    private readonly SubTreeAlignmentType subTreeAlignment = SubTreeAlignmentType.Right;
    private readonly int rows = 0;
    private readonly double offset = 20;

    private SfDiagramComponent Diagram;
    private LayoutOrientation OrientationType;
    private LayoutType Type = LayoutType.HierarchicalTree;
    private HorizontalAlignment HorizontalAlignment = HorizontalAlignment.Auto;
    private VerticalAlignment VerticalAlignment = VerticalAlignment.Auto;

    private int HorizontalSpacing = 40;
    private int VerticalSpacing = 40;
    private double Top = 40;
    private double Bottom = 40;
    private double Right = 40;
    private double Left = 40;

    private double Width = 410;
    private double Height = 550;
    private DiagramPrintExportRegion region = DiagramPrintExportRegion.PageSettings;

    /// <summary>
    /// Images (root / leaf)
    /// </summary>
    private const string root = "root";
    private const string leaf = "leaf";

    /// <summary>
    /// DataSource
    /// </summary>
    private IList<ArboEtiquettes> DataSource = new List<ArboEtiquettes>();

    #endregion

    #region SIDEBAR

    SfSidebar sidebarDetailsEtiquette;

    private SidebarPosition Position { get; set; } = SidebarPosition.Right;

    private void CloseDetailsView() => SidebarToggle = false;

    //private void OpenDetailsView() => SidebarToggle = true;

    #endregion

    internal class EtiquetteDemandeEtendue
    {
        // TETQ_ETIQUETTES
        public string TETQ_CODE { get; set; }
        public string TETQ_LIB { get; set; }
        public string TETQ_DESC { get; set; }
        public DateTime TETQ_DATE_CREATION { get; set; }
        public string TDOM_LIB { get; set; }
        public string TPRCP_LIB { get; set; }

        // VDE_DEMANDES_ETENDUES
        public int TD_DEMANDEID { get; set; }
        public string TE_NOM_ETAT_VERSION { get; set; }
        public string TS_DESCR { get; set; }
        public string DEMANDEUR { get; set; }
        public string REFERENT { get; set; }
    }

    private EtiquetteDemandeEtendue etiquetteDemandeEtendue = null;

    private async Task<EtiquetteDemandeEtendue> GetExpandedEtiquette(int etqId)
    {
        //etqId = 6; // TEST

        var EtqDemandeEtendue = new EtiquetteDemandeEtendue();

        var etq = (await ProxyCore
            .GetEnumerableAsync<TETQ_ETIQUETTES>($"?$expand=TPRCP_PRC_PERIMETRE,TOBJE_OBJET_ETIQUETTE($expand=TDOM_DOMAINE)&$filter=TETQ_ETIQUETTEID eq {etqId}")).FirstOrDefault();

        if (etq is null)
        {
            return EtqDemandeEtendue;
        }

        EtqDemandeEtendue.TETQ_CODE = etq.TETQ_CODE;
        EtqDemandeEtendue.TETQ_LIB = etq.TETQ_LIB;
        EtqDemandeEtendue.TETQ_DESC = etq.TETQ_DESC;
        EtqDemandeEtendue.TETQ_DATE_CREATION = etq.TETQ_DATE_CREATION;
        EtqDemandeEtendue.TDOM_LIB = etq.TOBJE_OBJET_ETIQUETTE.TDOM_DOMAINE.TDOM_LIB;
        EtqDemandeEtendue.TPRCP_LIB = etq.TPRCP_PRC_PERIMETRE.TPRCP_LIB;

        var dde = (await ProxyCore.GetEnumerableAsync<VDE_DEMANDES_ETENDUES>($"?$filter=TD_DEMANDEID eq {etq.DEMANDEID ?? 0}")).FirstOrDefault();

        if (dde is null)
        {
            return EtqDemandeEtendue;
        }

        EtqDemandeEtendue.TD_DEMANDEID = dde.TD_DEMANDEID;
        EtqDemandeEtendue.TE_NOM_ETAT_VERSION = dde.TE_NOM_ETAT_VERSION;
        EtqDemandeEtendue.TS_DESCR = dde.TS_DESCR;
        EtqDemandeEtendue.DEMANDEUR = dde.DEMANDEUR;
        EtqDemandeEtendue.REFERENT = dde.REFERENT;

        return EtqDemandeEtendue;
    }

    /// <summary>
    /// Etiquettes : consommation -> TopToBottom / production -> BottomToTop
    /// </summary>
    /// <param name="id">Etiquette Id</param>
    /// <param name="mode">Consommation or Production</param>
    /// <returns></returns>
    private async Task<IEnumerable<ArboEtiquettes>> GetArboEtiquettes(int id, ArboEtiquettes.Mode mode)
    {
        if (mode == ArboEtiquettes.Mode.Consommation)
        {
            OrientationType = LayoutOrientation.TopToBottom;

            return await ProxyCore.GetAllSqlRaw<TETQ_ETIQUETTES, ArboEtiquettes>(ArboEtiquettes.Consommation(id));
        }

        OrientationType = LayoutOrientation.BottomToTop;

        return await ProxyCore.GetAllSqlRaw<TETQ_ETIQUETTES, ArboEtiquettes>(ArboEtiquettes.Production(id));
    }

    protected override async Task OnInitializedAsync()
    {
        if (EtiquetteId.HasValue)
        {
            // 1- Get original treeview with its Ids
            DataSource = (await GetArboEtiquettes(EtiquetteId.Value, Mode))?.ToList();

            // 2- Apply the transformation based on ID/PARENTID values which causes an exception when 2 children have a common parent
            var mode = Mode == ArboEtiquettes.Mode.Consommation ? -1 : 1;
            for (var i = 0; i < DataSource.Count; i++)
            {
                DataSource[i].ComputedId = i + 1;

                if (DataSource[i].LEVEL != 0 && DataSource[i].PARENTID != null)
                    DataSource[i].ComputedParentId = DataSource.FirstOrDefault(x => x.LEVEL == DataSource[i].LEVEL + mode
                        && x.ID == DataSource[i].PARENTID)?.ComputedId;
            }

            etiquetteDemandeEtendue = await GetExpandedEtiquette(EtiquetteId.Value);
        }
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            //Diagram.Height = "377px";
            // Set dynamic diagram height.
            if (Diagram is not null)
            {
                return Task.Run(() =>
                {
                    // Get browser height minus top position of the element
                    int getbrowserHeightMinusTop = JsInterop.GetBrowserHeightMinusTop(_diagramId);

                    // WARNING: never return 0, else, default to a minimum acceptable value this avoids divide par zero js stack crash when component is rendering!
                    getbrowserHeightMinusTop = getbrowserHeightMinusTop > 0 ? getbrowserHeightMinusTop : 480;

                    Diagram.Height = $"{getbrowserHeightMinusTop - 20}px";

                    StateHasChanged();
                });
            }
        }

        return base.OnAfterRenderAsync(firstRender);
    }

    // Defines default values for Node object
    private void OnNodeCreating(IDiagramObject obj)
    {
        if (obj is not Node node)
        {
            return;
        }

        if (node.Data is JsonElement)
        {
            node.Data = JsonSerializer.Deserialize<ArboEtiquettes>(node.Data.ToString() ?? string.Empty);
        }

        //var arbo = node.Data as ArboEtiquettes;

        //switch (arbo.LEVEL)
        //{
        //    case 0:
        //        node.Style = new ShapeStyle() { Fill = "#13ab11", StrokeColor = "white" };
        //        break;

        //    case -1:
        //        node.Style = new ShapeStyle() { Fill = "#1859B7", StrokeColor = "white" };
        //        break;

        //    default:
        //        node.Style = new ShapeStyle() { Fill = "#2E95D8", StrokeColor = "white" };
        //        break;
        //}

        node.Width = 360; // 177
        node.Height = 140; //82;

        node.Shape = new Shape
        {
            Type = NodeShapes.HTML,
        };

        node.ExpandIcon = new DiagramExpandIcon
        {
            Shape = DiagramExpandIcons.Minus,
            Height = 15,
            Width = 15,
            CornerRadius = 10,
        };
        node.CollapseIcon = new DiagramCollapseIcon
        {
            Shape = DiagramCollapseIcons.Plus,
            Height = 15,
            Width = 15,
            CornerRadius = 10,
            Fill = "orange",
        };
    }

    // Defines default values for Connector object
    private void OnConnectorCreating(IDiagramObject obj)
    {
        if (obj is not Connector connector)
        {
            return;
        }

        connector.Type = ConnectorSegmentType.Orthogonal;
        connector.CornerRadius = 10;
        connector.Style = new ShapeStyle
        {
            StrokeWidth = 1,
            StrokeDashArray = "1,1"
        };
        connector.TargetDecorator.Shape = DecoratorShape.None;
        connector.SourceDecorator.Shape = DecoratorShape.None;

        // Lock connector
        connector.Constraints = connector.Constraints & ~(ConnectorConstraints.PointerEvents | ConnectorConstraints.Default);
        connector.Constraints = connector.Constraints | ConnectorConstraints.Delete;
        connector.Annotations[0].Constraints = AnnotationConstraints.ReadOnly;
    }

    // Event to notify the selection changed event after selecting/unselecting the diagram elements
    private async Task OnSelectionChanged(SelectionChangedEventArgs args)
    {
        if (args.NewValue?.FirstOrDefault() is Node { Data: ArboEtiquettes nodeData })
        {
            //DataSource = (await GetArboEtiquettes(nodeData.ID, Mode))?.ToList();

            etiquetteDemandeEtendue = await GetExpandedEtiquette(nodeData.ID);

            EtiquetteId = nodeData.ID;

            //OpenDetailsView();
        }
    }

    // Refresh Datasource treeview
    private async Task DataSourceRefreshAsync()
    {
        if (EtiquetteId != null)
        {
            DataSource = (await GetArboEtiquettes(EtiquetteId.Value, Mode))?.ToList();
        }
    }

    private TreeInfo GetLayoutInfo(IDiagramObject obj, TreeInfo options)
    {
        if (rows == 0)
        {
            if (rows == 0 && options.Rows != null)
                options.Rows = null;

            //Node node = obj as Node;

            //if (pattern == "LeftOrientationVertical50" || pattern == "RightOrientationVertical50")
            //{
            //    options.Offset = -50;
            //}

            if (!options.HasSubTree)
            {
                options.AlignmentType = subTreeAlignment;
                options.Orientation = subTreeOrientation;
                options.AlignmentType = subTreeAlignment;
            }
        }
        else
        {
            if (!options.HasSubTree)
            {
                options.AlignmentType = subTreeAlignment;
                options.Orientation = subTreeOrientation;
                options.Offset = offset;
            }
        }

        return options;
    }

    //private void UpdateLockValue(bool locked, Connector item)
    //{
    //    if (locked)
    //    {
    //        item.Constraints = item.Constraints & ~(ConnectorConstraints.PointerEvents | ConnectorConstraints.Default);
    //        item.Constraints = item.Constraints | ConnectorConstraints.ReadOnly;
    //    }
    //    else
    //    {
    //        item.Constraints = ConnectorConstraints.Default; // item.Constraints | ConnectorConstraints.Default & ~(ConnectorConstraints.ReadOnly);
    //    }
    //}

    /// <summary>
    /// Shortenize an input string
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    private string Shortenize(string input, int maxLength = 58)
    {
        if (!string.IsNullOrEmpty(input))
        {
            input = input.TrimEnd('\r', '\n');
            if (input.Length > maxLength)
            {
                input = string.Concat(input.AsSpan(0, maxLength - 3), "...");
            }
        }

        return input;
    }

    private async Task OnExportDropDown(MenuEventArgs args)
    {
        var export = new DiagramExportSettings
        {
            Region = DiagramPrintExportRegion.PageSettings,
            PageWidth = 816,
            PageHeight = 1054,
            Margin = new DiagramThickness { Left = 10, Top = 10, Right = 10, Bottom = 10 }
        };

        switch (args.Item.Text.ToUpper())
        {
            case "CSV":
                if (EtiquetteId != null)
                {
                    var data = await GetArboEtiquettes(EtiquetteId.Value, Mode);
                    await JsInterop.DownloadFile($"{Mode}_{etiquetteDemandeEtendue.TETQ_CODE}.csv", "text/csv", data.ToCsvFromList());
                }
                break;

            case "JPG":
                await Diagram.ExportAsync($"{Mode}_{etiquetteDemandeEtendue.TETQ_CODE}", DiagramExportFormat.JPEG, export);
                break;

            case "PNG":
                await Diagram.ExportAsync($"{Mode}_{etiquetteDemandeEtendue.TETQ_CODE}", DiagramExportFormat.PNG, export);
                break;

            case "SVG":
                await Diagram.ExportAsync($"{Mode}_{etiquetteDemandeEtendue.TETQ_CODE}", DiagramExportFormat.SVG, export);
                break;
        }

        await Task.CompletedTask;
    }

    private Task OnPrint()
    {
        var print = new DiagramPrintSettings
        {
            PageWidth = Width,
            PageHeight = Height,
            Region = region,
            FitToPage = true,
            Orientation = PageOrientation.Landscape,
            Margin = new DiagramThickness { Left = Left, Top = Top, Right = Right, Bottom = Bottom }
        };

        return Diagram.PrintAsync(print);
    }
}