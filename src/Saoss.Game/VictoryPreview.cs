using Saoss.Core;
using Saoss.Core.Simulation;

namespace Saoss.Game;

/// <summary>
/// Developer start-up option that ends a fresh match on its first tick, to look at the end-of-match screen
/// without playing a whole match. Started with <c>--victory-preview</c> (Flower Domination) or
/// <c>--victory-preview-credits</c> (credits game); <c>--winners &lt;n&gt;</c> sets the size of the winning team.
/// </summary>
public static class VictoryPreview
{
    /// <summary>Winning team size when none is given: two ships in a row.</summary>
    public const int DefaultWinners = 2;

    private const int MaxWinners = GameConstants.MaxPlayers - 2;

    /// <summary>A human on team 1, <paramref name="winners"/> AI players on team 2 (the winners) and one AI on team 3.</summary>
    public static IReadOnlyList<PlayerSetup> Players(int winners = DefaultWinners) =>
    [
        new PlayerSetup(Team: 0, IsHuman: true),
        .. Enumerable.Repeat(new PlayerSetup(Team: 1, IsHuman: false), Math.Clamp(winners, 1, MaxWinners)),
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
