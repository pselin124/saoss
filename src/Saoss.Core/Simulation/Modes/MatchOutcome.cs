namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// How a match ended. The winner is the team of the player with the highest score, picked like localgame.cpp;
/// a credits game in which every player dropped out is a draw (drawVictory).
/// </summary>
/// <param name="WinningTeam">Winning team, or null for a draw.</param>
/// <param name="FinalScores">Every player's score or remaining credits at the end, by ship index.</param>
/// <param name="WinnerCredits">
/// Credits game only: remaining credits plus the price of the current ship, summed over the players still in the
/// game (the "won with N credits" figure); zero in other modes.
/// </param>
public sealed record MatchOutcome(int? WinningTeam, IReadOnlyList<float> FinalScores, int WinnerCredits)
{
    private const int InitialBest = -2;

    /// <summary>Nobody won.</summary>
    public bool IsDraw => WinningTeam is null;

    /// <summary>Builds the outcome from the world's current state.</summary>
    public static MatchOutcome From(World world)
    {
        var ships = world.Ships;
        var scores = ships.Select(s => s.Score).ToList();
        if (world.Settings.UsesCredits)
        {
            var remaining = ships.Where(s => s.IsInGame).ToList();
            return remaining.Count == 0
                ? new MatchOutcome(null, scores, 0)
                : new MatchOutcome(BestTeam(world), scores, remaining.Aggregate(0, (total, s) => (int)(total + s.Score + s.Definition.Cost)));
        }

        return new MatchOutcome(BestTeam(world), scores, 0);
    }

    private static int BestTeam(World world)
    {
        // The running best was an int, so a later score in the same whole number beats an earlier, higher one.
        var best = InitialBest;
        var team = 0;
        foreach (var ship in world.Ships)
        {
            if (ship.Score > best)
            {
                best = (int)ship.Score;
                team = ship.Team;
            }
        }

        return team;
    }
}
