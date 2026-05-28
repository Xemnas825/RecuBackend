using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/characters/{characterId:guid}/rolls")]
public sealed class RollsController(
    IRollService rolls,
    IUserContext userContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RollResponse>>> GetAll(Guid characterId, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var items = await rolls.ListAsync(characterId, ownerId, ct);
        return items is null ? NotFound() : Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult<RollResponse>> Roll(Guid characterId, CreateRollRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var (roll, error, notFound) = await rolls.RollAsync(characterId, ownerId, request, ct);
        if (notFound) return NotFound();
        if (error is not null) return BadRequest(new { message = error });
        return CreatedAtAction(nameof(GetAll), new { characterId }, roll!);
    }
}
