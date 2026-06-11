using RecuBackend.Api.Dtos;
using RecuBackend.Api.Dnd;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class CharacterService(ICampaignRepository campaigns, ICharacterRepository characters) : ICharacterService
{
    public async Task<(List<CharacterResponse>? Items, string? ErrorMessage)> ListAsync(
        Guid campaignId,
        Guid userId,
        bool isMaster,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        if (isMaster)
        {
            if (!await campaigns.ExistsForOwnerAsync(campaignId, userId, ct))
                return (null, null);

            var all = await characters.ListAllInOwnedCampaignAsync(
                campaignId, userId, name, race, characterClass, isNpc, sortBy, sortDir, ct);
            return (all.Select(CharacterDtoMapping.ToResponse).ToList(), null);
        }

        if (await campaigns.GetAccessibleForPlayerAsync(campaignId, userId, ct) is null)
            return (null, null);

        var mine = await characters.ListByCampaignAsync(
            campaignId, userId, name, race, characterClass, isNpc, sortBy, sortDir, ct);
        return (mine.Select(CharacterDtoMapping.ToResponse).ToList(), null);
    }

    public async Task<CharacterResponse?> GetByIdAsync(Guid campaignId, Guid id, Guid userId, bool isMaster, CancellationToken ct)
    {
        if (isMaster)
        {
            var dmChar = await characters.GetByIdInOwnedCampaignAsync(campaignId, id, userId, ct);
            return dmChar is null ? null : CharacterDtoMapping.ToResponse(dmChar);
        }

        var playerChar = await characters.GetByIdAsync(campaignId, id, userId, ct);
        return playerChar is null ? null : CharacterDtoMapping.ToResponse(playerChar);
    }

    public async Task<(CharacterResponse? Character, string? ErrorMessage)> CreateAsync(
        Guid campaignId, Guid userId, bool isMaster, CreateCharacterRequest request, CancellationToken ct)
    {
        if (isMaster)
        {
            if (!await campaigns.ExistsForOwnerAsync(campaignId, userId, ct))
                return (null, null);

            return (await CreateCharacterAsync(campaignId, userId, request, ct), null);
        }

        if (!await campaigns.IsPublicActiveAsync(campaignId, ct))
            return (null, "Solo puedes crear personajes en campañas públicas y activas.");

        if (request.IsNpc)
            return (null, "Los jugadores no pueden crear NPCs.");

        return (await CreateCharacterAsync(campaignId, userId, request with { IsNpc = false }, ct), null);
    }

    public async Task<(CharacterResponse? Character, string? ErrorMessage)> UpdateAsync(
        Guid campaignId, Guid id, Guid userId, bool isMaster, UpdateCharacterRequest request, CancellationToken ct)
    {
        if (isMaster)
        {
            var dmChar = await characters.GetTrackedByIdInOwnedCampaignAsync(campaignId, id, userId, ct);
            if (dmChar is null) return (null, null);
            return (await ApplyUpdateAsync(dmChar, request, ct), null);
        }

        var playerChar = await characters.GetTrackedByIdAsync(campaignId, id, userId, ct);
        if (playerChar is null) return (null, null);

        if (request.IsNpc)
            return (null, "Los jugadores no pueden convertir personajes en NPCs.");

        return (await ApplyUpdateAsync(playerChar, request with { IsNpc = false }, ct), null);
    }

    public async Task<(bool Deleted, string? ErrorMessage)> DeleteAsync(
        Guid campaignId, Guid id, Guid userId, bool isMaster, CancellationToken ct)
    {
        if (isMaster)
        {
            var dmChar = await characters.GetByIdInOwnedCampaignAsync(campaignId, id, userId, ct);
            if (dmChar is null) return (false, null);
            characters.Remove(dmChar);
            await characters.SaveChangesAsync(ct);
            return (true, null);
        }

        var playerChar = await characters.GetByIdAsync(campaignId, id, userId, ct);
        if (playerChar is null) return (false, null);

        characters.Remove(playerChar);
        await characters.SaveChangesAsync(ct);
        return (true, null);
    }

    private async Task<CharacterResponse> CreateCharacterAsync(
        Guid campaignId, Guid ownerId, CreateCharacterRequest request, CancellationToken ct)
    {
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

    private async Task<CharacterResponse> ApplyUpdateAsync(Character character, UpdateCharacterRequest request, CancellationToken ct)
    {
        var pb = request.ProficiencyBonus > 0
            ? request.ProficiencyBonus
            : DndRules.ProficiencyBonusFromLevel(request.Level);

        CharacterDtoMapping.Apply(character, request with { ProficiencyBonus = pb });
        await characters.SaveChangesAsync(ct);
        return CharacterDtoMapping.ToResponse(character);
    }
}
