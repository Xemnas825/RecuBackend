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
        if (await db.AppUsers.AnyAsync(ct))
            return;

        var now = DateTime.UtcNow;
        db.AppUsers.AddRange(
            new AppUser
            {
                Id = AdminUserId,
                Username = "admin",
                DisplayName = "Dungeon Master",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                Role = AppRoles.Admin,
                IsActive = true,
                CreatedAtUtc = now
            },
            new AppUser
            {
                Id = PlayerUserId,
                Username = "player",
                DisplayName = "Jugador",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Player123!"),
                Role = AppRoles.User,
                IsActive = true,
                CreatedAtUtc = now
            });

        await db.SaveChangesAsync(ct);
    }
}
