using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
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
