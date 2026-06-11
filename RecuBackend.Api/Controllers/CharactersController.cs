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
    /// <summary>
    /// Filtros: name, race, characterClass, isNpc.
    /// Orden: sortBy=name|level|race|class, sortDir=asc|desc.
    /// </summary>
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
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var items = await characters.ListAsync(campaignId, ownerId, name, race, characterClass, isNpc, sortBy, sortDir, ct);
        return items is null ? NotFound() : Ok(items);
    }

    [HttpGet("{id:guid}", Name = "GetCharacter")]
    public async Task<ActionResult<CharacterResponse>> GetById(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var character = await characters.GetByIdAsync(campaignId, id, ownerId, ct);
        return character is null ? NotFound() : Ok(character);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> Create(
        Guid campaignId, CreateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        var created = await characters.CreateAsync(campaignId, ownerId, request, ct);
        if (created is null) return NotFound();
        return CreatedAtRoute("GetCharacter", new { campaignId, id = created.Id }, created);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CharacterResponse>> Update(
        Guid campaignId, Guid id, UpdateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var updated = await characters.UpdateAsync(campaignId, id, ownerId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var deleted = await characters.DeleteAsync(campaignId, id, ownerId, ct);
        return deleted ? NoContent() : NotFound();
    }
}
