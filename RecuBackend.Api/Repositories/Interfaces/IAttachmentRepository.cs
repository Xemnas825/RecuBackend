using RecuBackend.Api.Models;

namespace RecuBackend.Api.Repositories.Interfaces;

public interface IAttachmentRepository
{
    Task<List<FileAttachment>> ListByCharacterAsync(Guid characterId, Guid ownerId, CancellationToken ct);
    Task<FileAttachment?> GetByIdAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task<FileAttachment?> GetByIdAsync(Guid characterId, Guid id, Guid ownerId, CancellationToken ct);
    void Add(FileAttachment attachment);
    void Remove(FileAttachment attachment);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

