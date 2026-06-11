namespace RecuBackend.Api.Dtos;

public sealed record FileAttachmentResponse(
    Guid Id,
    Guid CharacterId,
    string FileName,
    string ContentType,
    long SizeBytes,
    bool IsImage,
    string? SecureUrl,
    DateTime UploadedAtUtc);
