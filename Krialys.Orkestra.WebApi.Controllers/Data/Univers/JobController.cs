using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Services.Data;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.UNIVERS;

[ApiExplorerSettings]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class JobController : ControllerBase
{
    private readonly IJobServices _jobServices;

    public JobController(IJobServices jobServices)
    {
        _jobServices = jobServices ?? throw new ArgumentNullException(nameof(jobServices));
    }

    [Authorize]
    [HttpGet("GetAll")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<TEM_ETAT_MASTERS>))]
    public async Task<List<TEM_ETAT_MASTERS>> GetAll() => await _jobServices.GetAll();

    [Authorize]
    [HttpGet("Export")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async Task<string> Export(int jobId) => await _jobServices.ExportJob(jobId);

    [Authorize]
    [HttpGet("Import")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> Import(string JsonJob) => await _jobServices.ImportJob(JsonJob);
}
