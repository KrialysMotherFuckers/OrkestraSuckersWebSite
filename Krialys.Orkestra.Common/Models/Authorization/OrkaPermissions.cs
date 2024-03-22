using System.Collections.ObjectModel;

namespace Krialys.Orkestra.Common.Models.Authorization;

public static class OrkaAction
{
    public const string View = nameof(View);
    public const string Search = nameof(Search);
    public const string Create = nameof(Create);
    public const string Update = nameof(Update);
    public const string Delete = nameof(Delete);
    public const string Export = nameof(Export);
    public const string Generate = nameof(Generate);
    public const string Clean = nameof(Clean);
    public const string UpgradeSubscription = nameof(UpgradeSubscription);
}

public static class OrkaResource
{
    public const string Tenants = nameof(Tenants);
    public const string Dashboard = nameof(Dashboard);
    public const string Hangfire = nameof(Hangfire);
    public const string Users = nameof(Users);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
    public const string Products = nameof(Products);
    public const string Brands = nameof(Brands);
}

//public static class OrkaPermissions
//{
//    private static readonly OrkaPermission[] _all = new OrkaPermission[]
//    {
//        new("View Dashboard", OrkaAction.View, OrkaResource.Dashboard),
//        new("View Hangfire", OrkaAction.View, OrkaResource.Hangfire),
//        new("View Users", OrkaAction.View, OrkaResource.Users),
//        new("Search Users", OrkaAction.Search, OrkaResource.Users),
//        new("Create Users", OrkaAction.Create, OrkaResource.Users),
//        new("Update Users", OrkaAction.Update, OrkaResource.Users),
//        new("Delete Users", OrkaAction.Delete, OrkaResource.Users),
//        new("Export Users", OrkaAction.Export, OrkaResource.Users),
//        new("View UserRoles", OrkaAction.View, OrkaResource.UserRoles),
//        new("Update UserRoles", OrkaAction.Update, OrkaResource.UserRoles),
//        new("View Roles", OrkaAction.View, OrkaResource.Roles),
//        new("Create Roles", OrkaAction.Create, OrkaResource.Roles),
//        new("Update Roles", OrkaAction.Update, OrkaResource.Roles),
//        new("Delete Roles", OrkaAction.Delete, OrkaResource.Roles),
//        new("View RoleClaims", OrkaAction.View, OrkaResource.RoleClaims),
//        new("Update RoleClaims", OrkaAction.Update, OrkaResource.RoleClaims),
//        new("View Products", OrkaAction.View, OrkaResource.Products, IsBasic: true),
//        new("Search Products", OrkaAction.Search, OrkaResource.Products, IsBasic: true),
//        new("Create Products", OrkaAction.Create, OrkaResource.Products),
//        new("Update Products", OrkaAction.Update, OrkaResource.Products),
//        new("Delete Products", OrkaAction.Delete, OrkaResource.Products),
//        new("Export Products", OrkaAction.Export, OrkaResource.Products),
//        new("View Brands", OrkaAction.View, OrkaResource.Brands, IsBasic: true),
//        new("Search Brands", OrkaAction.Search, OrkaResource.Brands, IsBasic: true),
//        new("Create Brands", OrkaAction.Create, OrkaResource.Brands),
//        new("Update Brands", OrkaAction.Update, OrkaResource.Brands),
//        new("Delete Brands", OrkaAction.Delete, OrkaResource.Brands),
//        new("Generate Brands", OrkaAction.Generate, OrkaResource.Brands),
//        new("Clean Brands", OrkaAction.Clean, OrkaResource.Brands),
//        new("View Tenants", OrkaAction.View, OrkaResource.Tenants, IsRoot: true),
//        new("Create Tenants", OrkaAction.Create, OrkaResource.Tenants, IsRoot: true),
//        new("Update Tenants", OrkaAction.Update, OrkaResource.Tenants, IsRoot: true),
//        new("Upgrade Tenant Subscription", OrkaAction.UpgradeSubscription, OrkaResource.Tenants, IsRoot: true)
//    };

//    public static IReadOnlyList<OrkaPermission> All { get; } = new ReadOnlyCollection<OrkaPermission>(_all);
//    public static IReadOnlyList<OrkaPermission> Root { get; } = new ReadOnlyCollection<OrkaPermission>(_all.Where(p => p.IsRoot).ToArray());
//    public static IReadOnlyList<OrkaPermission> Admin { get; } = new ReadOnlyCollection<OrkaPermission>(_all.Where(p => !p.IsRoot).ToArray());
//    public static IReadOnlyList<OrkaPermission> Basic { get; } = new ReadOnlyCollection<OrkaPermission>(_all.Where(p => p.IsBasic).ToArray());
//}

public record OrkaPermission(string Description, string Action, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(Action, Resource);
    public static string NameFor(string action, string resource) => $"Permissions.{resource}.{action}";
}