using Krialys.Data.EF.Univers;

namespace Krialys.Orkestra.Web.Module.ADM.Components.Claims;

public static class ClaimTools
{
    /// <summary>
    /// Controls whether the claim is in the catalog.
    /// </summary>
    /// <param name="claimsCatalog">Claims catalog.</param>
    /// <param name="claimId">Id of the claim to control.</param>
    public static bool IsClaimInClaimsCatalog(IEnumerable<TRCCL_CATALOG_CLAIMS> claimsCatalog, int claimId)
        => claimsCatalog.Any(x => x.TRCL_CLAIMID == claimId);

    /// <summary>
    /// Convert the role claim value to a readable string.
    /// </summary>
    /// <param name="claimsCatalog">Claims catalog.</param>
    /// <param name="roleId">Id of the role claim.</param>
    /// <param name="roleValue">Value of the claim.</param>
    public static string GetRoleValuesAsText(IEnumerable<TRCCL_CATALOG_CLAIMS> claimsCatalog, int roleId, string roleValue)
    {
        // Value returned by this method.
        string roleValueAsText = "";

        // Get value as enum.
        if (Enum.TryParse(roleValue, out RolesEnums.RolesValues roleValueAsEnum))
        {
            // Get claims catalog entry for the selected claim Id.
            // Browse through claim catalog.
            foreach (var entryCatalog in claimsCatalog.Where(x => x.TRCL_CLAIMID == roleId))
            {
                // Get claim catalog value as enum.
                if (Enum.TryParse(entryCatalog.TRCCL_VALUE, out RolesEnums.RolesValues roleCatalogValue))
                {
                    // Find if the claim value matches the value of this catalog entry.
                    if (roleValueAsEnum.HasFlag(roleCatalogValue))
                    {
                        // Convert value to text.
                        // The text is from the value label in catalog.
                        if (string.IsNullOrEmpty(roleValueAsText))
                        {
                            roleValueAsText += entryCatalog.TRCCL_VALUE_LABEL;
                        }
                        else
                        {
                            roleValueAsText += $", {entryCatalog.TRCCL_VALUE_LABEL}";
                        }
                    }
                }
            }
        }

        return roleValueAsText;
    }
}
