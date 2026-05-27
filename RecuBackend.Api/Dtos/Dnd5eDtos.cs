namespace RecuBackend.Api.Dtos;

public sealed record SpellPublicDto(
    string Index,
    string Name,
    int Level,
    string School,
    string CastingTime,
    string Range,
    string Description);

public sealed record MonsterPublicDto(
    string Index,
    string Name,
    string Size,
    string Type,
    string Alignment,
    int ArmorClass,
    int HitPoints,
    string ChallengeRating);
