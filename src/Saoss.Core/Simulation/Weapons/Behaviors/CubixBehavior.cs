namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Cubix: fires single shots from the nose; the special leaves a stationary mine behind and jumps the ship
/// forward, so holding it lays a trail of mines.
/// </summary>
public sealed class CubixBehavior : ShipBehavior
{
    private const float MuzzleDistance = 0.6f;
    private const float MineDropDistance = 1.1f;
    private const float SlowSpeed = 0.4f;
    private const float SlowBoostDivisor = 5f;
    private const float BoostDivisor = 30f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;

        // The original only reused slots whose lifetime is exactly zero, not negative.
        var shot = ship.PrimaryProjectiles.Take(weapon.MaxProjectiles).FirstOrDefault(p => p.Lifetime == 0);
        if (shot == null)
        {
            return;
        }

        world.PlaySound(ship, ShipSound.Fire);
        Combat.Launch(shot, ship, 0f, MuzzleDistance);
    }

    /// <summary>Drops a mine behind the ship, then jumps the ship forward and adds some forward speed.</summary>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);

        var mine = Combat.FreeSlot(ship.SpecialProjectiles, special.MaxProjectiles);
        if (mine != null)
        {
            var backward = (ship.Angle + 180f) / 180f * MathF.PI;
            mine.X = ship.X + (MathF.Cos(backward) * ship.Definition.Radius * MineDropDistance);
            mine.Y = ship.Y + (MathF.Sin(backward) * ship.Definition.Radius * MineDropDistance);
            mine.VelocityX = 0;
            mine.VelocityY = 0;
            mine.Lifetime = special.Duration;
            mine.Angle = 0;
            mine.Energy = special.ProjectileEnergy;
        }

        var (headingX, headingY) = ship.Heading;
        ship.X += headingX * special.Speed;
        ship.Y += headingY * special.Speed;
        if (MathF.Sqrt((ship.VelocityX * ship.VelocityX) + (ship.VelocityY * ship.VelocityY)) < SlowSpeed)
        {
            ship.VelocityX += headingX * special.Speed / SlowBoostDivisor;
            ship.VelocityY += headingY * special.Speed / SlowBoostDivisor;
        }

        ship.VelocityX += headingX * special.Speed / BoostDivisor;
        ship.VelocityY += headingY * special.Speed / BoostDivisor;
    }

    /// <summary>Mines stay put, count down and explode on any other ship that touches them (no team check).</summary>
    public override void UpdateSpecialProjectiles(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        for (var i = 0; i < special.MaxProjectiles; i++)
        {
            var mine = ship.SpecialProjectiles[i];
            if (!mine.IsActive)
            {
                continue;
            }

            mine.Lifetime--;
            foreach (var target in Combat.OtherTargets(world, ship))
            {
                var reach = (special.Radius + target.Definition.Radius) * Combat.RadiusScale;
                var distanceSquared = Combat.Square(mine.X - target.X) + Combat.Square(mine.Y - target.Y);
                if (distanceSquared >= reach * reach || Combat.IsShielded(target))
                {
                    continue;
                }

                // The loop goes on after the mine is used up, so it can hit several ships in the same tick.
                target.Energy -= special.Damage;
                mine.Lifetime = 0;
                world.PlaySound(ship, ShipSound.SpecialHit);
            }
        }
    }
}
