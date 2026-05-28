using RecuBackend.Api.Dtos;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class PublicService(
    ICampaignRepository campaigns,
    ICharacterRepository characters) : IPublicService
{
    public async Task<List<CampaignResponse>> ListPublicCampaignsAsync(
        string? search,
        string? setting,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        var items = await campaigns.ListPublicAsync(search, setting, sortBy, sortDir, ct);
        return items.Select(c => new CampaignResponse(
            c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc)).ToList();
    }

    public async Task<List<CharacterResponse>?> ListPublicCharactersAsync(
        Guid campaignId,
        string? name,
        string? race,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        var campaign = await campaigns.GetByIdAsync(campaignId, ct);
        if (campaign is null || !campaign.IsPublic || !campaign.IsActive) return null;

        var items = await characters.ListPublicByCampaignAsync(campaignId, name, race, sortBy, sortDir, ct);
        return items.Select(ch => new CharacterResponse(
            ch.Id, ch.CampaignId, ch.Name, ch.Race, ch.CharacterClass, ch.Level, ch.ArmorClass,
            ch.HitPoints, ch.ProficiencyBonus, ch.Strength, ch.Dexterity,
            ch.Constitution, ch.Intelligence, ch.Wisdom, ch.Charisma,
            ch.Initiative, ch.SpeedFeet, ch.Size,
            ch.IsNpc, ch.CreatedAtUtc)).ToList();
    }
}

