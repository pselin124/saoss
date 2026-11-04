using Saoss.Core.Simulation;
using Saoss.Game.Settings;

namespace Saoss.Game;

/// <summary>
/// Turns the settings into the players and rules of a local match.
/// </summary>
public static class LocalPlayerSetup
{
    /// <summary>
    /// The first <see cref="GameSettings.PlayerCount"/> slots. Humans beyond the four keyboard layouts play as CPU;
    /// the match gives the humans the keyboard layouts in slot order.
    /// </summary>
    public static IReadOnlyList<PlayerSetup> Players(GameSettings settings)
    {
        var humans = 0;
        var players = new List<PlayerSetup>();
        foreach (var slot in settings.Players.Take(settings.PlayerCount))
        {
            var isHuman = slot.IsHuman && humans < GameSettings.MaxLocalHumans;
            humans += isHuman ? 1 : 0;
            players.Add(new PlayerSetup(slot.Team, isHuman));
        }

        return players;
    }

    /// <summary>Settings of a new match of <paramref name="kind"/> started from the local game menu.</summary>
    public static MatchSettings CreateMatch(LocalGameKind kind, GameSettings settings) =>
        MatchSettings.ForLocalMenu(kind, Players(settings), settings.Credits) with
        {
            AiTargetsOnlyEnemies = settings.AiTargetsOnlyEnemies,
        };
}
