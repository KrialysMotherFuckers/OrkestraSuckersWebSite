using System.Security.Claims;

namespace Krialys.Orkestra.WebApi.Services.Identity.Tokens;

public interface ICurrentUserInitializer
{
    void SetCurrentUser(ClaimsPrincipal user);

    void SetCurrentUserId(string userId);
}