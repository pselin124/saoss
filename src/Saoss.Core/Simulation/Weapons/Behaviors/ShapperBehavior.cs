namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Shapper: normally fires a short blue laser bolt that hurts every ship and weakens every projectile it crosses;
/// the special toggles a slow shield mode in which it fires pairs of straight shots instead.
/// </summary>
public sealed class ShapperBehavior : ShipBehavior
{
    private const float ShotSideAngle = 15f;
    private const float ShotMuzzleDistance = 1.6f;
    private const float BoltLengthDivisor = 4f;
    private const float BoltSpeedDivisor = 6f;
    private const int BoltLifetime = 6;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        if (ship.IsShieldActive)
        {
            FireShots(ship, world);
        }
        else
        {
            FireBolt(ship, world);
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialHit);
        ship.IsShieldActive = !ship.IsShieldActive;
    }

    private static void FireShots(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var left = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (left != null)
        {
            world.PlaySound(ship, ShipSound.SpecialFire);
            Combat.Launch(left, ship, ShotSideAngle, ShotMuzzleDistance);

            // The original gives the first shot the special projectile's energy (zero).
            left.Energy = ship.Definition.Special.ProjectileEnergy;
        }

        var right = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (right != null)
        {
            Combat.Launch(right, ship, -ShotSideAngle, ShotMuzzleDistance);
        }
    }

    /// <summary>
    /// Steps: restart the fire sound, pay, place the moving bolt in beam 0, then hurt every other ship and weaken every
    /// projectile of other ships that a bolt sample touches. No team or shield checks.
    /// </summary>
    private static void FireBolt(Ship ship, World world)
    {
        var definition = ship.Definition;
        var weapon = definition.Primary;
        var special = definition.Special;
        ship.FireCooldown = weapon.FireDelay;

        // The original's "already playing" check is commented out, so the sound restarts with every bolt.
        world.PlaySound(ship, ShipSound.Fire, loop: true);
        ship.IsFireLoopPlaying = true;
        ship.Fuel -= weapon.FuelCost;

        var (headingX, headingY) = ship.Heading;
        var end = ((special.Radius - 1f) / BoltLengthDivisor) + 1f;
        var beam = ship.Beams[0];
        beam.Set(
            ship.X + (definition.Radius * headingX) + ship.VelocityX,
            ship.Y + (definition.Radius * headingY) + ship.VelocityY,
            ship.X + (end * headingX) + ship.VelocityX,
            ship.Y + (end * headingY) + ship.VelocityY,
            red: 0f,
            green: 0f,
            blue: 1f,
            BoltLifetime);
        beam.VelocityX = headingX * (special.Radius - 1f) / BoltSpeedDivisor;
        beam.VelocityY = headingY * (special.Radius - 1f) / BoltSpeedDivisor;

        var backward = MathF.Atan2(beam.Y1 - beam.Y2, beam.X1 - beam.X2);
        foreach (var target in Combat.OtherTargets(world, ship))
        {
            if (Combat.BeamCrosses(beam, backward, special.Radius, target.X, target.Y, target.Definition.Radius))
            {
                target.Energy -= special.Damage;
            }
        }

        foreach (var other in world.Ships.Where(s => s.IsInGame && s.Index != ship.Index))
        {
            var otherWeapons = other.Definition;
            Burn(ship, beam, backward, other.PrimaryProjectiles, otherWeapons.Primary.MaxProjectiles, otherWeapons.Primary.Radius, killAtZero: true);
            Burn(ship, beam, backward, other.SpecialProjectiles, otherWeapons.Special.MaxProjectiles, otherWeapons.Special.Radius, killAtZero: false);
        }
    }

    // The original destroys primary projectiles at zero energy but special projectiles only below zero.
    private static void Burn(Ship ship, Beam beam, float backward, Projectile[] pool, int count, float radius, bool killAtZero)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < count; i++)
        {
            var projectile = pool[i];
            if (!projectile.IsActive || !Combat.BeamCrosses(beam, backward, special.Radius, projectile.X, projectile.Y, radius))
            {
                continue;
            }

            projectile.Energy -= special.Damage;
            if (killAtZero ? projectile.Energy <= 0 : projectile.Energy < 0)
            {
                projectile.Energy = 0;
                projectile.Lifetime = 0;
            }
        }
    }
}
