using Saoss.Core.Ships;

namespace Saoss.Core.Simulation;

/// <summary>
/// The asteroids of a match: spawning, gravity, bouncing off each other and ships, and being shot
/// [laskeAsteroidit].
/// </summary>
public sealed class AsteroidField
{
    private const float PlanetPull = 0.1f;
    private const float MaxSpeed = 0.7f;
    private const float OverSpeedDamping = 1.01f;
    private const float ShipMassScale = 100f;
    private const float InertiaFreePushBack = 0.4f;
    private const float MinOutwardAngle = MathF.PI / 6f;

    private readonly GameRandom _random;

    /// <summary>Creates the field with asteroids outside the inner part of the arena.</summary>
    public AsteroidField(GameRandom random)
    {
        _random = random;
        Asteroids = [.. Enumerable.Range(0, GameConstants.AsteroidCount).Select(_ => new Asteroid())];
        foreach (var asteroid in Asteroids)
        {
            Spawn(asteroid, innerLimit: GameConstants.ArenaMaxX / 4f, outerLimit: GameConstants.ArenaMaxX * 2f, maxSpeed: 0.5f);
        }
    }

    /// <summary>All asteroids; there is always the same number of them.</summary>
    public IReadOnlyList<Asteroid> Asteroids { get; }

    /// <summary>
    /// Advances all asteroids one tick. For each asteroid: planet impact, bounces with later asteroids,
    /// bounces with ships, hits by ship projectiles, planet pull and movement, then break-up and respawn.
    /// </summary>
    public void Update(World world)
    {
        for (var i = 0; i < Asteroids.Count; i++)
        {
            var asteroid = Asteroids[i];
            var distanceSquared = (asteroid.X * asteroid.X) + (asteroid.Y * asteroid.Y);
            if (distanceSquared < Square(GameConstants.PlanetRadius + 1f))
            {
                asteroid.Energy = 0;
            }

            for (var j = i + 1; j < Asteroids.Count; j++)
            {
                BounceAsteroids(asteroid, Asteroids[j]);
            }

            foreach (var ship in world.Ships)
            {
                if (ship.IsInGame && ship.IsAlive)
                {
                    BounceShip(asteroid, ship);
                    HitByProjectiles(asteroid, ship);
                }
            }

            asteroid.VelocityX -= asteroid.X / distanceSquared * PlanetPull;
            asteroid.VelocityY -= asteroid.Y / distanceSquared * PlanetPull;
            if (MathF.Sqrt(Square(asteroid.VelocityX) + Square(asteroid.VelocityY)) > MaxSpeed)
            {
                asteroid.VelocityX /= OverSpeedDamping;
                asteroid.VelocityY /= OverSpeedDamping;
            }

            asteroid.X += asteroid.VelocityX;
            asteroid.Y += asteroid.VelocityY;

            if (asteroid.Energy <= 0)
            {
                world.Emit(new AsteroidExploded(asteroid.X, asteroid.Y));
                Spawn(asteroid, innerLimit: GameConstants.ArenaMaxX, outerLimit: GameConstants.ArenaMaxX * 1.2f, maxSpeed: float.MaxValue);
            }
        }
    }

    private static void BounceAsteroids(Asteroid first, Asteroid second)
    {
        var dx = second.X - (first.X + first.VelocityX);
        var dy = second.Y - (first.Y + first.VelocityY);
        if ((dx * dx) + (dy * dy) >= Square(first.CollisionDistance(second.Size)))
        {
            return;
        }

        var normal = MathF.Atan2(dy, dx);
        var relativeSpeed = MathF.Sqrt(Square(second.VelocityX - first.VelocityX) + Square(second.VelocityY - first.VelocityY));
        var impulse = relativeSpeed * (1f / ((1f / second.Size) + (1f / first.Size))) * 2f;
        second.VelocityX += MathF.Cos(normal) * impulse / second.Size;
        second.VelocityY += MathF.Sin(normal) * impulse / second.Size;
        first.VelocityX -= MathF.Cos(normal) * impulse / first.Size;
        first.VelocityY -= MathF.Sin(normal) * impulse / first.Size;
    }

    private static void BounceShip(Asteroid asteroid, Ship ship)
    {
        var dx = ship.X - (asteroid.X + asteroid.VelocityX);
        var dy = ship.Y - (asteroid.Y + asteroid.VelocityY);
        if ((dx * dx) + (dy * dy) >= Square(asteroid.CollisionDistance(ship.Definition.Radius)))
        {
            return;
        }

        var normal = MathF.Atan2(dy, dx);
        var asteroidMass = asteroid.Size * ShipMassScale;
        var relativeSpeed = MathF.Sqrt(Square(ship.VelocityX - asteroid.VelocityX) + Square(ship.VelocityY - asteroid.VelocityY));
        var impulse = relativeSpeed * (1f / ((1f / ship.Definition.Mass) + (1f / asteroidMass))) * 2f;
        if (ship.Definition.IsInertiaFree)
        {
            ship.X -= (asteroid.X - ship.X) * InertiaFreePushBack;
            ship.Y -= (asteroid.Y - ship.Y) * InertiaFreePushBack;
        }
        else
        {
            ship.VelocityX += MathF.Cos(normal) * impulse / ship.Definition.Mass;
            ship.VelocityY += MathF.Sin(normal) * impulse / ship.Definition.Mass;
        }

        asteroid.VelocityX -= MathF.Cos(normal) * impulse / asteroidMass;
        asteroid.VelocityY -= MathF.Sin(normal) * impulse / asteroidMass;

        if (ship.Type == ShipType.Bulldog)
        {
            asteroid.Energy -= ship.Definition.Special.Damage * ship.Fuel / ship.Definition.MaxFuel;
            ship.Fuel = 0;
        }
        else if (ship.Type == ShipType.Wiiii)
        {
            asteroid.Energy -= ship.Definition.Special.Damage;
        }
    }

    private static void HitByProjectiles(Asteroid asteroid, Ship ship)
    {
        var nextX = asteroid.X + asteroid.VelocityX;
        var nextY = asteroid.Y + asteroid.VelocityY;
        var primary = ship.Definition.Primary;
        for (var k = 0; k < primary.MaxProjectiles; k++)
        {
            var projectile = ship.PrimaryProjectiles[k];
            if (projectile.IsActive && IsWithin(projectile, nextX, nextY, asteroid.CollisionDistance(primary.Radius)))
            {
                asteroid.Energy -= primary.Damage;
                projectile.Lifetime = 0;
            }
        }

        // Peace Cannon special objects pass through asteroids.
        if (ship.Type == ShipType.PeaceCannon)
        {
            return;
        }

        var special = ship.Definition.Special;
        for (var k = 0; k < special.MaxProjectiles; k++)
        {
            var projectile = ship.SpecialProjectiles[k];
            if (projectile.IsActive && IsWithin(projectile, nextX, nextY, asteroid.CollisionDistance(special.Radius)))
            {
                asteroid.Energy -= special.Damage;
                projectile.Lifetime = 0;
            }
        }
    }

    private static bool IsWithin(Projectile projectile, float x, float y, float distance) =>
        Square(projectile.X - x) + Square(projectile.Y - y) < Square(distance);

    private static float Square(float value) => value * value;

    // Places the asteroid outside the inner square and aims it so it is not flying straight outward.
    private void Spawn(Asteroid asteroid, float innerLimit, float outerLimit, float maxSpeed)
    {
        asteroid.Size = _random.Next(0.5f) + 0.8f;
        asteroid.SpinPhase = _random.Next(1000f);
        asteroid.SpinAxisX = _random.Range(-1.5f, 1.5f);
        asteroid.SpinAxisY = _random.Range(-1.5f, 1.5f);
        asteroid.SpinAxisZ = _random.Range(-1.5f, 1.5f);
        asteroid.Energy = GameConstants.AsteroidEnergy;
        do
        {
            asteroid.X = _random.Range(-outerLimit, outerLimit);
            asteroid.Y = _random.Range(-outerLimit, outerLimit);
        }
        while (MathF.Abs(asteroid.X) <= innerLimit && MathF.Abs(asteroid.Y) <= innerLimit);

        var outward = MathF.Atan2(asteroid.Y, asteroid.X);
        do
        {
            asteroid.VelocityX = _random.Range(-0.25f, 0.25f);
            asteroid.VelocityY = _random.Range(-0.25f, 0.25f);
        }
        while (MathF.Abs(MathF.Atan2(asteroid.VelocityY, asteroid.VelocityX) - outward) < MinOutwardAngle
            || MathF.Sqrt(Square(asteroid.VelocityX) + Square(asteroid.VelocityY)) > maxSpeed);
    }
}
