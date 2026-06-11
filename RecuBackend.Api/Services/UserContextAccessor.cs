using System.Security.Claims;
using RecuBackend.Api.Auth;

namespace RecuBackend.Api.Services;

public sealed class UserContextAccessor(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid? UserId
    {
        get
        {
            var sub = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Role =>
        httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);

    public bool IsAuthenticated => UserId.HasValue;

    public bool IsAdmin => AppRoles.IsAdmin(Role);

    public bool IsMaster => AppRoles.IsMaster(Role);

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}
