namespace RecuBackend.Api.Models;

public sealed class BootstrapPing
{
    public Guid Id { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}

