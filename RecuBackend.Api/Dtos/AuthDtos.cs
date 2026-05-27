namespace RecuBackend.Api.Dtos;

public sealed record LoginRequest(string Username, string Password);

public sealed record RegisterRequest(
    string Username,
    string Password,
    string DisplayName);

public sealed record LoginResponse(
    string AccessToken,
    Guid UserId,
    string Username,
    string Role,
    DateTime ExpiresAtUtc);
