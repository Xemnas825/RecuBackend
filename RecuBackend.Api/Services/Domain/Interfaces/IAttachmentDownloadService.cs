namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IAttachmentDownloadService
{
    Task<(Stream? Stream, string? RedirectUrl, string FileName, string ContentType, bool NotFound, string? ErrorMessage)> OpenAsync(
        Guid attachmentId,
        Guid ownerId,
        CancellationToken ct);
}

