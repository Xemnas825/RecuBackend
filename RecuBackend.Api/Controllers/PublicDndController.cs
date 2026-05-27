using Microsoft.AspNetCore.Mvc;
using RecuBackend.Api.Services.Dnd5e;

namespace RecuBackend.Api.Controllers;

/// <summary>
/// Consulta reglas D&amp;D 5e desde la API externa https://www.dnd5eapi.co (sin autenticación).
/// </summary>
[ApiController]
[Route("api/public/dnd")]
public sealed class PublicDndController(IDnd5eApiClient dnd5e) : ControllerBase
{
    [HttpGet("spells")]
    public async Task<IActionResult> GetSpell([FromQuery] string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Query 'name' requerido (ej. fireball)." });

        var spell = await dnd5e.FindSpellByNameAsync(name, ct);
        return spell is null ? NotFound(new { message = $"Hechizo '{name}' no encontrado." }) : Ok(spell);
    }

    [HttpGet("monsters")]
    public async Task<IActionResult> GetMonster([FromQuery] string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { message = "Query 'name' requerido (ej. goblin)." });

        var monster = await dnd5e.FindMonsterByNameAsync(name, ct);
        return monster is null ? NotFound(new { message = $"Monstruo '{name}' no encontrado." }) : Ok(monster);
    }
}
