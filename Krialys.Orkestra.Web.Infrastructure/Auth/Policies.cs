using Krialys.Common.Enums;
using Krialys.Common.Literals;
using Krialys.Orkestra.Web.Module.Common.PolicyBuilders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Krialys.Orkestra.Web.Infrastructure.Auth;

/// <summary>
/// Define applications policies.
/// A policy is a claims-based authorization strategy.
/// </summary>
public static class Policies
{
    public static IServiceCollection AddOrkestraPolicies(this IServiceCollection services, Action<AuthorizationOptions> authOptions)
    {
        // Authorization and policies (to be continued if we want to use Policies, BUT they must be as generic as possible, means few but efficient policies
        services.TryAddSingleton<IAuthorizationHandler, RequireRolesHandler>();

        // Add specific application Roles & Policies options
        services.AddAuthorizationCore(authOptions);

        return services;
    }

    /// <summary>
    /// Define authorization options delegate.
    /// Used to provide policies.
    /// </summary>
    /// <returns>Authorization options delegate.</returns>
    public static Action<AuthorizationOptions> AuthOptions(string[] rolesAppName)
    {
        return opt =>
        {
            // By default, all incoming requests will be authorized only if user is authenticated, or we can use a default policy to authorize all incoming requests
            // References: https://scottsauber.com/2020/01/20/globally-require-authenticated-users-by-default-using-fallback-policies-in-asp-net-core/#:~:text=A%20Fallback%20Policy%20means%20that,will%20use%20the%20Fallback%20Policy.
            opt.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build(); // opt.DefaultPolicy;

            if (!rolesAppName.Any())
            {
                return;
            }

            foreach (var roleAppName in rolesAppName)
            {
                switch (roleAppName)
                {
                    case ClaimsLiterals.MSORole:
                        // Default Viewer can view logs and rapprochements pages.
                        opt.AddPolicy(PoliciesLiterals.RapprochementsViewer, p =>
                            p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.Viewer, RolesEnums.RolesValues.AdminExploit));

                        // Rapprochements Editor can edit logs and rapprochements pages.
                        opt.AddPolicy(PoliciesLiterals.RapprochementsEditor, p =>
                            p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin));

                        // Attendus Viewer can view attendus page.
                        opt.AddPolicy(PoliciesLiterals.AttendusViewer, p =>
                            p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.Viewer, RolesEnums.RolesValues.AdminExploit,
                                RolesEnums.RolesValues.AdminRefs, RolesEnums.RolesValues.Creator));

                        // Attendus Editor can edit attendus page.
                        opt.AddPolicy(PoliciesLiterals.AttendusEditor, p =>
                            p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.Creator));

                        // References Viewer can view references page.
                        opt.AddPolicy(PoliciesLiterals.ReferencesViewer, p => p.RequireRoles(roleAppName,
                            RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.AdminExploit, RolesEnums.RolesValues.AdminRefs, RolesEnums.RolesValues.Creator));

                        // References Editor can edit references page.
                        opt.AddPolicy(PoliciesLiterals.ReferencesEditor, p =>
                            p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.AdminRefs));

                        // Planifications Viewer can view planification page.
                        opt.AddPolicy(PoliciesLiterals.PlanifsViewer, p => p.RequireRoles(roleAppName,
                            RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.AdminExploit, RolesEnums.RolesValues.AdminRefs, RolesEnums.RolesValues.Creator));

                        // Planifications Editor can edit planification page.
                        opt.AddPolicy(PoliciesLiterals.PlanifsEditor, p => p.RequireRoles(roleAppName,
                            RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.Creator));
                        //Console.WriteLine("[x] AddPolicy roleAppName added: " + roleAppName);
                        break;

                    case ClaimsLiterals.ADMRole:
                        // Default ADM user.
                        opt.AddPolicy(PoliciesLiterals.Administrator, p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin));
                        //Console.WriteLine("[x] AddPolicy roleAppName added: " + roleAppName);
                        break;

                    case ClaimsLiterals.DTFRole:
                        opt.AddPolicy(PoliciesLiterals.ReferencesViewer, p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.Creator, RolesEnums.RolesValues.AdminRefs));
                        opt.AddPolicy(nameof(PoliciesLiterals.ReferencesEditor), p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.AdminRefs));
                        //Console.WriteLine("[x] AddPolicy roleAppName added: " + roleAppName);


                        // for launch or consult the authorised UTDs  
                        opt.AddPolicy(PoliciesLiterals.DTFDataDriven, p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.DataDriven));

                        opt.AddPolicy(PoliciesLiterals.DTFAdm, p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.SuperAdmin, RolesEnums.RolesValues.AdminExploit));
                        opt.AddPolicy(PoliciesLiterals.DTFConsul, p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.Viewer));

                        break;

                    case ClaimsLiterals.DTMRole:
                        opt.AddPolicy(nameof(PoliciesLiterals.UTDViewer), p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.Creator, RolesEnums.RolesValues.AdminRefs));
                        opt.AddPolicy(nameof(PoliciesLiterals.UTDEditor), p => p.RequireRoles(roleAppName, PoliciesLiterals.UTDEditor));
                        opt.AddPolicy(nameof(PoliciesLiterals.ReferencesViewer), p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.Creator, RolesEnums.RolesValues.AdminRefs));
                        opt.AddPolicy(nameof(PoliciesLiterals.ReferencesEditor), p => p.RequireRoles(roleAppName, RolesEnums.RolesValues.AdminRefs));
                        //Console.WriteLine("[x] AddPolicy roleAppName added: " + roleAppName);
                        break;

                    case ClaimsLiterals.ETQRole:
                        // TODO
                        break;

                    default:
                        Console.WriteLine($@"[!] Missing: {roleAppName}");
                        break;
                }
            }
            //Console.WriteLine("[*] AddPolicy report: AuthorizationOptions added!");
        };
    }
}
