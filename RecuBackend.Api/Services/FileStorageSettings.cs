namespace RecuBackend.Api.Services;

public sealed class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    public string RootPath { get; set; } = "uploads";
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp", ".pdf"];
}
