using Krialys.Orkestra.Web.Common;
using Krialys.Orkestra.Web.Common.ApiClient;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using Syncfusion.Blazor.Data;
using System.Text.Json;
using TextCopy;

namespace Krialys.Orkestra.Web.Module.ADM.Pages.Settings;
public partial class MenuSettings
{

    #region UserMenuExt Extension

    public class UserMenuExt : UserMenu
    {
        public new HashSet<UserMenuExt> SubMenu { get; set; } = new();
        public bool IsExpanded { get; set; } = true;

        public bool HasChild => SubMenu != null && SubMenu.Count > 0;
        public bool HasPartialChildSelection()
        {
            int iChildrenCheckedCount = (from c in SubMenu where c.IsVisible select c).Count();
            return HasChild && iChildrenCheckedCount > 0 && iChildrenCheckedCount < SubMenu.Count();
        }

        public UserMenuExt(UserMenu item)
        {
            Code = item.Code;
            Title = item.Title;
            Url = item.Url;
            HeaderImageUrl = item.HeaderImageUrl;
            MenuCategory = item.MenuCategory;
            Title = item.Title;
            IsVisible = item.IsVisible;
            IsDisabled = item.IsDisabled;
            SubMenu = new HashSet<UserMenuExt>();
        }
    }

    private HashSet<UserMenuExt> ConvertToHashSet(ICollection<UserMenu> item)
    {
        HashSet<UserMenuExt> result = new();
        item.ToList().ForEach(x =>
        {
            var menu = new UserMenuExt(x);
            if (x.SubMenu.Any()) menu.SubMenu = ConvertToHashSet(x.SubMenu);
            result.Add(menu);
        });
        return result;
    }

    private IList<UserMenu> ConvertToList(HashSet<UserMenuExt> item)
    {
        IList<UserMenu> result = new List<UserMenu>();
        item.ToList().ForEach(x =>
        {
            var menu = new UserMenu()
            {
                Code = x.Code,
                Title = x.Title,
                Description = x.Description,
                MenuCategory = x.MenuCategory,

                Url = x.Url,
                HeaderImageUrl = x.HeaderImageUrl,
                SideMenuImageUrl = x.HeaderImageUrl,

                SubMenu = new List<UserMenu>(),

                IsVisible = x.IsVisible,
                IsDisabled = x.IsDisabled,
            };
            if (x.SubMenu.Any()) menu.SubMenu = ConvertToList(x.SubMenu);
            result.Add(menu);
        });
        return result;
    }

    protected void CheckedChanged(UserMenuExt item)
    {
        item.IsVisible = !item.IsVisible;
        if (item.SubMenu != null && item.SubMenu.Any())
            item.SubMenu.ToList().ForEach(x =>
            {
                if (item.SubMenu != null && item.SubMenu.Any()) CheckedChanged(x);
                x.IsVisible = item.IsVisible;
            });
    }

    #endregion

    [Inject] private IUserClient _iUserClient { get; set; }
    [Inject] private TextCopy.IClipboard _iClipboard { get; set; }

#if DEBUG
    private bool isDebug = true;
#else
    private bool isDebug;
#endif

    private CustomValidation _customValidation = default;
    private HashSet<UserMenuExt> Menus = new HashSet<UserMenuExt>();

    protected override async Task OnInitializedAsync()
    {
        var result = await ApiHelper.ExecuteCallGuardedAsync(async () => await _iUserClient.GetMenuSettingsAsync(), Snackbar, _customValidation);
        Menus = ConvertToHashSet(result);

        await base.OnInitializedAsync();
    }

    private async Task Save()
    {
        if (await ApiHelper.ExecuteCallGuardedAsync(async () => await _iUserClient.SaveMenuSettingsAsync(ConvertToList(Menus)), Snackbar, _customValidation))
            Snackbar.Add($"Menu saved", Severity.Info);
    }

    private async Task CopyToClipBoard()
    {
        await _iClipboard.SetTextAsync(JsonSerializer.Serialize<IEnumerable<UserMenu>>(ConvertToList(Menus)));
        Snackbar.Add($"Json Menu copied to Clipboard", Severity.Info);
    }
}