using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Controllers;

/// <summary>
/// Recursos visibles sin autenticación (campañas marcadas como públicas).
/// </summary>
[ApiController]
[Route("api/public")]
public sealed class PublicController(AppDbContext db) : ControllerBase
{
    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetPublicCampaigns(CancellationToken ct)
    {
        var items = await db.Campaigns
            .AsNoTracking()
            .Where(c => c.IsPublic && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CampaignResponse(
                c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc))
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpGet("campaigns/{campaignId:guid}/characters")]
    public async Task<ActionResult<IEnumerable<CharacterResponse>>> GetPublicCharacters(
        Guid campaignId, CancellationToken ct)
    {
        var isPublic = await db.Campaigns
            .AsNoTracking()
            .AnyAsync(c => c.Id == campaignId && c.IsPublic && c.IsActive, ct);

        if (!isPublic) return NotFound();

        var items = await db.Characters
            .AsNoTracking()
            .Where(ch => ch.CampaignId == campaignId && !ch.IsNpc)
            .OrderBy(ch => ch.Name)
            .Select(ch => new CharacterResponse(
                ch.Id, ch.CampaignId, ch.Name, ch.Race, ch.CharacterClass, ch.Level, ch.ArmorClass,
                ch.HitPoints, ch.ProficiencyBonus, ch.Strength, ch.Dexterity, ch.IsNpc, ch.CreatedAtUtc))
            .ToListAsync(ct);

        return Ok(items);
    }
}
