using Krialys.Common.Extensions;
using Krialys.Common.Literals;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Krialys.Orkestra.WebApi.Services.Auth;

public interface IAuthentificationServices : IScopedService
{
    /// <summary>
    /// Create authentication code.
    /// </summary>
    string CreateAuthCode();

    /// <summary>
    /// Create access token.
    /// </summary>
    string CreateAccessToken(IEnumerable<Claim> claims, out int expIn,
        string _issuer = null, string _audience = null, int? _accessTokenLifetime = null, string _tokenSigningKey = null);

    /// <summary>
    /// Create refresh token.
    /// </summary>
    string CreateRefreshToken();
}

public class AuthentificationServices : IAuthentificationServices
{
    /* Values from appsettings.json. */
    private readonly IConfiguration _appSettings;

    public AuthentificationServices(IConfiguration configuration)
    {
        _appSettings = configuration;
    }

    /// <summary>
    /// Generate a random string based on a cryptographic library.
    /// </summary>
    private static string GenerateRandomString()
    {
        var randomNumber = new byte[32];

        /* Use a cryptographic library. */
        using var rng = RandomNumberGenerator.Create();

        /* Fill array with random numbers. */
        rng.GetBytes(randomNumber);

        /* Convert to string. */
        return Convert.ToBase64String(randomNumber);
    }

    /// <summary>
    /// Create authentication code.
    /// </summary>
    public string CreateAuthCode() => GenerateRandomString();

    /// <summary>
    /// Create access token.
    /// </summary>
    public string CreateAccessToken(IEnumerable<Claim> claims, out int expIn,
        string _issuer = null, string _audience = null, int? _accessTokenLifetime = null, string _tokenSigningKey = null)
    {
        // Url of the issuer of the token
        var issuer = _issuer ?? _appSettings[_appSettings["Authentication:Issuer"] ?? string.Empty];

        // URL of the audience accepting this token
        var audience = _audience ?? _appSettings[_appSettings["Authentication:Audience"] ?? string.Empty];

        // Override token Lifetime (defaulted to Authentication:AccessTokenLifetime when TokenLifetime was not declared in TRUCL_UTILISATEURS_CLAIMS)
        var enumerable = claims;
        var enumerable1 = enumerable as Claim[] ?? enumerable.ToArray();
        var tokenLifetime = Convert.ToInt32(enumerable1.FirstOrDefault(x => x.Type.Equals(ClaimsLiterals.TokenLifetime, StringComparison.Ordinal))?.Value ?? "0");
        tokenLifetime = _accessTokenLifetime ?? tokenLifetime;
        expIn = tokenLifetime != 0 ? tokenLifetime : Convert.ToInt32(_appSettings["Authentication:AccessTokenLifetime"]);

        var tokenSigningKey = _tokenSigningKey ?? _appSettings["Authentication:TokenSigningKey"];

        // Convert string into binary
        if (tokenSigningKey != null)
        {
            var secretBytes = Encoding.UTF8.GetBytes(tokenSigningKey);

            // Token credentials
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(secretBytes), SecurityAlgorithms.HmacSha256);

            // Create JSON Web Token (JWT)/
            var token = new JwtSecurityToken(
                // Issuer of the token (URI)
                issuer,
                // Recipients of the token (Array of URI)
                audience,
                // Claims table */
                enumerable1,
                // Time before which the token must not be accepted for processing
                notBefore: DateExtensions.GetUtcNow(),
                // Time after which the JWTmust not be accepted for processing
                expires: DateExtensions.GetUtcNow().AddSeconds(expIn),
                // Credentials used to sign the token
                signingCredentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        return null;
    }

    /// <summary>
    /// Create refresh token.
    /// </summary>
    public string CreateRefreshToken() => GenerateRandomString();
}

