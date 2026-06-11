using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AttachmentDownloadService(
    IAttachmentRepository attachments,
    IFileStorageService fileStorage) : IAttachmentDownloadService
{
    public async Task<(Stream? Stream, string? RedirectUrl, string FileName, string ContentType, bool NotFound, string? ErrorMessage)> OpenAsync(
        Guid attachmentId,
        Guid ownerId,
        CancellationToken ct)
    {
        var attachment = await attachments.GetByIdAsync(attachmentId, ownerId, ct);
        if (attachment is null) return (null, null, string.Empty, string.Empty, true, null);

        if (IsRemoteUrl(attachment.StoragePath))
            return (null, attachment.StoragePath, attachment.FileName, attachment.ContentType, false, null);

        try
        {
            var stream = await fileStorage.OpenReadAsync(attachment.StoragePath, ct);
            return (stream, null, attachment.FileName, attachment.ContentType, false, null);
        }
        catch (FileNotFoundException)
        {
            return (null, null, attachment.FileName, attachment.ContentType, true, "El archivo no existe en el almacenamiento.");
        }
    }

    private static bool IsRemoteUrl(string path) =>
        path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
}
