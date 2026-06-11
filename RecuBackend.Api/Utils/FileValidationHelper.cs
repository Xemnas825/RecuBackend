using RecuBackend.Api.Services;

namespace RecuBackend.Api.Utils;

public static class FileValidationHelper
{
    private static readonly HashSet<string> ImageContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp"
    };

    public static bool IsImageContentType(string contentType) =>
        ImageContentTypes.Contains(contentType);

    public static bool IsPdf(string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName);
        return ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public static void ValidateUpload(FileStorageSettings settings, string fileName, string contentType, long sizeBytes)
    {
        if (sizeBytes <= 0)
            throw new ArgumentException("El archivo está vacío.");

        if (sizeBytes > settings.MaxFileSizeBytes)
            throw new ArgumentException($"El archivo supera el máximo de {settings.MaxFileSizeBytes / (1024 * 1024)} MB.");

        var ext = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(ext) || !settings.AllowedExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"Extensión no permitida. Permitidas: {string.Join(", ", settings.AllowedExtensions)}.");

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content-Type requerido.");

        var isPdf = IsPdf(fileName, contentType);
        var isImage = IsImageContentType(contentType);
        if (!isPdf && !isImage)
            throw new ArgumentException("Solo se permiten imágenes (jpeg, png, webp) o PDF.");
    }
}
