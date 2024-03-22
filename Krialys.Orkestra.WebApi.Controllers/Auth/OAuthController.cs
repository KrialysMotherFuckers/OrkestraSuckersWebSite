using Krialys.Orkestra.WebApi.Controllers.Attributes;
using Krialys.Orkestra.WebApi.Services.Authentification;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Controllers.Auth;

/// <summary>
/// Controller implementing the server side of OAuth2 authorization RFC 6749
/// </summary>
/// <remarks>
/// https://tools.ietf.org/html/rfc6749
/// </remarks>
[ApiExplorerSettings(IgnoreApi = true)]
[Route(Litterals.OAuthRootPath)]
public class OAuthController : Controller
{
    private readonly IOAuthServices _authServices;
    private readonly Serilog.ILogger _logger;
    private readonly string _remoteIpAddress;
    //private readonly string _hostName;
    private readonly string _na = "N/A";

    public OAuthController(IOAuthServices authServices, Serilog.ILogger logger, IHttpContextAccessor accessor)
    {
        _authServices = authServices ?? throw new ArgumentNullException(nameof(authServices));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ = accessor ?? throw new ArgumentNullException(nameof(accessor));
        _remoteIpAddress = accessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? _na;
        //_hostName = _remoteIpAddress.Equals(_na, StringComparison.Ordinal)
        //    ? _na
        //    : Dns.GetHostEntry(_remoteIpAddress)?.HostName;
    }

    [NoCache]
    [HttpPost(Litterals.AuthorizeUser)]
    public async Task<IActionResult> AuthorizeUserAsync(string grant_type, string login, string password)
    {
        // *** Check if the request is valid (if any grant type does not exist, then error) ***
        if (!grant_type.Equals(Litterals.PasswordCredentials, StringComparison.Ordinal))
        {
            await TokenFailedAsync(Litterals.UnsupportedGrantType, $"Unknown grant type: {grant_type}");

            return BadRequest();
        }

        // User found?
        if (string.IsNullOrEmpty(_authServices.GetUserIdFromLoginAndPassword(login, _authServices.DecodeFrom64(password))))
        {
            _logger.Warning($"Login failed: {login}, IpAddress: {_remoteIpAddress}"); //, HostName: {_hostName}");
            ViewBag.Message = "Login or password incorrect. Please try again.";

            return BadRequest();
        }

        // *** Generate a Jwt Token ***
        string access_token = _authServices.GetUserAccessToken(login, password);

        return Ok(new
        {
            Access_Token = access_token,
            LogIn = login
        });
    }

    /// <summary>
    /// Method called when token endpoint fails.
    /// Prepare failed reponse from token endpoint.
    /// </summary>
    /// <param name="error_message">Message of error send to client.</param>
    /// <param name="error_description">Message of error send to client.</param>
    /// <param name="traceError"></param>
    private ValueTask TokenFailedAsync(
        string error_message,
        string error_description, bool traceError = true)
    {
        if (traceError)
            _logger.Warning($"Messages: {error_message}, Description: {error_description}");

        // Create response
        var responseBytes = JsonSerializer.SerializeToUtf8Bytes(new
        {
            error_message,
            error_description
        });

        // Set response headers and status code
        Response.Headers.Clear();
        Response.Headers.Add("Content-Type", $"{Litterals.ApplicationJson};charset=utf-8");
        Response.Headers.Add("Cache-Control", "no-store");
        Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return Response.Body.WriteAsync(responseBytes.AsMemory(0, responseBytes.Length));
    }

    /// <summary>
    /// Create a ciphered password
    /// </summary>
    /// <param name="clearPassword"></param>
    /// <returns></returns>
    [HttpPut("CreateCipheredPassword")]
    public string CreateCipheredPassword([FromQuery] string clearPassword)
    {
        var ciphered = _authServices.CreateCipheredPassword(clearPassword);

        return ciphered;
    }

    /// <summary>
    /// Check validity from a ciphered password versus clear password
    /// </summary>
    /// <param name = "cipherPassword" ></param>
    /// < param name="clearPassword"></param>
    /// <returns></returns>
    [HttpPut("IsValidPassword")]
    public bool IsValidPassword([FromQuery] string cipherPassword, [FromQuery] string clearPassword)
        => _authServices.IsValidPassword(cipherPassword, clearPassword);
}