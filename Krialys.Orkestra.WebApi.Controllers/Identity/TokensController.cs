using Krialys.Orkestra.WebApi.Controllers.Attributes;
using Krialys.Orkestra.WebApi.Services.Identity.Tokens;
using Microsoft.AspNetCore.Http;

namespace Krialys.Orkestra.WebApi.Controllers.Identity;

[NoCache]
[Area(Litterals.UniversRootPath)]
[Route("[area]/[controller]")]
[ApiController]
public sealed class TokensController : ControllerBase
{
    private readonly ITokenService _tokenService;
    private readonly string _remoteIpAddress;

    public TokensController(ITokenService tokenService, IHttpContextAccessor accessor)
    {
        _tokenService = tokenService;
        _remoteIpAddress = accessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "N/A";
    }

    [HttpPost("GetTokenAsync")]
    [AllowAnonymous]
    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken)
        => await _tokenService.GetTokenAsync(request, _remoteIpAddress, cancellationToken);

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<TokenResponse> RefreshAsync(RefreshTokenRequest request)
        => await _tokenService.RefreshTokenAsync(request, _remoteIpAddress);
}