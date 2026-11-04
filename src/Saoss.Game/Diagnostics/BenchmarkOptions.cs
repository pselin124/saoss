namespace Saoss.Game.Diagnostics;

/// <summary>
/// A developer benchmark run (<c>--bench &lt;seconds&gt;</c>): the startup screen runs uncapped (no vsync, one update per
/// frame) in a 1920x1080 window; after a warm-up the frames of <paramref name="Seconds"/> seconds are measured, the
/// report is written and the game exits.
/// </summary>
/// <param name="Seconds">Measured seconds after the warm-up.</param>
/// <param name="Label">Describes the run in the report.</param>
/// <param name="OutputPath">File the report line is appended to, besides standard output.</param>
public sealed record BenchmarkOptions(double Seconds, string Label, string OutputPath)
{
    /// <summary>Seconds run before measuring, for loading and JIT compilation.</summary>
    public const double WarmUpSeconds = 2;

    /// <summary>Back buffer width of a benchmark run.</summary>
    public const int Width = 1920;

    /// <summary>Back buffer height of a benchmark run.</summary>
    public const int Height = 1080;
}
