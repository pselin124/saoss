using System.Globalization;
using Saoss.Core;
using Saoss.Core.Simulation;
using Saoss.Core.Simulation.Modes;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// What the end-of-match screen shows (peliloppu.cpp drawVictory): the winners' ships in a row and the
/// "team N won" text. A credits game shows every player still in the game and the credits they won with;
/// the point modes show the members of the team that reached the target score.
/// </summary>
public sealed class VictoryLayout
{
    /// <summary>Height of the row of ships, before <see cref="ShipScale"/>.</summary>
    public const float ShipY = -6f;

    /// <summary>Depth of the row of ships, before <see cref="ShipScale"/>.</summary>
    public const float ShipZ = -3f;

    /// <summary>Heading of the ships in degrees: nose up.</summary>
    public const float ShipHeading = 90f;

    private const int CrowdedShipCount = 5;
    private const float NormalScale = 0.5f;
    private const float CrowdedScale = 0.32f;
    private const float HalfSlotSpacing = 4f;
    private const float SlotSpacing = 8f;
    private const float TitleY = 12f;
    private const float CreditsY = 6f;

    private VictoryLayout(IReadOnlyList<Ship> ships, IReadOnlyList<VictoryText> texts)
    {
        Ships = ships;
        Texts = texts;
        ShipScale = ships.Count < CrowdedShipCount ? NormalScale : CrowdedScale;
    }

    /// <summary>The ships to show, in player order.</summary>
    public IReadOnlyList<Ship> Ships { get; }

    /// <summary>Scale of the ship row; it shrinks when five or more ships are shown.</summary>
    public float ShipScale { get; }

    /// <summary>The text: "team N won" (and "with N credits"), or "draw" when nobody is shown.</summary>
    public IReadOnlyList<VictoryText> Texts { get; }

    /// <summary>Horizontal position of ship <paramref name="slot"/> in the row, before <see cref="ShipScale"/>.</summary>
    public float ShipX(int slot) => ((Ships.Count - 1) * -HalfSlotSpacing) + (SlotSpacing * slot);

    /// <summary>Builds the layout for the finished match in <paramref name="world"/>.</summary>
    public static VictoryLayout Create(World world, MatchOutcome outcome) =>
        world.Settings.Mode is GameMode.Deathmatch or GameMode.RandomShipsDeathmatch
            ? ForCreditsGame(world, outcome)
            : ForPointMode(world);

    // Players with credits left (score >= 0) are shown; the team is the outcome's winner.
    private static VictoryLayout ForCreditsGame(World world, MatchOutcome outcome)
    {
        var ships = world.Ships.Where(s => s.Score >= 0).ToList();
        if (ships.Count == 0)
        {
            return Draw(ships);
        }

        var team = (outcome.WinningTeam ?? 0) + 1;
        return new VictoryLayout(
            ships,
            [
                .. TeamWon(team),
                new VictoryText(-16, CreditsY, "with"),
                new VictoryText(-2, CreditsY, Number(outcome.WinnerCredits)),
                new VictoryText(6, CreditsY, "credits"),
            ]);
    }

    // The winner is the team of the last player that reached the target score.
    private static VictoryLayout ForPointMode(World world)
    {
        var winner = world.Ships.LastOrDefault(s => s.Score >= world.Settings.Credits);
        if (winner == null)
        {
            return Draw([]);
        }

        var ships = world.Ships.Where(s => s.Team == winner.Team).ToList();
        return new VictoryLayout(ships, TeamWon(winner.Team + 1));
    }

    private static VictoryLayout Draw(IReadOnlyList<Ship> ships) => new(ships, [new VictoryText(-4, 0, "draw")]);

    private static VictoryText[] TeamWon(int team) =>
    [
        new VictoryText(-17, TitleY, "team"),
        new VictoryText(-4, TitleY, Number(team)),
        new VictoryText(1, TitleY, "won"),
    ];

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);
}
