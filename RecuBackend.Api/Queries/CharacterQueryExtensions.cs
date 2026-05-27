using RecuBackend.Api.Models;

namespace RecuBackend.Api.Queries;

public static class CharacterQueryExtensions
{
    public static IQueryable<Character> ApplyCharacterFilters(
        this IQueryable<Character> query,
        string? name,
        string? race,
        string? characterClass,
        bool? isNpc)
    {
        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(ch => ch.Name.Contains(name.Trim()));

        if (!string.IsNullOrWhiteSpace(race))
            query = query.Where(ch => ch.Race.Contains(race.Trim()));

        if (!string.IsNullOrWhiteSpace(characterClass))
            query = query.Where(ch => ch.CharacterClass.Contains(characterClass.Trim()));

        if (isNpc.HasValue)
            query = query.Where(ch => ch.IsNpc == isNpc.Value);

        return query;
    }

    public static IQueryable<Character> ApplyCharacterSort(
        this IQueryable<Character> query,
        string? sortBy,
        string? sortDir)
    {
        var desc = QuerySort.IsDescending(sortDir);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "level" => desc ? query.OrderByDescending(ch => ch.Level) : query.OrderBy(ch => ch.Level),
            "race" => desc ? query.OrderByDescending(ch => ch.Race) : query.OrderBy(ch => ch.Race),
            "class" or "characterclass" => desc
                ? query.OrderByDescending(ch => ch.CharacterClass)
                : query.OrderBy(ch => ch.CharacterClass),
            _ => desc ? query.OrderByDescending(ch => ch.Name) : query.OrderBy(ch => ch.Name)
        };
    }
}
