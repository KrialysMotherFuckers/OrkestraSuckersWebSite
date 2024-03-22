using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.WebApi.Controllers.oData;

[ApiExplorerSettings]
public class TR_MEL_EMail_TemplatesController : ODataController
{
    private readonly KrialysDbContext _dbContext;
    private readonly Serilog.ILogger _ilogger;

    public TR_MEL_EMail_TemplatesController(
        KrialysDbContext dbRefContext,
        Serilog.ILogger ilogger)
    {
        _dbContext = dbRefContext ?? throw new ArgumentNullException(nameof(dbRefContext));
        _ilogger = ilogger ?? throw new ArgumentNullException(nameof(ilogger));
    }

    [HttpPost("TR_MEL_EMail_TemplatesListPagedResponse")]
    public async Task<PagedResponse<TR_MEL_EMail_Templates>> TR_MEL_EMail_TemplatesListPagedResponse([FromBody] DataManagerRequest dm)
    {
        IEnumerable<TR_MEL_EMail_Templates> DataSource = await _dbContext.TR_MEL_EMail_Templates.ToListAsync();
        if (dm.Search != null && dm.Search.Count > 0)
        {
            DataSource = DataOperations.PerformSearching(DataSource, dm.Search); 
        }
        if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting 
        {
            DataSource = DataOperations.PerformSorting(DataSource, dm.Sorted);
        }
        if (dm.Where != null && dm.Where.Count > 0) //Filtering 
        {
            DataSource = DataOperations.PerformFiltering(DataSource, dm.Where, dm.Where[0].Operator);
        }
        int count = DataSource.Cast<TR_MEL_EMail_Templates>().Count();
        if (dm.Skip != 0)
        {
            DataSource = DataOperations.PerformSkip(DataSource, dm.Skip);
        }
        if (dm.Take != 0)
        {
            DataSource = DataOperations.PerformTake(DataSource, dm.Take);
        }

        return await Task.FromResult(
            new PagedResponse<TR_MEL_EMail_Templates>()
            {
                result = DataSource.Cast<TR_MEL_EMail_Templates>(),
                count = count
            });
    }

    [HttpPost("TR_MEL_EMail_TemplatesUpdate")]
    public void TR_MEL_EMail_TemplatesUpdate([FromBody] CRUDModel<TR_MEL_EMail_Templates> record)
    {
        _dbContext.Entry(record.Value).State = EntityState.Modified;
        _dbContext.SaveChanges();
    }

    [HttpPost("TR_MEL_EMail_TemplatesAdd")]
    public void TR_MEL_EMail_TemplatesAdd([FromBody] CRUDModel<TR_MEL_EMail_Templates> record)
    {
        _dbContext.TR_MEL_EMail_Templates.Add(record.Value);
        _dbContext.SaveChanges();
    }

    [HttpPost("TR_MEL_EMail_TemplatesRemove")]
    public void TR_MEL_EMail_TemplatesRemove([FromBody] CRUDModel<TR_MEL_EMail_Templates> record)
    {
        var rfsId = Convert.ToInt32(Convert.ToString(record.Key));

        _dbContext.TR_MEL_EMail_Templates.Remove(_dbContext.TR_MEL_EMail_Templates.Find(rfsId));
        _dbContext.SaveChanges();
    }
}