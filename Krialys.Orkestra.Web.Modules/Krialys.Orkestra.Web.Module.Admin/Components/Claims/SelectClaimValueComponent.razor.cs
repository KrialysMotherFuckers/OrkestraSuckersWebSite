using Krialys.Common.Literals;
using Krialys.Data.EF.Univers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.DropDowns;

namespace Krialys.Orkestra.Web.Module.ADM.Components.Claims;

public partial class SelectClaimValueComponent
{
    #region Parameters
    /// <summary>
    /// Value of the claim.
    /// </summary>
    [Parameter] public string ClaimValue { get; set; }

    /// <summary>
    /// EventCallback used to pass the claim value to the parent component.
    /// </summary>
    [Parameter] public EventCallback<string> ClaimValueChanged { get; set; }
    #endregion

    ///// <summary>
    ///// Claim catalog data.
    ///// </summary>
    private IEnumerable<TRCCL_CATALOG_CLAIMS> _catalogClaims = Enumerable.Empty<TRCCL_CATALOG_CLAIMS>();

    #region Blazor life cycle
    /// <summary>
    /// Method called by the framework when the component is initialized.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        // Get claim catalog
        // expanded with claims,
        // where claim name is "rôle".
        string expand = "TRCL_CLAIM";
        string filter = $"(TRCL_CLAIM/TRCL_CLAIM_NAME eq '{ClaimsLiterals.Role}')";
        _catalogClaims = await ProxyCore.GetEnumerableAsync<TRCCL_CATALOG_CLAIMS>($"?$expand={expand}&$filter={filter}");

        // Order by claims ranking
        _catalogClaims = _catalogClaims.OrderBy(e => Convert.ToInt32(e.TRCCL_VALUE));
    }

    /// <summary>
    /// Method called by the framework when parent component rerenders
    /// and parameters have changed.
    /// </summary>
    protected override void OnParametersSet()
    {
        // List of the selected catalog entries.
        // It will be completed using the claim value parameter.
        List<TRCCL_CATALOG_CLAIMS> selectedCatalogEntries = new();

        // Get claim value (from parent componant)
        if (Enum.TryParse<RolesEnums.RolesValues>(ClaimValue, out var claimValue))
        {
            // Browse through catalog.
            foreach (var catalogClaimEntry in _catalogClaims)
            {
                // Get catalog claim value as enum.
                if (Enum.TryParse<RolesEnums.RolesValues>(catalogClaimEntry.TRCCL_VALUE,
                        out var catalogClaimValue))
                {
                    // Verify if catalog claim value is in the claim value.
                    if ((claimValue & catalogClaimValue) == catalogClaimValue)
                    {
                        // Add catalog entry to claim value list.
                        selectedCatalogEntries.Add(catalogClaimEntry);
                    }
                }
            }
        }

        // Update SfMultiSelect selected values.
        MultiselectValues = selectedCatalogEntries.ToArray();
    }
    #endregion

    #region SfMultiSelect Claim Value
    /// <summary>
    /// Reference to the multiselect object. (MUST be PUBLIC to avoid crash)
    /// </summary>
    public SfMultiSelect<TRCCL_CATALOG_CLAIMS[], TRCCL_CATALOG_CLAIMS> _multiselectRef;

    /// <summary>
    /// Array of the selected values. (MUST be PUBLIC to avoid crash)
    /// </summary>
    public TRCCL_CATALOG_CLAIMS[] MultiselectValues { get; set; } = Array.Empty<TRCCL_CATALOG_CLAIMS>();

    /// <summary>
    /// Action called when selection is cleared.
    /// </summary>
    /// <param name="args">Information about the mouse event.</param>
    private async Task OnClaimValueCleared(MouseEventArgs args)
    {
        // Update claim value.
        ClaimValue = ((int)RolesEnums.RolesValues.None).ToString();
        // Pass the new value to the parent component.
        await ClaimValueChanged.InvokeAsync(ClaimValue);
    }

    /// <summary>
    /// Action called when a new value is selected.
    /// </summary>
    /// <param name="args">Object selected.</param>
    private async Task OnClaimValueChange(MultiSelectChangeEventArgs<TRCCL_CATALOG_CLAIMS[]> args)
    {
        // Enum of the selected roles values.
        var rolesValues = RolesEnums.RolesValues.None;

        if (args.Value is not null)
        {
            // Browse through selected values.
            foreach (var value in args.Value)
            {
                // Try to parse claim to get the role value as enum.
                if (Enum.TryParse<RolesEnums.RolesValues>(value.TRCCL_VALUE, out var selectedRoleValue))
                {
                    // Add selected value.
                    rolesValues |= selectedRoleValue;
                }
            }
        }

        // Update claim value.
        ClaimValue = ((int)rolesValues).ToString();

        // Pass the new value to the parent component.
        await ClaimValueChanged.InvokeAsync(ClaimValue);
    }
    #endregion
}
