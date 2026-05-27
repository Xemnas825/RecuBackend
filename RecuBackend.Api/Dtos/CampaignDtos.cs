namespace RecuBackend.Api.Dtos;

public sealed record CampaignResponse(
    Guid Id,
    string Name,
    string Setting,
    string Description,
    bool IsPublic,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record CreateCampaignRequest(
    string Name,
    string Setting,
    string Description,
    bool IsPublic,
    bool IsActive = true);

public sealed record UpdateCampaignRequest(
    string Name,
    string Setting,
    string Description,
    bool IsPublic,
    bool IsActive);
