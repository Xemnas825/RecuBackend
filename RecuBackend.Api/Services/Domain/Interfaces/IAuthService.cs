using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IAuthService
{
    Task<(LoginResponse? Response, string? ErrorMessage, int StatusCode)> LoginAsync(LoginRequest request, CancellationToken ct);
    Task<(LoginResponse? Response, string? ErrorMessage, int StatusCode)> RegisterAsync(RegisterRequest request, CancellationToken ct);
}

