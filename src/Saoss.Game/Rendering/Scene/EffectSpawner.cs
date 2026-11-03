using Saoss.Core.Ships;
using Saoss.Core.Simulation;

namespace Saoss.Game.Rendering.Scene;

/// <summary>
/// Spawns the particles of game events (explosions, impacts, boosts) and of flying ships (damage smoke and
/// thrust sparks from Player::liiku). Uses the visual random source only.
/// </summary>
public sealed class EffectSpawner(int detail)
{
    /// <summary>
    /// Per-tick particles of a living ship [liiku]: occasional smoke from a damaged ship and engine
    /// sparks while thrusting. Both need detail 1 or more.
    /// </summary>
    public void SpawnShipParticles(Ship ship, SceneState state)
    {
        if (detail <= 0)
        {
            return;
        }

        var random = state.Random;
        var maxEnergy = ship.Definition.MaxEnergy;
        if (ship.Energy < maxEnergy && random.Next(15000) / 100f * ship.Energy < maxEnergy)
        {
            state.Particles.Spawn(1, p => DamageSmoke(p, ship, random));
        }

        if (ship.IsThrusting && ship.Type is not ShipType.StarWorm and not ShipType.Elephant)
        {
            state.Particles.Spawn(detail, p => ThrustSpark(p, ship, random));
        }
    }

    /// <summary>Spawns the particles for one simulation event, if it has any.</summary>
    public void Handle(GameEvent gameEvent, World world, SceneState state)
    {
        var random = state.Random;
        var particles = state.Particles;
        switch (gameEvent)
        {
            case ShipExploded e:
                particles.Spawn(200 + (detail * 100), p => ShipDebris(p, e.X, e.Y, random));
                break;
            case AsteroidExploded e:
                particles.Spawn(35 + (detail * 15), p => AsteroidDebris(p, e.X, e.Y, random));
                break;
            case EffectEvent { Kind: EffectKind.HeavyShotImpact } e:
                particles.Spawn(150 + (detail * 50), p => HeavyShotSpark(p, e, random));
                break;
            case EffectEvent { Kind: EffectKind.LeechPlanetImpact } e:
                particles.Spawn(50, p => LeechSpark(p, e, random));
                break;
            case EffectEvent { Kind: EffectKind.DriftMineImpact } e:
                particles.Spawn(150 + (detail * 50), p => DriftMineSpark(p, e, random));
                break;
            case EffectEvent { Kind: EffectKind.BulldogBoostExhaust } e:
                var heading = BoosterHeading(world, e);
                particles.Spawn(50, p => BoostSpark(p, e, heading, random));
                break;
            case EffectEvent { Kind: EffectKind.PopperMineExplosion } e:
                particles.Spawn(200 + (detail * 50), p => PopperSpark(p, e, random));
                break;
            default:
                break;
        }
    }

    private static void DamageSmoke(Particle p, Ship ship, GameRandom random)
    {
        p.X = ship.X;
        p.Y = ship.Y;
        p.VelocityX = ship.VelocityX + (random.Next(200) / 200f) - 0.5f;
        p.VelocityY = ship.VelocityY + (random.Next(200) / 200f) - 0.5f;
        p.VelocityZ = (random.Next(100) / 200f) - 0.25f;
        p.MaxLife = 150;
        p.Life = (int)(random.Next(120) + 50);
        p.Size = (random.Next(45) / 100f) + 0.45f;
        p.Red = (random.Next(50) / 100f) + 0.5f;
        p.Green = random.Next(40) / 100f;
        p.Blue = random.Next(40) / 100f;
        if (random.Next(10) > 7)
        {
            p.IsSquare = true;
            p.Size /= 2f;
        }
        else
        {
            p.Life = (int)(p.Life * 1.5);
        }
    }

    private static void ThrustSpark(Particle p, Ship ship, GameRandom random)
    {
        var (first, second) = ExhaustTrails.NozzleOffsets(ship.Type);
        var offset = second is { } other && random.Next(2) >= 1 ? other : first;
        var (x, y) = ExhaustTrails.NozzlePosition(ship, offset);
        p.X = x + (random.Next(300) / 1000f) - 0.15f;
        p.Y = y + (random.Next(300) / 1000f) - 0.15f;
        var back = (ship.Angle - 180f) / 180f * MathF.PI;
        p.VelocityX = ship.VelocityX + (MathF.Cos(back) * 0.35f) + (random.Next(200) / 1000f) - 0.1f;
        p.VelocityY = ship.VelocityY + (MathF.Sin(back) * 0.35f) + (random.Next(200) / 1000f) - 0.1f;
        p.VelocityZ = (random.Next(200) / 1000f) - 0.1f;
        p.MaxLife = 100;
        p.Life = (int)(random.Next(50) + 50);
        p.Size = (random.Next(80) / 100f) + 0.15f + (ship.Definition.Radius / 5f);
        p.Red = (random.Next(60) / 100f) + 0.5f;
        p.Green = random.Next(50) / 100f;
        p.Blue = random.Next(50) / 100f;
        if (random.Next(10) > 9)
        {
            p.IsSquare = true;
            p.Size /= 4f;
        }
    }

    // Explosion debris, speed below 0.64 in all three dimensions (scene.cpp calculateFrame).
    private static void ShipDebris(Particle p, float x, float y, GameRandom random)
    {
        p.X = x;
        p.Y = y;
        do
        {
            p.VelocityX = (random.Next(1500) / 1000f) - 0.75f;
            p.VelocityY = (random.Next(1500) / 1000f) - 0.75f;
            p.VelocityZ = (random.Next(1500) / 1000f) - 0.75f;
        }
        while (MathF.Sqrt((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) + (p.VelocityZ * p.VelocityZ)) > 0.64f);

        p.MaxLife = 400;
        p.Life = (int)(random.Next(200) + 200);
        p.Size = (random.Next(200) / 100f) + 0.6f;
        p.Red = (random.Next(50) / 100f) + 0.5f;
        p.Green = random.Next(100) / 100f;
        p.Blue = random.Next(100) / 100f;
    }

    // The speed check adds the y velocity twice instead of squaring it, as in the original.
    private static void AsteroidDebris(Particle p, float x, float y, GameRandom random)
    {
        p.X = x;
        p.Y = y;
        do
        {
            p.VelocityX = (random.Next(200) / 500f) - 0.2f;
            p.VelocityY = (random.Next(200) / 500f) - 0.2f;
            p.VelocityZ = (random.Next(200) / 500f) - 0.2f;
        }
        while ((p.VelocityX * p.VelocityX) + p.VelocityY + p.VelocityY + (p.VelocityZ * p.VelocityZ) > 0.09f);

        p.MaxLife = 250;
        p.Life = (int)(random.Next(200) + 50);
        p.Size = (random.Next(30) / 100f) + 0.2f;
        p.Red = (random.Next(20) / 100f) + 0.5f;
        p.Green = (random.Next(20) / 100f) + 0.5f;
        p.Blue = (random.Next(15) / 100f) + 0.35f;
    }

    private static void HeavyShotSpark(Particle p, EffectEvent e, GameRandom random)
    {
        p.X = e.X;
        p.Y = e.Y;
        do
        {
            p.VelocityX = (random.Next(400) / 1000f) - 0.2f;
            p.VelocityY = (random.Next(400) / 1000f) - 0.2f;
        }
        while ((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) > 0.04f);

        p.VelocityX += e.VelocityX;
        p.VelocityY += e.VelocityY;
        p.MaxLife = 75;
        p.Life = (int)(random.Next(50) + 25);
        p.Size = random.Next(30) / 100f * 2f;
        p.Red = (random.Next(100) / 50f) + 0.5f;
        p.Green = random.Next(100) / 100f;
        p.Blue = random.Next(100) / 100f;
    }

    // The original never set the size, so a reused slot keeps its previous particle's size.
    private static void LeechSpark(Particle p, EffectEvent e, GameRandom random)
    {
        p.X = e.X;
        p.Y = e.Y;
        do
        {
            p.VelocityX = (random.Next(25) / 50f) - 0.25f;
            p.VelocityY = (random.Next(25) / 50f) - 0.25f;
        }
        while ((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) > 0.09f);

        p.MaxLife = 150;
        p.Life = (int)(random.Next(100) + 50);
        p.Red = random.Next(100) / 100f;
        p.Green = random.Next(100) / 100f;
        p.Blue = random.Next(100) / 100f;
    }

    private static void DriftMineSpark(Particle p, EffectEvent e, GameRandom random)
    {
        p.X = e.X;
        p.Y = e.Y;
        do
        {
            p.VelocityX = (random.Next(200) / 1000f) - 0.1f;
            p.VelocityY = (random.Next(200) / 1000f) - 0.1f;
        }
        while (MathF.Sqrt((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY)) > 0.1f);

        p.VelocityX += e.VelocityX;
        p.VelocityY += e.VelocityY;
        p.MaxLife = 75;
        p.Life = (int)(random.Next(50) + 25);
        p.Size = random.Next(40) / 100f;
        p.Red = (random.Next(50) / 40f) + 0.2f;
        p.Green = p.Red;
        p.Blue = p.Red;
    }

    // The original offset the start point by cos/sin of the heading in degrees taken as radians.
    private static void BoostSpark(Particle p, EffectEvent e, float heading, GameRandom random)
    {
        p.X = e.X - MathF.Cos(heading);
        p.Y = e.Y - MathF.Sin(heading);
        do
        {
            p.VelocityX = (random.Next(25) / 50f) - 0.25f;
            p.VelocityY = (random.Next(25) / 50f) - 0.25f;
        }
        while ((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) > 0.015f);

        var back = (heading / 180f * MathF.PI) - MathF.PI;
        p.VelocityX += (MathF.Cos(back) * ((random.Next(100) / 100f) + 0.5f)) + e.VelocityX;
        p.VelocityY += (MathF.Sin(back) * ((random.Next(100) / 100f) + 0.5f)) + e.VelocityY;
        p.MaxLife = 100;
        p.Life = (int)(random.Next(100) + 50);
        p.Size = (random.Next(20) / 100f) + 0.2f;
        p.Red = (random.Next(40) / 100f) + 0.6f;
        p.Green = random.Next(20) / 100f;
        p.Blue = random.Next(20) / 100f;
    }

    private static void PopperSpark(Particle p, EffectEvent e, GameRandom random)
    {
        p.X = e.X;
        p.Y = e.Y;
        do
        {
            p.VelocityX = (random.Next(100) / 200f) - 0.25f;
            p.VelocityY = (random.Next(100) / 200f) - 0.25f;
        }
        while ((p.VelocityX * p.VelocityX) + (p.VelocityY * p.VelocityY) > 0.0625f);

        p.MaxLife = 400;
        p.Life = (int)(random.Next(300) + 100);
        p.Size = random.Next(70) / 100f;
        p.Red = (random.Next(50) / 100f) + 0.5f;
        p.Green = random.Next(30) / 100f;
        p.Blue = random.Next(30) / 100f;
    }

    // The event carries no ship index; the boosting Bulldog is the one nearest to the event (it has
    // moved by one tick since the event was raised).
    private static float BoosterHeading(World world, EffectEvent e) =>
        world.Ships
            .Where(s => s.Type == ShipType.Bulldog)
            .MinBy(s => ((s.X - e.X) * (s.X - e.X)) + ((s.Y - e.Y) * (s.Y - e.Y)))?.Angle ?? 0f;
}
