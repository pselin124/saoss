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
}
