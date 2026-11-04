namespace Saoss.Game.Screens.Network;

/// <summary>How a <see cref="NetworkSettingsMenu"/> is opened.</summary>
/// <param name="Resume">Runs for "resume": closes the menu.</param>
public sealed record NetworkSettingsMenuOptions(Action Resume)
{
    /// <summary>The game speed can be changed: only the host, before the match (the original's fps rule).</summary>
    public bool CanChangeSpeed { get; init; }

    /// <summary>During a match: runs for "exit multiplayer", the last entry; null in the lobby, where it is not shown.</summary>
    public Action? ExitMultiplayer { get; init; }
}
