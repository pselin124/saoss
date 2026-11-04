using Saoss.Core.Simulation;
using Saoss.Game.Diagnostics;
using Saoss.Game.Rendering.Scene;
using Saoss.Game.Settings;

namespace Saoss.Tests.Rendering.CameraVisibility;

/// <summary>
/// Plays all-AI deathmatches (the benchmark match line-up with other seeds) and follows each with one camera per
/// mode and aspect ratio, gathering how long flying ships were off screen.
/// </summary>
internal static class CameraVisibilityRun
{
    /// <summary>Aspect ratios checked: 4:3, 16:9 and 21:9.</summary>
    public static readonly float[] Aspects = [4f / 3f, 16f / 9f, 21f / 9f];

    /// <summary>Runs a match of <paramref name="players"/> ships per seed for <paramref name="ticks"/> ticks.</summary>
    public static IReadOnlyList<CameraVisibilityStats> Measure(IEnumerable<int> seeds, int players, int ticks)
    {
        var results = new List<CameraVisibilityStats>();
        foreach (var seed in seeds)
        {
            var world = new World(BenchmarkMatch.Create(new GameSettings(), players), seed);
            var probes = (from mode in new[] { CameraMode.Tilted, CameraMode.TopDown }
                          from aspect in Aspects
                          select new CameraVisibilityProbe(mode, aspect, world.Ships.Count)).ToList();
            for (var tick = 0; tick < ticks; tick++)
            {
                world.Step();
                foreach (var probe in probes)
                {
                    probe.Observe(world);
                }
            }

            results.AddRange(probes.Select(p => p.Stats()));
        }

        return [.. results.GroupBy(s => (s.Mode, s.Aspect)).Select(Combine)];
    }

    private static CameraVisibilityStats Combine(IGrouping<(CameraMode Mode, float Aspect), CameraVisibilityStats> group)
    {
        var longest = group.MaxBy(s => s.LongestStreak)!;
        return new CameraVisibilityStats(
            group.Key.Mode,
            group.Key.Aspect,
            group.Sum(s => s.ShipTicks),
            group.Sum(s => s.OffScreenTicks),
            group.Sum(s => s.RespawnOffScreenTicks),
            longest.LongestStreak,
            longest.LongestStreakNote);
    }
}
