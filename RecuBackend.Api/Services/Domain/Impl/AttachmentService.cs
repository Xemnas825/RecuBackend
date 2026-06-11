using RecuBackend.Api.Cloudinary;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;
using RecuBackend.Api.Utils;
using Microsoft.Extensions.Options;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AttachmentService(
    ICharacterRepository characters,
    IAttachmentRepository attachments,
    ICloudinaryUploadService cloudinary,
    IFileStorageService fileStorage,
    IOptions<FileStorageSettings> fileStorageSettings) : IAttachmentService
{
    private readonly FileStorageSettings _settings = fileStorageSettings.Value;

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
            FileValidationHelper.ValidateUpload(_settings, fileName, contentType, file.Length);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message, false);
        }
        catch (InvalidOperationException ex)
        {
            return (null, ex.Message, false);
        }

        var attachmentId = Guid.NewGuid();
        var publicId = $"recubackend/attachments/{attachmentId}";
        var isImage = FileValidationHelper.IsImageContentType(contentType);

        CloudinaryUploadResult upload;
        try
        {
            upload = isImage
                ? await cloudinary.UploadImageAsync(file, publicId, ct)
                : await cloudinary.UploadPdfAsync(file, publicId, ct);
        }
        catch (InvalidOperationException ex)
        {
            return (null, ex.Message, false);
        }

        var attachment = new FileAttachment
        {
            Id = attachmentId,
            CharacterId = characterId,
            OwnerUserId = ownerId,
            FileName = fileName,
            ContentType = contentType,
            SizeBytes = file.Length,
            StoragePath = upload.SecureUrl,
            PublicId = upload.PublicId,
            ResourceType = upload.ResourceType,
            IsImage = isImage,
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

        if (!string.IsNullOrWhiteSpace(attachment.PublicId))
        {
            await cloudinary.DeleteAsync(attachment.PublicId, attachment.ResourceType ?? "image", ct);
        }
        else if (!IsRemoteUrl(attachment.StoragePath))
        {
            await fileStorage.DeleteAsync(attachment.StoragePath, ct);
        }

        attachments.Remove(attachment);
        await attachments.SaveChangesAsync(ct);
        return (true, false);
    }

    private static FileAttachmentResponse ToResponse(FileAttachment a) => new(
        a.Id,
        a.CharacterId,
        a.FileName,
        a.ContentType,
        a.SizeBytes,
        a.IsImage,
        IsRemoteUrl(a.StoragePath) ? a.StoragePath : null,
        a.UploadedAtUtc);

    private static bool IsRemoteUrl(string path) =>
        path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("http://", StringComparison.OrdinalIgnoreCase);
}
