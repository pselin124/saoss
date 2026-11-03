namespace Saoss.Core.Simulation.Weapons.Behaviors;

/// <summary>
/// Tongu: fires a pair of shots from both sides that spread more in range, speed and strength the longer fire is
/// held; the special shoots out a tongue that hits ships hard on contact.
/// </summary>
public sealed class TonguBehavior : ShipBehavior
{
    private const float SideAngle = 26f;
    private const float MuzzleDistance = 1.5f;
    private const float MaxSpread = 100f;
    private const float SpreadPerShot = 7f;
    private const int TongueActive = 2;
    private const int TongueDone = 1;
    private const float TongueGrowthDivisor = 20f;
    private const float TongueHitMargin = 0.2f;

    /// <inheritdoc/>
    public override void Fire(Ship ship, World world)
    {
        var weapon = ship.Definition.Primary;
        ship.FireCooldown = weapon.FireDelay;
        ship.Fuel -= weapon.FuelCost;
        if (ship.WeaponCharge < MaxSpread)
        {
            ship.WeaponCharge += SpreadPerShot;
        }

        // The second shot uses the first free slot after the first shot's slot.
        var next = 0;
        foreach (var side in new[] { 1f, -1f })
        {
            for (; next < weapon.MaxProjectiles; next++)
            {
                if (!ship.PrimaryProjectiles[next].IsActive)
                {
                    Launch(ship, world, ship.PrimaryProjectiles[next], side);
                    next++;
                    break;
                }
            }
        }
    }

    /// <inheritdoc/>
    public override void ActivateSpecial(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        ship.Fuel -= special.FuelCost;
        ship.SpecialCooldown = special.Cooldown;
        world.PlaySound(ship, ShipSound.SpecialFire);
        ship.FireSide = TongueActive;
    }

    /// <summary>
    /// While the special cools down, the tongue tip grows out in front of the ship and shrinks back; it damages
    /// every other ship it touches and then retracts for the rest of the cooldown.
    /// </summary>
    public override void BeforePositionUpdate(Ship ship, World world)
    {
        var special = ship.Definition.Special;
        if (ship.SpecialCooldown <= 0 || ship.FireSide < TongueActive)
        {
            return;
        }

        var length = ship.SpecialCooldown < special.Cooldown / 2f
            ? ship.SpecialCooldown / TongueGrowthDivisor
            : (special.Cooldown - ship.SpecialCooldown) / TongueGrowthDivisor;

        // The original passed the heading in degrees to cos/sin as radians, so the tongue points in odd directions.
        var reach = (length * 2f) + ship.Definition.Radius;
        var tipX = ship.X + (MathF.Cos(ship.Angle) * reach);
        var tipY = ship.Y + (MathF.Sin(ship.Angle) * reach);

        // No team or shield check, as in the original.
        foreach (var target in Combat.OtherTargets(world, ship))
        {
            var dx = tipX - target.X;
            var dy = tipY - target.Y;
            if ((dx * dx) + (dy * dy) < Combat.Square((target.Definition.Radius + TongueHitMargin) * Combat.RadiusScale))
            {
                target.Energy -= special.Damage;
                ship.FireSide = TongueDone;
                world.PlaySound(ship, ShipSound.SpecialHit);
            }
        }
    }

    // Lifetime, speed and energy are each reduced by a random amount that grows with the charge.
    private static void Launch(Ship ship, World world, Projectile shot, float side)
    {
        var weapon = ship.Definition.Primary;
        var spread = ship.WeaponCharge;
        var (headingX, headingY) = ship.Heading;
        var offset = (ship.Angle + (SideAngle * side)) / 180f * MathF.PI;
        world.PlaySound(ship, ShipSound.Fire);
        shot.Lifetime = (int)(weapon.Lifetime - world.Random.Next(spread));
        shot.X = ship.X + (MathF.Cos(offset) * ship.Definition.Radius * MuzzleDistance);
        shot.Y = ship.Y + (MathF.Sin(offset) * ship.Definition.Radius * MuzzleDistance);
        shot.VelocityX = (headingX * weapon.Speed * SpeedFactor(world, spread)) + ship.VelocityX;
        shot.VelocityY = (headingY * weapon.Speed * SpeedFactor(world, spread)) + ship.VelocityY;
        shot.Angle = ship.Angle;
        shot.Energy = weapon.ProjectileEnergy / ((world.Random.Next(spread) / 100f) + 1);
    }

    private static float SpeedFactor(World world, float spread) => (world.Random.Next(spread) / 100f) + 1 - (spread / 100f);
}
