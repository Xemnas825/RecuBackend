using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

/// <summary>
/// Recursos visibles sin autenticación (campañas marcadas como públicas).
/// </summary>
[ApiController]
[Route("api/public")]
public sealed class PublicController(IPublicService publicService) : ControllerBase
{
    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetPublicCampaigns(
        [FromQuery] string? search,
        [FromQuery] string? setting,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken ct)
    {
        var items = await publicService.ListPublicCampaignsAsync(search, setting, sortBy, sortDir, ct);
        return Ok(items);
    }

    [HttpGet("campaigns/{campaignId:guid}/characters")]
    public async Task<ActionResult<IEnumerable<CharacterResponse>>> GetPublicCharacters(
        Guid campaignId,
        [FromQuery] string? name,
        [FromQuery] string? race,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken ct)
    {
        var items = await publicService.ListPublicCharactersAsync(campaignId, name, race, sortBy, sortDir, ct);
        return items is null ? NotFound() : Ok(items);
    }
}
