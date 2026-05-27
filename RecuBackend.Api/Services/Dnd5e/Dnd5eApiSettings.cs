namespace RecuBackend.Api.Services.Dnd5e;

public sealed class Dnd5eApiSettings
{
    public const string SectionName = "Dnd5eApi";

    public string BaseUrl { get; set; } = "https://www.dnd5eapi.co/";
}
