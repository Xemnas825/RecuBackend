namespace RecuBackend.Api.Dtos;

public sealed record FileAttachmentResponse(
    Guid Id,
    Guid CharacterId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    bool IsImage,
    DateTime UploadedAtUtc);

public sealed record CreateFileAttachmentRequest(
    string FileName,
    string ContentType,
    long SizeBytes,
    string StoragePath,
    bool IsImage);
