using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Route("api/characters/{characterId:guid}/attachments")]
public sealed class AttachmentsController(AppDbContext db, IUserContext userContext) : ApiControllerBase
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
    public async Task<ActionResult<FileAttachmentResponse>> Create(
        Guid characterId, CreateFileAttachmentRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCharacter(characterId, ownerId, ct)) return NotFound();

        var attachment = new FileAttachment
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            OwnerUserId = ownerId,
            FileName = request.FileName,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes,
            StoragePath = request.StoragePath,
            IsImage = request.IsImage,
            UploadedAtUtc = DateTime.UtcNow
        };

        db.FileAttachments.Add(attachment);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { characterId }, ToResponse(attachment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid characterId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var attachment = await db.FileAttachments
            .FirstOrDefaultAsync(a => a.Id == id && a.CharacterId == characterId && a.OwnerUserId == ownerId, ct);

        if (attachment is null) return NotFound();

        db.FileAttachments.Remove(attachment);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Task<bool> OwnsCharacter(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.Characters.AnyAsync(ch => ch.Id == characterId && ch.OwnerUserId == ownerId, ct);

    private static FileAttachmentResponse ToResponse(FileAttachment a) => new(
        a.Id, a.CharacterId, a.FileName, a.ContentType, a.SizeBytes, a.StoragePath, a.IsImage, a.UploadedAtUtc);
}
