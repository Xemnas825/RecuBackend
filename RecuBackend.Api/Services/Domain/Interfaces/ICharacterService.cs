using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface ICharacterService
{
    Task<List<CharacterResponse>?> ListAsync(
        Guid campaignId,
        Guid ownerId,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<CharacterResponse?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct);

    Task<CharacterResponse?> CreateAsync(Guid campaignId, Guid ownerId, CreateCharacterRequest request, CancellationToken ct);

    Task<CharacterResponse?> UpdateAsync(Guid campaignId, Guid id, Guid ownerId, UpdateCharacterRequest request, CancellationToken ct);

    Task<bool> DeleteAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct);
}

