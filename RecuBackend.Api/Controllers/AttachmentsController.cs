using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/characters/{characterId:guid}/attachments")]
public sealed class AttachmentsController(
    IUserContext userContext,
    IAttachmentService attachments) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileAttachmentResponse>>> GetAll(Guid characterId, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var items = await attachments.ListAsync(characterId, ownerId, ct);
        return items is null ? NotFound() : Ok(items);
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
        var (attachment, error, notFound) = await attachments.UploadAsync(characterId, ownerId, file, ct);
        if (notFound) return NotFound();
        if (error is not null) return BadRequest(new { message = error });

        return CreatedAtRoute(
            routeName: "DownloadAttachment",
            routeValues: new { id = attachment!.Id },
            value: attachment);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid characterId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var (deleted, notFound) = await attachments.DeleteAsync(characterId, ownerId, id, ct);
        return notFound ? NotFound() : NoContent();
    }
}
