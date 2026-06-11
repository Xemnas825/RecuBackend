using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[ApiController]
[Authorize(Roles = AppRoles.Admin)]
[Route("api/admin")]
public sealed class AdminController(IAdminService admin, IUserContext userContext) : ApiControllerBase
{
    [HttpGet("campaigns")]
    public async Task<ActionResult<IEnumerable<CampaignResponse>>> GetAllCampaigns(CancellationToken ct)
    {
        var campaigns = await admin.ListAllCampaignsAsync(ct);
        return Ok(campaigns);
    }

    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserAdminResponse>>> GetUsers(CancellationToken ct)
    {
        var users = await admin.ListUsersAsync(ct);
        return Ok(users);
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var actorId, out var authError)) return authError;

        var (success, error, status) = await admin.DeleteUserAsync(id, actorId, ct);
        return status switch
        {
            StatusCodes.Status204NoContent => NoContent(),
            StatusCodes.Status404NotFound => NotFound(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<ActionResult<UserAdminResponse>> UpdateUserRole(
        Guid id, UpdateUserRoleRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var actorId, out var authError)) return authError;

        var (user, error, status) = await admin.UpdateUserRoleAsync(id, request.Role, actorId, ct);
        return status switch
        {
            StatusCodes.Status200OK => Ok(user),
            StatusCodes.Status404NotFound => NotFound(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }

    [HttpPatch("campaigns/{id:guid}/status")]
    public async Task<ActionResult<CampaignResponse>> UpdateCampaignStatus(
        Guid id, UpdateCampaignStatusRequest request, CancellationToken ct)
    {
        var (campaign, error, status) = await admin.UpdateCampaignStatusAsync(id, request.IsPublic, request.IsActive, ct);
        return status switch
        {
            StatusCodes.Status200OK => Ok(campaign),
            StatusCodes.Status404NotFound => NotFound(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }
}
