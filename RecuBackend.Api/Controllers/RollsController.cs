using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Route("api/characters/{characterId:guid}/rolls")]
public sealed class RollsController(
    AppDbContext db,
    IUserContext userContext,
    DiceRollerService diceRoller) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RollResponse>>> GetAll(Guid characterId, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCharacter(characterId, ownerId, ct)) return NotFound();

        var rolls = await db.RollLogs
            .AsNoTracking()
            .Where(r => r.CharacterId == characterId && r.OwnerUserId == ownerId)
            .OrderByDescending(r => r.RolledAtUtc)
            .ToListAsync(ct);

        return Ok(rolls.Select(ToResponse));
    }

    [HttpPost]
    public async Task<ActionResult<RollResponse>> Roll(Guid characterId, CreateRollRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCharacter(characterId, ownerId, ct)) return NotFound();

        RollResult result;
        try
        {
            result = diceRoller.Roll(request.DiceExpression, request.D20Mode);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
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

        db.RollLogs.Add(log);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetAll), new { characterId }, ToResponse(log));
    }

    private Task<bool> OwnsCharacter(Guid characterId, Guid ownerId, CancellationToken ct) =>
        db.Characters.AnyAsync(ch => ch.Id == characterId && ch.OwnerUserId == ownerId, ct);

    private static RollResponse ToResponse(RollLog r) => new(
        r.Id, r.CharacterId, r.Label, r.DiceExpression, r.D20Mode,
        r.RawResults, r.Total, r.IsCritical, r.RolledAtUtc);
}
