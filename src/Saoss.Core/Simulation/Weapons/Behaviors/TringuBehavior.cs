namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Tringu: fires one big, slow shot that kicks the ship backwards; the special releases a ring of small
/// shots flying outward in every direction.
/// </summary>
public sealed class TringuBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.6f;
    private const float Recoil = 0.7f;
    private const float RingRadiusFactor = 5f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var shot = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (shot != null)
        {
            world.PlaySound(ship, ShipSound.Fire);
            Combat.Launch(shot, ship, 0f, MuzzleDistance);
        }

        var (headingX, headingY) = ship.Heading;
        ship.VelocityX -= Recoil * headingX;
        ship.VelocityY -= Recoil * headingY;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.Fuel -= special.FuelCost;

        // Every slot is reused, even ones still flying from the previous ring.
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var direction = (float)i / special.MaxProjectiles * 2f * MathF.PI;
            var projectile = ship.SpecialProjectiles[i];
            projectile.X = ship.X + (MathF.Cos(direction) * ship.Definition.Radius * RingRadiusFactor);
            projectile.Y = ship.Y + (MathF.Sin(direction) * ship.Definition.Radius * RingRadiusFactor);
            projectile.VelocityX = ship.VelocityX + (MathF.Cos(direction) * special.Speed);
            projectile.VelocityY = ship.VelocityY + (MathF.Sin(direction) * special.Speed);
            projectile.Lifetime = special.Duration;
            projectile.Energy = special.ProjectileEnergy;
            projectile.Angle = 0;
        }
    }

    /// <inheritdoc/>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var projectile = ship.SpecialProjectiles[i];
            if (!projectile.IsActive)
            {
                continue;
            }

            projectile.Lifetime--;
            projectile.X += projectile.VelocityX;
            projectile.Y += projectile.VelocityY;
            Combat.ApplyPlanet(projectile);

            // No team check: the ring hurts teammates too.
            foreach (var target in Combat.OtherTargets(world, ship))
            {
                if (Combat.Touches(target, projectile, ship.Definition.Primary.Radius))
                {
                    projectile.Lifetime = 0;
                    target.Energy -= special.Damage;
                    Combat.Knock(target, projectile, special.Knockback);
                }
            }
        }
    }
}
