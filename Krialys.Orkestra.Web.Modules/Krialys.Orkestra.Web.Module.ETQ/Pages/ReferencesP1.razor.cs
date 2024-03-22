using Krialys.Common.Extensions;
using Krialys.Data.EF.Etq;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Web.Module.Common.BusEvents;
using Krialys.Orkestra.Web.Module.Common.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace Krialys.Orkestra.Web.Module.ETQ.Pages;

public partial class ReferencesP1
{
    // Parent instances
    private OrkaGenericGridComponent<TDOM_DOMAINES> Ref_TDOM_DOMAINES;
    private OrkaGenericGridComponent<TPRCP_PRC_PERIMETRES> Ref_TPRCP_PRC_PERIMETRES;
    private IEnumerable<TRU_USERS> TruUsersData { get; set; } = Enumerable.Empty<TRU_USERS>();
    // private bool IsEditOrAdd { get; set; }
    //private bool IsEditOrAddPerimetre { get; set; }

    /// <summary>
    /// Retrieve ALL users from db_univers for local mapping
    /// </summary>
    private async Task GetAllUser() => TruUsersData = await ProxyCore.GetEnumerableAsync<TRU_USERS>();

    private bool Disabled { get; set; }

    /// <summary>
    /// Assign event
    /// </summary>
    protected override Task OnInitializedAsync()
    {
        // Subscribe SfTabEvent
        Bus.Subscribe<SfTabBusEvent>(e =>
        {
            var result = e.GetMessage<SfTabBusEvent>().Disabled;
            if (Disabled != result)
            {
                Disabled = result;
                // Refresh UI
                StateHasChanged();
            }
        });

        return GetAllUser();
    }

    private string UserId => Session.GetUserId();

    //public string[] CustomEditFields = new string[] {
    //    "TDOM_CODE",
    //};

    #region OnRowSelected

    /// <summary>
    /// Custom event when a row is selected we can add some controls onto any cells
    /// </summary>
    /// <param name="args"></param>
    private void OnRowSelected<TEntity>(RowSelectEventArgs<TEntity> args) where TEntity : class, new()
    {
        // Without this, you may see noticable delay in selection with 75 rows in grid.
        args.PreventRender = true;

        // Here you can customize your code
        //var row = args.Data;

        //if(args.Data is TEQC_ETQ_CODIFS teqc)
        //{
        //    teqc.TEQC_SEPARATEUR ??= "z";
        //}

        args.PreventRender = false;
    }

    #endregion

    #region OnDataBound

    /// <summary>
    /// Custom event triggered before data is bound to datagrid
    /// Some business rules can be applied precisely onto all or onto several lines and cells using LINQ
    /// CSS rules can be applied to enable line edition for example or for colouring a cell...
    /// </summary>
    /// <param name="args"></param>
    public void BeforeDataBound<TEntity>(BeforeDataBoundArgs<TEntity> args) where TEntity : class, new()
    {
        // Without this, you may see noticable delay in selection with 75 rows in grid.
        args.PreventRender = true;

        //// Update datas here if necessary
        //if (args.Result is List<TPRCP_PRC_PERIMETRES> tprcp)
        //{
        //    tprcp.FirstOrDefault().TPRCP_DATE_CREATION = tprcp.FirstOrDefault().TPRCP_DATE_CREATION.ToUniversalTime();
        //}

        // Here you can customize your code: apply rules onto each row/cell
        //foreach (var row in args.Result)
        //{
        //}

        args.PreventRender = false;
    }

    #endregion

    #region SaveAsync

    /// <summary>
    /// Custom event to handle and manipulate datas before saving to database
    /// Some business rules can be applied here using EndEdit, or reject globally using CloseEdit()
    /// </summary>
    /// <param name="instance">Parent OrkaGenericGridComponent instance</param>
    /// <param name="entity">Incoming datas to be saved</param>
    /// <returns></returns>
    private async Task SaveAsync<TEntity>(OrkaGenericGridComponent<TEntity> instance, object entity) where TEntity : class, new()
    {
        // Update datas here if necessary
        if (entity is TPRCP_PRC_PERIMETRES tprcp)
        {
            if (tprcp.TPRCP_PRC_PERIMETREID == 0)
            {
                if (!string.IsNullOrEmpty(tprcp.TPRCP_CODE))
                    tprcp.TPRCP_CODE = tprcp.TPRCP_CODE.ToUpper().Trim();

                tprcp.TRU_ACTEURID = UserId;//   id user de la personne connectée 
                tprcp.TPRCP_DATE_CREATION = DateExtensions.GetLocaleNow(); //DateExtensions.GetUtcNow();
            }
        }
        else if (entity is TDOM_DOMAINES { TDOM_DOMAINEID: 0 } tprs)
        {
            if (!string.IsNullOrEmpty(tprs.TDOM_CODE))
                tprs.TDOM_CODE = tprs.TDOM_CODE.ToUpper().Trim();

            tprs.TRU_ACTEURID = UserId;//   id user de la personne connectée 
            tprs.TDOM_DATE_CREATION = DateExtensions.GetLocaleNow(); //DateExtensions.GetUtcNow();
        }

        await instance.DataGrid.EndEditAsync();
    }

    #endregion

    private void ActionBeginHandler(ActionEventArgs<TDOM_DOMAINES> args)
    {
        //args.PreventRender = true;

        //if (args.RequestType is Syncfusion.Blazor.Grids.Action.BeginEdit or Syncfusion.Blazor.Grids.Action.Add)
        //{
        //    IsEditOrAdd = true;
        //}
        //else
        // if (args.RequestType != Syncfusion.Blazor.Grids.Action.Refresh) IsEditOrAdd = false;

        //args.PreventRender = false;

    }

    private void ActionBeginHandlerPerimetres(ActionEventArgs<TPRCP_PRC_PERIMETRES> args)
    {
        //args.PreventRender = true;

        //if (args.RequestType is Syncfusion.Blazor.Grids.Action.BeginEdit or Syncfusion.Blazor.Grids.Action.Add)
        //{
        //    IsEditOrAddPerimetre = true;
        //}
        //else
        // if (args.RequestType != Syncfusion.Blazor.Grids.Action.Refresh) IsEditOrAddPerimetre = false;

        //args.PreventRender = false;
    }

    public string GetHeaderDomaine(TDOM_DOMAINES value)
    {
        return value.TDOM_DOMAINEID == 0
            ? "Créer un domaine."
            : "Editer le domaine : " + value.TDOM_CODE;
    }

    public string GetHeaderPerimetre(TPRCP_PRC_PERIMETRES value)
    {
        return value.TPRCP_PRC_PERIMETREID == 0
            ? "Créer un périmètre."
            : "Editer le périmètre : " + value.TPRCP_CODE;
    }

    private void OnTabSelecting(SelectingEventArgs args)
    {
        // Disable Tab navigation on Tab selection.
        if (args.IsSwiped)
        {
            args.Cancel = true;
        }
        else
        {
            // Set Disabled value then fire event to SfTab
            Bus.Publish(new SfTabBusEvent { Disabled = true });

            // OB-649: destroy cache, mandatory for adding/editing a perimeter within its related grid
            ProxyCore.GetOrSetDisablingCacheStatus(disableCache: true);
        }
    }
}