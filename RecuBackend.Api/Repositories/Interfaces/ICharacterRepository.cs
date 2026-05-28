using RecuBackend.Api.Models;

namespace RecuBackend.Api.Repositories.Interfaces;

public interface ICharacterRepository
{
    Task<List<Character>> ListByCampaignAsync(
        Guid campaignId,
        Guid ownerId,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<Character?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct);
    Task<Character?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task<Character?> GetTrackedByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct);
    Task<bool> ExistsForOwnerAsync(Guid characterId, Guid ownerId, CancellationToken ct);

    void Add(Character character);
    void Remove(Character character);
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task<List<Character>> ListPublicByCampaignAsync(
        Guid campaignId,
        string? name,
        string? race,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);
}

