using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AttachmentService(
    ICharacterRepository characters,
    IAttachmentRepository attachments,
    IFileStorageService fileStorage) : IAttachmentService
{
    public async Task<List<FileAttachmentResponse>?> ListAsync(Guid characterId, Guid ownerId, CancellationToken ct)
    {
        var owns = await characters.ExistsForOwnerAsync(characterId, ownerId, ct);
        if (!owns) return null;

        var items = await attachments.ListByCharacterAsync(characterId, ownerId, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<(FileAttachmentResponse? Attachment, string? ErrorMessage, bool NotFound)> UploadAsync(
        Guid characterId,
        Guid ownerId,
        IFormFile file,
        CancellationToken ct)
    {
        var owns = await characters.ExistsForOwnerAsync(characterId, ownerId, ct);
        if (!owns) return (null, null, true);

        if (file.Length == 0)
            return (null, "Debes enviar un archivo en el campo 'file'.", false);

        var fileName = Path.GetFileName(file.FileName);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        try
        {
            fileStorage.ValidateUpload(fileName, contentType, file.Length);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message, false);
        }

        var attachmentId = Guid.NewGuid();
        var ext = Path.GetExtension(fileName);
        var relativePath = $"{ownerId}/{characterId}/{attachmentId}{ext}";

        await using (var stream = file.OpenReadStream())
        {
            await fileStorage.SaveAsync(stream, relativePath, ct);
        }

        var attachment = new FileAttachment
        {
            Id = attachmentId,
            CharacterId = characterId,
            OwnerUserId = ownerId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = file.Length,
            StoragePath = relativePath,
            IsImage = LocalFileStorageService.IsImageContentType(contentType),
            UploadedAtUtc = DateTime.UtcNow
        };

        attachments.Add(attachment);
        await attachments.SaveChangesAsync(ct);

        return (ToResponse(attachment), null, false);
    }

    public async Task<(bool Deleted, bool NotFound)> DeleteAsync(
        Guid characterId,
        Guid ownerId,
        Guid attachmentId,
        CancellationToken ct)
    {
        var attachment = await attachments.GetByIdAsync(characterId, attachmentId, ownerId, ct);
        if (attachment is null) return (false, true);

        await fileStorage.DeleteAsync(attachment.StoragePath, ct);
        attachments.Remove(attachment);
        await attachments.SaveChangesAsync(ct);
        return (true, false);
    }

    private static FileAttachmentResponse ToResponse(FileAttachment a) => new(
        a.Id, a.CharacterId, a.FileName, a.ContentType, a.SizeBytes, a.IsImage, a.UploadedAtUtc);
}

