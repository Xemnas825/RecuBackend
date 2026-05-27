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
            error = Unauthorized(new
            {
                message = $"Envía la cabecera {UserContextMiddleware.UserIdHeader} con un GUID (temporal hasta JWT)."
            });
            return false;
        }

        userId = userContext.UserId.Value;
        return true;
    }
}
