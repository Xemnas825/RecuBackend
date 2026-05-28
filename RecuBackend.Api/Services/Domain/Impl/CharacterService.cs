using RecuBackend.Api.Dtos;
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

        return items.Select(ToResponse).ToList();
    }

    public async Task<CharacterResponse?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(campaignId, id, ownerId, ct);
        return character is null ? null : ToResponse(character);
    }

    public async Task<CharacterResponse?> CreateAsync(Guid campaignId, Guid ownerId, CreateCharacterRequest request, CancellationToken ct)
    {
        var owns = await campaigns.ExistsForOwnerAsync(campaignId, ownerId, ct);
        if (!owns) return null;

        var character = new Character
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            OwnerUserId = ownerId,
            Name = request.Name,
            Race = request.Race,
            CharacterClass = request.CharacterClass,
            Level = request.Level,
            ArmorClass = request.ArmorClass,
            HitPoints = request.HitPoints,
            ProficiencyBonus = request.ProficiencyBonus,
            Strength = request.Strength,
            Dexterity = request.Dexterity,
            IsNpc = request.IsNpc,
            CreatedAtUtc = DateTime.UtcNow
        };

        characters.Add(character);
        await characters.SaveChangesAsync(ct);
        return ToResponse(character);
    }

    public async Task<CharacterResponse?> UpdateAsync(Guid campaignId, Guid id, Guid ownerId, UpdateCharacterRequest request, CancellationToken ct)
    {
        var character = await characters.GetTrackedByIdAsync(campaignId, id, ownerId, ct);
        if (character is null) return null;

        character.Name = request.Name;
        character.Race = request.Race;
        character.CharacterClass = request.CharacterClass;
        character.Level = request.Level;
        character.ArmorClass = request.ArmorClass;
        character.HitPoints = request.HitPoints;
        character.ProficiencyBonus = request.ProficiencyBonus;
        character.Strength = request.Strength;
        character.Dexterity = request.Dexterity;
        character.IsNpc = request.IsNpc;

        await characters.SaveChangesAsync(ct);
        return ToResponse(character);
    }

    public async Task<bool> DeleteAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct)
    {
        var character = await characters.GetByIdAsync(campaignId, id, ownerId, ct);
        if (character is null) return false;

        characters.Remove(character);
        await characters.SaveChangesAsync(ct);
        return true;
    }

    private static CharacterResponse ToResponse(Character ch) => new(
        ch.Id, ch.CampaignId, ch.Name, ch.Race, ch.CharacterClass, ch.Level, ch.ArmorClass,
        ch.HitPoints, ch.ProficiencyBonus, ch.Strength, ch.Dexterity, ch.IsNpc, ch.CreatedAtUtc);
}

