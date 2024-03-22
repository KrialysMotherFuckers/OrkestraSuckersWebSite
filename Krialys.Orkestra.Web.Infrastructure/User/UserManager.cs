using Krialys.Orkestra.Web.Common.ApiClient;

namespace Krialys.Orkestra.Web.Infrastructure.User;

public interface IUserManager
{
    ValueTask<IEnumerable<UserMenu>> GetUserMenuFromLocalStorage();
}

public class UserManager : IUserManager
{
    private readonly ILocalStorageService _localStorage;

    public UserManager(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async ValueTask<IEnumerable<UserMenu>> GetUserMenuFromLocalStorage()
        //=> await GetUserMenu();
        => await _localStorage.GetItemAsync<IList<UserMenu>>(Litterals.UserMenu);

    private async ValueTask<IEnumerable<UserMenu>> GetUserMenu()
        => await ValueTask.FromResult(await _localStorage.GetItemAsync<List<UserMenu>>(Litterals.UserMenu).ConfigureAwait(false));
}