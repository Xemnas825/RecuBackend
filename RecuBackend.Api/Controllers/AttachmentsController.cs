using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/characters/{characterId:guid}/attachments")]
public sealed class AttachmentsController(
    AppDbContext db,
    IUserContext userContext,
    IFileStorageService fileStorage) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileAttachmentResponse>>> GetAll(Guid characterId, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCharacter(characterId, ownerId, ct)) return NotFound();

        var attachments = await db.FileAttachments
            .AsNoTracking()
            .Where(a => a.CharacterId == characterId && a.OwnerUserId == ownerId)
            .OrderByDescending(a => a.UploadedAtUtc)
            .ToListAsync(ct);

        return Ok(attachments.Select(ToResponse));
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<FileAttachmentResponse>> Upload(
        Guid characterId,
        IFormFile file,
        CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCharacter(characterId, ownerId, ct)) return NotFound();

        if (file.Length == 0)
            return BadRequest(new { message = "Debes enviar un archivo en el campo 'file'." });

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
            return BadRequest(new { message = ex.Message });
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

        db.FileAttachments.Add(attachment);
        await db.SaveChangesAsync(ct);

        return CreatedAtRoute(
            routeName: "DownloadAttachment",
            routeValues: new { id = attachment.Id },
            value: ToResponse(attachment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid characterId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var attachment = await db.FileAttachments
            .FirstOrDefaultAsync(a => a.Id == id && a.CharacterId == characterId && a.OwnerUserId == ownerId, ct);

        if (attachment is null) return NotFound();

        await fileStorage.DeleteAsync(attachment.StoragePath, ct);
        db.FileAttachments.Remove(attachment);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Task<bool> OwnsCharacter(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.Characters.AnyAsync(ch => ch.Id == characterId && ch.OwnerUserId == ownerId, ct);

    private static FileAttachmentResponse ToResponse(FileAttachment a) => new(
        a.Id, a.CharacterId, a.FileName, a.ContentType, a.SizeBytes, a.IsImage, a.UploadedAtUtc);
}
