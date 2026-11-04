using Saoss.Core.Simulation;

namespace Saoss.Game;

/// <summary>
/// The players of a local match. Until the player setup menu exists this is a fixed line-up.
/// </summary>
public static class LocalPlayerSetup
{
    /// <summary>Player 1 is human on team 0; three AI players are on teams 1 to 3.</summary>
    public static IReadOnlyList<PlayerSetup> Default { get; } =
    [
        new PlayerSetup(Team: 0, IsHuman: true),
        new PlayerSetup(Team: 1, IsHuman: false),
        new PlayerSetup(Team: 2, IsHuman: false),
        new PlayerSetup(Team: 3, IsHuman: false),
    ];
}
