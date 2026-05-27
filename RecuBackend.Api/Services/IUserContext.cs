namespace RecuBackend.Api.Services;

public interface IUserContext
{
    Guid? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
