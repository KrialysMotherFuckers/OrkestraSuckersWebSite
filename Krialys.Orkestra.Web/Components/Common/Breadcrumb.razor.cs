using Krialys.Orkestra.Web.Common;
using Krialys.Orkestra.Web.Common.ApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Krialys.Orkestra.Web.Components.Common;

public partial class Breadcrumb : IDisposable
{
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IUserClient IUserClient { get; set; }

    private string CurrentPage { get; set; } = "";

    private ICollection<UserMenu> _menus;
    private record MenuItem(string currentPage = null, string picturePath = null);

    private bool _isUserConnected = true;
    private string _url = "";
    private string _urlActiveItem = "";

    private CustomValidation _customValidation = default;

    protected override async Task OnInitializedAsync()
    {
        _menus = await ApiHelper.ExecuteCallGuardedAsync(async () =>
            await IUserClient.GetUserMenuAsync(), Snackbar, _customValidation);

        NavigationManager.LocationChanged += LocationChanged;

        _url = NavigationManager.BaseUri;
        _urlActiveItem = NavigationManager.Uri;

        LocationManagement();
    }

    private void LocationChanged(object sender, LocationChangedEventArgs e) => LocationManagement();

    private void LocationManagement()
    {
        CurrentPage = NavigationManager.Uri.Replace(NavigationManager.BaseUri, "");

        if (string.IsNullOrEmpty(CurrentPage))
        {
            CurrentPage = "Dashboard";
        }
        else
        {
            var menuItem = GetMenuTile(_menus, CurrentPage, new MenuItem());
            CurrentPage = menuItem.currentPage;
        }
        StateHasChanged();
    }

    void IDisposable.Dispose()
    {
        NavigationManager.LocationChanged -= LocationChanged;
    }

    private MenuItem GetMenuTile(IEnumerable<UserMenu> menus, string url, MenuItem menuItem)
    {
        if (menuItem != new MenuItem()) return menuItem;
        foreach (var menu in menus)
            if (menu.Url != null && menu.Url == url)
                menuItem = new MenuItem(menu.Title, menu.SideMenuImageUrl);
            else if (menu.SubMenu.Any())
                menuItem = GetMenuTile(menu.SubMenu, url, menuItem);

        return menuItem;
    }
}