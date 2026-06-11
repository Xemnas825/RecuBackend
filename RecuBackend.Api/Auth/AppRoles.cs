namespace RecuBackend.Api.Auth;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Master = "Master";
    public const string User = "User";
    public const string Guest = "Guest";

    public const string Authenticated = $"{Admin},{Master},{User}";
    public const string GameManagement = Master;

    public static readonly IReadOnlySet<string> AssignableRoles =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Admin, Master, User };

    public static bool IsAdmin(string? role) =>
        string.Equals(role, Admin, StringComparison.OrdinalIgnoreCase);

    public static bool IsMaster(string? role) =>
        string.Equals(role, Master, StringComparison.OrdinalIgnoreCase);

    public static string DisplayName(string? role) => role switch
    {
        _ when IsAdmin(role) => "Administrador",
        _ when IsMaster(role) => "Dungeon Master",
        _ when string.Equals(role, User, StringComparison.OrdinalIgnoreCase) => "Jugador",
        _ => role ?? Guest
    };
}
