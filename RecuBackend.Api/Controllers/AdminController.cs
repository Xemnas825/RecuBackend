using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(IAdminService admin) : ControllerBase
{
    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetAllCampaigns(CancellationToken ct)
    {
        var campaigns = await admin.ListAllCampaignsAsync(ct);
        return Ok(campaigns);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<object>>> GetUsers(CancellationToken ct)
    {
        var users = await admin.ListUsersAsync(ct);
        return Ok(users);
    }
}
