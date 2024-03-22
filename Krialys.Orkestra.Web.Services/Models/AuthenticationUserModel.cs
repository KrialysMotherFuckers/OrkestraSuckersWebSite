using System.ComponentModel.DataAnnotations;

namespace Krialys.Orkestra.Web.Module.Common.Models; // OK https://youtu.be/2c4p6RGtkps

public class AuthenticationUserModel
{
    [Required(ErrorMessage = "Login is required")]
    public string Login { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }
}