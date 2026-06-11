using Microsoft.Extensions.Options;
using RecuBackend.Api.Utils;

namespace RecuBackend.Api.Services;

public sealed class LocalFileStorageService(IOptions<FileStorageSettings> options) : IFileStorageService
{
    private readonly FileStorageSettings _settings = options.Value;

    public void ValidateUpload(string fileName, string contentType, long sizeBytes) =>
        FileValidationHelper.ValidateUpload(_settings, fileName, contentType, sizeBytes);

    public async Task<string> SaveAsync(Stream content, string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(fileStream, ct);
        return relativePath;
    }

    public Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Archivo no encontrado en almacenamiento.", relativePath);

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        var fullPath = GetFullPath(relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }

    private string GetFullPath(string relativePath)
    {
        var root = Path.GetFullPath(_settings.RootPath);
        var combined = Path.GetFullPath(Path.Combine(root, relativePath));
        if (!combined.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Ruta de archivo no válida.");

        return combined;
    }

    public static bool IsImageContentType(string contentType) =>
        FileValidationHelper.IsImageContentType(contentType);
}
