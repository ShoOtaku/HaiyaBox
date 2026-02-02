using AOESafetyCalculator.Core;
using AOESafetyCalculator.Shapes;
using AOESafetyCalculator.SafetyZone;
using Xunit;

namespace AOESafetyCalculator.Tests;

public sealed class AOESafetyCalculatorBehaviorTests
{
    [Fact]
    public void CircleDistance_RespectsInvertForbiddenZone()
    {
        var origin = new WPos(0, 0);
        var normal = new AOEShapeCircle(5f);
        var inverted = new AOEShapeCircle(5f, invertForbiddenZone: true);

        var normalDistance = normal.Distance(origin, default).Distance(origin);
        var invertedDistance = inverted.Distance(origin, default).Distance(origin);

        Assert.True(normalDistance < 0f);
        Assert.True(invertedDistance > 0f);
    }

    [Fact]
    public void MinDistanceBetween_ClampsToMinimum()
    {
        var calculator = new SafeZoneCalculator();
        var points = calculator.FindSafePositions(1, new WPos(0, 0), 5f, DateTime.MinValue)
            .MinDistanceBetween(0f)
            .Execute();

        Assert.NotEmpty(points);
    }

    [Fact]
    public void RectArenaBounds_ZeroDirection_UsesDefault()
    {
        var bounds = new RectArenaBounds(new WPos(0, 0), new WDir(0, 0), 1f, 1f);

        Assert.True(bounds.Contains(new WPos(0, 0)));
    }

    [Fact]
    public void InRect_ZeroStartToEnd_ReturnsFalse()
    {
        var point = new WPos(0, 0);

        Assert.False(point.InRect(new WPos(0, 0), new WPos(0, 0), 1f));
    }

    [Fact]
    public void IsSafe_OutOfArena_IsNotSafe()
    {
        var calculator = new SafeZoneCalculator();
        calculator.SetArenaBounds(new CircleArenaBounds(new WPos(0, 0), 1f));

        Assert.False(calculator.IsSafe(new WPos(2f, 0f), DateTime.MinValue));
    }
}
