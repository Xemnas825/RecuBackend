using System.Text.RegularExpressions;
using RecuBackend.Api.Models;

namespace RecuBackend.Api.Services;

public sealed class DiceRollerService
{
    private static readonly Regex DiceRegex = new(
        @"^(?<count>\d*)d(?<sides>\d+)(?<modifier>[+-]\d+)?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public RollResult Roll(string expression, D20RollMode mode = D20RollMode.Normal)
    {
        var trimmed = expression.Trim().Replace(" ", "");
        var match = DiceRegex.Match(trimmed);
        if (!match.Success)
            throw new ArgumentException($"Expresión de dados no válida: {expression}");

        var count = string.IsNullOrEmpty(match.Groups["count"].Value)
            ? 1
            : int.Parse(match.Groups["count"].Value);
        var sides = int.Parse(match.Groups["sides"].Value);
        var modifier = match.Groups["modifier"].Success
            ? int.Parse(match.Groups["modifier"].Value)
            : 0;

        if (count < 1 || count > 20 || sides < 2 || sides > 100)
            throw new ArgumentException("Cantidad de dados o caras fuera de rango permitido.");

        var rolls = new List<int>();
        if (sides == 20 && count == 1 && mode != D20RollMode.Normal)
        {
            var first = Random.Shared.Next(1, 21);
            var second = Random.Shared.Next(1, 21);
            rolls.Add(mode == D20RollMode.Advantage ? Math.Max(first, second) : Math.Min(first, second));
        }
        else
        {
            for (var i = 0; i < count; i++)
                rolls.Add(Random.Shared.Next(1, sides + 1));
        }

        var total = rolls.Sum() + modifier;
        var isCritical = sides == 20 && rolls.Any(r => r == 20);

        return new RollResult(
            trimmed,
            rolls,
            modifier,
            total,
            isCritical);
    }
}

public sealed record RollResult(
    string Expression,
    IReadOnlyList<int> RawRolls,
    int Modifier,
    int Total,
    bool IsCritical);
