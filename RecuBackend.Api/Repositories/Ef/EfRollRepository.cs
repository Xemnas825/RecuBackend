using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;

namespace RecuBackend.Api.Repositories.Ef;

public sealed class EfRollRepository(AppDbContext db) : IRollRepository
{
    public Task<List<RollLog>> ListByCharacterAsync(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.RollLogs
            .AsNoTracking()
            .Where(r => r.CharacterId == characterId && r.OwnerUserId == ownerId)
            .OrderByDescending(r => r.RolledAtUtc)
            .ToListAsync(ct);

    public void Add(RollLog roll) => db.RollLogs.Add(roll);

    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

