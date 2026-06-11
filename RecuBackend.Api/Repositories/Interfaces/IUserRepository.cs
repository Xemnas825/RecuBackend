using RecuBackend.Api.Models;

namespace RecuBackend.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<AppUser?> FindActiveByUsernameAsync(string username, CancellationToken ct);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
    void Add(AppUser user);
    Task<int> SaveChangesAsync(CancellationToken ct);

    Task<List<AppUser>> ListAllAsync(CancellationToken ct);
    Task<AppUser?> FindByIdAsync(Guid id, CancellationToken ct);
}

