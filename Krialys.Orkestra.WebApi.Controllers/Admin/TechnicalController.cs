using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.WebApi.Services.Admin;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Admin;

[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class TechnicalController : ControllerBase
{
    private readonly ITechnicalService _iTechnicalService;

    public TechnicalController(ITechnicalService iUserServices)
        => _iTechnicalService = iUserServices ?? throw new ArgumentNullException(nameof(iUserServices));

    [HttpGet("DatabasePurgeAsync")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> DatabasePurgeAsync() => await _iTechnicalService.DatabasePurgeAsync();

    [HttpGet("OrkestraNodeWorkerUpdateCheck")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> OrkestraNodeWorkerUpdateCheck(int MajorVersion, int MinorVersion, int BuildVersion, int RevisionVersion, DateTime fileCreationDateUtc)
        => await _iTechnicalService.OrkestraNodeWorkerUpdateCheck(new Version(MajorVersion, MinorVersion, BuildVersion, RevisionVersion), fileCreationDateUtc);

    [HttpGet("OrkestraNodeWorkerFileNameGet")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async Task<string> OrkestraNodeWorkerFileNameGet() => await _iTechnicalService.OrkestraNodeWorkerFileNameGet();

    [HttpGet("OrkestraNodeWorkerGetUpdate")]
    [Produces("application/octet-stream")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(FileStreamResult))]
    public async Task<FileStreamResult> OrkestraNodeWorkerGetUpdate()
        => await Task.FromResult(File(await _iTechnicalService.OrkestraNodeWorkerGetUpdate(), "application/octet-stream", await _iTechnicalService.OrkestraNodeWorkerFileNameGet()));

    [HttpGet("OrkestraWebSiteUpdateCheck")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateDetails))]
    public async Task<UpdateDetails> OrkestraWebSiteUpdateCheck(int MajorVersion, int MinorVersion, int BuildVersion, int RevisionVersion)
        => await _iTechnicalService.OrkestraWebSiteUpdateCheck(new Version(MajorVersion, MinorVersion, BuildVersion, RevisionVersion));
}