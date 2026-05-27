namespace RecuBackend.Api.Auth;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "RecuBackend";
    public string Audience { get; set; } = "RecuBackend";
    public int ExpirationMinutes { get; set; } = 60;
}
