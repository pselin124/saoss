using Saoss.Core;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Modes;

/// <summary>
/// Settings shortcuts for game mode tests.
/// </summary>
internal static class ModeTestSettings
{
    /// <summary>Human players on the given teams, in a mode without credits.</summary>
    public static MatchSettings Points(GameMode mode, params int[] teams) => new()
    {
        Players = [.. teams.Select(team => new PlayerSetup(team, IsHuman: true))],
        Mode = mode,
    };

    /// <summary>A credits deathmatch; <paramref name="humans"/> says which slots are human, one team each.</summary>
    public static MatchSettings Credits(bool choose, int credits = MatchSettings.DefaultCredits, params bool[] humans) => new()
    {
        Players = [.. humans.Select((human, i) => new PlayerSetup(i, human))],
        Mode = choose ? GameMode.Deathmatch : GameMode.RandomShipsDeathmatch,
        UsesCredits = true,
        PlayersChooseShips = choose,
        Credits = credits,
    };

    /// <summary>Makes the ship reach the respawn point during the next step.</summary>
    public static void KillBeforeRespawnPoint(Ship ship) => ship.DeathTimer = GameConstants.SpawnTicks + 1;
}
