using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class CampaignService(ICampaignRepository repo) : ICampaignService
{
    public async Task<List<CampaignResponse>> ListAsync(
        Guid userId,
        bool isMaster,
        string? search,
        string? setting,
        bool? isActive,
        bool? isPublic,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        var campaigns = isMaster
            ? await repo.ListByOwnerAsync(userId, search, setting, isActive, isPublic, sortBy, sortDir, ct)
            : await repo.ListJoinedByUserAsync(userId, search, setting, sortBy, sortDir, ct);

        return campaigns.Select(ToResponse).ToList();
    }

    public async Task<List<CampaignResponse>> ExplorePublicAsync(
        string? search,
        string? setting,
        string? sortBy,
        string? sortDir,
        CancellationToken ct)
    {
        var campaigns = await repo.ListPublicAsync(search, setting, sortBy, sortDir, ct);
        return campaigns.Select(ToResponse).ToList();
    }

    public async Task<CampaignResponse?> GetByIdAsync(Guid id, Guid userId, bool isMaster, CancellationToken ct)
    {
        if (isMaster)
        {
            var owned = await repo.GetByIdAsync(id, userId, ct);
            return owned is null ? null : ToResponse(owned);
        }

        var joined = await repo.GetAccessibleForPlayerAsync(id, userId, ct);
        return joined is null ? null : ToResponse(joined);
    }

    public async Task<CampaignResponse> CreateAsync(Guid ownerId, CreateCampaignRequest request, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            Name = request.Name,
            Setting = request.Setting,
            Description = request.Description,
            IsPublic = request.IsPublic,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        repo.Add(campaign);
        await repo.SaveChangesAsync(ct);
        return ToResponse(campaign);
    }

    public async Task<CampaignResponse?> UpdateAsync(Guid id, Guid ownerId, UpdateCampaignRequest request, CancellationToken ct)
    {
        var campaign = await repo.GetTrackedByIdAsync(id, ownerId, ct);
        if (campaign is null) return null;

        campaign.Name = request.Name;
        campaign.Setting = request.Setting;
        campaign.Description = request.Description;
        campaign.IsPublic = request.IsPublic;
        campaign.IsActive = request.IsActive;
        campaign.UpdatedAtUtc = DateTime.UtcNow;

        await repo.SaveChangesAsync(ct);
        return ToResponse(campaign);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid ownerId, CancellationToken ct)
    {
        var campaign = await repo.GetByIdAsync(id, ownerId, ct);
        if (campaign is null) return false;
        repo.Remove(campaign);
        await repo.SaveChangesAsync(ct);
        return true;
    }

    private static CampaignResponse ToResponse(Campaign c) => new(
        c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc);
}
