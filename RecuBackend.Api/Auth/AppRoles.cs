namespace RecuBackend.Api.Auth;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Guest = "Guest";

    public const string Authenticated = $"{Admin},{User}";
}
