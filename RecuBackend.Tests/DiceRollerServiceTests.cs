using RecuBackend.Api.Models;
using RecuBackend.Api.Services;

namespace RecuBackend.Tests;

public sealed class DiceRollerServiceTests
{
    private readonly DiceRollerService _sut = new();

    [Fact]
    public void Roll_1d20_plus_modifier_includes_modifier_in_total()
    {
        var result = _sut.Roll("1d20+5");

        Assert.Single(result.RawRolls);
        Assert.InRange(result.RawRolls[0], 1, 20);
        Assert.Equal(5, result.Modifier);
        Assert.Equal(result.RawRolls[0] + 5, result.Total);
    }

    [Fact]
    public void Roll_2d6_sums_both_dice()
    {
        var result = _sut.Roll("2d6");

        Assert.Equal(2, result.RawRolls.Count);
        Assert.All(result.RawRolls, r => Assert.InRange(r, 1, 6));
        Assert.Equal(result.RawRolls.Sum(), result.Total);
    }

    [Fact]
    public void Roll_advantage_on_1d20_uses_higher_die()
    {
        var result = _sut.Roll("1d20", D20RollMode.Advantage);

        Assert.Single(result.RawRolls);
        Assert.InRange(result.RawRolls[0], 1, 20);
    }

    [Fact]
    public void Roll_disadvantage_on_1d20_uses_lower_die()
    {
        var result = _sut.Roll("1d20", D20RollMode.Disadvantage);

        Assert.Single(result.RawRolls);
        Assert.InRange(result.RawRolls[0], 1, 20);
    }

    [Fact]
    public void Roll_natural_20_sets_critical_flag()
    {
        for (var i = 0; i < 500; i++)
        {
            var result = _sut.Roll("1d20");
            if (result.RawRolls[0] == 20)
            {
                Assert.True(result.IsCritical);
                return;
            }
        }

        Assert.Fail("No se obtuvo un 20 natural en 500 tiradas (muy improbable).");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("0d6")]
    [InlineData("1d200")]
    public void Roll_invalid_expression_throws(string expression)
    {
        Assert.Throws<ArgumentException>(() => _sut.Roll(expression));
    }
}
