namespace RecuBackend.Api.Dtos;

public sealed record UserAdminResponse(
    Guid Id,
    string Username,
    string Role,
    string DisplayName,
    bool IsActive,
    DateTime CreatedAtUtc);

public sealed record UpdateUserRoleRequest(string Role);

public sealed record UpdateCampaignStatusRequest(bool IsPublic, bool IsActive);
