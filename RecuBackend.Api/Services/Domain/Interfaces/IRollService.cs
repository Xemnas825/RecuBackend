using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Domain.Interfaces;

public interface IRollService
{
    Task<List<RollResponse>?> ListAsync(Guid characterId, Guid ownerId, CancellationToken ct);
    Task<(RollResponse? Roll, string? ErrorMessage, bool NotFound)> RollAsync(
        Guid characterId,
        Guid ownerId,
        CreateRollRequest request,
        CancellationToken ct);
}

