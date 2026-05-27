using Microsoft.Extensions.Options;

namespace RecuBackend.Api.Services;

public sealed class LocalFileStorageService(IOptions<FileStorageSettings> options) : IFileStorageService
{
    private readonly FileStorageSettings _settings = options.Value;

    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public void ValidateUpload(string fileName, string contentType, long sizeBytes)
    {
        if (sizeBytes <= 0)
            throw new ArgumentException("El archivo está vacío.");

        if (sizeBytes > _settings.MaxFileSizeBytes)
            throw new ArgumentException($"El archivo supera el máximo de {_settings.MaxFileSizeBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !_settings.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Extensión no permitida. Permitidas: {string.Join(", ", _settings.AllowedExtensions)}.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content-Type requerido.");

        var isPdf = ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        var isImage = ImageContentTypes.Contains(contentType);
        if (!isPdf && !isImage)
            throw new ArgumentException("Solo se permiten imágenes (jpeg, png, webp) o PDF.");
    }

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
        ImageContentTypes.Contains(contentType);
}
