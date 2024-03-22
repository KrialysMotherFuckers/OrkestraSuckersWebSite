namespace Krialys.Orkestra.WebApi.Services.Identity.Users;

public class ToggleUserStatusRequest
{
    public bool ActivateUser { get; set; }
    public string UserId { get; set; }
}
