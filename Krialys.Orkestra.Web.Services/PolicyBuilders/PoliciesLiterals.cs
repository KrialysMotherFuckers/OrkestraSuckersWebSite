using Krialys.Common.Enums;

namespace Krialys.Orkestra.Web.Module.Common.PolicyBuilders;

/// <summary>
/// List all policies for MSO & ADM
/// A policy is a claims-based authorization strategy.
/// They are defined in Startup.cs.
/// </summary>
public static class PoliciesLiterals
{
    #region MSO
    /// <summary>
    /// Can view logs and rapprochements MSO pages.
    /// </summary>
    public const string RapprochementsViewer = "RapprochementsViewer";

    /// <summary>
    /// Can edit logs and rapprochements MSO pages.
    /// </summary>
    public const string RapprochementsEditor = "RapprochementsEditor";

    /// <summary>
    /// Can view attendus MSO page.
    /// </summary>
    public const string AttendusViewer = "AttendusViewer";

    /// <summary>
    /// Can edit attendus MSO page.
    /// </summary>
    public const string AttendusEditor = "AttendusEditor";

    /// <summary>
    /// Can view references MSO page.
    /// </summary>
    public const string ReferencesViewer = "ReferencesViewer";

    /// <summary>
    /// Can edit references MSO page.
    /// </summary>
    public const string ReferencesEditor = "ReferencesEditor";

    /// <summary>
    /// Can view planification MSO page.
    /// </summary>
    public const string PlanifsViewer = "PlanifsViewer";

    /// <summary>
    /// Can edit planification MSO page.
    /// </summary>
    public const string PlanifsEditor = "PlanifsEditor";
    #endregion

    #region ADM
    /// <summary>
    /// Can view and edit ADM pages (only ADM).
    /// </summary>
    public const string Administrator = "Administrator";
    #endregion

    #region DTM
    /// <summary>
    /// Can view UTD pages.
    /// </summary>
    public const RolesEnums.RolesValues UTDViewer = RolesEnums.RolesValues.Creator | RolesEnums.RolesValues.AdminRefs;

    /// <summary>
    /// Can edit UTD pages.
    /// </summary>
    public const RolesEnums.RolesValues UTDEditor = RolesEnums.RolesValues.Creator;

    // <summary>
    // Can view references page.
    // </summary>
    //public const RolesEnums.RolesValues ReferencesViewer = RolesEnums.RolesValues.Creator | RolesEnums.RolesValues.AdminRefs;

    // <summary>
    // Can edit references page.
    // </summary>
    //public const RolesEnums.RolesValues ReferencesEditor = RolesEnums.RolesValues.AdminRefs;
    #endregion

    #region DTF
    public const string DTFAdm = "DTFAdm";
    public const string DTFConsul = "Viewer";
    public const string DTFDataDriven = "DataDriven";
    #endregion

    #region ETQ
    #endregion
}