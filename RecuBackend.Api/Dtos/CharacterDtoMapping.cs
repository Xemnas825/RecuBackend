using System.Text.Json;
using RecuBackend.Api.Models;

namespace RecuBackend.Api.Dtos;

internal static class CharacterDtoMapping
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static CharacterResponse ToResponse(Character ch) => new(
        ch.Id,
        ch.CampaignId,
        ch.Name,
        ch.Race,
        ch.CharacterClass,
        ch.Level,
        ch.ArmorClass,
        ch.HitPoints,
        ch.CurrentHitPoints,
        ch.ProficiencyBonus,
        ch.Strength,
        ch.Dexterity,
        ch.Constitution,
        ch.Intelligence,
        ch.Wisdom,
        ch.Charisma,
        ch.Alignment,
        ch.Background,
        ch.Languages,
        ParseSkills(ch.SkillProficienciesJson),
        ch.IsNpc,
        ch.CreatedAtUtc);

    public static void Apply(Character character, CreateCharacterRequest request)
    {
        character.Name = request.Name;
        character.Race = request.Race;
        character.CharacterClass = request.CharacterClass;
        character.Level = request.Level;
        character.ArmorClass = request.ArmorClass;
        character.HitPoints = request.HitPoints;
        character.CurrentHitPoints = request.CurrentHitPoints > 0 ? request.CurrentHitPoints : request.HitPoints;
        character.ProficiencyBonus = request.ProficiencyBonus;
        character.Strength = request.Strength;
        character.Dexterity = request.Dexterity;
        character.Constitution = request.Constitution;
        character.Intelligence = request.Intelligence;
        character.Wisdom = request.Wisdom;
        character.Charisma = request.Charisma;
        character.Alignment = request.Alignment ?? string.Empty;
        character.Background = request.Background ?? string.Empty;
        character.Languages = request.Languages ?? string.Empty;
        character.SkillProficienciesJson = SerializeSkills(request.SkillProficiencies);
        character.IsNpc = request.IsNpc;
    }

    public static void Apply(Character character, UpdateCharacterRequest request)
    {
        character.Name = request.Name;
        character.Race = request.Race;
        character.CharacterClass = request.CharacterClass;
        character.Level = request.Level;
        character.ArmorClass = request.ArmorClass;
        character.HitPoints = request.HitPoints;
        character.CurrentHitPoints = Math.Min(request.CurrentHitPoints, request.HitPoints);
        if (character.CurrentHitPoints < 0) character.CurrentHitPoints = 0;
        character.ProficiencyBonus = request.ProficiencyBonus;
        character.Strength = request.Strength;
        character.Dexterity = request.Dexterity;
        character.Constitution = request.Constitution;
        character.Intelligence = request.Intelligence;
        character.Wisdom = request.Wisdom;
        character.Charisma = request.Charisma;
        character.Alignment = request.Alignment ?? string.Empty;
        character.Background = request.Background ?? string.Empty;
        character.Languages = request.Languages ?? string.Empty;
        character.SkillProficienciesJson = SerializeSkills(request.SkillProficiencies);
        character.IsNpc = request.IsNpc;
    }

    private static string SerializeSkills(IReadOnlyList<string>? skills)
    {
        var list = skills?
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToLowerInvariant())
            .Distinct()
            .ToList() ?? [];
        return JsonSerializer.Serialize(list, JsonOptions);
    }

    private static IReadOnlyList<string> ParseSkills(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
