using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Dnd5e;

public interface IDnd5eApiClient
{
    Task<SpellPublicDto?> FindSpellByNameAsync(string name, CancellationToken ct = default);
    Task<MonsterPublicDto?> FindMonsterByNameAsync(string name, CancellationToken ct = default);
}
