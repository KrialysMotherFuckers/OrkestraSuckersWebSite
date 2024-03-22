using Krialys.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace Krialys.Orkestra.Web.Module.Common.PolicyBuilders;

public static class RequireRolesPolicyBuilder
{
    /// <summary>
    /// Create the RequireRoles authorization policy.
    /// A policy is the entry point of authorization requirements.
    /// </summary>
    /// <param name="builder">Builder to extend.</param>
    /// <param name="roleClaimName">Name of the claim designing the role.</param>
    /// <param name="allowedRolesValues">Allowed roles values.</param>
    /// <returns></returns>
    public static AuthorizationPolicyBuilder RequireRoles(this AuthorizationPolicyBuilder builder,
        string roleClaimName,
        params RolesEnums.RolesValues[] allowedRolesValues)
    {
        // Extend the authorization policy builder with custom requirements.
        return builder.AddRequirements(new RequireRolesRequirement(roleClaimName, allowedRolesValues));
    }
}

/// <summary>
/// Authorization requirement.
/// Provides the intrinsic data of the authorization requirement.
/// These data will be used by the handler.
/// </summary>
public class RequireRolesRequirement : IAuthorizationRequirement
{
    public string RoleClaimName { get; set; }

    public RolesEnums.RolesValues[] AllowedRolesValues { get; set; }

    public RequireRolesRequirement(string roleClaimName, params RolesEnums.RolesValues[] allowedRolesValues)
    {
        RoleClaimName = roleClaimName;
        AllowedRolesValues = allowedRolesValues;
    }
}

/// <summary>
/// Authorization handler.
/// Defines how to handle the RequireRoles requirement.
/// </summary>
public class RequireRolesHandler : AuthorizationHandler<RequireRolesRequirement>
{
    /// <summary>
    /// Makes a decision if authorization is allowed based on a specific requirement.
    /// Here, the user role value should be one of the allowed value.
    /// See https://docs.microsoft.com/en-us/dotnet/api/system.enum.hasflag?view=net-6.0
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <param name="requirement">The requirement to evaluate.</param>
    /// <returns>Task.</returns>
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequireRolesRequirement requirement)
    {
        switch (context.User?.Identity)
        {
            case { IsAuthenticated: true }:
                {
                    var roleClaimNames = context.User.FindFirst(requirement.RoleClaimName)?.Value;

                    if (!string.IsNullOrEmpty(roleClaimNames))
                    {
                        // Deserialize if claim value is a table, roleClaimNames is an array if it starts with '[', else default to roleClaimNames
                        var roleClaimValues = roleClaimNames.StartsWith("[", StringComparison.Ordinal)
                            ? JsonSerializer.Deserialize<string[]>(roleClaimNames)
                            : new[] { roleClaimNames };

                        if (roleClaimValues != null)
                            Parallel.ForEach(roleClaimValues, roleClaimValue =>
                            {
                                // Try to parse claim to get the role value as enum.
                                if (!Enum.TryParse(roleClaimValue, out RolesEnums.RolesValues userRoleValue))
                                {
                                    return;
                                }

                                // Browse through all allowed values for roles.
                                if (requirement.AllowedRolesValues.Any(allowedRoleValue => userRoleValue.HasFlag(allowedRoleValue)))
                                {
                                    context.Succeed(requirement);
                                }
                            });
                    }
                    break;
                }
        }

        return Task.CompletedTask;
    }
}