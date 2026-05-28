using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IPublicService
{
    Task<List<CampaignResponse>> ListPublicCampaignsAsync(
        string? search,
        string? setting,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<List<CharacterResponse>?> ListPublicCharactersAsync(
        Guid campaignId,
        string? name,
        string? race,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);
}

