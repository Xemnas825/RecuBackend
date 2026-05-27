using System.Net.Http.Json;
using System.Text.Json;
using RecuBackend.Api.Dtos;

namespace RecuBackend.Api.Services.Dnd5e;

public sealed class Dnd5eApiClient(HttpClient http, IOptions<Dnd5eApiSettings> options) : IDnd5eApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<SpellPublicDto?> FindSpellByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var index = ToIndex(name);
        var direct = await TryGetSpellAsync(index, ct);
        if (direct is not null) return direct;

        var list = await http.GetFromJsonAsync<ApiListResponse>("api/spells", JsonOptions, ct);
        var match = list?.Results?
            .FirstOrDefault(r => r.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return match is null ? null : await TryGetSpellAsync(match.Index, ct);
    }

    public async Task<MonsterPublicDto?> FindMonsterByNameAsync(string name, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;

        var index = ToIndex(name);
        var direct = await TryGetMonsterAsync(index, ct);
        if (direct is not null) return direct;

        var list = await http.GetFromJsonAsync<ApiListResponse>("api/monsters", JsonOptions, ct);
        var match = list?.Results?
            .FirstOrDefault(r => r.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));

        return match is null ? null : await TryGetMonsterAsync(match.Index, ct);
    }

    private async Task<SpellPublicDto?> TryGetSpellAsync(string index, CancellationToken ct)
    {
        var response = await http.GetAsync($"api/spells/{index}", ct);
        if (!response.IsSuccessStatusCode) return null;

        var spell = await response.Content.ReadFromJsonAsync<SpellApiModel>(JsonOptions, ct);
        if (spell is null) return null;

        var school = spell.School?.Name ?? "—";
        var desc = spell.Desc is { Count: > 0 } ? string.Join(" ", spell.Desc) : "—";

        return new SpellPublicDto(
            spell.Index ?? index,
            spell.Name ?? index,
            spell.Level,
            school,
            spell.CastingTime ?? "—",
            spell.Range ?? "—",
            desc);
    }

    private async Task<MonsterPublicDto?> TryGetMonsterAsync(string index, CancellationToken ct)
    {
        var response = await http.GetAsync($"api/monsters/{index}", ct);
        if (!response.IsSuccessStatusCode) return null;

        var monster = await response.Content.ReadFromJsonAsync<MonsterApiModel>(JsonOptions, ct);
        if (monster is null) return null;

        var ac = monster.ArmorClass is { Count: > 0 } ? monster.ArmorClass[0].Value : 0;

        return new MonsterPublicDto(
            monster.Index ?? index,
            monster.Name ?? index,
            monster.Size ?? "—",
            monster.Type ?? "—",
            monster.Alignment ?? "—",
            ac,
            monster.HitPoints,
            monster.ChallengeRating.ToString());
    }

    private static string ToIndex(string name) =>
        name.Trim().ToLowerInvariant().Replace(' ', '-');

    private sealed class ApiListResponse
    {
        public List<ApiIndexResult>? Results { get; set; }
    }

    private sealed class ApiIndexResult
    {
        public string Index { get; set; } = "";
        public string Name { get; set; } = "";
    }

    private sealed class SpellApiModel
    {
        public string? Index { get; set; }
        public string? Name { get; set; }
        public int Level { get; set; }
        public NamedRef? School { get; set; }
        public string? CastingTime { get; set; }
        public string? Range { get; set; }
        public List<string>? Desc { get; set; }
    }

    private sealed class MonsterApiModel
    {
        public string? Index { get; set; }
        public string? Name { get; set; }
        public string? Size { get; set; }
        public string? Type { get; set; }
        public string? Alignment { get; set; }
        public List<ArmorClassRef>? ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public double ChallengeRating { get; set; }
    }

    private sealed class NamedRef
    {
        public string? Name { get; set; }
    }

    private sealed class ArmorClassRef
    {
        public int Value { get; set; }
    }
}
