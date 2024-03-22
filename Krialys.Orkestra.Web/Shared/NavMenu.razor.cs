using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Web.Common;
using Krialys.Orkestra.Web.Common.ApiClient;
using Krialys.Orkestra.Web.Infrastructure.User;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Globalization;
using System.Reflection;

namespace Krialys.Orkestra.Web.Shared;

public partial class NavMenu
{
    [Parameter] public EventCallback<string> MenuOnChanged { get; set; }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IUserClient _iUserClient { get; set; }
    [Inject] private IUserManager UserManager { get; set; } = default!;

    private CustomValidation _customValidation = default;
    private IDictionary<string, MudNavGroup> _subMenuItems = new Dictionary<string, MudNavGroup>();
    private IEnumerable<UserMenu> _userMenus = Enumerable.Empty<UserMenu>();
    private bool isDebug;

    private System.Version version = typeof(Program).Assembly.GetName().Version;
    public string AssemblyCopyright
    {
        get
        {
            object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (attributes.Length == 0) return "";
            return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
        }
    }
    public DateTime BuildDate => GetBuildDate(Assembly.GetExecutingAssembly());

    protected override async Task OnInitializedAsync()
    {
#if DEBUG
        isDebug = true;
#endif

        var res = await ApiHelper.ExecuteCallGuardedAsync(
                                    async () => await _iUserClient.GetUserMenuAsync(),
                                    Snackbar,
                                    _customValidation);

        _userMenus = res?.FirstOrDefault(x => x.MenuCategory == MenuCategoryType.LeftMenu)?.SubMenu
            ?? Enumerable.Empty<UserMenu>();
    }

    private void ToggleExpanded(bool expanded, string itemName)
    {
        if (expanded)
            _subMenuItems
                .Where(entry => entry.Key != itemName)
                .ToList()
                .ForEach(entry => entry.Value.Expanded = false);
    }

    private async Task OnMenuClick(UserMenu parentMenu = null, UserMenu menu = null)
    {
        var urlHeaderImage = "icons/HomePage/logo_orkestra.svg";
        if (parentMenu != null) urlHeaderImage = parentMenu.HeaderImageUrl;

        await MenuOnChanged.InvokeAsync(urlHeaderImage);
        NavigationManager.NavigateTo(menu == null ? (parentMenu?.Url ?? "home") : (menu?.Url ?? "home"));
    }

    private static DateTime GetBuildDate(Assembly assembly)
    {
        const string BuildVersionMetadataPrefix = "+releaseDate";

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute?.InformationalVersion != null)
        {
            var value = attribute.InformationalVersion;
            var index = value.IndexOf(BuildVersionMetadataPrefix);
            if (index > 0)
            {
                value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                if (DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }
            }
        }

        return default;
    }
}