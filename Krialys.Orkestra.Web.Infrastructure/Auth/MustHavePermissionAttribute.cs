using Krialys.Orkestra.Common.Models.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Krialys.Orkestra.Web.Infrastructure.Auth;

public class MustHavePermissionAttribute : AuthorizeAttribute
{
    public MustHavePermissionAttribute(string action, string resource) =>
        Policy = OrkaPermission.NameFor(action, resource);
}