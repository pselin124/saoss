using Saoss.Core.Simulation;

namespace Saoss.Game;

/// <summary>
/// Developer start-up option that ends a fresh match on its first tick, to look at the end-of-match screen
/// without playing a whole match. Started with <c>--victory-preview</c> (Flower Domination) or
/// <c>--victory-preview-credits</c> (credits game).
/// </summary>
public static class VictoryPreview
{
    /// <summary>Two AI players share the winning team so the screen shows a row of ships.</summary>
    public static IReadOnlyList<PlayerSetup> Players { get; } =
    [
        new PlayerSetup(Team: 0, IsHuman: true),
        new PlayerSetup(Team: 1, IsHuman: false),
        new PlayerSetup(Team: 1, IsHuman: false),
        new PlayerSetup(Team: 2, IsHuman: false),
    ];

    /// <summary>
    /// Sets up <paramref name="world"/> so that its next step ends the match with the team of ship 1 winning:
    /// in a credits game every other team drops out, otherwise ship 1 reaches the target score.
    /// </summary>
    public static void EndOnNextStep(World world)
    {
        var winner = world.Ships[1];
        if (!world.Settings.UsesCredits)
        {
            winner.Score = world.Settings.Credits;
            return;
        }

        foreach (var ship in world.Ships.Where(s => s.Team != winner.Team))
        {
            ship.Score = -1;
            ship.IsInGame = false;
        }
    }
}
