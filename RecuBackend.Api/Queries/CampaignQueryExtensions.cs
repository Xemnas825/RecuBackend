using RecuBackend.Api.Models;

namespace RecuBackend.Api.Queries;

public static class CampaignQueryExtensions
{
    public static IQueryable<Campaign> ApplyCampaignFilters(
        this IQueryable<Campaign> query,
        string? search,
        string? setting,
        bool? isActive,
        bool? isPublic)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Setting.Contains(term) ||
                c.Description.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(setting))
            query = query.Where(c => c.Setting.Contains(setting.Trim()));

        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        if (isPublic.HasValue)
            query = query.Where(c => c.IsPublic == isPublic.Value);

        return query;
    }

    public static IQueryable<Campaign> ApplyCampaignSort(
        this IQueryable<Campaign> query,
        string? sortBy,
        string? sortDir)
    {
        var desc = QuerySort.IsDescending(sortDir);
        return (sortBy?.ToLowerInvariant()) switch
        {
            "name" => desc ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "setting" => desc ? query.OrderByDescending(c => c.Setting) : query.OrderBy(c => c.Setting),
            "createdat" or "created" => desc
                ? query.OrderByDescending(c => c.CreatedAtUtc)
                : query.OrderBy(c => c.CreatedAtUtc),
            _ => desc
                ? query.OrderByDescending(c => c.UpdatedAtUtc)
                : query.OrderBy(c => c.UpdatedAtUtc)
        };
    }
}
