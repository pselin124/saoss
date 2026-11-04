using Saoss.Core.Simulation;
using Saoss.Game.Settings;

namespace Saoss.Game.Diagnostics;

/// <summary>The match of a benchmark run (<c>--bench-match &lt;players&gt;</c>): AI pilots only, each on its own team.</summary>
public static class BenchmarkMatch
{
    /// <summary>Seed of the benchmark match, so every run plays the same game.</summary>
    public const int Seed = 12345;

    /// <summary>A random-ships deathmatch (it never ends) of <paramref name="players"/> AI ships.</summary>
    public static MatchSettings Create(GameSettings settings, int players)
    {
        var count = Math.Clamp(players, GameSettings.MinPlayers, GameSettings.MaxPlayers);
        var slots = Enumerable.Range(0, GameSettings.MaxPlayers).Select(i => new PlayerSlotSettings(IsHuman: false, Team: i)).ToArray();
        return LocalPlayerSetup.CreateMatch(LocalGameKind.RandomShips, settings with { PlayerCount = count, Players = slots });
    }
}
