using Krialys.Data.EF.RefManager;
using Krialys.Orkestra.Common.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.WebApi.Controllers.oData;

[ApiExplorerSettings]
public class TR_CNX_ConnectionsController : ODataController
{
    private readonly KrialysDbContext _dbRefContext;
    private readonly Serilog.ILogger _ilogger;

    public TR_CNX_ConnectionsController(
        KrialysDbContext dbRefContext,
        Serilog.ILogger ilogger)
    {
        _dbRefContext = dbRefContext ?? throw new ArgumentNullException(nameof(dbRefContext));
        _ilogger = ilogger ?? throw new ArgumentNullException(nameof(ilogger));
    }

    [HttpPost("TR_CNX_ConnectionsListPagedResponse")]
    public async Task<PagedResponse<TR_CNX_Connections>> TR_CNX_ConnectionsListPagedResponse([FromBody] DataManagerRequest dm)
    {
        IEnumerable<TR_CNX_Connections> DataSource = await _dbRefContext.TR_CNX_Connections.ToListAsync();
        if (dm.Search != null && dm.Search.Count > 0)
        {
            DataSource = DataOperations.PerformSearching(DataSource, dm.Search);  //Search 
        }
        if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting 
        {
            DataSource = DataOperations.PerformSorting(DataSource, dm.Sorted);
        }
        if (dm.Where != null && dm.Where.Count > 0) //Filtering 
        {
            DataSource = DataOperations.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
        }
        int count = DataSource.Cast<TR_CNX_Connections>().Count();
        if (dm.Skip != 0)
        {
            DataSource = DataOperations.PerformSkip(DataSource, dm.Skip); //Paging 
        }
        if (dm.Take != 0)
        {
            DataSource = DataOperations.PerformTake(DataSource, dm.Take);
        }

        return await Task.FromResult(
            new PagedResponse<TR_CNX_Connections>()
            {
                result = DataSource.Cast<TR_CNX_Connections>(),
                count = count
            });
    }

    [HttpPost("TR_CNX_ConnectionsUpdate")]
    public void TR_CNX_ConnectionsUpdate([FromBody] CRUDModel<TR_CNX_Connections> record)
    {
        _dbRefContext.Entry(record.Value).State = EntityState.Modified;
        _dbRefContext.SaveChanges();
    }

    [HttpPost("TR_CNX_ConnectionsAdd")]
    public void TR_CNX_ConnectionsAdd([FromBody] CRUDModel<TR_CNX_Connections> record)
    {
        _dbRefContext.TR_CNX_Connections.Add(record.Value);
        _dbRefContext.SaveChanges();
    }

    [HttpPost("TR_CNX_ConnectionsRemove")]
    public void TR_CNX_ConnectionsRemove([FromBody] CRUDModel<TR_CNX_Connections> record)
    {
        var recordToDelete = _dbRefContext.TR_CNX_Connections.Find(Convert.ToInt32(Convert.ToString(record.Key)));
        _dbRefContext.TR_CNX_Connections.Remove(recordToDelete);
        _dbRefContext.SaveChanges();
    }
}