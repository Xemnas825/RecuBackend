using RecuBackend.Api.Models;

namespace RecuBackend.Api.Auth;

public static class SessionRoleResolver
{
    public static (string? EffectiveRole, string? ErrorMessage) ResolveForLogin(AppUser user, string? sessionRole)
    {
        if (AppRoles.IsAdmin(user.Role))
            return (AppRoles.Admin, null);

        var role = sessionRole?.Trim();
        if (string.IsNullOrWhiteSpace(role))
            return (null, "Elige si entras como Dungeon Master o como Jugador.");

        if (AppRoles.IsMaster(role))
            return (AppRoles.Master, null);

        if (string.Equals(role, AppRoles.User, StringComparison.OrdinalIgnoreCase))
            return (AppRoles.User, null);

        return (null, "Rol de sesión no válido. Usa Master o User.");
    }
}
