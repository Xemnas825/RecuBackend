using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Models;
using RecuBackend.Api.Queries;
using RecuBackend.Api.Repositories.Interfaces;

namespace RecuBackend.Api.Repositories.Ef;

public sealed class EfCharacterRepository(AppDbContext db) : ICharacterRepository
{
    public Task<List<Character>> ListByCampaignAsync(
        Guid campaignId,
        Guid ownerId,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc,
        string? sortBy,
        string? sortDir,
        CancellationToken ct) =>
        db.Characters
            .AsNoTracking()
            .Where(ch => ch.CampaignId == campaignId && ch.OwnerUserId == ownerId)
            .ApplyCharacterFilters(name, race, characterClass, isNpc)
            .ApplyCharacterSort(sortBy, sortDir)
            .ToListAsync(ct);

    public Task<Character?> GetByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct) =>
        db.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(ch => ch.Id == id && ch.CampaignId == campaignId && ch.OwnerUserId == ownerId, ct);

    public Task<Character?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct) =>
        db.Characters.AsNoTracking().FirstOrDefaultAsync(ch => ch.Id == id && ch.OwnerUserId == ownerId, ct);

    public Task<Character?> GetTrackedByIdAsync(Guid campaignId, Guid id, Guid ownerId, CancellationToken ct) =>
        db.Characters.FirstOrDefaultAsync(ch => ch.Id == id && ch.CampaignId == campaignId && ch.OwnerUserId == ownerId, ct);

    public Task<bool> ExistsForOwnerAsync(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.Characters.AnyAsync(ch => ch.Id == characterId && ch.OwnerUserId == ownerId, ct);

    public void Add(Character character) => db.Characters.Add(character);
    public void Remove(Character character) => db.Characters.Remove(character);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    public Task<List<Character>> ListPublicByCampaignAsync(
        Guid campaignId,
        string? name,
        string? race,
        string? sortBy,
        string? sortDir,
        CancellationToken ct) =>
        db.Characters
            .AsNoTracking()
            .Where(ch => ch.CampaignId == campaignId)
            .ApplyCharacterFilters(name, race, characterClass: null, isNpc: false)
            .ApplyCharacterSort(sortBy, sortDir)
            .ToListAsync(ct);
}

