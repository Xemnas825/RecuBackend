using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class AuthService(IUserRepository users, TokenService tokenService) : IAuthService
{
    public async Task<(LoginResponse? Response, string? ErrorMessage, int StatusCode)> LoginAsync(LoginRequest request, CancellationToken ct)
    {
        var user = await users.FindActiveByUsernameAsync(request.Username, ct);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return (null, "Usuario o contraseña incorrectos.", StatusCodes.Status401Unauthorized);

        var (token, expires) = tokenService.CreateToken(user);
        return (new LoginResponse(token, user.Id, user.Username, user.Role, expires), null, StatusCodes.Status200OK);
    }

    public async Task<(LoginResponse? Response, string? ErrorMessage, int StatusCode)> RegisterAsync(RegisterRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var displayName = request.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 60)
            return (null, "Username debe tener entre 3 y 60 caracteres.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 || request.Password.Length > 100)
            return (null, "Password debe tener entre 6 y 100 caracteres.", StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 2 || displayName.Length > 120)
            return (null, "DisplayName debe tener entre 2 y 120 caracteres.", StatusCodes.Status400BadRequest);

        var exists = await users.UsernameExistsAsync(username, ct);
        if (exists)
            return (null, "Ese username ya existe.", StatusCodes.Status409Conflict);

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = AppRoles.User,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        users.Add(user);
        await users.SaveChangesAsync(ct);

        var (token, expires) = tokenService.CreateToken(user);
        return (new LoginResponse(token, user.Id, user.Username, user.Role, expires), null, StatusCodes.Status200OK);
    }
}

