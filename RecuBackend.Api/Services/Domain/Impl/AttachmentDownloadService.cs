using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AttachmentDownloadService(
    IAttachmentRepository attachments,
    IFileStorageService fileStorage) : IAttachmentDownloadService
{
    public async Task<(Stream? Stream, string FileName, string ContentType, bool NotFound, string? ErrorMessage)> OpenAsync(
        Guid attachmentId,
        Guid ownerId,
        CancellationToken ct)
    {
        var attachment = await attachments.GetByIdAsync(attachmentId, ownerId, ct);
        if (attachment is null) return (null, string.Empty, string.Empty, true, null);

        try
        {
            var stream = await fileStorage.OpenReadAsync(attachment.StoragePath, ct);
            return (stream, attachment.FileName, attachment.ContentType, false, null);
        }
        catch (FileNotFoundException)
        {
            return (null, attachment.FileName, attachment.ContentType, true, "El archivo no existe en el almacenamiento.");
        }
    }
}

