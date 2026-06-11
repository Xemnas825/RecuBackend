using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Models;

namespace RecuBackend.Api.Data;

public static class DbSeeder
{
    public static readonly Guid AdminUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PlayerUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        await EnsureDemoUserAsync(db, AdminUserId, "admin", "Administrador", "Admin123!", AppRoles.Admin, now, ct);
        await EnsureDemoUserAsync(db, PlayerUserId, "player", "Jugador demo", "Player123!", AppRoles.User, now, ct);

        await db.SaveChangesAsync(ct);
    }

    private static async Task EnsureDemoUserAsync(
        AppDbContext db,
        Guid id,
        string username,
        string displayName,
        string password,
        string role,
        DateTime now,
        CancellationToken ct)
    {
        var user = await db.AppUsers.FirstOrDefaultAsync(u => u.Id == id || u.Username == username, ct);
        if (user is null)
        {
            db.AppUsers.Add(new AppUser
            {
                Id = id,
                Username = username,
                DisplayName = displayName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                IsActive = true,
                CreatedAtUtc = now
            });
            return;
        }

        user.Username = username;
        user.DisplayName = displayName;
        user.Role = role;
        user.IsActive = true;
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
    }
}
