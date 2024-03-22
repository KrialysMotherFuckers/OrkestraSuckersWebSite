namespace Krialys.Orkestra.WebApi.Services.Identity.Tokens;

public record RefreshTokenRequest(string Token, string RefreshToken);