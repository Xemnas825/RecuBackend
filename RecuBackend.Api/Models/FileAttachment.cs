namespace RecuBackend.Api.Models;

public sealed class FileAttachment
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public bool IsImage { get; set; }
    public DateTime UploadedAtUtc { get; set; }

    public Character Character { get; set; } = null!;
}
