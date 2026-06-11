using RecuBackend.Api.Models;

namespace RecuBackend.Api.Repositories.Interfaces;

public interface ICampaignRepository
{
    Task<List<Campaign>> ListByOwnerAsync(
        Guid ownerId,
        string? search,
        string? setting,
        bool? isActive,
        bool? isPublic,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<Campaign?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Campaign?> GetTrackedByIdAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task<Campaign?> GetTrackedByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsForOwnerAsync(Guid id, Guid ownerId, CancellationToken ct);

    void Add(Campaign campaign);
    void Remove(Campaign campaign);
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task<List<Campaign>> ListPublicAsync(
        string? search,
        string? setting,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<List<Campaign>> ListAllAsync(CancellationToken ct);
}

