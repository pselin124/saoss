namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Bulldog: fires slow rockets that keep accelerating along their own heading, which the pilot steers by turning;
/// the special is a forward boost for ramming.
/// </summary>
public sealed class BulldogBehavior : ShipBehavior
{
    private const float MuzzleDistance = 1.4f;
    private const float LaunchSpeedFactor = 150f;
    private const float RocketDrag = 1f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        var rocket = Combat.FreeSlot(ship.PrimaryProjectiles, weapon.MaxProjectiles);
        if (rocket == null)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        var (headingX, headingY) = ship.Heading;
        var launchSpeed = ship.Definition.Thrust * LaunchSpeedFactor;
        rocket.Lifetime = weapon.Lifetime;
        rocket.X = ship.X + (headingX * ship.Definition.Radius * MuzzleDistance);
        rocket.Y = ship.Y + (headingY * ship.Definition.Radius * MuzzleDistance);
        rocket.VelocityX = (headingX * launchSpeed) + ship.VelocityX;
        rocket.VelocityY = (headingY * launchSpeed) + ship.VelocityY;
        rocket.Angle = ship.Angle;
        rocket.Energy = weapon.ProjectileEnergy;
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.SpecialCooldown = special.Cooldown;
        ship.Fuel -= special.FuelCost;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var (headingX, headingY) = ship.Heading;
        ship.VelocityY += headingY * special.Speed;
        ship.VelocityX += headingX * special.Speed;
        world.Emit(new EffectEvent(EffectKind.BulldogBoostExhaust, ship.X, ship.Y, ship.VelocityX, ship.VelocityY));
    }

    /// <summary>
    /// Per rocket: count down, move, planet, hit ships, then accelerate along the rocket's heading with drag.
    /// </summary>
    public override void UpdatePrimaryProjectiles(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        for (var i = 0; i < weapon.MaxProjectiles; i++)
        {
            var rocket = ship.PrimaryProjectiles[i];
            if (!rocket.IsActive)
            {
                continue;
            }

            rocket.Lifetime--;
            rocket.X += rocket.VelocityX;
            rocket.Y += rocket.VelocityY;
            Combat.ApplyPlanetAndPull(rocket);

            // No shooter check: a rocket can hit the Bulldog that fired it.
            foreach (var target in Combat.Targets(world))
            {
                if (!Combat.Touches(target, rocket, weapon.Radius))
                {
                    continue;
                }

                world.PlaySound(ship, ShipSound.Hit);
                rocket.Lifetime = 0;

                // The original subtracts the damage twice.
                target.Energy -= weapon.Damage * 2f;
                Combat.Knock(target, rocket, weapon.Knockback);
            }

            var heading = rocket.Angle / 180f * MathF.PI;
            rocket.VelocityX = ShipMovement.Drag(rocket.VelocityX + (weapon.Speed * MathF.Cos(heading)), RocketDrag);
            rocket.VelocityY = ShipMovement.Drag(rocket.VelocityY + (weapon.Speed * MathF.Sin(heading)), RocketDrag);
        }
    }
}
