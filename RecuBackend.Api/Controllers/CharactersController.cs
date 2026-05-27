using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecuBackend.Api.Data;
using RecuBackend.Api.Dtos;
using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Api.Controllers;

[Route("api/campaigns/{campaignId:guid}/characters")]
public sealed class CharactersController(AppDbContext db, IUserContext userContext) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CharacterResponse>>> GetAll(Guid campaignId, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCampaign(campaignId, ownerId, ct)) return NotFound();

        var characters = await db.Characters
            .AsNoTracking()
            .Where(ch => ch.CampaignId == campaignId && ch.OwnerUserId == ownerId)
            .OrderBy(ch => ch.Name)
            .ToListAsync(ct);

        return Ok(characters.Select(ToResponse));
    }

    [HttpGet("{id:guid}", Name = "GetCharacter")]
    public async Task<ActionResult<CharacterResponse>> GetById(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var character = await db.Characters
            .AsNoTracking()
            .FirstOrDefaultAsync(ch => ch.Id == id && ch.CampaignId == campaignId && ch.OwnerUserId == ownerId, ct);

        return character is null ? NotFound() : Ok(ToResponse(character));
    }

    [HttpPost]
    public async Task<ActionResult<CharacterResponse>> Create(
        Guid campaignId, CreateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;
        if (!await OwnsCampaign(campaignId, ownerId, ct)) return NotFound();

        var character = new Character
        {
            Id = Guid.NewGuid(),
            CampaignId = campaignId,
            OwnerUserId = ownerId,
            Name = request.Name,
            Race = request.Race,
            CharacterClass = request.CharacterClass,
            Level = request.Level,
            ArmorClass = request.ArmorClass,
            HitPoints = request.HitPoints,
            ProficiencyBonus = request.ProficiencyBonus,
            Strength = request.Strength,
            Dexterity = request.Dexterity,
            IsNpc = request.IsNpc,
            CreatedAtUtc = DateTime.UtcNow
        };

        db.Characters.Add(character);
        await db.SaveChangesAsync(ct);

        return CreatedAtRoute("GetCharacter", new { campaignId, id = character.Id }, ToResponse(character));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CharacterResponse>> Update(
        Guid campaignId, Guid id, UpdateCharacterRequest request, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var character = await db.Characters
            .FirstOrDefaultAsync(ch => ch.Id == id && ch.CampaignId == campaignId && ch.OwnerUserId == ownerId, ct);

        if (character is null) return NotFound();

        character.Name = request.Name;
        character.Race = request.Race;
        character.CharacterClass = request.CharacterClass;
        character.Level = request.Level;
        character.ArmorClass = request.ArmorClass;
        character.HitPoints = request.HitPoints;
        character.ProficiencyBonus = request.ProficiencyBonus;
        character.Strength = request.Strength;
        character.Dexterity = request.Dexterity;
        character.IsNpc = request.IsNpc;

        await db.SaveChangesAsync(ct);
        return Ok(ToResponse(character));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid campaignId, Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(userContext, out var ownerId, out var authError)) return authError;

        var character = await db.Characters
            .FirstOrDefaultAsync(ch => ch.Id == id && ch.CampaignId == campaignId && ch.OwnerUserId == ownerId, ct);

        if (character is null) return NotFound();

        db.Characters.Remove(character);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private Task<bool> OwnsCampaign(Guid campaignId, Guid ownerId, CancellationToken ct) =>
        db.Campaigns.AnyAsync(c => c.Id == campaignId && c.OwnerUserId == ownerId, ct);

    private static CharacterResponse ToResponse(Character ch) => new(
        ch.Id, ch.CampaignId, ch.Name, ch.Race, ch.CharacterClass, ch.Level, ch.ArmorClass,
        ch.HitPoints, ch.ProficiencyBonus, ch.Strength, ch.Dexterity, ch.IsNpc, ch.CreatedAtUtc);
}
