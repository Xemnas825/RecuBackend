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

    public async Task<List<object>> ListUsersAsync(CancellationToken ct)
    {
        var list = await users.ListAllAsync(ct);
        return list
            .Select(u => (object)new { u.Id, u.Username, u.Role, u.DisplayName, u.IsActive, u.CreatedAtUtc })
            .ToList();
    }
}

