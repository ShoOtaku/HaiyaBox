using System.Runtime.CompilerServices;
using AOESafetyCalculator.Core;

namespace AOESafetyCalculator.SafetyZone;

[SkipLocalsInit]
internal static class SafeZoneDrawRegistry
{
    private sealed class CalculatorState
    {
        public int Generation;
        public int SafePointGeneration;
        public DateTime SafePointTime;
        public List<WPos> SafePoints = [];
    }

    private static readonly object Sync = new();
    private static readonly List<WeakReference<SafeZoneCalculator>> Calculators = [];
    private static readonly ConditionalWeakTable<SafeZoneCalculator, CalculatorState> States = new();
    private static int Generation;

    internal static void Register(SafeZoneCalculator calculator)
    {
        if (calculator == null) return;

        lock (Sync)
        {
            var state = States.GetOrCreateValue(calculator);
            state.Generation = Generation;

            for (int i = Calculators.Count - 1; i >= 0; i--)
            {
                if (!Calculators[i].TryGetTarget(out var existing))
                {
                    Calculators.RemoveAt(i);
                    continue;
                }

                if (ReferenceEquals(existing, calculator))
                {
                    return;
                }
            }

            Calculators.Add(new WeakReference<SafeZoneCalculator>(calculator));
        }
    }

    internal static void Touch(SafeZoneCalculator calculator) => Register(calculator);

    internal static void ReportSafePoints(SafeZoneCalculator calculator, IReadOnlyList<WPos> points, DateTime currentTime)
    {
        if (calculator == null) return;
        Register(calculator);

        var state = States.GetOrCreateValue(calculator);
        state.SafePoints.Clear();
        if (points != null && points.Count > 0)
        {
            state.SafePoints.AddRange(points);
        }
        state.SafePointGeneration = Generation;
        state.SafePointTime = currentTime;
    }

    internal static bool TryGetSafePoints(SafeZoneCalculator calculator, out IReadOnlyList<WPos> points)
    {
        points = Array.Empty<WPos>();
        if (calculator == null) return false;
        if (!States.TryGetValue(calculator, out var state)) return false;
        if (state.SafePointGeneration != Generation) return false;
        if (state.SafePoints.Count == 0) return false;

        points = state.SafePoints;
        return true;
    }

    internal static IReadOnlyList<SafeZoneCalculator> GetLiveCalculators()
    {
        var result = new List<SafeZoneCalculator>();
        lock (Sync)
        {
            for (int i = Calculators.Count - 1; i >= 0; i--)
            {
                if (!Calculators[i].TryGetTarget(out var calculator))
                {
                    Calculators.RemoveAt(i);
                    continue;
                }

                if (States.TryGetValue(calculator, out var state) && state.Generation == Generation)
                {
                    result.Add(calculator);
                }
            }
        }

        return result;
    }

    internal static void ClearAll()
    {
        lock (Sync)
        {
            Generation++;
            Calculators.Clear();
        }
    }

    internal static void ClearCalculator(SafeZoneCalculator calculator)
    {
        if (calculator == null) return;
        if (!States.TryGetValue(calculator, out var state)) return;
        state.SafePoints.Clear();
        state.SafePointGeneration = Generation;
    }
}
