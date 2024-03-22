using System.Security.Claims;
using System.Text.Json;

namespace Krialys.Common.Auth; // OK https://youtu.be/2c4p6RGtkps

public static class JwtParser
{
    /// <summary>
    /// Get claims from a given Jwt token
    /// </summary>
    /// <param name="jwt"></param>
    /// <returns></returns>
    public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        List<Claim> claims = new();

        try
        {
            var payload = jwt.Split('.')[1];

            var keyValuePairs = JsonSerializer.Deserialize<IDictionary<string, object>>(Convert.FromBase64String((payload.Length % 4) switch
            {
                2 => $"{payload}==",
                3 => $"{payload}=",
                _ => payload,
            }));

            ExtractRolesFromJwt(claims, keyValuePairs);

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString() ?? string.Empty)));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERR] ParseClaimsFromJwt => {ex.Message}");
        }

        return claims;
    }

    /// <summary>
    /// Extract roles from a given Jwt token
    /// </summary>
    /// <param name="claims"></param>
    /// <param name="keyValuePairs"></param>
    private static void ExtractRolesFromJwt(List<Claim> claims, IDictionary<string, object> keyValuePairs)
    {
        keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles);

        if (roles is null)
        {
            return;
        }

        var parsedRoles = roles.ToString()?.Trim().TrimStart('[').TrimEnd(']').Split(',');

        if (parsedRoles is { Length: > 1 })
            claims.AddRange(parsedRoles.Select(parsedRole => new Claim(ClaimTypes.Role, parsedRole.Trim('"'))));
        else
            claims.Add(new Claim(ClaimTypes.Role, parsedRoles?[0] ?? string.Empty));

        keyValuePairs.Remove(ClaimTypes.Role);
    }
}