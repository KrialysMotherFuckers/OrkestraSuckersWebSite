using Krialys.Data.EF.RefManager;
using Krialys.Orkestra.Common.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Data;

namespace Krialys.Orkestra.WebApi.Controllers.oData;

[ApiExplorerSettings]
public class TM_RFS_ReferentialSettingsController : ODataController
{
    private readonly KrialysDbContext _dbRefContext;
    private readonly Serilog.ILogger _ilogger;

    public TM_RFS_ReferentialSettingsController(
        KrialysDbContext dbRefContext,
        Serilog.ILogger ilogger)
    {
        _dbRefContext = dbRefContext ?? throw new ArgumentNullException(nameof(dbRefContext));
        _ilogger = ilogger ?? throw new ArgumentNullException(nameof(ilogger));
    }

    [HttpPost("TM_RFS_ReferentialSettingsListPagedResponse")]
    public async Task<PagedResponse<TM_RFS_ReferentialSettings>> TM_RFS_ReferentialSettingsListPagedResponse([FromBody] DataManagerRequest dm)
    {
        IEnumerable<TM_RFS_ReferentialSettings> DataSource = await _dbRefContext.TM_RFS_ReferentialSettings.ToListAsync();
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
        int count = DataSource.Cast<TM_RFS_ReferentialSettings>().Count();
        if (dm.Skip != 0)
        {
            DataSource = DataOperations.PerformSkip(DataSource, dm.Skip);
        }
        if (dm.Take != 0)
        {
            DataSource = DataOperations.PerformTake(DataSource, dm.Take);
        }

        return await Task.FromResult(
            new PagedResponse<TM_RFS_ReferentialSettings>()
            {
                result = DataSource.Cast<TM_RFS_ReferentialSettings>(),
                count = count
            });
    }

    [HttpPost("TM_RFS_ReferentialSettingsUpdate")]
    public void TM_RFS_ReferentialSettingsUpdate([FromBody] CRUDModel<TM_RFS_ReferentialSettings> record)
    {
        _dbRefContext.Entry(record.Value).State = EntityState.Modified;
        _dbRefContext.SaveChanges();
    }

    [HttpPost("TM_RFS_ReferentialSettingsAdd")]
    public void TM_RFS_ReferentialSettingsAdd([FromBody] CRUDModel<TM_RFS_ReferentialSettings> record)
    {
        record.Value.TR_CNX_Connections = _dbRefContext.TR_CNX_Connections.FirstOrDefault(x => x.Cnx_Id == record.Value.Cnx_Id);
        record.Value.TX_RFX_ReferentialSettingsData = new TX_RFX_ReferentialSettingsData { Rfs_id = record.Value.Rfs_id };

        _dbRefContext.TM_RFS_ReferentialSettings.Add(record.Value);
        _dbRefContext.SaveChanges();
    }

    [HttpPost("TM_RFS_ReferentialSettingsRemove")]
    public void TM_RFS_ReferentialSettingsRemove([FromBody] CRUDModel<TM_RFS_ReferentialSettings> record)
    {
        var rfsId = Convert.ToInt32(Convert.ToString(record.Key));

        _dbRefContext.TX_RFX_ReferentialSettingsData.Remove(_dbRefContext.TX_RFX_ReferentialSettingsData.FirstOrDefault(x => x.Rfs_id == rfsId));
        _dbRefContext.TM_RFS_ReferentialSettings.Remove(_dbRefContext.TM_RFS_ReferentialSettings.Find(rfsId));
        _dbRefContext.SaveChanges();
    }
}