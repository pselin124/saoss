using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// Buying ships in a credits game (scene.cpp calculateFrame and valitseAlus): random purchases,
/// the AI's purchase with its counter-picks, and the drop-out rule.
/// </summary>
public static class ShipShop
{
    private const int RichAiCredits = 50;
    private const float CounterPickRoll = 100f;

    // Ufo is answered with Ufo or Moth, Trek with Leechmother or Wasp, Power Vacuum with Leechmother, Elephant with
    // Magneto. A row applies when the percentage roll is below its limit; rows of one opponent are checked in order.
    private static readonly (ShipType Opponent, int RollBelow, ShipType Pick)[] CounterPicks =
    [
        (ShipType.Ufo, 40, ShipType.Ufo),
        (ShipType.Ufo, 80, ShipType.Moth),
        (ShipType.Trek, 50, ShipType.Leechmother),
        (ShipType.Trek, 70, ShipType.Wasp),
        (ShipType.PowerVacuum, 50, ShipType.Leechmother),
        (ShipType.Elephant, 50, ShipType.Magneto),
    ];

    /// <summary>
    /// The ship whose price decides whether a player can stay in the game [alus[2]]; it is also the cheapest
    /// and the fallback when the purchase menu is confirmed on a ship the player cannot afford.
    /// </summary>
    public static ShipDefinition FallbackShip => ShipCatalog.Get(ShipType.Wasp);

    /// <summary>
    /// Whether a player reaching the respawn point is out of the game. Having exactly the price of the cheapest
    /// ship is not enough, as in the original.
    /// </summary>
    public static bool MustDropOut(Ship ship) => ship.Score <= FallbackShip.Cost;

    /// <summary>Whether the player has the credits for <paramref name="definition"/>.</summary>
    public static bool CanAfford(Ship ship, ShipDefinition definition) => ship.Score >= definition.Cost;

    /// <summary>Gives the ship the new type at a new position and charges its price.</summary>
    public static void Buy(Ship ship, ShipDefinition definition, GameRandom random)
    {
        ship.Respawn(definition, random);
        ship.Score -= definition.Cost;
    }

    /// <summary>
    /// Rolls ship types until one is affordable. The caller must make sure the player can afford at least one.
    /// </summary>
    public static ShipDefinition RandomAffordable(Ship ship, GameRandom random)
    {
        var definition = ShipCatalog.All[random.NextInt(ShipCatalog.All.Count)];
        while (!CanAfford(ship, definition))
        {
            definition = ShipCatalog.All[random.NextInt(ShipCatalog.All.Count)];
        }

        return definition;
    }

    /// <summary>
    /// What the AI buys from the purchase menu. Steps: look at the first player of another team; against
    /// some ship types pick a counter ship with a set chance, otherwise take the menu's random entry. A poor AI
    /// (under 50 credits) picks a random affordable ship instead. A pick it cannot afford falls back to
    /// <see cref="FallbackShip"/>. Returns null for the random entry.
    /// </summary>
    public static ShipDefinition? AiChoice(World world, Ship ship)
    {
        // The original rolled a menu position first and always overwrote it for the AI.
        _ = world.Random.NextInt(ShipCatalog.All.Count);
        var opponent = world.Ships.FirstOrDefault(s => s.Team != ship.Team);
        var pick = opponent == null ? -1 : CounterPick(opponent.Type, world.Random);
        if (ship.Score < RichAiCredits)
        {
            if (pick < 0)
            {
                pick = world.Random.NextInt(ShipCatalog.All.Count);
            }

            while (ShipCatalog.All[pick].Cost > ship.Score)
            {
                pick = world.Random.NextInt(ShipCatalog.All.Count);
            }
        }

        if (pick < 0)
        {
            return null;
        }

        var definition = ShipCatalog.All[pick];
        return CanAfford(ship, definition) ? definition : FallbackShip;
    }

    private static int CounterPick(ShipType opponent, GameRandom random)
    {
        if (!CounterPicks.Any(row => row.Opponent == opponent))
        {
            return -1;
        }

        var roll = (int)random.Next(CounterPickRoll);
        foreach (var row in CounterPicks)
        {
            if (row.Opponent == opponent && roll < row.RollBelow)
            {
                return (int)row.Pick;
            }
        }

        return -1;
    }
}
