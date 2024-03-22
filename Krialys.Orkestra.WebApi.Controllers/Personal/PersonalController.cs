using Krialys.Orkestra.Common.Extensions;
using Krialys.Orkestra.WebApi.Services.Identity.Users;
using Krialys.Orkestra.WebApi.Services.Identity.Users.Password;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Personal;

[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public class PersonalController : ControllerBase// VersionNeutralApiController
{
    private readonly IUserService _userService;

    public PersonalController(IUserService userService) => _userService = userService;

    [HttpGet("GetProfileAsync")]
    [OpenApiOperation("Get profile details of currently logged in user.", "")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDetailsDto))]
    public async Task<ActionResult<UserDetailsDto>> GetProfileAsync(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetAsync(userId, cancellationToken));
    }

    [HttpPut("UpdateProfileAsync")]
    [OpenApiOperation("Update profile details of currently logged in user.", "")]
    public async Task<ActionResult> UpdateProfileAsync(UpdateUserRequest request)
    {
        if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _userService.UpdateAsync(request, userId);
        return Ok();
    }

    [HttpPut("change-password")]
    [OpenApiOperation("Change password of currently logged in user.", "")]
    [ApiConventionMethod(typeof(OrkaApiConvention), nameof(OrkaApiConvention.Register))]
    public async Task<ActionResult> ChangePasswordAsync(ChangePasswordRequest model)
    {
        if (User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        await _userService.ChangePasswordAsync(model, userId);
        return Ok();
    }

    [HttpGet("GetPermissionsAsync")]
    [OpenApiOperation("Get permissions of currently logged in user.", "")]
    public async Task<ActionResult<List<string>>> GetPermissionsAsync(CancellationToken cancellationToken)
    {
        return User.GetUserId() is not { } userId || string.IsNullOrEmpty(userId)
            ? Unauthorized()
            : Ok(await _userService.GetPermissionsAsync(userId, cancellationToken));
    }

    //[HttpGet("logs")]
    //[OpenApiOperation("Get audit logs of currently logged in user.", "")]
    //public Task<List<AuditDto>> GetLogsAsync()
    //{
    //    return Mediator.Send(new GetMyAuditLogsRequest());
    //}
}