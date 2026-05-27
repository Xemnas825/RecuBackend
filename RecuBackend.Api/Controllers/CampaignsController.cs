using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Route("api/campaigns")]
public sealed class CampaignsController(AppDbContext db, IUserContext userContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetAll(CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var campaigns = await db.Campaigns
            .AsNoTracking()
            .Where(c => c.OwnerUserId == ownerId)
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);

        return Ok(campaigns.Select(ToResponse));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CampaignResponse>> GetById(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var campaign = await db.Campaigns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

        return campaign is null ? NotFound() : Ok(ToResponse(campaign));
    }

    [HttpPost]
    public async Task<ActionResult<CampaignResponse>> Create(CreateCampaignRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var now = DateTime.UtcNow;
        var campaign = new Campaign
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerId,
            Name = request.Name,
            Setting = request.Setting,
            Description = request.Description,
            IsPublic = request.IsPublic,
            IsActive = request.IsActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.Campaigns.Add(campaign);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = campaign.Id }, ToResponse(campaign));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CampaignResponse>> Update(Guid id, UpdateCampaignRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var campaign = await db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

        if (campaign is null) return NotFound();

        campaign.Name = request.Name;
        campaign.Setting = request.Setting;
        campaign.Description = request.Description;
        campaign.IsPublic = request.IsPublic;
        campaign.IsActive = request.IsActive;
        campaign.UpdatedAtUtc = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        return Ok(ToResponse(campaign));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var campaign = await db.Campaigns
            .FirstOrDefaultAsync(c => c.Id == id && c.OwnerUserId == ownerId, ct);

        if (campaign is null) return NotFound();

        db.Campaigns.Remove(campaign);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static CampaignResponse ToResponse(Campaign c) => new(
        c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc);
}
