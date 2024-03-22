using System.Security.Claims;

namespace Krialys.Orkestra.WebApi.Services.Identity.Tokens;

public interface ICurrentUser
{
    string Name { get; }

    Guid GetUserId();

    string GetUserEmail();

    string GetTenant();

    bool IsAuthenticated();

    bool IsInRole(string role);

    IEnumerable<Claim> GetUserClaims();
}