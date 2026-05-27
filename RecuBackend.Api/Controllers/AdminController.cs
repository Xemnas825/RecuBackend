using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetAllCampaigns(CancellationToken ct)
    {
        var campaigns = await db.Campaigns
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAtUtc)
            .ToListAsync(ct);

        return Ok(campaigns.Select(c => new CampaignResponse(
            c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc)));
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers(CancellationToken ct)
    {
        var users = await db.AppUsers
            .AsNoTracking()
            .OrderBy(u => u.Username)
            .Select(u => new { u.Id, u.Username, u.Role, u.DisplayName, u.IsActive, u.CreatedAtUtc })
            .ToListAsync(ct);

        return Ok(users);
    }
}
