using System.Globalization;

namespace Saoss.Game.Diagnostics;

/// <summary>Frame time statistics over a run of frames.</summary>
/// <param name="Frames">Number of frames.</param>
/// <param name="Seconds">Wall time the frames took.</param>
/// <param name="AverageFrameMs">Mean frame time.</param>
/// <param name="P99FrameMs">99th percentile frame time.</param>
/// <param name="MaxFrameMs">Longest frame.</param>
/// <param name="AverageUpdateMs">Mean update time per frame.</param>
/// <param name="AverageDrawMs">Mean draw and present time per frame.</param>
/// <param name="AverageDrawCalls">Mean device draw calls per frame.</param>
/// <param name="AverageVertices">Mean CPU-lit vertices per frame.</param>
/// <param name="AllocatedBytesPerFrame">Mean managed allocation per frame.</param>
/// <param name="Collections">Garbage collections (generation 0, 1, 2) during the frames.</param>
public sealed record FrameReport(
    int Frames,
    double Seconds,
    double AverageFrameMs,
    double P99FrameMs,
    double MaxFrameMs,
    double AverageUpdateMs,
    double AverageDrawMs,
    double AverageDrawCalls,
    double AverageVertices,
    double AllocatedBytesPerFrame,
    (int Gen0, int Gen1, int Gen2) Collections)
{
    /// <summary>Frames per second over the run.</summary>
    public double FramesPerSecond => Seconds > 0 ? Frames / Seconds : 0;

    /// <summary>One line for logs and the benchmark output.</summary>
    public override string ToString() => string.Format(
        CultureInfo.InvariantCulture,
        "{0:F0} fps, frame avg {1:F2} ms p99 {2:F2} ms max {3:F2} ms, update {4:F2} ms, draw {5:F2} ms, {6:F0} draw calls, {7:F0} vertices, {8:F1} KB/frame, GC {9}/{10}/{11}",
        FramesPerSecond,
        AverageFrameMs,
        P99FrameMs,
        MaxFrameMs,
        AverageUpdateMs,
        AverageDrawMs,
        AverageDrawCalls,
        AverageVertices,
        AllocatedBytesPerFrame / 1024,
        Collections.Gen0,
        Collections.Gen1,
        Collections.Gen2);

    /// <summary>
    /// Summarizes <paramref name="samples"/> that took <paramref name="seconds"/>, with the garbage collections
    /// counted meanwhile.
    /// </summary>
    public static FrameReport From(IReadOnlyList<FrameSample> samples, double seconds, (int Gen0, int Gen1, int Gen2) collections)
    {
        var frameTimes = samples.Select(s => s.FrameMs).Order().ToArray();
        return new FrameReport(
            samples.Count,
            seconds,
            Average(samples, s => s.FrameMs),
            Percentile(frameTimes, 0.99),
            frameTimes.Length > 0 ? frameTimes[^1] : 0,
            Average(samples, s => s.UpdateMs),
            Average(samples, s => s.DrawMs),
            Average(samples, s => s.DrawCalls),
            Average(samples, s => s.Vertices),
            Average(samples, s => s.AllocatedBytes),
            collections);
    }

    /// <summary>Nearest-rank percentile of ascending <paramref name="sorted"/> values; 0 when empty.</summary>
    public static double Percentile(IReadOnlyList<double> sorted, double fraction)
    {
        if (sorted.Count == 0)
        {
            return 0;
        }

        var rank = (int)Math.Ceiling(fraction * sorted.Count);
        return sorted[Math.Clamp(rank - 1, 0, sorted.Count - 1)];
    }

    private static double Average(IReadOnlyList<FrameSample> samples, Func<FrameSample, double> value) =>
        samples.Count > 0 ? samples.Average(value) : 0;
}
