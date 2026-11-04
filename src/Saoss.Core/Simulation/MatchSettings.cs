namespace Saoss.Core.Simulation;

/// <summary>
/// One player slot at match start.
/// </summary>
/// <param name="Team">Team number; players on the same team do not hurt each other with area weapons.</param>
/// <param name="IsHuman">Controlled by a person rather than the AI.</param>
public sealed record PlayerSetup(int Team, bool IsHuman);

/// <summary>
/// Everything needed to start a match.
/// </summary>
public sealed record MatchSettings
{
    /// <summary>Player slots; the list index is the ship index.</summary>
    public required IReadOnlyList<PlayerSetup> Players { get; init; }

    /// <summary>Game mode.</summary>
    public GameMode Mode { get; init; } = GameMode.Deathmatch;

    /// <summary>
    /// When false, an AI pilot with no enemy left attacks the nearest other ship of any team [aikohteet, inverted].
    /// </summary>
    public bool AiTargetsOnlyEnemies { get; init; }
}
