using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Models;
using RecuBackend.Api.Queries;
using RecuBackend.Api.Repositories.Interfaces;

namespace RecuBackend.Api.Repositories.Ef;

public sealed class EfCampaignRepository(AppDbContext db) : ICampaignRepository
{
    public Task<List<Campaign>> ListByOwnerAsync(
        Guid ownerId,
        string? search,
        string? setting,
        bool? isActive,
        bool? isPublic,
        string? sortBy,
        string? sortDir,
        CancellationToken ct) =>
        db.Campaigns
            .AsNoTracking()
            .Where(c => c.OwnerUserId == ownerId)
            .ApplyCampaignFilters(search, setting, isActive, isPublic)
            .ApplyCampaignSort(sortBy, sortDir)
            .ToListAsync(ct);

    public Task<Campaign?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct) =>
        db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

    public Task<Campaign?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Campaigns.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Campaign?> GetTrackedByIdAsync(Guid id, Guid ownerId, CancellationToken ct) =>
        db.Campaigns.FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

    public Task<bool> ExistsForOwnerAsync(Guid id, Guid ownerId, CancellationToken ct) =>
        db.Campaigns.AnyAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

    public void Add(Campaign campaign) => db.Campaigns.Add(campaign);
    public void Remove(Campaign campaign) => db.Campaigns.Remove(campaign);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public Task<List<Campaign>> ListPublicAsync(
        string? search,
        string? setting,
        string? sortBy,
        string? sortDir,
        CancellationToken ct) =>
        db.Campaigns
            .AsNoTracking()
            .Where(c => c.IsPublic && c.IsActive)
            .ApplyCampaignFilters(search, setting, isActive: true, isPublic: true)
            .ApplyCampaignSort(sortBy, sortDir)
            .ToListAsync(ct);

    public Task<List<Campaign>> ListAllAsync(CancellationToken ct) =>
        db.Campaigns
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);
}

