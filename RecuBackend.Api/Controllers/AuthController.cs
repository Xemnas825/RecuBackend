using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Auth;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth, IUserContext userContext) : ControllerBase
{
    [Authorize(Roles = AppRoles.Authenticated)]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        if (!userContext.IsAuthenticated || userContext.UserId is null)
            return Unauthorized(new { message = "Token JWT requerido." });

        return Ok(new
        {
            userId = userContext.UserId.Value,
            role = userContext.Role,
            isAdmin = userContext.IsAdmin,
            isMaster = userContext.IsMaster,
            roleLabel = AppRoles.DisplayName(userContext.Role)
        });
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var (response, error, status) = await auth.LoginAsync(request, ct);
        return status switch
        {
            StatusCodes.Status200OK => Ok(response),
            StatusCodes.Status401Unauthorized => Unauthorized(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var (response, error, status) = await auth.RegisterAsync(request, ct);
        return status switch
        {
            StatusCodes.Status200OK => Ok(response),
            StatusCodes.Status409Conflict => Conflict(new { message = error }),
            _ => BadRequest(new { message = error })
        };
    }
}
