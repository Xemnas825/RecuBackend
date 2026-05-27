namespace RecuBackend.Api.Services;

public sealed class UserContext : IUserContext
{
    public Guid? UserId { get; init; }
    public bool IsAuthenticated => UserId.HasValue;
}
