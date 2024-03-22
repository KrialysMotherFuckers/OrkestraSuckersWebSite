using Krialys.Orkestra.Web.Module.Common.Models;

namespace Krialys.Orkestra.Web.Module.Common.Authentication; // OK https://youtu.be/2c4p6RGtkps

public interface IAuthenticationService
{
    Task<AuthenticatedUserModel> LogIn(AuthenticationUserModel userForAuthentication);

    Task LogOut();
}