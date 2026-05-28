using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Repositories.Interfaces;
using RecuBackend.Api.Services.Domain.Interfaces;

namespace RecuBackend.Api.Services.Domain.Impl;

public sealed class RollService(
    ICharacterRepository characters,
    IRollRepository rolls,
    DiceRollerService diceRoller) : IRollService
{
    public async Task<List<RollResponse>?> ListAsync(Guid characterId, Guid ownerId, CancellationToken ct)
    {
        var owns = await characters.ExistsForOwnerAsync(characterId, ownerId, ct);
        if (!owns) return null;

        var items = await rolls.ListByCharacterAsync(characterId, ownerId, ct);
        return items.Select(ToResponse).ToList();
    }

    public async Task<(RollResponse? Roll, string? ErrorMessage, bool NotFound)> RollAsync(
        Guid characterId,
        Guid ownerId,
        CreateRollRequest request,
        CancellationToken ct)
    {
        var owns = await characters.ExistsForOwnerAsync(characterId, ownerId, ct);
        if (!owns) return (null, null, true);

        RollResult result;
        try
        {
            result = diceRoller.Roll(request.DiceExpression, request.D20Mode);
        }
        catch (ArgumentException ex)
        {
            return (null, ex.Message, false);
        }

        var log = new RollLog
        {
            Id = Guid.NewGuid(),
            CharacterId = characterId,
            OwnerUserId = ownerId,
            Label = request.Label,
            DiceExpression = result.Expression,
            D20Mode = request.D20Mode,
            RawResults = string.Join(",", result.RawRolls),
            Total = result.Total,
            IsCritical = result.IsCritical,
            RolledAtUtc = DateTime.UtcNow
        };

        rolls.Add(log);
        await rolls.SaveChangesAsync(ct);

        return (ToResponse(log), null, false);
    }

    private static RollResponse ToResponse(RollLog r) => new(
        r.Id, r.CharacterId, r.Label, r.DiceExpression, r.D20Mode,
        r.RawResults, r.Total, r.IsCritical, r.RolledAtUtc);
}

