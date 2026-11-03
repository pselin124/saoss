namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Ufo: a short cyan laser that locks onto the first enemy ship in range, or else a breakable enemy projectile
/// in range; the special teleports the Ufo to a random spot away from the planet.
/// </summary>
public sealed class UfoBehavior : ShipBehavior
{
    private const float NoTargetMargin = 10f;
    private const float IdleBeamExtra = 1f;
    private const float MinTeleportDistance = 20f;

    /// <summary>
    /// Fires the laser. Steps: pay, then try an enemy ship, enemy primary projectiles and enemy special projectiles in
    /// that order; if nothing is in reach the beam points forward. While thrusting the ship end of the beam leads ahead.
    /// </summary>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        world.PlaySound(ship, ShipSound.Fire);
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var noTarget = weapon.Radius + NoTargetMargin;
        var distance = ZapShip(ship, world) ?? noTarget;
        if (distance >= noTarget)
        {
            distance = ZapProjectile(ship, world, special: false) ?? noTarget;
        }

        if (distance >= noTarget)
        {
            distance = ZapProjectile(ship, world, special: true) ?? noTarget;
        }

        var (headingX, headingY) = ship.Heading;
        if (distance >= noTarget)
        {
            var reach = weapon.Radius + IdleBeamExtra;
            SetBeam(ship, ship.X + (headingX * reach), ship.Y + (headingY * reach));
            ship.WeaponAngle = ship.Angle;
        }

        if (ship.Controls.Thrust)
        {
            ship.Beams[0].X2 += headingX * ship.Definition.Thrust;
            ship.Beams[0].Y2 += headingY * ship.Definition.Thrust;
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.SpecialCooldown = special.Cooldown;
        ship.IsTeleportPending = true;
        ship.TeleportX = 0;
        ship.TeleportY = 0;
        ship.Fuel -= special.FuelCost;
        while (MathF.Sqrt(Combat.Square(ship.TeleportX) + Combat.Square(ship.TeleportY)) < MinTeleportDistance)
        {
            ship.TeleportX = world.Random.Next(GameConstants.ArenaMaxX * 2f) - GameConstants.ArenaMaxX;
            ship.TeleportY = world.Random.Next(GameConstants.ArenaMaxY * 2f) - GameConstants.ArenaMaxY;
        }

        // The ShipMovement step moves the Ufo to the target later in this tick.
    }

    // Hits the first enemy ship (in player order, not the nearest) inside the laser's reach.
    private static float? ZapShip(Ship ufo, World world)
    {
        var weapon = ufo.Definition.Primary;
        foreach (var target in Combat.EnemyTargets(world, ufo))
        {
            var dx = target.X - ufo.X;
            var dy = target.Y - ufo.Y;
            var distanceSquared = (dx * dx) + (dy * dy);
            if (distanceSquared < Combat.Square(weapon.Radius + (target.Definition.Radius * Combat.RadiusScale)))
            {
                if (!Combat.IsShielded(target))
                {
                    target.Energy -= weapon.Damage;
                }

                LockOn(ufo, target.X, target.Y, dx, dy);
                return MathF.Sqrt(distanceSquared);
            }
        }

        return null;
    }

    // Hits the first breakable enemy projectile inside the laser's reach.
    private static float? ZapProjectile(Ship ufo, World world, bool special)
    {
        var weapon = ufo.Definition.Primary;
        var owners = world.Ships.Where(s => s.IsInGame && s.Index != ufo.Index && s.Team != ufo.Team);
        foreach (var owner in owners)
        {
            var ownerWeapon = owner.Definition;
            var (pool, count, radius, canBreak) = special
                ? (owner.SpecialProjectiles, ownerWeapon.Special.MaxProjectiles, ownerWeapon.Special.Radius, ownerWeapon.Special.CanBeDestroyed)
                : (owner.PrimaryProjectiles, ownerWeapon.Primary.MaxProjectiles, ownerWeapon.Primary.Radius, ownerWeapon.Primary.CanBeDestroyed);
            if (!canBreak)
            {
                continue;
            }

            for (var i = 0; i < count; i++)
            {
                var projectile = pool[i];
                if (!projectile.IsActive)
                {
                    continue;
                }

                var dx = projectile.X - ufo.X;
                var dy = projectile.Y - ufo.Y;
                var distanceSquared = (dx * dx) + (dy * dy);
                if (distanceSquared >= Combat.Square(weapon.Radius + (radius * Combat.RadiusScale)))
                {
                    continue;
                }

                projectile.Energy -= weapon.Damage;

                // A special projectile left at exactly zero energy survives in the original.
                if (special ? projectile.Energy < 0 : projectile.Energy <= 0)
                {
                    projectile.Energy = 0;
                    projectile.Lifetime = 0;
                }

                LockOn(ufo, projectile.X, projectile.Y, dx, dy);
                return MathF.Sqrt(distanceSquared);
            }
        }

        return null;
    }

    private static void LockOn(Ship ufo, float x, float y, float dx, float dy)
    {
        SetBeam(ufo, x, y);
        ufo.WeaponAngle = MathF.Atan2(dy, dx) * 180f / MathF.PI;
    }

    // The laser always uses the first beam slot, drawn from the far end back to the Ufo.
    private static void SetBeam(Ship ufo, float x, float y)
    {
        var beam = ufo.Beams[0];
        beam.Set(x, y, ufo.X, ufo.Y, red: 0f, green: 1f, blue: 1f, ufo.Definition.Primary.Lifetime);
        beam.VelocityX = 0;
        beam.VelocityY = 0;
    }
}
