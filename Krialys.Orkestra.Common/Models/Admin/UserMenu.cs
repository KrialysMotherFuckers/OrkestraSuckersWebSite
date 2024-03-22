using System.Text.Json.Serialization;

namespace Krialys.Orkestra.Common.Models.Admin;

#region UserMenu

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MenuCategoryType
{
    Basic = 0,
    Management = 1,
    Admin = 2,
    Technical = 3,
    LeftMenu,
    RightMenu,
    ProfileMenu
}

public class UserMenu
{
    public string Code { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public MenuCategoryType MenuCategory { get; set; }

    public string Url { get; set; }
    public string HeaderImageUrl { get; set; } = "icons/unknown.svg";
    public string SideMenuImageUrl { get; set; } = "icons/unknown.svg";

    public HashSet<UserMenu> SubMenu { get; set; } = new();

    public bool IsVisible { get; set; } = true;
    public bool IsDisabled { get; set; }
}

#endregion
