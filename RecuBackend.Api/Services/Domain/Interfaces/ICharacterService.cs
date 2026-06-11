using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface ICharacterService
{
    Task<(List<CharacterResponse>? Items, string? ErrorMessage)> ListAsync(
        Guid campaignId,
        Guid userId,
        bool isMaster,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<CharacterResponse?> GetByIdAsync(Guid campaignId, Guid id, Guid userId, bool isMaster, CancellationToken ct);

    Task<(CharacterResponse? Character, string? ErrorMessage)> CreateAsync(
        Guid campaignId, Guid userId, bool isMaster, CreateCharacterRequest request, CancellationToken ct);

    Task<(CharacterResponse? Character, string? ErrorMessage)> UpdateAsync(
        Guid campaignId, Guid id, Guid userId, bool isMaster, UpdateCharacterRequest request, CancellationToken ct);

    Task<(bool Deleted, string? ErrorMessage)> DeleteAsync(
        Guid campaignId, Guid id, Guid userId, bool isMaster, CancellationToken ct);
}
