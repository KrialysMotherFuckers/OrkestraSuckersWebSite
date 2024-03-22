using Krialys.Data.EF.Univers;
using Krialys.Orkestra.Common.Constants;
using Krialys.Orkestra.Common.Models.Admin;
using System.Text.Json;

namespace Krialys.Orkestra.WebApi.Services.Admin;

public interface IMenuService : IScopedService
{
    ValueTask<IEnumerable<UserMenu>> GetUserMenu(string userId);
    ValueTask<IEnumerable<UserMenu>> GetMenuSettings();
    ValueTask<bool> SaveMenuSettings(IEnumerable<UserMenu> menuList);
}

public class MenuService : ILicence, IMenuService
{
    private readonly KrialysDbContext _dbContext;

    public MenuService(KrialysDbContext dbContext)
        => _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));

    public async ValueTask<IEnumerable<UserMenu>> GetUserMenu(string userId)
    {
        var menu = _dbContext.TR_WST_WebSite_Settings.FirstOrDefault(x => x.Wst_Code.ToLower() == "WebSiteMenu".ToLower())?.Wst_Value;
        if (menu == null) { throw new ArgumentNullException("No seetings for WebSiteMenu in TR_WST_WebSite_Settings Table"); }

        var menuList = JsonSerializer.Deserialize<IEnumerable<UserMenu>>(menu);

#if DEBUG
        if (true)
#else
        if (_dbContext.TRU_USERS.Any(x => !string.IsNullOrEmpty(userId) && x.TRU_USERID == userId && x.TRU_LOGIN.ToUpper() == Litterals.KADMIN))
#endif
            menuList
                .First(x => x.MenuCategory == MenuCategoryType.LeftMenu)
                ?.SubMenu?.First(x => x.Code.Equals("Settings", StringComparison.InvariantCultureIgnoreCase))
                ?.SubMenu?.First(x => x.Code.Equals("Configuration", StringComparison.InvariantCultureIgnoreCase))
                ?.SubMenu?.Add(
                    new UserMenu
                    {
                        Code = "MenuSettings",
                        Url = "Admin_MenuSettings"
                    });

        //Fecth all nodes in order to set the translation in the Title and Description Menu
        menuList= AddTranslationInMenu(menuList);

        return await ValueTask.FromResult(menuList);
    }

    private IEnumerable<UserMenu> AddTranslationInMenu(IEnumerable<UserMenu> item)
    {
        var result = item;
        //item.ToList().ForEach(x =>
        //{
        //    var menu = new UserMenuExt(x);
        //    if (x.SubMenu.Any()) menu.SubMenu = ConvertToHashSet(x.SubMenu);
        //    result.Add(menu);
        //});
        return result;
    }

    public async ValueTask<IEnumerable<UserMenu>> GetMenuSettings()
    {
        var menu = _dbContext.TR_WST_WebSite_Settings.FirstOrDefault(x => x.Wst_Code.ToLower() == "WebSiteMenu".ToLower())?.Wst_Value;
        if (menu == null) { throw new ArgumentNullException("No seetings for WebSiteMenu in TR_WST_WebSite_Settings Table"); }

        return await ValueTask.FromResult(JsonSerializer.Deserialize<IEnumerable<UserMenu>>(menu));
    }

    public ValueTask<bool> SaveMenuSettings(IEnumerable<UserMenu> menuList)
    {
        if (menuList == null || !menuList.Any()) throw new ArgumentNullException(nameof(menuList));

        var result = false;
        var menus = new TR_WST_WebSite_Settings() { Wst_Code = "WebSiteMenu", Wst_Value = JsonSerializer.Serialize<IEnumerable<UserMenu>>(menuList) };
        try
        {
            var record = _dbContext.TR_WST_WebSite_Settings.FirstOrDefault(x => x.Wst_Code.ToLower() == "WebSiteMenu".ToLower());
            if (record != null)
            {
                record.Wst_Value = menus.Wst_Value;
                _dbContext.TR_WST_WebSite_Settings.Update(record);
            }
            else _dbContext.TR_WST_WebSite_Settings.Add(menus);

            _dbContext.SaveChanges();
            result = true;
        }
        catch
        {
            throw;
        }

        return ValueTask.FromResult(result);
    }
}
