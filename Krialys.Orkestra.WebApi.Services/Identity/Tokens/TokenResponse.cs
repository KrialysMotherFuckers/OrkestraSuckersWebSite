namespace Krialys.Orkestra.WebApi.Services.Identity.Tokens;

public record TokenResponse(string Token, string RefreshToken, DateTime RefreshTokenExpiryTime);