namespace RecuBackend.Api.Dnd;

public static class DndRules
{
    /// <summary>PHB: bonificador de competencia por nivel de personaje.</summary>
    public static int ProficiencyBonusFromLevel(int level) =>
        2 + Math.Max(0, (Math.Clamp(level, 1, 20) - 1) / 4);

    public static int AbilityModifier(int score) => (int)Math.Floor((score - 10) / 2.0);
}
