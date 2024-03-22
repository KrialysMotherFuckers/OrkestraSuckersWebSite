using Krialys.Data.EF.RefManager;
using Krialys.Orkestra.Common.Models;
using Krialys.Orkestra.WebApi.Services.RefManager;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Admin;

[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class RefManagerController : ControllerBase
{
    private readonly IRefManagerServices _iRefManagerServices;

    public RefManagerController(IRefManagerServices iRefManagerServices)
        => _iRefManagerServices = iRefManagerServices ?? throw new ArgumentNullException(nameof(iRefManagerServices));

    [HttpPost("RefreshData")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask RefreshData()
        => await _iRefManagerServices.FireAndForgetRefreshData();

    [HttpGet("GetReferentialTableInfo")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ReferentialTableInfo))]
    public async ValueTask<ReferentialTableInfo> GetReferentialTableInfo(int referentialTableId)
        => await _iRefManagerServices.GetReferentialTableInfo(referentialTableId);

    [HttpGet("GetReferentialTableData")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(byte[]))]
    public async ValueTask<byte[]> GetReferentialTableData(int referentialTableId)
        => await _iRefManagerServices.GetReferentialTableData(referentialTableId);

    [HttpPost("UpdateReferentialTableData")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async ValueTask<bool> UpdateReferentialTableData(int referentialTableId, [FromBody] byte[] jsonData, bool approved)
        => await _iRefManagerServices.UpdateReferentialTableData(referentialTableId, jsonData, approved);

    [HttpGet("CloneLabelObjectCodeEntries")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    public async ValueTask<int> CloneLabelObjectCodeEntries(int referentialTableId, string labelcodeObject)
        => await _iRefManagerServices.CloneLabelObjectCodeEntriesAsync(referentialTableId, labelcodeObject);

    [HttpPost("UpdateReferential")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async ValueTask<bool> UpdateReferential(ReferentialTable record)
        => await _iRefManagerServices.UpdateReferential(record);

    [HttpGet("ApproveReferential")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async ValueTask<bool> ApproveReferential(TM_RFS_ReferentialSettings record)
        => await _iRefManagerServices.ApproveReferential(record);

    [HttpPost("AddOrUpdateHistory")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async ValueTask AddOrUpdateHistory(TM_RFH_ReferentialHistorical record)
        => await _iRefManagerServices.AddOrUpdateHistory(record);

    [HttpPost("GetGdbRequestTohandle")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GdbRequestToHandle))]
    public async ValueTask<GdbRequestToHandle> GetGdbRequestTohandle(GdbRequestAction action)
        => await _iRefManagerServices.GetGdbRequestTohandle(action);

    [HttpPost("GdbRequestHandled")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async ValueTask<bool> GdbRequestHandled(GdbRequestHandled requestHandled)
        => await _iRefManagerServices.GdbRequestHandled(requestHandled);
}