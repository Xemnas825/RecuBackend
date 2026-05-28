using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface ICampaignService
{
    Task<List<CampaignResponse>> ListAsync(
        Guid ownerId,
        string? search,
        string? setting,
        bool? isActive,
        bool? isPublic,
        string? sortBy,
        string? sortDir,
        CancellationToken ct);

    Task<CampaignResponse?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct);

    Task<CampaignResponse> CreateAsync(Guid ownerId, CreateCampaignRequest request, CancellationToken ct);

    Task<CampaignResponse?> UpdateAsync(Guid id, Guid ownerId, UpdateCampaignRequest request, CancellationToken ct);

    Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken ct);
}

