using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected bool TryGetUserId(IUserContext userContext, out Guid userId, out ActionResult error)
    {
        userId = default;
        error = null!;

        if (!userContext.IsAuthenticated || userContext.UserId is null)
        {
            error = Unauthorized(new { message = "Token JWT requerido. Usa POST /api/auth/login." });
            return false;
        }

        userId = userContext.UserId.Value;
        return true;
    }
}
