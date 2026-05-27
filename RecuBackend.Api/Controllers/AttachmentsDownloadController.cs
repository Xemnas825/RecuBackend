using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Data;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/attachments")]
public sealed class AttachmentsDownloadController(
    AppDbContext db,
    IUserContext userContext,
    IFileStorageService fileStorage) : ApiControllerBase
{
    [HttpGet("{id:guid}", Name = "DownloadAttachment")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var attachment = await db.FileAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id && a.OwnerUserId == ownerId, ct);

        if (attachment is null) return NotFound();

        Stream stream;
        try
        {
            stream = await fileStorage.OpenReadAsync(attachment.StoragePath, ct);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "El archivo no existe en el almacenamiento." });
        }

        return File(stream, attachment.ContentType, attachment.FileName);
    }
}
