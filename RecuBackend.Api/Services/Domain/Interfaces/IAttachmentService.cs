using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IAttachmentService
{
    Task<List<FileAttachmentResponse>?> ListAsync(Guid characterId, Guid ownerId, CancellationToken ct);
    Task<(FileAttachmentResponse? Attachment, string? ErrorMessage, bool NotFound)> UploadAsync(
        Guid characterId,
        Guid ownerId,
        IFormFile file,
        CancellationToken ct);
    Task<(bool Deleted, bool NotFound)> DeleteAsync(Guid characterId, Guid ownerId, Guid attachmentId, CancellationToken ct);
}

