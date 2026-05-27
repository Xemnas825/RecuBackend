using RecuBackend.Api.Auth;

namespace RecuBackend.Api.Models;

public sealed class AppUser
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = AppRoles.User;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
