namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Electra: fires a short-range lightning bolt that zaps the nearest enemy ship, or else an enemy projectile, in
/// reach; the special eats a nearby asteroid to refill the fuel tank.
/// </summary>
public sealed class ElectraBehavior : ShipBehavior
{
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

    /// <summary>Eats every asteroid within reach: each one is destroyed and refills the fuel tank.</summary>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        foreach (var asteroid in world.Asteroids.Asteroids)
        {
            var dx = ship.X - asteroid.X;
            var dy = ship.Y - asteroid.Y;
            if (MathF.Sqrt((dx * dx) + (dy * dy)) < special.Radius)
            {
                world.PlaySound(ship, ShipSound.SpecialFire);
                ship.SpecialCooldown = special.Cooldown;
                asteroid.Energy = 0;
                ship.Fuel = ship.Definition.MaxFuel;
            }
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
}
