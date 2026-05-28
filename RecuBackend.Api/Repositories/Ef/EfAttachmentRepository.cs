using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;

namespace RecuBackend.Api.Repositories.Ef;

public sealed class EfAttachmentRepository(AppDbContext db) : IAttachmentRepository
{
    public Task<List<FileAttachment>> ListByCharacterAsync(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.FileAttachments
            .AsNoTracking()
            .Where(a => a.CharacterId == characterId && a.OwnerUserId == ownerId)
            .OrderByDescending(a => a.UploadedAtUtc)
            .ToListAsync(ct);

    public Task<FileAttachment?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct) =>
        db.FileAttachments.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id && a.OwnerUserId == ownerId, ct);

    public Task<FileAttachment?> GetByIdAsync(Guid characterId, Guid id, Guid ownerId, CancellationToken ct) =>
        db.FileAttachments.FirstOrDefaultAsync(a =>
            a.Id == id && a.CharacterId == characterId && a.OwnerUserId == ownerId, ct);

    public void Add(FileAttachment attachment) => db.FileAttachments.Add(attachment);
    public void Remove(FileAttachment attachment) => db.FileAttachments.Remove(attachment);
    public Task<int> SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}

