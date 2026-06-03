using RecuBackend.Api.Dtos;
using RecuBackend.Api.Dnd;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class CharacterService(ICampaignRepository campaigns, ICharacterRepository characters) : ICharacterService
{
    public async Task<List<CharacterResponse>?> ListAsync(
        Guid campaignId,
        Guid ownerId,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        var owns = await campaigns.ExistsForOwnerAsync(campaignId, ownerId, ct);
        if (!owns) return null;

        var items = await characters.ListByCampaignAsync(
            campaignId, ownerId, name, race, characterClass, isNpc, sortBy, sortDir, ct);

        return items.Select(CharacterDtoMapping.ToResponse).ToList();
    }

    public async Task<CharacterResponse?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(campaignId, id, ownerId, ct);
        return character is null ? null : CharacterDtoMapping.ToResponse(character);
    }

    public async Task<CharacterResponse?> CreateAsync(Guid campaignId, Guid ownerId, CreateCharacterRequest request, CancellationToken ct)
    {
        var owns = await campaigns.ExistsForOwnerAsync(campaignId, ownerId, ct);
        if (!owns) return null;

        var pb = request.ProficiencyBonus > 0
            ? request.ProficiencyBonus
            : DndRules.ProficiencyBonusFromLevel(request.Level);

        var character = new Character
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            OwnerUserId = ownerId,
            ProficiencyBonus = pb,
            CreatedAtUtc = DateTime.UtcNow
        };

        CharacterDtoMapping.Apply(character, request with { ProficiencyBonus = pb });
        characters.Add(character);
        await characters.SaveChangesAsync(ct);
        return CharacterDtoMapping.ToResponse(character);
    }

    public async Task<CharacterResponse?> UpdateAsync(Guid campaignId, Guid id, Guid ownerId, UpdateCharacterRequest request, CancellationToken ct)
    {
        var character = await characters.GetTrackedByIdAsync(campaignId, id, ownerId, ct);
        if (character is null) return null;

        var pb = request.ProficiencyBonus > 0
            ? request.ProficiencyBonus
            : DndRules.ProficiencyBonusFromLevel(request.Level);

        CharacterDtoMapping.Apply(character, request with { ProficiencyBonus = pb });
        await characters.SaveChangesAsync(ct);
        return CharacterDtoMapping.ToResponse(character);
    }

    public async Task<bool> DeleteAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(campaignId, id, ownerId, ct);
        if (character is null) return false;

        characters.Remove(character);
        await characters.SaveChangesAsync(ct);
        return true;
    }
}
