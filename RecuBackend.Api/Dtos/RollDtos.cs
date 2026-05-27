using RecuBackend.Api.Models;

namespace RecuBackend.Api.Dtos;

public sealed record RollResponse(
    Guid Id,
    Guid CharacterId,
    string Label,
    string DiceExpression,
    D20RollMode D20Mode,
    string RawResults,
    int Total,
    bool IsCritical,
    DateTime RolledAtUtc);

public sealed record CreateRollRequest(
    string Label,
    string DiceExpression,
    D20RollMode D20Mode = D20RollMode.Normal);
