using RecuBackend.Api.Models;

namespace RecuBackend.Api.Repositories.Interfaces;

public interface IRollRepository
{
    Task<List<RollLog>> ListByCharacterAsync(Guid characterId, Guid ownerId, CancellationToken ct);
    void Add(RollLog roll);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

