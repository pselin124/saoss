using Saoss.Core;
using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation.Snapshots;

/// <summary>
/// Plays matches with pseudo-random but reproducible input: every human's buttons and ship purchases depend only
/// on a salt, the tick and the ship index, so two worlds driven alike stay identical.
/// </summary>
internal static class ScriptedMatch
{
    private const int TicksPerInput = 15;
    private const int RandomShipChoice = GameConstants.ShipTypeCount;

    /// <summary>Scenario names accepted by <see cref="Settings"/>.</summary>
    public static readonly string[] Scenarios =
    [
        "Deathmatch", "RandomShips", "CreditsRandom", "CreditsChoose", "Domination", "CaptureTheFlower", "Mushroom", "DominationQuickEnd",
    ];

    /// <summary>
    /// Settings for a scenario: eight ships alternating human and AI on four teams, in every mode and credits variant.
    /// </summary>
    public static MatchSettings Settings(string scenario, int players = 8)
    {
        var setups = Enumerable.Range(0, players).Select(i => new PlayerSetup(i % 4, IsHuman: i % 2 == 0)).ToList();
        var settings = new MatchSettings { Players = setups, Credits = 100_000 };
        return scenario switch
        {
            "Deathmatch" => settings with { Mode = GameMode.Deathmatch },
            "RandomShips" => settings with { Mode = GameMode.RandomShipsDeathmatch, AiTargetsOnlyEnemies = true },
            "CreditsRandom" => settings with { Mode = GameMode.RandomShipsDeathmatch, UsesCredits = true, Credits = 400 },
            "CreditsChoose" => settings with { Mode = GameMode.Deathmatch, UsesCredits = true, PlayersChooseShips = true, Credits = 400 },
            "Domination" => settings with { Mode = GameMode.FlowerDomination },
            "CaptureTheFlower" => settings with { Mode = GameMode.CaptureTheFlower },
            "Mushroom" => settings with { Mode = GameMode.EatTheMushroom },
            "DominationQuickEnd" => settings with { Mode = GameMode.FlowerDomination, Credits = 2 },
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown scenario."),
        };
    }

    /// <summary>Sets the humans' buttons for the coming step and buys pending ships.</summary>
    public static void Drive(World world, int salt)
    {
        ResolveChoices(world, salt);
        foreach (var ship in world.Ships.Where(s => s.IsHuman))
        {
            ship.Controls = Controls(salt, world.Tick, ship.Index);
        }
    }

    /// <summary>Drives and steps the world.</summary>
    public static void Run(World world, int ticks, int salt)
    {
        for (var i = 0; i < ticks; i++)
        {
            Drive(world, salt);
            world.Step();
        }
    }

    /// <summary>The scripted buttons of a ship; they change every 15 ticks.</summary>
    public static ShipControls Controls(int salt, int tick, int shipIndex) =>
        ShipControlsBits.FromBits((byte)(Hash(salt, tick / TicksPerInput, shipIndex) & ShipControlsBits.AllBits));

    private static void ResolveChoices(World world, int salt)
    {
        while (world.PendingChoice is { } choice)
        {
            var roll = (int)(Hash(salt, world.Tick, choice.ShipIndex + 100) % (RandomShipChoice + 1));
            ShipType? type = roll == RandomShipChoice ? null : (ShipType)roll;
            if (!world.ChooseShip(choice.ShipIndex, type))
            {
                world.ChooseShip(choice.ShipIndex, null);
            }
        }
    }

    private static uint Hash(int salt, int a, int b)
    {
        var h = (uint)salt * 0x9E3779B1u ^ (uint)a * 0x85EBCA77u ^ (uint)b * 0xC2B2AE3Du;
        h ^= h >> 15;
        h *= 0x2C1B3C6Du;
        h ^= h >> 12;
        return h;
    }
}
