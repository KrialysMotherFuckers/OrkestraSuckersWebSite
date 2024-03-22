using Asp.Versioning;

namespace Krialys.Orkestra.WebApi.Controllers;

[Route("api/[controller]")]
[ApiVersionNeutral]
public class VersionNeutralApiController : BaseApiController
{
}
