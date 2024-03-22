namespace Krialys.Common.Literals;

/// <summary>
/// List the roles available in authentication.
/// </summary>
public static class RolesLiterals
{
    /// <summary>
    /// The super administrator is the most advanced user.
    /// It has all accesses.
    /// </summary>
    public const string SuperAdmin = "Super-Admin";

    /// <summary>
    /// This user can only read application data.
    /// </summary>
    public const string Viewer = "Visualisateur";

    /// <summary>
    /// Creator, can modify the application data.
    /// </summary>
    public const string Creator = "Créateur";

    /// <summary>
    /// Operations administrator, can read application data and administration data.
    /// </summary>
    public const string AdminExploit = "Admin-Exploit";

    /// <summary>
    /// Repository administrator, can modify repository/references data.
    /// </summary>
    public const string AdminRefs = "Admin-Référentiels";

    /// <summary>
    /// Determined by authent table according to data.
    /// </summary>
    public const string DataDriven = "DataDriven";
}