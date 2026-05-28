using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IAdminService
{
    Task<List<CampaignResponse>> ListAllCampaignsAsync(CancellationToken ct);
    Task<List<object>> ListUsersAsync(CancellationToken ct);
}

