namespace Krialys.Common.Literals;

/// <summary>
/// List the most commons claims.
/// </summary>
public static class ClaimsLiterals
{
    /// <summary>
    /// LifeTime of the access token.
    /// </summary>
    public const string TokenLifetime = "TokenLifetime";

    /// <summary>
    /// Role of the user.
    /// </summary>
    public const string Role = "Role";

    /// <summary>
    /// Role of the user in MSO application.
    /// </summary>
    public const string MSORole = "MSORole";

    /// <summary>
    /// Role of the user in ADM application.
    /// </summary>
    public const string ADMRole = "ADMRole";

    /// <summary>
    /// Role of the user in DTM (DataManager) application.
    /// </summary>
    public const string DTMRole = "DTMRole";

    /// <summary>
    /// Role of the user in DTF (DataFabrik) application. (TODO: add roles for DTF into DB)
    /// </summary>
    public const string DTFRole = "DTFRole";

    /// <summary>
    /// Role of the user in ETQ (EtiquettesManager) application.
    /// </summary>
    public const string ETQRole = "ETQRole";

    /// <summary>
    /// Add extra 'claims' for each user.
    /// Defaulted values are stored in appsettingsXXX.json (ApiUnivers)
    /// </summary>
    public static class ClaimTypes
    {
        public const string Login = "Login";

        public const string CultureInfo = "CultureInfo";

        public const string TimeZone = "TimeZone";
    }
}