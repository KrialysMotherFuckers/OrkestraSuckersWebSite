using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Data;

namespace Krialys.Orkestra.WebApi.Controllers.Data.Etq;

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Litterals.EtqRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class EtiquettesController : ControllerBase
{
    #region Injected services
    /// <summary>
    /// Services related to labels (TETQ_ETIQUETTES).
    /// </summary>
    private readonly IEtiquettesServices _etiquettesServices;

    /// <summary>
    /// Common services for the server.
    /// </summary>
    private readonly ICommonServices _commonServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="EtiquettesController"/> class.
    /// </summary>
    /// <param name="etiquettesServices">Services related to labels (TETQ_ETIQUETTES).</param>
    /// <param name="commonServices">Common services for the server.</param>
    /// <exception cref="ArgumentNullException">Dependency injection failed.</exception>
    public EtiquettesController(IEtiquettesServices etiquettesServices,
        ICommonServices commonServices)
    {
        _etiquettesServices = etiquettesServices ?? throw new ArgumentNullException(nameof(etiquettesServices));
        _commonServices = commonServices;
    }
    #endregion

    #region Authorizations
    /// <summary>
    /// Apply authorizations on selected label.
    /// </summary>
    /// <param name="args">Label authorization arguments.</param>
    /// <param name="id">Id of selected label.</param>
    /// <returns>Http status code.</returns>
    [Authorize]
    [HttpPut("{id}/[action]")]
    public Task<IActionResult> AuthorizationsAsync([FromBody] EtqAuthorizationArguments args,
        int id)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _etiquettesServices.SetEtqAuthorizationsAsync(id, userId, args);
    }
    #endregion
}
