using AuthenticatedEncryption;
using Krialys.Common.Interfaces;
using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.WebApi.Services.Auth;
using LdapForNet;
using LdapForNet.Native;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Runtime.ExceptionServices;
using System.Security.Claims;
using System.Text;

namespace Krialys.Orkestra.WebApi.Services.Authentification;

public interface IOAuthServices : IScopedService
{
    #region PHYSICAL USERS
    string GetUserIdFromLoginAndPassword(string login, string password);

    IEnumerable<Claim> GetUserClaimsFromUserId(string userId);
    #endregion PHYSICAL USERS

    #region CLIENT APPLICATIONS
    int? GetClientAppIdFromAuthPublicKey(string clientAppAuthPublicKey);

    IEnumerable<Claim> GetClientAppClaimsFromAuthPublicKey(string clientAppAuthPublicKey);

    bool CheckClientAppFromAuthPublicAndSecretKey(string clientAppAuthPublicKey, string clientAppAuthSecretKey);
    #endregion CLIENT APPLICATIONS

    string GetUserAccessToken(string login, string password);

    string GetAppAccessToken(string clientId);//, string publicKey);

    string DecodeFrom64(string encodedData);

    #region PASSWORD
    string CreateCipheredPassword(string clearPassword);

    bool IsValidPassword(string cipherPassword, string clearPassword);
    #endregion
}

public class OAuthServices : IOAuthServices
{
    private readonly KrialysDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IAuthentificationServices _credentialServices;

    public OAuthServices(KrialysDbContext dbContext, IConfiguration configuration, IAuthentificationServices credentialServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _credentialServices = credentialServices ?? throw new ArgumentNullException(nameof(credentialServices));
    }

    /// <summary>
    /// Check if if the client is known and active using its public and secret Ids.
    /// </summary>
    /// <param name="clientAppAuthPublicKey">Client public authentication auth public key (ex: Krialys.Mso)</param>
    /// <param name="clientAppAuthSecretKey">Client secret authentication auth secret key (ex: BF4C1FDF-C85E-45FA-B6A6-EE1FC7DB78A1)</param>
    /// <returns>True if client is identified and active, false otherwise.</returns>
    public bool CheckClientAppFromAuthPublicAndSecretKey(string clientAppAuthPublicKey, string clientAppAuthSecretKey)
    {
        bool ret = false;
        ExceptionDispatchInfo exceptionDispatchInfo = null;

        try
        {
            ret = _dbContext.TRCLI_CLIENTAPPLICATIONS
                .Any(cli => cli.TRCLI_AUTH_PUBLIC.Equals(clientAppAuthPublicKey) &&
                    cli.TRCLI_AUTH_SECRET.Equals(clientAppAuthSecretKey) &&
                    cli.TRCLI_STATUS.Equals(StatusLiteral.Available));
        }
        catch (Exception ex)
        {
            exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
        }

        exceptionDispatchInfo?.Throw();

        return ret;
    }

    /// <summary>
    /// Check if if the user credentials are correct and user is active, then return user Id (case sensitive)
    /// </summary>
    /// <param name="login">User login (ex: TOTO)</param>
    /// <param name="password">User password (ex: azerty)</param>
    /// <returns>User ID if credentials are correct and user is active, null otherwise.</returns>
    public string GetUserIdFromLoginAndPassword(string login, string password)
    {
        var user = _dbContext.TRU_USERS
            .AsEnumerable().FirstOrDefault(u => u.TRU_LOGIN.Equals(login, StringComparison.InvariantCultureIgnoreCase) && u.TRU_STATUS.Equals(StatusLiteral.Available));

        return user == null ? null : (user.TRU_ALLOW_INTERNAL_AUTH
            ? IsValidPassword(user.TRU_PWD, password) ? user.TRU_USERID : null   // user/pwd DB
            : IsValidLdapCredentials(login, password) ? user.TRU_USERID : null); // user/pwd LDAP
    }

    /// <summary>
    /// Check if user can challenge LDAP
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private bool IsValidLdapCredentials(string login, string password)
    {
        bool result;

        try
        {
            var search = _configuration["LdapKit:SearchBase"];
            var host = _configuration["LdapKit:Host"];
            var port = int.Parse((_configuration["LdapKit:Port"] ?? "0"));
            var who = $@"{search}\{login}";

            using var cn = new LdapConnection();
            cn.Connect(host, port);
            cn.Bind(Native.LdapAuthMechanism.SIMPLE, who, password);

            result = true;
        }
        catch
        {
            result = false;
        }

        return result;
    }

    /// <summary>
    /// TODO: check if user can challenge FID
    /// </summary>
    /// <returns></returns>
    private bool IsValidFidCredentials()
    {
        if (_configuration.GetValue<bool>("FidKit:UseFid"))
        {
            //// Lecture des données
            //_login = User.Claims.FirstOrDefault(c => c.Type == "login").Value;
            //_nom = User.Claims.FirstOrDefault(c => c.Type == "family_name").Value;
            //_prenom = User.Claims.FirstOrDefault(c => c.Type == "first_name").Value;
            //_email = User.Claims.FirstOrDefault(c => c.Type == "Mail").Value;

            // Cinématique
            // Quand l’utilisateur souhaite accéder à une page nécessitant d’être authentifié(attribut[Authorize]) alors,
            // il est redirigé vers la FID puis revient sur le site avec le jeton d’authentification.

            return true;
        }

        return false;
    }

    /// <summary>
    /// Create a ciphered password from a given clear text password
    /// This algo will always generate different results for same password, but with same content inside!
    /// </summary>
    /// <param name="clearPassword"></param>
    /// <returns></returns>
    public string CreateCipheredPassword(string clearPassword)
    {
        try
        {
            return Encryption.Encrypt(clearPassword,
             Litterals.User.GetValueOrDefault(Litterals.UserCiph),
             Litterals.User.GetValueOrDefault(Litterals.UserAuth));
        }
        catch
        {
            // ignored
        }

        return null;
    }

    /// <summary>
    /// Decode a ciphered password to clear text, check it against clear text password
    /// </summary>
    /// <param name="cipherPassword"></param>
    /// <param name="clearPassword"></param>
    /// <returns></returns>
    public bool IsValidPassword(string cipherPassword, string clearPassword)
    {
        try
        {
            return cipherPassword is not null
              && Encryption.Decrypt(cipherPassword,
              Litterals.User.GetValueOrDefault(Litterals.UserCiph),
              Litterals.User.GetValueOrDefault(Litterals.UserAuth))
              .Equals(clearPassword, StringComparison.Ordinal);
        }
        catch
        {
            // ignored
        }

        return false;
    }

    /// <summary>
    /// Get user's claims.
    /// </summary>
    /// <param name = "userId" > User Id (ex: 1)</param>
    /// <returns>User claims if code is correct, empty otherwise.</returns>
    public IEnumerable<Claim> GetUserClaimsFromUserId(string userId)
    {
        // on supprime l'ex 'UserName' user
        //DELETE FROM TRCL_CLAIMS where TRCL_CLAIMID = 1;
        //DELETE FROM TRUCL_USERS_CLAIMS where TRCL_CLAIMID = 1;

        var claims = Enumerable.Empty<Claim>();

        switch (string.IsNullOrEmpty(userId))
        {
            // Get active claims based on user ID.
            case false:
                {
                    var user = _dbContext.TRU_USERS.FirstOrDefault(x => x.TRU_USERID.Equals(userId));

                    if (user is not null)
                    {
                        // Get claims
                        claims = from ucl in _dbContext.TRUCL_USERS_CLAIMS.Include(ucl => ucl.TRCL_CLAIM)
                                 where ucl.TRU_USERID.Equals(userId) && ucl.TRUCL_STATUS.Equals(StatusLiteral.Available)
                                 where ucl.TRCL_CLAIM.TRCL_STATUS.Equals(StatusLiteral.Available)
                                 join cl in _dbContext.TRCL_CLAIMS on ucl.TRCL_CLAIMID equals cl.TRCL_CLAIMID
                                 join cli in _dbContext.TRCLI_CLIENTAPPLICATIONS on ucl.TRCLI_CLIENTAPPLICATIONID equals cli.TRCLI_CLIENTAPPLICATIONID
                                 select new Claim(cli.TRCLI_LABEL + cl.TRCL_CLAIM_NAME, ucl.TRUCL_CLAIM_VALUE);

                        if (claims.Any())
                        {
                            // Add UserId
                            claims = claims.Append(new Claim(ClaimTypes.NameIdentifier, user.TRU_USERID));

                            // Add UserName
                            claims = claims.Append(new Claim(ClaimTypes.Name, user.TRU_FULLNAME));

                            // Add Login
                            claims = claims.Append(new Claim(ClaimsLiterals.ClaimTypes.Login, user.TRU_LOGIN));

                            // Add UserMail
                            claims = claims.Append(new Claim(ClaimTypes.Email, user.TRU_EMAIL ?? string.Empty));

                            // Add Culture (for UI)
                            claims = claims.Append(new Claim(ClaimsLiterals.ClaimTypes.CultureInfo, (user.TRLG_LNGID
                                ?? _configuration[ClaimsLiterals.ClaimTypes.CultureInfo]) ?? string.Empty));

                            // Add TimeZone (for sending emails)
                            claims = claims.Append(new Claim(ClaimsLiterals.ClaimTypes.TimeZone, (user.TRTZ_TZID
                                ?? _configuration[ClaimsLiterals.ClaimTypes.TimeZone]) ?? string.Empty));
                        }
                    }

                    break;
                }
        }

        return claims;
    }

    /// <summary>
    /// Get client applications's claims.
    /// </summary>
    /// <param name = "clientAppAuthPublicKey"> Client public auth key (ex: Krialys.Mso)</param>
    /// <returns>Application claims if code is correct, empty otherwise.</returns>
    public IEnumerable<Claim> GetClientAppClaimsFromAuthPublicKey(string clientAppAuthPublicKey)
    {
        // on supprime l'ex 'UserName' app cliente
        //DELETE FROM TRCL_CLAIMS where TRCL_CLAIMID = 4;
        //DELETE FROM TRCLICL_CLIENTAPPLICATIONS_CLAIMS where TRCL_CLAIMID = 4;
        // on supprime SF-PROTO qui n'a plus de raison d'être
        //DELETE FROM TRCLI_CLIENTAPPLICATIONS where TRCLI_CLIENTAPPLICATIONID in (2);

        // Get client application database ID based on oAuth client public auth key (ex: Krialys.Mso)
        int? appId = GetClientAppIdFromAuthPublicKey(clientAppAuthPublicKey);
        var claims = Enumerable.Empty<Claim>();

        // Get active claims based on client application ID.
        if (appId.HasValue)
        {
            claims = from clicl in _dbContext.TRCLICL_CLIENTAPPLICATIONS_CLAIMS.Include(clicl => clicl.TRCL_CLAIM)
                     where clicl.TRCLI_CLIENTAPPLICATIONID.Equals(appId) && clicl.TRCLICL_STATUS.Equals(StatusLiteral.Available)
                     where clicl.TRCL_CLAIM.TRCL_STATUS.Equals(StatusLiteral.Available)
                     join cl in _dbContext.TRCL_CLAIMS on clicl.TRCL_CLAIMID equals cl.TRCL_CLAIMID
                     select new Claim(cl.TRCL_CLAIM_NAME, clicl.TRCLICL_CLAIM_VALUE);

            if (claims.Any())
            {
                // Add ClientId
                claims = claims.Append(new Claim(ClaimTypes.NameIdentifier, appId.Value.ToString()));

                var client = _dbContext.TRCLI_CLIENTAPPLICATIONS
                    .FirstOrDefault(x => x.TRCLI_AUTH_PUBLIC.Equals(clientAppAuthPublicKey));

                // Add AppName
                if (client != null)
                    claims = claims.Append(new Claim(ClaimTypes.Name, client.TRCLI_LABEL));

                // Add Culture (for UI)
                claims = claims.Append(new Claim(ClaimsLiterals.ClaimTypes.CultureInfo, _configuration[ClaimsLiterals.ClaimTypes.CultureInfo] ?? string.Empty));

                // Add TimeZone (for sending emails)
                claims = claims.Append(new Claim(ClaimsLiterals.ClaimTypes.TimeZone, _configuration[ClaimsLiterals.ClaimTypes.TimeZone] ?? string.Empty));
            }
        }

        return claims;
    }

    /// <summary>
    /// Get client application Id
    /// </summary>
    /// <param name="clientAppAuthPublicKey">Client public auth key (ex: Krialys.Mso)</param>
    /// <returns>Client application Id</returns>
    public int? GetClientAppIdFromAuthPublicKey(string clientAppAuthPublicKey)
    {
        int? clientAppId = null;

        if (!string.IsNullOrEmpty(clientAppAuthPublicKey))
        {
            clientAppId = _dbContext.TRCLI_CLIENTAPPLICATIONS
                .AsEnumerable()
                .FirstOrDefault(cli => cli.TRCLI_AUTH_PUBLIC.Equals(clientAppAuthPublicKey)
                             && cli.TRCLI_STATUS.Equals(StatusLiteral.Available))?.TRCLI_CLIENTAPPLICATIONID; // 2021.04.16 seb : suite renommage de champ 
        }

        return clientAppId;
    }

    /// <summary>
    /// Get User AccessToken From "password" grant type
    /// </summary>
    /// <param name="login"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public string GetUserAccessToken(string login, string password)
    {
        string accessToken = string.Empty;

        // *** Check if the request is valid (if any grant type does not exist, then error) ***
        string userId = GetUserIdFromLoginAndPassword(login, DecodeFrom64(password));

        if (string.IsNullOrEmpty(userId))
        {
            return accessToken;
        }

        // Get claims related to the user if there is one, otherwise get claims related to the client application
        var claims = GetUserClaimsFromUserId(userId);

        accessToken = _credentialServices.CreateAccessToken(claims, out int _);

        return accessToken;
    }

    /// <summary>
    /// Get Application AccessToken From "client_credentials" grant type
    /// Mainly used by CPUservices to feed etlsettings.json that will provide a token to exchange with ApiUnivers
    /// </summary>
    /// <param name="clientId"></param>
    /// <returns></returns>
    public string GetAppAccessToken(string clientId)//, string publicKey)
    {
        string accessToken = string.Empty;

        // Client application Id
        var clientAppId = GetClientAppIdFromAuthPublicKey(clientId);

        if (clientAppId is null)
        {
            return accessToken;
        }

        // Check client application public and secret Id
        //if (CheckClientAppFromAuthPublicAndSecretKey(clientId, publicKey))
        {
            // Get claims related to the Application if there is one
            var claims = GetClientAppClaimsFromAuthPublicKey(clientId);

            accessToken = _credentialServices.CreateAccessToken(claims, out var _);
        }

        return accessToken;
    }

    public string DecodeFrom64(string encodedData)
    {
        byte[] encodedDataAsBytes = Convert.FromBase64String(encodedData);

        string returnValue = Encoding.UTF8.GetString(encodedDataAsBytes);

        return returnValue;
    }
}