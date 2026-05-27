namespace RecuBackend.Api.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream content,
        string relativePath,
        CancellationToken ct = default);

    Task<Stream> OpenReadAsync(string relativePath, CancellationToken ct = default);

    Task DeleteAsync(string relativePath, CancellationToken ct = default);

    void ValidateUpload(string fileName, string contentType, long sizeBytes);
}
