namespace RecuBackend.Api.Cloudinary;

public sealed record CloudinaryUploadResult(
    string SecureUrl,
    string PublicId,
    string ResourceType);
