namespace Krialys.Orkestra.Common.Constants;

public static class Litterals
{
    // Portail
    public const string Portail = "Portail_";

    // KADMIN as super user
    public const string KADMIN = "KADMIN";

    // OAuth
    public const string Authorization = "Authorization";
    public const string AuthorizeUser = "authorizeUser";
    public const string PasswordCredentials = "password";
    public const string BearerTokenAuthorizationHeader = "__BearerTokenAuthorizationHeader__";

    // Global menu
    public const string UserMenu = "Ref.UserMenu";

    // CRUD for Tracked entities service
    public const string Insert = "Insert";
    public const string Update = "Update";
    public const string InsertOrUpdate = "InsertOrUpdate";
    public const string Patch = "Patch";
    public const string Delete = "Delete";

    // Dedicated to Serilog.ILogger for Tracked entities service
    public const string LogError = "Error";
    public const string LogWarning = "Warning";
    public const string ApiCallException = "$_ApiCallException_$";

    // Dedicated to ETQ
    public const string FiltersEtq = "FiltersEtq";

    // ERROR CODES
    public const int NoDataRow = -1;

    // API v1
    public const string Version1 = "v1";

    /// <summary>
    /// API CATALOG (specific not versioned here)
    /// </summary>
    public const string CatalogRootPath = "api/catalog";

    public const string CommonRootPath = $"api/common/{Version1}";
    public const string LogsRootPath = $"api/logs/{Version1}";
    public const string MsoRootPath = $"api/mso/{Version1}";
    public const string UniversRootPath = $"api/univers/{Version1}";
    public const string FileStorageRootPath = $"api/filestorage/{Version1}";
    public const string RefManagerRootPath = $"api/refManager/{Version1}";
    public const string EtqRootPath = $"api/etq/{Version1}";

    public const string OAuthRootPath = $"{CommonRootPath}/oauth";
    public const string TokenBearer = "Bearer";
    public const string UnsupportedGrantType = "unsupported_grant_type";
    public const string ApplicationJson = "application/json";

    public const string ProxyUrl = "ProxyUrl";
    public const string ApiUniversProxyClient = "ApiUniversProxyClient";
    public const string PortailWwwRootClient = "WwwRootClient";
    public const int MaxPollyRetryCount = 5;

    public const string ApplicationNamespace = "ApplicationNamespace";
    public const string ApplicationClientName = "ApplicationClientName";
    public const string ApplicationClientSessionId = "ApplicationClientSessionId";

    // Local storage
    public const string AuthToken = "auth";
    public static string RefreshToken = "refreshToken";
    public static string ImageUri = "userImageURL";
    public static string Permissions = "permissions";

    // JS methods
    public const string JsSetObserver = "setObserver";
    public const string JsDispose = "dispose";
    public const string JsGetElementPosition = "getElementPosition";
    public const string JsGetBrowserHeight = "getBrowserHeight";
    public const string JsGetElementOffsetTop = "getElementOffsetTop";
    public const string JsDownloadFile = "downloadFile";
    public const string JsSendClickToRefreshDatagrid = "sendClickToRefreshDatagrid";
    public const string JsInvokeCsharpCallback = "invokeCsharpCallback";

    // Datagrid constant parameters
    public const int RowHeight = 37;

    // DataAdaptor custom parameters
    public const string ConvertToUTtc = "ConvertToUTtc";
    public const string OdataQueryParameters = "OdataQueryParameters";

    public static TimeSpan AbsoluteExpiration = TimeSpan.FromMinutes(20);

    public const int AuthUnAuthorized = -401;
    public const int NotFound = -404;

    public const string FakeId = "0xDEADBEEF";

    public const string ApplicationDTF = "DTF";
    public const string ApplicationCpuClient = "CpuClient";

    // Connexion client application scenario (cf. https://tools.ietf.org/html/rfc6749)
    public const string ScenarioAuthCode = "authorization_code";

    public const string ScenarioPassword = "password";
    public const string ScenarioCredentials = "client_credentials";

    // AEAD section (cf. https://en.wikipedia.org/wiki/Authenticated_encryption)
    public const string UserCiph = "User-Ciph-Login-Challenge";
    public const string UserAuth = "User-Auth-Login-Challenge";

    /// <summary>
    /// User management (see CreateCipheredPassword from OAuthServices if you need to create any)
    /// WARNING: you will have to renew password when changing CrypKeyUser/AuthKeyUser
    /// </summary>
    public static readonly Dictionary<string, byte[]> User = new()
    {
        // AEAD for User/Password login challenge
        [UserCiph] = "12-94-C1-73-85-77-EC-2E-36-42-A7"u8.ToArray(), //Private Key
        [UserAuth] = "84-CE-D7-C6-D0-48-D9-F0-53-38-4F"u8.ToArray(), //Public Key
    };

    //public const string ProdCiph = "Prod-Ciph-License-Challenge";
    //public const string ProdAuth = "Prod-Auth-License-Challenge";

    ///// <summary>
    ///// License management (see CreateCipheredPassword from OAuthServices as example)
    ///// WARNING: you will have to renew license when changing CrypKeyUser/AuthKeyUser
    ///// </summary>
    //public static readonly Dictionary<string, byte[]> Product = new()
    //{
    //    // TODO add AEAD for Product license: idea: these keys may be changed when upgrading to new products and/or future capacities
    //    [ProdCiph] = Encoding.UTF8.GetBytes("20-21-C1-73-DE-AF-EC-2E-36-36-15"),
    //    [ProdAuth] = Encoding.UTF8.GetBytes("20-25-D7-C6-B0-D0-D9-F0-53-BE-EF"),
    //};
}