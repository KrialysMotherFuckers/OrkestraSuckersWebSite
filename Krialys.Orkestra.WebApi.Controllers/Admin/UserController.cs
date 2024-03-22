using Krialys.Orkestra.Common.Models.Admin;
using Krialys.Orkestra.WebApi.Controllers.Attributes;
using Krialys.Orkestra.WebApi.Services.Admin;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Krialys.Orkestra.WebApi.Controllers.Admin;

[NoCache]
[Authorize]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IMenuService _iMenuServices;

    public UserController(IMenuService iMenuServices)
        => _iMenuServices = iMenuServices ?? throw new ArgumentNullException(nameof(iMenuServices));

    [HttpGet]
    [AllowAnonymous]
    [Route("GetUserMenu")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserMenu>))]
    [Produces(Litterals.ApplicationJson)]
    public async Task<IEnumerable<UserMenu>> GetUserMenuAsync()
        => await _iMenuServices.GetUserMenu(User!.FindFirstValue(ClaimTypes.NameIdentifier));
    
    [HttpGet]
    [Route("GetMenuSettings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<UserMenu>))]
    [Produces(Litterals.ApplicationJson)]
    public async Task<IEnumerable<UserMenu>> GetMenuSettings()
        => await _iMenuServices.GetMenuSettings();

    [HttpPost]
    [Route("SaveMenuSettings")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [Produces(Litterals.ApplicationJson)]
    public async Task<bool> SaveMenuSettings(IEnumerable<UserMenu> menuList)
        => await _iMenuServices.SaveMenuSettings(menuList);
}