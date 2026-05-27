namespace RecuBackend.Api.Dtos;

public sealed record CharacterResponse(
    Guid Id,
    Guid CampaignId,
    string Name,
    string Race,
    string CharacterClass,
    int Level,
    int ArmorClass,
    int HitPoints,
    int ProficiencyBonus,
    int Strength,
    int Dexterity,
    bool IsNpc,
    DateTime CreatedAtUtc);

public sealed record CreateCharacterRequest(
    string Name,
    string Race,
    string CharacterClass,
    int Level,
    int ArmorClass,
    int HitPoints,
    int ProficiencyBonus,
    int Strength,
    int Dexterity,
    bool IsNpc);

public sealed record UpdateCharacterRequest(
    string Name,
    string Race,
    string CharacterClass,
    int Level,
    int ArmorClass,
    int HitPoints,
    int ProficiencyBonus,
    int Strength,
    int Dexterity,
    bool IsNpc);
