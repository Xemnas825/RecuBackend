using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(AppDbContext db, TokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var user = await db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive, ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var (token, expires) = tokenService.CreateToken(user);
        return Ok(new LoginResponse(token, user.Id, user.Username, user.Role, expires));
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var username = request.Username.Trim();
        var displayName = request.DisplayName.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Length < 3 || username.Length > 60)
            return BadRequest(new { message = "Username debe tener entre 3 y 60 caracteres." });

        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6 || request.Password.Length > 100)
            return BadRequest(new { message = "Password debe tener entre 6 y 100 caracteres." });

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length < 2 || displayName.Length > 120)
            return BadRequest(new { message = "DisplayName debe tener entre 2 y 120 caracteres." });

        var exists = await db.AppUsers.AnyAsync(u => u.Username == username, ct);
        if (exists) return Conflict(new { message = "Ese username ya existe." });

        var now = DateTime.UtcNow;
        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            Username = username,
            DisplayName = displayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = AppRoles.User,
            IsActive = true,
            CreatedAtUtc = now
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(ct);

        var (token, expires) = tokenService.CreateToken(user);
        return Ok(new LoginResponse(token, user.Id, user.Username, user.Role, expires));
    }
}
