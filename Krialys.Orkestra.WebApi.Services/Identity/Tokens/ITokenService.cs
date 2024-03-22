namespace Krialys.Orkestra.WebApi.Services.Identity.Tokens;

public interface ITokenService //: ITransientService
{
    ValueTask<TokenResponse> GetTokenAsync(TokenRequest request, string ipAddress, CancellationToken cancellationToken);

    ValueTask<TokenResponse> RefreshTokenAsync(RefreshTokenRequest request, string ipAddress);
}