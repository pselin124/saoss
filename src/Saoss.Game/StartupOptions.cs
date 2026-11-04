using Saoss.Game.Diagnostics;
using Saoss.Game.Settings;

namespace Saoss.Game;

/// <summary>How the game starts, from the command line.</summary>
/// <param name="Screen">The first screen.</param>
public sealed record StartupOptions(StartupScreen Screen = StartupScreen.Intro)
{
    /// <summary>Host to connect to with <see cref="StartupScreen.Join"/>.</summary>
    public string? JoinHost { get; init; }

    /// <summary>With <see cref="StartupScreen.HostLobby"/>: start the first match once this many humans are in the lobby.</summary>
    public int? AutoStartPlayers { get; init; }

    /// <summary>Size of the winning team on the victory preview screens.</summary>
    public int PreviewWinners { get; init; } = VictoryPreview.DefaultWinners;

    /// <summary>With <see cref="StartupScreen.BenchmarkMatch"/>: number of AI ships.</summary>
    public int BenchmarkPlayers { get; init; } = GameSettings.MaxPlayers;

    /// <summary>Detail level for this run instead of the saved one, or null.</summary>
    public int? Detail { get; init; }

    /// <summary>Show frame statistics in the window title and on standard output (<c>--perf</c>).</summary>
    public bool ShowPerformance { get; init; }

    /// <summary>Measure frame times and exit (<c>--bench</c>), or null.</summary>
    public BenchmarkOptions? Benchmark { get; init; }

    /// <summary>Save one frame and exit (<c>--capture</c>), or null.</summary>
    public FrameCapture? Capture { get; init; }
}
