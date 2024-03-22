using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.WebApi.Services.Admin;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Admin;

[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class LicenseController : ControllerBase
{
    private readonly ILicenceService _iLicenseServices;

    public LicenseController(ILicenceService iUserServices)
        => _iLicenseServices = iUserServices ?? throw new ArgumentNullException(nameof(iUserServices));

    [HttpGet("GenerateLicenseKeyAsync")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    public async Task<string> GenerateLicenseKeyAsync([FromQuery] Licence licence) => await _iLicenseServices.GenerateLicenseKey(licence);

    [HttpGet("DecryptLicenseKeyAsync")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Licence))]
    public async Task<Licence> DecryptLicenseKeyAsync([FromQuery] string licenceKey) => await _iLicenseServices.DecryptLicenseKey(licenceKey);

    [HttpGet("IsActualLicenseValidAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Licence))]
    public async Task<Licence> IsActualLicenseValidAsync() => await _iLicenseServices.IsActualLicenseValid();

    [HttpGet("IsLicenseKeyValidAsync")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> IsLicenseKeyValidAsync([FromQuery] Licence licence) => await _iLicenseServices.IsLicenseKeyValid(licence);

    [HttpPost("UpdateLicenseAsync")]
    [Produces(Litterals.ApplicationJson)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    public async Task<bool> UpdateLicenseAsync([FromQuery] string licence) => await _iLicenseServices.UpdateLicence(licence);
}