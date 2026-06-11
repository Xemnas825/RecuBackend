using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/attachments")]
public sealed class AttachmentsDownloadController(
    IUserContext userContext,
    IAttachmentDownloadService downloads) : ApiControllerBase
{
    [HttpGet("{id:guid}", Name = "DownloadAttachment")]
    public async Task<IActionResult> Download(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var (stream, redirectUrl, fileName, contentType, notFound, error) = await downloads.OpenAsync(id, ownerId, ct);
        if (notFound)
            return error is null ? NotFound() : NotFound(new { message = error });

        if (!string.IsNullOrWhiteSpace(redirectUrl))
            return Redirect(redirectUrl);

        return File(stream!, contentType, fileName);
    }
}
