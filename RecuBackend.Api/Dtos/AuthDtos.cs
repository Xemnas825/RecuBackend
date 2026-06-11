namespace RecuBackend.Api.Dtos;

public sealed record LoginRequest(string Username, string Password, string? SessionRole);

public sealed record RegisterRequest(
    string Username,
    string Password,
    string DisplayName,
    string? SessionRole);

public sealed record LoginResponse(
    string AccessToken,
    Guid UserId,
    string Username,
    string Role,
    bool IsAdmin,
    bool IsMaster,
    DateTime ExpiresAtUtc);
