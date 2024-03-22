using Krialys.Orkestra.Common.Shared;
using Krialys.Orkestra.WebApi.Services.Common;
using Krialys.Orkestra.WebApi.Services.Data;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.UNIVERS;

[ApiExplorerSettings(IgnoreApi = true)]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    /// <summary>
    /// Services related to orders (TCMD_COMMANDES).
    /// </summary>
    private readonly IOrdersServices _ordersServices;

    /// <summary>
    /// Common services for the server.
    /// </summary>
    private readonly ICommonServices _commonServices;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="ordersServices">Services related to orders (TCMD_COMMANDES).</param>
    /// <param name="commonServices">Common services for the server.</param>
    /// <exception cref="ArgumentNullException">Dependency injection failed.</exception>
    public OrdersController(IOrdersServices ordersServices, ICommonServices commonServices)
    {
        _ordersServices = ordersServices ?? throw new ArgumentNullException(nameof(ordersServices));
        _commonServices = commonServices;
    }

    /// <summary>
    /// API endpoint to delete an element of table TCMD_COMMANDES.
    /// It is only possible for an order in phase draft.
    /// </summary>
    /// <param name="id">Id of the order (TCMD_COMMANDEID).</param>
    /// <returns>Http status code.</returns>
    [Authorize]
    [HttpDelete("{id}")]
    public Task<IActionResult> DeleteOrderAsync(int id)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _ordersServices.DeleteOrderAsync(id, userId);
    }

    /// <summary>
    /// API endpoint to change phase of an element of table TCMD_COMMANDES.
    /// </summary>
    /// <param name="args">Change order phase arguments.</param>
    /// <param name="id">Id of the order (TCMD_COMMANDEID).</param>
    /// <returns>Http status code.</returns>
    [Authorize]
    [HttpPut("{id}")]
    public Task<IActionResult> ChangeOrderPhaseAsync([FromBody] ChangeOrderPhaseArguments args,
        int id)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _ordersServices.ChangeOrderPhaseAsync(id, userId, args);
    }

    /// <summary>
    /// API endpoint to duplicate an order.
    /// </summary>
    /// <param name="id">Id of the order to duplicate.</param>
    /// <returns>HTTP response (with the given response status code).</returns>
    [Authorize]
    [HttpPost("{id}/[action]")]
    public Task<IActionResult> DuplicateAsync(int id)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _ordersServices.DuplicateOrderAsync(id, userId);
    }

    /// <summary>
    /// Save a document (in a directory and in database).
    /// </summary>
    /// <param name="UploadFiles">Uploaded document.</param>
    /// <param name="orderId">Id of the order to which the document is attached.</param>
    /// <returns>Http status code.</returns>
    [Authorize]
    [HttpPost("[action]")]
    public Task<IActionResult> SaveDocumentAsync(IList<IFormFile> UploadFiles,
        [FromForm] int orderId)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _ordersServices.SaveDocumentAsync(UploadFiles, orderId, userId);
    }

    /// <summary>
    /// Remove a document (from directory and database).
    /// </summary>
    /// <param name="UploadFiles">Document to remove.</param>
    /// <param name="orderId">Id of the order to which the document is attached.</param>
    /// <returns>Http status code.</returns>
    [Authorize]
    [HttpPost("[action]")]
    public Task<IActionResult> RemoveDocumentAsync([FromForm] string UploadFiles,
        [FromForm] int orderId)
    {
        // Get user Id from claims.
        (string userId, _) = _commonServices.GetUserIdAndName();

        return _ordersServices.RemoveDocumentAsync(UploadFiles, orderId, userId);
    }

    /// <summary>
    /// API endpoint to read productions associated with the selected order.
    /// </summary>
    /// <param name="id">Id of the selected order.</param>
    /// <returns>HTTP response (with the given response status code).</returns>
    [Authorize]
    [HttpGet("{id}/[action]")]
    public IActionResult Productions(int id)
        => _ordersServices.ReadAssociatedProductions(id);
}
