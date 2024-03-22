using Krialys.Orkestra.WebApi.Infrastructure.Common.Models;

namespace Krialys.Orkestra.WebApi.Services.Identity.Users;

public class UserListFilter : PaginationFilter
{
    public bool? IsActive { get; set; }
}