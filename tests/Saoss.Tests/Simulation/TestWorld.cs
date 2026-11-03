using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Tests.Simulation;

/// <summary>
/// Helpers for building small deterministic worlds in tests.
/// </summary>
internal static class TestWorld
{
    /// <summary>Creates a world with the given number of AI players on separate teams.</summary>
    public static World Create(int players = 2, int seed = 1)
    {
        var settings = new MatchSettings
        {
            Players = [.. Enumerable.Range(0, players).Select(i => new PlayerSetup(Team: i, IsHuman: false))],
        };
        var world = new World(settings, seed);
        ParkAsteroids(world);
        return world;
    }

    /// <summary>Gives the ship a type and puts it at rest at the given position and heading.</summary>
    public static Ship Place(World world, int index, ShipType type, float x, float y, float angle = 0f)
    {
        var ship = world.Ships[index];
        ship.Respawn(ShipCatalog.Get(type), world.Random);
        ship.X = x;
        ship.Y = y;
        ship.Angle = angle;
        ship.WeaponAngle = angle;
        ship.VelocityX = 0;
        ship.VelocityY = 0;
        ship.FireCooldown = 0;
        ship.SpecialCooldown = 0;
        return ship;
    }

    /// <summary>Moves every asteroid far outside the arena so it cannot interfere.</summary>
    public static void ParkAsteroids(World world)
    {
        var offset = 0f;
        foreach (var asteroid in world.Asteroids.Asteroids)
        {
            asteroid.X = 100_000f + offset;
            asteroid.Y = 100_000f;
            asteroid.VelocityX = 0;
            asteroid.VelocityY = 0;
            offset += 100f;
        }
    }
}
