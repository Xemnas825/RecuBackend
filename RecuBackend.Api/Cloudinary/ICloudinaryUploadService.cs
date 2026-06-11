namespace RecuBackend.Api.Cloudinary;

public interface ICloudinaryUploadService
{
    Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string publicId, CancellationToken ct = default);
    Task<CloudinaryUploadResult> UploadPdfAsync(IFormFile file, string publicId, CancellationToken ct = default);
    Task DeleteAsync(string publicId, string resourceType, CancellationToken ct = default);
}
