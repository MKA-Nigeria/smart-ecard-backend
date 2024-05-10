using System.Collections.ObjectModel;

namespace Shared.Authorization;
public static class AppAction
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

public static class Resource
{
    public const string Users = nameof(Users);
    public const string AppConfigurations = nameof(AppConfigurations);
    public const string CardRequest = nameof(CardRequest);
    public const string Card = nameof(Card);
    public const string UserRoles = nameof(UserRoles);
    public const string Roles = nameof(Roles);
    public const string RoleClaims = nameof(RoleClaims);
}

public static class Permissions
{
    private static readonly Permission[] _all =
    [
        new("View Users", AppAction.View, Resource.Users),
        new("Search Users", AppAction.Search, Resource.Users),
        new("Create Users", AppAction.Create, Resource.Users),
        new("Update Users", AppAction.Update, Resource.Users),
        new("Delete Users", AppAction.Delete, Resource.Users),
        new("Export Users", AppAction.Export, Resource.Users),

        new("View UserRoles", AppAction.View, Resource.UserRoles),
        new("Update UserRoles", AppAction.Update, Resource.UserRoles),

        new("View Roles", AppAction.View, Resource.Roles),
        new("Create Roles", AppAction.Create, Resource.Roles),
        new("Update Roles", AppAction.Update, Resource.Roles),
        new("Delete Roles", AppAction.Delete, Resource.Roles),
        new("View RoleClaims", AppAction.View, Resource.RoleClaims),
        new("Update RoleClaims", AppAction.Update, Resource.RoleClaims),

        new("View CardRequest", AppAction.View, Resource.CardRequest),
        new("Search CardRequest", AppAction.Search, Resource.CardRequest),
        new("Create CardRequest", AppAction.Create, Resource.CardRequest),
        new("Update CardRequest", AppAction.Update, Resource.CardRequest),

        new("View Card", AppAction.View, Resource.Card),
        new("Search Card", AppAction.Search, Resource.Card),
        new("Create Card", AppAction.Create, Resource.Card),
        new("Update Card", AppAction.Update, Resource.Card),

        new("View Configurations", AppAction.View, Resource.AppConfigurations),
        new("Search Configurations", AppAction.Search, Resource.AppConfigurations),
        new("Create Configurations", AppAction.Create, Resource.AppConfigurations),
        new("Update Configurations", AppAction.Update, Resource.AppConfigurations),
    ];

    public static IReadOnlyList<Permission> All { get; } = new ReadOnlyCollection<Permission>(_all);
    public static IReadOnlyList<Permission> Root { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.IsRoot).ToArray());
    public static IReadOnlyList<Permission> Admin { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => !p.IsRoot).ToArray());
    public static IReadOnlyList<Permission> Basic { get; } = new ReadOnlyCollection<Permission>(_all.Where(p => p.IsBasic).ToArray());
}

public record Permission(string Description, string AppAction, string Resource, bool IsBasic = false, bool IsRoot = false)
{
    public string Name => NameFor(AppAction, Resource);
    public static string NameFor(string AppAction, string resource) => $"Permissions.{resource}.{AppAction}";
}
