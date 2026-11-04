namespace Saoss.Core.Simulation.Modes;

/// <summary>
/// Eat The Mushroom (sieni.cpp): one mushroom in the arena. The first ship to touch it scores for its whole team
/// and the mushroom jumps to a random spot away from the planet. Every death costs the dying player's team a point.
/// </summary>
public sealed class MushroomMode : GameModeRules
{
    /// <summary>Eating radius [SSADE].</summary>
    public const float EatRadius = 5f;

    /// <summary>Points for every player of the eating team [sienipisteet].</summary>
    public const float EatPoints = 10f;

    /// <summary>Points every player of a team loses when one of them dies [skuolemapisteet].</summary>
    public const float DeathPenalty = 1f;

    /// <summary>Where the mushroom is at the start [x, y after alustaSPisteet in localgame.cpp].</summary>
    public const int StartY = 50;

    private const int MinDistanceSquared = 500;
    private const float RollScale = 2000f;
    private const float RollDivisor = 1000f;

    /// <summary>Mushroom position; always whole numbers like the original's int fields [spiste].</summary>
    public int X { get; private set; }

    /// <inheritdoc cref="X"/>
    public int Y { get; private set; } = StartY;

    /// <inheritdoc/>
    public override void OnRespawnPoint(World world, Ship ship) => AddTeamScore(world, ship.Team, -DeathPenalty);

    /// <summary>Lets the first ship in range eat the mushroom and moves it (laskeSPisteet); destroyed ships count too.</summary>
    public override void Update(World world)
    {
        var eater = world.Ships.FirstOrDefault(s => Distance(X, Y, s.X, s.Y) < EatRadius);
        if (eater == null)
        {
            return;
        }

        AddTeamScore(world, eater.Team, EatPoints);
        X = 0;
        Y = 0;
        while ((X * X) + (Y * Y) < MinDistanceSquared)
        {
            // Truncated toward zero when stored, as in the original's int fields.
            X = (int)((world.Random.Next(GameConstants.ArenaMaxX * RollScale) / RollDivisor) - GameConstants.ArenaMaxX);
            Y = (int)((world.Random.Next(GameConstants.ArenaMaxY * RollScale) / RollDivisor) - GameConstants.ArenaMaxY);
        }
    }
}
