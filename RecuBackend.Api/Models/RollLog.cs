namespace RecuBackend.Api.Models;

public sealed class RollLog
{
    public Guid Id { get; set; }
    public Guid CharacterId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string DiceExpression { get; set; } = string.Empty;
    public D20RollMode D20Mode { get; set; }
    public string RawResults { get; set; } = string.Empty;
    public int Total { get; set; }
    public bool IsCritical { get; set; }
    public DateTime RolledAtUtc { get; set; }

    public Character Character { get; set; } = null!;
}
