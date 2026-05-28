using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;

namespace RecuBackend.Api.Repositories.Ef;

public sealed class EfUserRepository(AppDbContext db) : IUserRepository
{
    public Task<AppUser?> FindActiveByUsernameAsync(string username, CancellationToken ct) =>
        db.AppUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username && u.IsActive, ct);

    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct) =>
        db.AppUsers.AnyAsync(u => u.Username == username, ct);

    public void Add(AppUser user) => db.AppUsers.Add(user);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public Task<List<AppUser>> ListAllAsync(CancellationToken ct) =>
        db.AppUsers.AsNoTracking().OrderBy(u => u.Username).ToListAsync(ct);
}

