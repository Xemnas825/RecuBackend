using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IAdminService
{
    Task<List<CampaignResponse>> ListAllCampaignsAsync(CancellationToken ct);
    Task<List<UserAdminResponse>> ListUsersAsync(CancellationToken ct);
    Task<(bool Success, string? ErrorMessage, int StatusCode)> DeleteUserAsync(Guid targetUserId, Guid actorUserId, CancellationToken ct);
    Task<(UserAdminResponse? User, string? ErrorMessage, int StatusCode)> UpdateUserRoleAsync(
        Guid targetUserId, string role, Guid actorUserId, CancellationToken ct);
    Task<(CampaignResponse? Campaign, string? ErrorMessage, int StatusCode)> UpdateCampaignStatusAsync(
        Guid campaignId, bool isPublic, bool isActive, CancellationToken ct);
}
