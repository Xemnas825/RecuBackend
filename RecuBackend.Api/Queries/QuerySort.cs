namespace RecuBackend.Api.Queries;

public static class QuerySort
{
    public static bool IsDescending(string? sortDir) =>
        string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);
}
