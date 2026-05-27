namespace RecuBackend.Api.Services;

public sealed class UserContextMiddleware(RequestDelegate next)
{
    public const string UserIdHeader = "X-User-Id";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(UserIdHeader, out var value)
            && Guid.TryParse(value.FirstOrDefault(), out var userId))
        {
            context.Items[nameof(IUserContext)] = new UserContext { UserId = userId };
        }

        await next(context);
    }
}

public sealed class UserContextAccessor(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid? UserId =>
        httpContextAccessor.HttpContext?.Items[nameof(IUserContext)] is UserContext ctx
            ? ctx.UserId
            : null;

    public bool IsAuthenticated => UserId.HasValue;
}
