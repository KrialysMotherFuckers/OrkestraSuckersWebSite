using Krialys.Orkestra.WebApi.Services.Data;

namespace Krialys.Orkestra.WebApi.Controllers.UNIVERS;

[ApiExplorerSettings]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class VersionController : ControllerBase
{
    private readonly IVersionServices _versionServices;

    public VersionController(IVersionServices jobServices)
    {
        _versionServices = jobServices ?? throw new ArgumentNullException(nameof(jobServices));
    }

    [Authorize]
    [HttpPost("Duplicate")]
    public async Task<bool> Duplicate(int dpuIdToDuplicate) => await _versionServices.Duplicate(dpuIdToDuplicate);
}
