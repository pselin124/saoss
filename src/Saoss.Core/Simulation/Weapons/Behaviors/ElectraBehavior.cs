using Saoss.Core.Ships;

namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Electra: fires a short-range lightning bolt that zaps the nearest enemy ship, or else an enemy projectile, in
/// reach. The special (new, replacing the original's asteroid eating) is an overcharge: triple speed for a moment,
/// then a chain lightning that jumps from enemy to enemy. Asteroids in reach are eaten automatically for a full tank.
/// </summary>
public sealed class ElectraBehavior : ShipBehavior
{
    /// <summary>Speed multiplier while the overcharge boost lasts.</summary>
    public const float BoostSpeedFactor = 3f;

    /// <summary>How close an asteroid must be to be eaten; the reach of the original special.</summary>
    public const float AsteroidReach = 8f;

    /// <summary>Most targets one chain lightning hits.</summary>
    public const int ChainLinks = 4;

    /// <summary>Damage of each chain link compared to the one before.</summary>
    public const float ChainFalloff = 0.6f;

    /// <summary>First beam of the chain lightning; beams 0-20 belong to the primary bolts.</summary>
    public const int FirstChainBeam = 1 + (2 * BoltSegments);

    /// <summary>Segments of each chain link's bolt.</summary>
    public const int ChainSegments = 8;

    private const float ExtraReach = 10f;
    private const float RandomFactorRoll = 50f;
    private const float RandomFactorBase = 0.5f;
    private const float IdleFuelDivisor = 3f;
    private const int BoltSegments = 10;
    private const float BoltJitter = 2.5f;
    private const int BoltLifetime = 2;
    private const float BoltRed = 0.2f;
    private const float AimRed = 0f;
    private const float Cyan = 1f;
    private const float ChainJitter = 1.5f;
    private const int ChainLifetime = 15;
    private const float ChainRed = 0.7f;
    private const float FizzleReachDivisor = 3f;

    /// <summary>Whether <paramref name="ship"/> is an Electra in its overcharge boost.</summary>
    public static bool IsBoosting(Ship ship) => ship.Type == ShipType.Electra && ship.SpecialTimeLeft > 0;

    /// <summary>
    /// Steps: play the sound, find a target (enemy ship, else enemy primary projectile, else enemy special
    /// projectile) and damage it, otherwise point the bolt straight ahead; pay fuel; draw two jagged bolts.
    /// </summary>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        world.PlaySound(ship, ShipSound.Fire);
        ship.FireCooldown = weapon.FireDelay;

        var aim = ship.Beams[0];
        aim.VelocityX = 0;
        aim.VelocityY = 0;

        // A very large target can be zapped at a distance that still counts as nothing found, as in the original.
        var nothingFound = weapon.Radius + ExtraReach;
        var distance = ZapShip(ship, world, aim);
        if (distance >= nothingFound)
        {
            distance = ZapProjectile(ship, world, aim, special: false);
        }

        if (distance >= nothingFound)
        {
            distance = ZapProjectile(ship, world, aim, special: true);
        }

        if (distance >= nothingFound)
        {
            var (headingX, headingY) = ship.Heading;
            aim.Set(ship.X + (headingX * (weapon.Radius + 1)), ship.Y + (headingY * (weapon.Radius + 1)), ship.X, ship.Y, AimRed, Cyan, Cyan, weapon.Lifetime);
            ship.WeaponAngle = ship.Angle;
            ship.Fuel -= weapon.FuelCost / IdleFuelDivisor;
        }
        else
        {
            ship.Fuel -= RandomFactor(world) * weapon.FuelCost;
        }

        // The aim line itself is never shown; only the two bolts drawn along it are.
        aim.Lifetime = 0;
        DrawBolts(ship, world, aim);
    }

    /// <summary>Starts the overcharge boost; the chain lightning follows in <see cref="EndSpecial"/>.</summary>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        ship.SpecialTimeLeft = special.Duration;
    }

    /// <summary>
    /// Fires the chain lightning as the boost ends. Steps: from the ship, find the nearest enemy ship in reach (else
    /// the nearest breakable enemy projectile) not hit yet, hurt it and draw a bolt to it; then jump on from there, each
    /// link weaker by <see cref="ChainFalloff"/>, for at most <see cref="ChainLinks"/> targets. A Magneto shield takes no
    /// damage but still passes the chain on. With nothing in reach a short bolt crackles ahead.
    /// </summary>
    public override void EndSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.Fire);
        var hit = new List<object>();
        var (fromX, fromY) = (ship.X, ship.Y);
        var damage = special.Damage;
        for (var link = 0; link < ChainLinks; link++)
        {
            if (FindChainTarget(ship, world, fromX, fromY, hit) is not { } target)
            {
                break;
            }

            hit.Add(target.Item);
            if (target.Item is Ship enemy)
            {
                if (!Combat.IsShielded(enemy))
                {
                    enemy.Energy -= damage;
                }
            }
            else if (target.Item is Projectile projectile)
            {
                projectile.Energy -= damage;
                if (projectile.Energy <= 0)
                {
                    projectile.Energy = 0;
                    projectile.Lifetime = 0;
                }
            }

            DrawChainBolt(ship.Beams, FirstChainBeam + (link * ChainSegments), fromX, fromY, target.X, target.Y, world);
            (fromX, fromY) = (target.X, target.Y);
            damage *= ChainFalloff;
        }

        if (hit.Count == 0)
        {
            var (headingX, headingY) = ship.Heading;
            var reach = special.Radius / FizzleReachDivisor;
            DrawChainBolt(ship.Beams, FirstChainBeam, ship.X, ship.Y, ship.X + (headingX * reach), ship.Y + (headingY * reach), world);
        }
    }

    /// <summary>
    /// Eats the first asteroid in <see cref="AsteroidReach"/> whenever the tank is not full: the asteroid breaks up
    /// and the tank is refilled.
    /// </summary>
    public override void BeforePositionUpdate(Ship ship, World world)
    {
        if (ship.Fuel >= ship.Definition.MaxFuel)
        {
            return;
        }

        foreach (var asteroid in world.Asteroids.Asteroids)
        {
            var dx = ship.X - asteroid.X;
            var dy = ship.Y - asteroid.Y;
            if (asteroid.Energy > 0 && MathF.Sqrt((dx * dx) + (dy * dy)) < AsteroidReach)
            {
                world.PlaySound(ship, ShipSound.SpecialHit);
                asteroid.Energy = 0;
                ship.Fuel = ship.Definition.MaxFuel;
                return;
            }
        }
    }

    // Nearest enemy ship within the special's reach of the point, else the nearest breakable enemy projectile.
    private static ChainTarget? FindChainTarget(Ship ship, World world, float x, float y, List<object> hit)
    {
        var reach = ship.Definition.Special.Radius;
        ChainTarget? best = null;
        var bestDistance = float.MaxValue;
        foreach (var enemy in Combat.EnemyTargets(world, ship))
        {
            var distance = MathF.Sqrt(Combat.Square(enemy.X - x) + Combat.Square(enemy.Y - y));
            if (!hit.Contains(enemy) && distance < reach + (enemy.Definition.Radius * Combat.RadiusScale) && distance < bestDistance)
            {
                best = new ChainTarget(enemy, enemy.X, enemy.Y);
                bestDistance = distance;
            }
        }

        if (best != null)
        {
            return best;
        }

        foreach (var enemy in world.Ships.Where(s => s.IsInGame && s.Index != ship.Index && s.Team != ship.Team))
        {
            var definition = enemy.Definition;
            foreach (var (pool, count, radius, breakable) in new[]
            {
                (enemy.PrimaryProjectiles, definition.Primary.MaxProjectiles, definition.Primary.Radius, definition.Primary.CanBeDestroyed),
                (enemy.SpecialProjectiles, definition.Special.MaxProjectiles, definition.Special.Radius, definition.Special.CanBeDestroyed),
            })
            {
                if (!breakable)
                {
                    continue;
                }

                for (var i = 0; i < count; i++)
                {
                    var projectile = pool[i];
                    var distance = MathF.Sqrt(Combat.Square(projectile.X - x) + Combat.Square(projectile.Y - y));
                    if (projectile.IsActive && !hit.Contains(projectile) && distance < reach + (radius * Combat.RadiusScale) && distance < bestDistance)
                    {
                        best = new ChainTarget(projectile, projectile.X, projectile.Y);
                        bestDistance = distance;
                    }
                }
            }
        }

        return best;
    }

    // One jagged bolt of ChainSegments beams from (x1, y1) to (x2, y2); the joints wander sideways, most in the middle.
    private static void DrawChainBolt(Beam[] beams, int first, float x1, float y1, float x2, float y2, World world)
    {
        var stepX = (x2 - x1) / ChainSegments;
        var stepY = (y2 - y1) / ChainSegments;
        for (var i = 0; i < ChainSegments; i++)
        {
            var segment = beams[first + i];
            segment.Set(x1 + (i * stepX), y1 + (i * stepY), x1 + ((i + 1) * stepX), y1 + ((i + 1) * stepY), ChainRed, Cyan, Cyan, ChainLifetime);
            segment.VelocityX = 0;
            segment.VelocityY = 0;
        }

        for (var i = 1; i < ChainSegments; i++)
        {
            var scale = MathF.Sin(MathF.PI * i / ChainSegments) * ChainJitter;
            var offsetX = ((world.Random.Next(100) / 50f) - 1f) * scale;
            var offsetY = ((world.Random.Next(100) / 50f) - 1f) * scale;
            var before = beams[first + i - 1];
            var after = beams[first + i];
            before.X2 += offsetX;
            before.Y2 += offsetY;
            after.X1 = before.X2;
            after.Y1 = before.Y2;
        }
    }

    private static float RandomFactor(World world) => (world.Random.Next(RandomFactorRoll) / 100f) + RandomFactorBase;

    // Zaps the first enemy ship in reach for a random 50-100% of the damage. Returns its distance.
    private static float ZapShip(Ship ship, World world, Beam aim)
    {
        var weapon = ship.Definition.Primary;
        foreach (var target in Combat.EnemyTargets(world, ship))
        {
            var distanceSquared = Combat.Square(target.X - ship.X) + Combat.Square(target.Y - ship.Y);
            if (distanceSquared >= Combat.Square(weapon.Radius + (target.Definition.Radius * Combat.RadiusScale)))
            {
                continue;
            }

            if (!Combat.IsShielded(target))
            {
                target.Energy -= weapon.Damage * RandomFactor(world);
            }

            aim.Set(target.X, target.Y, ship.X, ship.Y, AimRed, Cyan, Cyan, weapon.Lifetime);
            return MathF.Sqrt(distanceSquared);
        }

        return weapon.Radius + ExtraReach;
    }

    /// <summary>
    /// Damages the first breakable enemy projectile in reach by the full damage. Primary projectiles break at zero
    /// energy or less, special ones only below zero (as in the original). Returns its distance.
    /// </summary>
    private static float ZapProjectile(Ship ship, World world, Beam aim, bool special)
    {
        var weapon = ship.Definition.Primary;
        var enemies = world.Ships.Where(s => s.IsInGame && s.Index != ship.Index && s.Team != ship.Team);
        foreach (var enemy in enemies)
        {
            var (pool, count, radius, breakable) = special
                ? (enemy.SpecialProjectiles, enemy.Definition.Special.MaxProjectiles, enemy.Definition.Special.Radius, enemy.Definition.Special.CanBeDestroyed)
                : (enemy.PrimaryProjectiles, enemy.Definition.Primary.MaxProjectiles, enemy.Definition.Primary.Radius, enemy.Definition.Primary.CanBeDestroyed);
            if (!breakable)
            {
                continue;
            }

            for (var j = 0; j < count; j++)
            {
                var projectile = pool[j];
                var distanceSquared = Combat.Square(projectile.X - ship.X) + Combat.Square(projectile.Y - ship.Y);
                if (!projectile.IsActive || distanceSquared >= Combat.Square(weapon.Radius + (radius * Combat.RadiusScale)))
                {
                    continue;
                }

                projectile.Energy -= weapon.Damage;
                if (special ? projectile.Energy < 0 : projectile.Energy <= 0)
                {
                    projectile.Energy = 0;
                    projectile.Lifetime = 0;
                }

                aim.Set(projectile.X, projectile.Y, ship.X, ship.Y, AimRed, Cyan, Cyan, weapon.Lifetime);
                return MathF.Sqrt(distanceSquared);
            }
        }

        return weapon.Radius + ExtraReach;
    }

    /// <summary>
    /// Splits the aim line (from the ship at X2, Y2 to the target at X1, Y1) into two bolts of 10 segments in beams
    /// 1-10 and 11-20, then jitters the joints, more strongly toward the target.
    /// </summary>
    private static void DrawBolts(Ship ship, World world, Beam aim)
    {
        var beams = ship.Beams;
        var stepX = (aim.X1 - aim.X2) / BoltSegments;
        var stepY = (aim.Y1 - aim.Y2) / BoltSegments;
        for (var i = 0; i < BoltSegments; i++)
        {
            foreach (var segment in new[] { beams[i + 1], beams[i + 1 + BoltSegments] })
            {
                segment.Set(aim.X2 + (i * stepX), aim.Y2 + (i * stepY), aim.X2 + ((i + 1) * stepX), aim.Y2 + ((i + 1) * stepY), BoltRed, Cyan, Cyan, BoltLifetime);
                segment.VelocityX = 0;
                segment.VelocityY = 0;
            }
        }

        for (var i = 1; i < BoltSegments; i++)
        {
            JitterJoint(beams[i], beams[i + 1], i, world);
            JitterJoint(beams[i + BoltSegments], beams[i + 1 + BoltSegments], i, world);
        }
    }

    // Moves the end of one segment and the start of the next by the same random offset.
    private static void JitterJoint(Beam before, Beam after, int joint, World world)
    {
        var scale = joint / (BoltSegments / 2f);
        var offsetX = world.Random.Next(100) / 100f * BoltJitter * scale;
        var offsetY = world.Random.Next(100) / 100f * BoltJitter * scale;
        before.X2 += offsetX;
        after.X1 = before.X2;
        before.Y2 += offsetY;
        after.Y1 = before.Y2;
    }

    // A ship or projectile hit by a chain link, and where the bolt ends.
    private readonly record struct ChainTarget(object Item, float X, float Y);
}
