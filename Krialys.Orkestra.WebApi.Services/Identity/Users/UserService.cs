using Krialys.Data.EF.Univers;
using Krialys.Orkestra.WebApi.Infrastructure.Common.Models;
using Krialys.Orkestra.WebApi.Services.Admin;
using Krialys.Orkestra.WebApi.Services.Auth;
using Krialys.Orkestra.WebApi.Services.Identity.Users.Password;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace Krialys.Orkestra.WebApi.Services.Identity.Users;

internal class UserService : ILicence, IUserService
{
    private readonly KrialysDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public UserService(KrialysDbContext dbContext, IConfiguration configuration, IAuthentificationServices credentialServices)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    #region User Management

    public Task<bool> HasPermissionAsync(string userId, string permission, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task InvalidatePermissionCacheAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> ResetPasswordAsync(ResetPasswordRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<PaginationResponse<UserDetailsDto>> SearchAsync(UserListFilter filter, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ToggleStatusAsync(ToggleUserStatusRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(UpdateUserRequest request, string userId)
    {
        throw new NotImplementedException();
    }

    #endregion

    public Task<string> AssignRolesAsync(string userId, UserRolesRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task ChangePasswordAsync(ChangePasswordRequest request, string userId)
    {
        throw new NotImplementedException();
    }

    public Task<string> ConfirmEmailAsync(string userId, string code, string tenant, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> ConfirmPhoneNumberAsync(string userId, string code)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreateAsync(CreateUserRequest request, string origin)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsWithEmailAsync(string email, string exceptId = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsWithNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsWithPhoneNumberAsync(string phoneNumber, string exceptId = null)
    {
        throw new NotImplementedException();
    }

    public Task<string> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
    {
        throw new NotImplementedException();
    }

    public Task<UserDetailsDto> GetAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetCountAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<UserDetailsDto>> GetListAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<string> GetOrCreateFromPrincipalAsync(ClaimsPrincipal principal)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetPermissionsAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<UserRoleDto>> GetRolesAsync(string userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
