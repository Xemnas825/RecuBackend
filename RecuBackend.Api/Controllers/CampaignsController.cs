using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[Authorize(Roles = AppRoles.Authenticated)]
[Route("api/campaigns")]
public sealed class CampaignsController(ICampaignService campaigns, IUserContext userContext) : ApiControllerBase
{
    /// <summary>
    /// Filtros: search (name/setting/description), setting, isActive, isPublic.
    /// Orden: sortBy=name|setting|createdAt|updatedAt, sortDir=asc|desc.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? setting,
        [FromQuery] bool? isActive,
        [FromQuery] bool? isPublic,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var items = await campaigns.ListAsync(ownerId, search, setting, isActive, isPublic, sortBy, sortDir, ct);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignResponse>> GetById(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var campaign = await campaigns.GetByIdAsync(id, ownerId, ct);
        return campaign is null ? NotFound() : Ok(campaign);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpPost]
    public async Task<ActionResult<CampaignResponse>> Create(CreateCampaignRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var created = await campaigns.CreateAsync(ownerId, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CampaignResponse>> Update(Guid id, UpdateCampaignRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var updated = await campaigns.UpdateAsync(id, ownerId, request, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = AppRoles.GameManagement)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var deleted = await campaigns.DeleteAsync(id, ownerId, ct);
        return deleted ? NoContent() : NotFound();
    }
}
