using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AdminService(ICampaignRepository campaigns, IUserRepository users) : IAdminService
{
    public async Task<List<CampaignResponse>> ListAllCampaignsAsync(CancellationToken ct)
    {
        var list = await campaigns.ListAllAsync(ct);
        return list.Select(c => new CampaignResponse(
            c.Id, c.Name, c.Setting, c.Description, c.IsPublic, c.IsActive, c.CreatedAtUtc, c.UpdatedAtUtc)).ToList();
    }

    public async Task<List<UserAdminResponse>> ListUsersAsync(CancellationToken ct)
    {
        var list = await users.ListAllAsync(ct);
        return list.Select(ToUserResponse).ToList();
    }

    public async Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteUserAsync(
        Guid targetUserId, Guid actorUserId, CancellationToken ct)
    {
        if (targetUserId == actorUserId)
            return (false, "No puedes eliminar tu propia cuenta.", StatusCodes.Status400BadRequest);

        var user = await users.FindByIdAsync(targetUserId, ct);
        if (user is null)
            return (false, "Usuario no encontrado.", StatusCodes.Status404NotFound);

        if (!user.IsActive)
            return (false, "El usuario ya está desactivado.", StatusCodes.Status400BadRequest);

        user.IsActive = false;
        await users.SaveChangesAsync(ct);
        return (true, null, StatusCodes.Status204NoContent);
    }

    public async Task<(UserAdminResponse? User, string? ErrorMessage, int StatusCode)> UpdateUserRoleAsync(
        Guid targetUserId, string role, Guid actorUserId, CancellationToken ct)
    {
        if (targetUserId == actorUserId)
            return (null, "No puedes cambiar tu propio rol.", StatusCodes.Status400BadRequest);

        var normalizedRole = role.Trim();
        if (!AppRoles.AssignableRoles.Contains(normalizedRole))
            return (null, $"Rol no válido. Usa: {AppRoles.User}, {AppRoles.Master} o {AppRoles.Admin}.", StatusCodes.Status400BadRequest);

        var user = await users.FindByIdAsync(targetUserId, ct);
        if (user is null)
            return (null, "Usuario no encontrado.", StatusCodes.Status404NotFound);

        if (!user.IsActive)
            return (null, "No se puede cambiar el rol de un usuario desactivado.", StatusCodes.Status400BadRequest);

        user.Role = normalizedRole;
        await users.SaveChangesAsync(ct);
        return (ToUserResponse(user), null, StatusCodes.Status200OK);
    }

    public async Task<(CampaignResponse? Campaign, string? ErrorMessage, int StatusCode)> UpdateCampaignStatusAsync(
        Guid campaignId, bool isPublic, bool isActive, CancellationToken ct)
    {
        var campaign = await campaigns.GetTrackedByIdAsync(campaignId, ct);
        if (campaign is null)
            return (null, "Campaña no encontrada.", StatusCodes.Status404NotFound);

        campaign.IsPublic = isPublic;
        campaign.IsActive = isActive;
        campaign.UpdatedAtUtc = DateTime.UtcNow;
        await campaigns.SaveChangesAsync(ct);

        return (new CampaignResponse(
            campaign.Id, campaign.Name, campaign.Setting, campaign.Description,
            campaign.IsPublic, campaign.IsActive, campaign.CreatedAtUtc, campaign.UpdatedAtUtc),
            null, StatusCodes.Status200OK);
    }

    private static UserAdminResponse ToUserResponse(Models.AppUser u) =>
        new(u.Id, u.Username, u.Role, u.DisplayName, u.IsActive, u.CreatedAtUtc);
}
