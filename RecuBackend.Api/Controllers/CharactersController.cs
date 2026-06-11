using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/campaigns/{campaignId:guid}/characters")]
public sealed class CharactersController(ICharacterService characters, IUserContext userContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterResponse>>> GetAll(
        Guid campaignId,
        [FromQuery] string? name,
        [FromQuery] string? race,
        [FromQuery] string? characterClass,
        [FromQuery] bool? isNpc,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var userId, out var authError)) return authError;

        var (items, _) = await characters.ListAsync(
            campaignId, userId, userContext.IsMaster, name, race, characterClass, isNpc, sortBy, sortDir, ct);
        return items is null ? NotFound() : Ok(items);
    }

    [HttpGet("{id:guid}", Name = "GetCharacter")]
    public async Task<ActionResult<CharacterResponse>> GetById(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var userId, out var authError)) return authError;

        var character = await characters.GetByIdAsync(campaignId, id, userId, userContext.IsMaster, ct);
        return character is null ? NotFound() : Ok(character);
    }

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> Create(
        Guid campaignId, CreateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var userId, out var authError)) return authError;

        var (created, error) = await characters.CreateAsync(campaignId, userId, userContext.IsMaster, request, ct);
        if (error is not null) return BadRequest(new { message = error });
        if (created is null)
            return NotFound(new { message = "Campaña no encontrada o sin permiso para añadir personajes." });

        return CreatedAtRoute("GetCharacter", new { campaignId, id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CharacterResponse>> Update(
        Guid campaignId, Guid id, UpdateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var userId, out var authError)) return authError;

        var (updated, error) = await characters.UpdateAsync(campaignId, id, userId, userContext.IsMaster, request, ct);
        if (error is not null) return BadRequest(new { message = error });
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var userId, out var authError)) return authError;

        var (deleted, error) = await characters.DeleteAsync(campaignId, id, userId, userContext.IsMaster, ct);
        if (error is not null) return BadRequest(new { message = error });
        return deleted ? NoContent() : NotFound();
    }
}
