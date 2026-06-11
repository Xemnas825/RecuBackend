using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using RecuBackend.Api.Utils;
using CloudinaryClient = CloudinaryDotNet.Cloudinary;
using CloudinaryAccount = CloudinaryDotNet.Account;

namespace RecuBackend.Api.Cloudinary;

public sealed class CloudinaryUploadService(IOptions<CloudinarySettings> cloudinaryOptions) : ICloudinaryUploadService
{
    private readonly CloudinarySettings _settings = cloudinaryOptions.Value;
    private CloudinaryClient? _client;

    private CloudinaryClient Client => _client ??= CreateClient();

    public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string publicId, CancellationToken ct = default)
    {
        EnsureConfigured();
        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Overwrite = true
        };

        var result = await Client.UploadAsync(uploadParams);
        return ToResult(result, "image");
    }

    public async Task<CloudinaryUploadResult> UploadPdfAsync(IFormFile file, string publicId, CancellationToken ct = default)
    {
        EnsureConfigured();
        await using var stream = file.OpenReadStream();
        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            PublicId = publicId,
            Overwrite = true
        };

        var result = await Client.UploadAsync(uploadParams);
        return ToResult(result, "raw");
    }

    public Task DeleteAsync(string publicId, string resourceType, CancellationToken ct = default)
    {
        EnsureConfigured();
        var type = resourceType.Equals("raw", StringComparison.OrdinalIgnoreCase)
            ? ResourceType.Raw
            : ResourceType.Image;

        return Client.DestroyAsync(new DeletionParams(publicId) { ResourceType = type });
    }

    private void EnsureConfigured()
    {
        if (!_settings.IsConfigured)
        {
            throw new InvalidOperationException(
                "Cloudinary no está configurado. Define CloudinarySettings:CloudName, ApiKey y ApiSecret " +
                "(o variables CLOUDINARY_CLOUD_NAME, CLOUDINARY_API_KEY, CLOUDINARY_API_SECRET).");
        }
    }

    private CloudinaryClient CreateClient()
    {
        EnsureConfigured();
        var account = new CloudinaryAccount(_settings.CloudName, _settings.ApiKey, _settings.ApiSecret);
        return new CloudinaryClient(account);
    }

    private static CloudinaryUploadResult ToResult(UploadResult result, string resourceType)
    {
        if (result.Error is not null)
            throw new InvalidOperationException($"Error al subir a Cloudinary: {result.Error.Message}");

        var url = result.SecureUrl?.ToString();
        var publicId = result.PublicId;
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(publicId))
            throw new InvalidOperationException("Cloudinary no devolvió URL o PublicId.");

        return new CloudinaryUploadResult(url, publicId, resourceType);
    }
}
